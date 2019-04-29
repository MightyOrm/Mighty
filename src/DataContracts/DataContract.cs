using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Mighty.DataContracts
{
    /// <summary>
    /// Holds the data persistence contract for a given type in Mighty.
    /// This contract depends on the item type, the columns paramater and the specified data mapping.
    /// </summary>
    public class DataContract
    {
        /// <summary>
        /// The info about what this is a data contract for
        /// </summary>
        public DataContractKey Key { get; protected set; }

        /// <summary>
        /// All data read columns in one string, or "*" (mapping, if any, already applied)
        /// </summary>
        public string ReadColumns { get; protected set; }

        /// <summary>
        /// The reflected <see cref="MemberInfo"/> corresponding to all specified columns in the database table (null in data contracts for dynamic type)
        /// </summary>
        public Dictionary<string, DataContractMemberInfo> ColumnNameToMemberInfo;

        /// <summary>
        /// Create a new data contract corresponding to the values in the key
        /// </summary>
        /// <param name="Key">All the items on which the contract depends</param>
        public DataContract(DataContractKey Key)
        {
            this.Key = Key;
            if (!Key.IsDynamic)
            {
                ColumnNameToMemberInfo = new Dictionary<string, DataContractMemberInfo>(Key.mapper.UseCaseSensitiveMapping() ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase);
            }
            var ReadColumnList = new List<string>();

            if (Key.columns == null || Key.columns == "*")
            {
                if (Key.type != null)
                {
                    IncludeReflectedColumns(ReadColumnList, Key, BindingFlags.Instance | BindingFlags.Public);
                    IncludeReflectedColumns(ReadColumnList, Key, BindingFlags.Instance | BindingFlags.NonPublic);
                }
            }
            else
            {
                IncludeColumnsDrivenColumns(ReadColumnList, Key);
            }
            if (ReadColumnList.Count == 0)
            {
                ReadColumns = "*";
            }
            else
            {
                ReadColumns = string.Join(",", ReadColumnList);
            }
        }

        /// <summary>
        /// Include the columns specified in the <see cref="MightyOrm{T}"/> constructor's columns parameter.
        /// </summary>
        /// <param name="ReadColumnList"></param>
        /// <param name="key"></param>
        protected void IncludeColumnsDrivenColumns(List<string> ReadColumnList, DataContractKey key)
        {
            foreach (var column in key.columns.Split(',').Select(column => column.Trim()))
            {
                if (key.type == null)
                {
                    // for dynamic Mighty
                    ReadColumnList.Add(key.mapper.GetColumnName(null, column));
                }
                else
                {
                    MemberInfo[] members = key.type.GetMember(column, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    int count = members.Count();
                    if (count > 1)
                    {
                        throw new InvalidOperationException(
                            string.Format($"Specified column {column} found {count} times in {key.type.FullName}"));
                    }
                    if (count == 0 || (members[0] as FieldInfo == null && members[0] as PropertyInfo == null))
                    {
                        throw new InvalidOperationException(
                            string.Format($"Specified column {column} should be an exact match (including case) for a field or property in {key.type.FullName}"));
                    }
                    IncludeReflectedColumn(ReadColumnList, key, members[0], true);
                }
            }
        }

        /// <summary>
        /// Include reflected columns
        /// </summary>
        /// <param name="ReadColumnList"></param>
        /// <param name="key"></param>
        /// <param name="bindingFlags"></param>
        protected void IncludeReflectedColumns(List<string> ReadColumnList, DataContractKey key, BindingFlags bindingFlags)
        {
            foreach (var member in key.type.GetMembers(bindingFlags)
                .Where(m => m is FieldInfo || m is PropertyInfo))
            {
                IncludeReflectedColumn(ReadColumnList, key, member, (bindingFlags & BindingFlags.Public) != 0);
            }
        }

        /// <summary>
        /// Add a reflected field to the column list
        /// </summary>
        /// <param name="ReadColumnList"></param>
        /// <param name="key"></param>
        /// <param name="member"></param>
        /// <param name="include">The initial default include status (depending on public, non-public or columns-driven)</param>
        protected void IncludeReflectedColumn(List<string> ReadColumnList, DataContractKey key, MemberInfo member, bool include)
        {
            string sqlColumnName = null;
            DataDirection dataDirection = 0;
            foreach (var attr in member.GetCustomAttributes(false))
            {
                if (attr is DatabaseColumnAttribute)
                {
                    var colAttr = (DatabaseColumnAttribute)attr;
                    include = true;
                    sqlColumnName = colAttr.ColumnName;
                    dataDirection = colAttr.DataDirection;
                }
                if (attr is DatabaseIgnoreAttribute) include = false;
            }
            if (include)
            {
                sqlColumnName = sqlColumnName ?? key.mapper.GetColumnName(key.type, member.Name);
                ColumnNameToMemberInfo.Add(sqlColumnName, new DataContractMemberInfo(key.type, member, dataDirection));
                ReadColumnList.Add(sqlColumnName);
            }
        }


        /// <summary>
        /// Is this column managed by Mighty?
        /// </summary>
        /// <param name="columnName">The database column name</param>
        /// <returns></returns>
        public bool IsMightyColumn(string columnName)
        {
            return Key.IsDynamic || ColumnNameToMemberInfo.ContainsKey(columnName);
        }

        /// <summary>
        /// Get data member info for column name.
        /// </summary>
        /// <param name="columnName">The database column name</param>
        /// <param name="what">Brief description of what is being looked for</param>
        /// <returns></returns>
        public DataContractMemberInfo GetDataMemberInfo(string columnName, string what = null)
        {
            DataContractMemberInfo memberInfo;
            if (!ColumnNameToMemberInfo.TryGetValue(columnName, out memberInfo))
            {
                throw new InvalidOperationException(string.Format("No accessible field or property {0}{1}. Must be exact match including case, and must be publicly accessible or marked with DatabaseColumnAttribute.", what == null ? "" : $"for {what } ", columnName));
            }
            return memberInfo;
        }

        /// <summary>
        /// Try and get data member info for column name.
        /// </summary>
        /// <param name="columnName">The database column name</param>
        /// <param name="memberInfo">The data member info</param>
        /// <param name="dataDirection">The required data direction (only non-zero is tested)</param>
        /// <returns></returns>
        public bool TryGetDataMemberInfo(string columnName, out DataContractMemberInfo memberInfo, DataDirection dataDirection = 0)
        {
            if (!ColumnNameToMemberInfo.TryGetValue(columnName, out memberInfo)) return false;
            if (dataDirection != 0 && memberInfo.DataDirection != 0 && (memberInfo.DataDirection | dataDirection) == 0) memberInfo = null;
            return memberInfo != null;
        }

        /// <summary>
        /// Equals if <see cref="DataContractKey"/> is equal
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Key.GetHashCode();
        }

        /// <summary>
        /// Equals if <see cref="DataContractKey"/> is equal
        /// </summary>
        /// <returns></returns>
        public override bool Equals(object other)
        {
            if (!(other is DataContract)) return false;
            return Key == ((DataContract)other).Key;
        }
    }
}
