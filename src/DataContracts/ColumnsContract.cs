﻿using System;
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
    /// <remarks>
    /// It should always be possible to use `columns` for quick'n'dirty column mapping as in original
    /// Massive i.e. "film_id AS FilmId" etc.
    /// 
    /// Also New and Query are always driven by database column names. New is a special kind of data read, in this sense. So, db->field name mapping.
    /// 
    /// All data writes are always driven by the field/property names of the to-be-written data. So, field->db name mapping.
    /// 
    /// In the case of generic instances of Mighty:
    ///   - We always have a class, and we always create a map for *all* columns in the class
    ///   - The `columns` parameter just determines the default columns in a select (and can still be quick'n'dirty as above)
    ///     o If you do a *different* select (not the default columns) then all managed columns which come back in the results are updated in the object
    ///       (regardless of the initial `columns` value; we can't really filter on columns because we don't want to parse it)
    ///     o When you do New() all managed columns which come back in the table meta-data are updated
    /// 
    /// In the case of dynamic instances of Mighty:
    ///   - You cannot override any of the column mapping features of this mapper (you will get an exception if you do)
    ///   - You can still use quick'n'dirty mapping - see above
    ///     o If you do a query all returned fields are updated (regardless of the initial `columns` value)
    ///     o When you do New() all columns which are in the table meta-data are updated (you can't control this)
    ///     
    /// Remember, Mighty *never* parses the SQL fragments which the user sends in! It's been a pain to get this to a consistent state where it
    /// doesn't need to, but the result is better.
    /// </remarks>
    public class ColumnsContract
    {
        /// <summary>
        /// The info about what this is a data contract for
        /// </summary>
        public ColumnsContractKey Key { get; protected set; }

        /// <summary>
        /// All data read columns in one string (mapping, if any, already applied), or null
        /// </summary>
        public string ReadColumns { get; protected set; }

        /// <summary>
        /// All primary key fields in one string, or null
        /// </summary>
        public string KeyFields { get; protected set; }

        /// <summary>
        /// The reflected <see cref="MemberInfo"/> corresponding to all specified columns in the database table
        /// </summary>
        public Dictionary<string, ColumnsContractMemberInfo> ColumnNameToMemberInfo;

        /// <summary>
        /// The reverse mapping for all specified columns in the database table
        /// </summary>
        public Dictionary<string, string> MemberNameToColumnName;

        /// <summary>
        /// Create a new data contract corresponding to the values in the key
        /// </summary>
        /// <param name="Key">All the items on which the contract depends</param>
        public ColumnsContract(ColumnsContractKey Key)
        {
            this.Key = Key;
            if (!Key.NullContract)
            {
                var ReadColumnList = new List<string>();
                var KeyFieldList = new List<string>();
                bool foundControlledColumn = false;

                ColumnNameToMemberInfo = new Dictionary<string, ColumnsContractMemberInfo>(Key.CaseSensitiveColumnMapping ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase);
                MemberNameToColumnName = new Dictionary<string, string>(Key.CaseSensitiveColumnMapping ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase);

                if (Key.IsGeneric)
                {
                    var fc1 = AddReflectedColumns(ReadColumnList, KeyFieldList, Key, BindingFlags.Instance | BindingFlags.Public);
                    var fc2 = AddReflectedColumns(ReadColumnList, KeyFieldList, Key, BindingFlags.Instance | BindingFlags.NonPublic);
                    foundControlledColumn = fc1 || fc2;
                }
                else
                {
                    foreach (var column in Key.DynamicColumnSpec.Split(','))
                    {
                        AddReflectedColumn(ReadColumnList, KeyFieldList, Key, null, column, true);
                    }
                    foundControlledColumn = true;
                }

                if (foundControlledColumn)
                {
                    ReadColumns = string.Join(",", ReadColumnList);
                }
                if (KeyFieldList.Count > 0)
                {
                    KeyFields = string.Join(",", KeyFieldList);
                }
            }
        }

        /// <summary>
        /// Include reflected columns
        /// </summary>
        /// <param name="ReadColumnList"></param>
        /// <param name="KeyFieldList"></param>
        /// <param name="key"></param>
        /// <param name="bindingFlags"></param>
        /// <returns>Whether a controlled column (<see cref="DatabaseColumnAttribute"/> or <see cref="DatabaseIgnoreAttribute"/>) was found</returns>
        protected bool AddReflectedColumns(List<string> ReadColumnList, List<string> KeyFieldList, ColumnsContractKey key, BindingFlags bindingFlags)
        {
            bool foundControlledColumn = false;
            foreach (var member in key.DataItemType.GetMembers(bindingFlags)
                .Where(m => m is FieldInfo || m is PropertyInfo))
            {
                var fc = AddReflectedColumn(ReadColumnList, KeyFieldList, key, member, null, (bindingFlags & BindingFlags.Public) != 0);
                foundControlledColumn = foundControlledColumn || fc;
            }
            return foundControlledColumn;
        }

        /// <summary>
        /// Add a reflected field to the column list
        /// </summary>
        /// <param name="ReadColumnList"></param>
        /// <param name="KeyFieldList"></param>
        /// <param name="key"></param>
        /// <param name="member"></param>
        /// <param name="name"></param>
        /// <param name="include">The initial default include status (depending on public, non-public or columns-driven)</param>
        /// <returns>Whether a controlled column (<see cref="DatabaseColumnAttribute"/> or <see cref="DatabaseIgnoreAttribute"/>) was found</returns>
        protected bool AddReflectedColumn(List<string> ReadColumnList, List<string> KeyFieldList, ColumnsContractKey key, MemberInfo member, string name, bool include)
        {
            bool foundControlledColumn = false;
            bool isKey = false;
            string sqlColumnName = null;
            DataDirection dataDirection = 0;
            string transformSql = null;

            // Control things by attributes
            if (member != null)
            {
                foreach (var attr in member.GetCustomAttributes(false))
                {
                    if (attr is DatabaseColumnAttribute)
                    {
                        var colAttr = (DatabaseColumnAttribute)attr;
                        include = true;
                        sqlColumnName = colAttr.Name;
                        dataDirection = colAttr.Direction;
                        transformSql = colAttr.Transform;
                        foundControlledColumn = true;
                    }
                    if (attr is DatabaseIgnoreAttribute)
                    {
                        include = false;
                        foundControlledColumn = true;
                    }
                    if (attr is DatabaseKeyAttribute)
                    {
                        isKey = true;
                    }
                }
            }

            name = name ?? member.Name;

            // Also control things by the mapper
            DataDirection mapperDirection = key.ColumnDataDirection(key.DataItemType, name);
            if (mapperDirection != 0)
            {
                // We could do one of several things, but to make it like the ignore setting, we're
                // OR-ing the direction settings
                dataDirection |= mapperDirection;

                //// Not sure about this, but since having a column name in the mapper doesn't
                //// switch this on, then probably this shouldn't either?
                //foundcontrolledcolumn = true;
            }
            // The column will be ignored if the mapper or the attribute say so
            if (key.IgnoreColumn(key.DataItemType, name))
            {
                include = false;
                foundControlledColumn = true;
            }

            if (include)
            {
                // the column name from the attribute has precedence
                sqlColumnName = sqlColumnName ?? key.ColumnName(key.DataItemType, name);
                ColumnNameToMemberInfo.Add(sqlColumnName, new ColumnsContractMemberInfo(key.DataItemType, member, name, dataDirection));
                MemberNameToColumnName.Add(name, sqlColumnName);
                ReadColumnList.Add($"{(transformSql != null ? $"{transformSql} AS " : "")}{sqlColumnName}");
                if (isKey)
                {
                    KeyFieldList.Add(name);
                }
            }

            return foundControlledColumn;
        }


        /// <summary>
        /// Is this column managed by Mighty?
        /// </summary>
        /// <param name="columnName">The database column name</param>
        /// <returns></returns>
        public bool IsMightyColumn(string columnName)
        {
            return Key.NullContract || ColumnNameToMemberInfo.ContainsKey(columnName);
        }

        /// <summary>
        /// Get data member info for column name.
        /// </summary>
        /// <param name="columnName">The database column name</param>
        /// <param name="what">Brief description of what is being looked for</param>
        /// <returns></returns>
        public ColumnsContractMemberInfo GetDataMemberInfo(string columnName, string what = null)
        {
            ColumnsContractMemberInfo memberInfo;
            if (!TryGetDataMemberInfo(columnName, out memberInfo))
            {
                throw new InvalidOperationException(
                    string.Format(
                        "Cannot find any field or property{0} in {1} {2}.",
                        what == null ? "" : $" for named {what}",
                        Key.DataItemType.FullName,
                        string.Format(
                            Key.ColumnName == Mighty.Mapping.SqlNamingMapper.IdentityColumnMapping
                                ? "named {0}"
                                : "which is equal to \"{0}\" after column name mapping has been applied",
                            columnName)
                        ));
            }
            return memberInfo;
        }

        /// <summary>
        /// Try to get data member info for column name.
        /// </summary>
        /// <param name="columnName">The database column name</param>
        /// <param name="memberInfo">The data member info</param>
        /// <param name="dataDirection">The required data direction (only non-zero is tested)</param>
        /// <returns></returns>
        public bool TryGetDataMemberInfo(string columnName, out ColumnsContractMemberInfo memberInfo, DataDirection dataDirection = 0)
        {
            if (!TryGetDataMemberInfo(columnName, out memberInfo)) return false;
            if (dataDirection != 0 && memberInfo.DataDirection != 0 && (memberInfo.DataDirection | dataDirection) == 0) memberInfo = null;
            return memberInfo != null;
        }

        /// <summary>
        /// Internal lookup with sanity check: should never be called on dynamic instances of <see cref="MightyOrm"/>
        /// </summary>
        /// <param name="columnName">The database column name</param>
        /// <param name="memberInfo">The data member info</param>
        /// <returns></returns>
        private bool TryGetDataMemberInfo(string columnName, out ColumnsContractMemberInfo memberInfo)
        {
            if (Key.DataItemType == null)
            {
                // should not happen
                throw new Exception($"Trying to lookup {nameof(ColumnsContractMemberInfo)} on dynamic instance of {nameof(MightyOrm)}");
            }
            return ColumnNameToMemberInfo.TryGetValue(columnName, out memberInfo);
        }

        /// <summary>
        /// Equals if <see cref="ColumnsContractKey"/> is equal
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Key.GetHashCode();
        }

        /// <summary>
        /// Equals if <see cref="ColumnsContractKey"/> is equal
        /// </summary>
        /// <returns></returns>
        public override bool Equals(object other)
        {
            if (!(other is ColumnsContract)) return false;
            return Key == ((ColumnsContract)other).Key;
        }

        /// <summary>
        /// Get field or property based on C# name not column name
        /// (not a hashed lookup, so should not be used frequently)
        /// NB This will search in all and only managed data members, including managed non-public members,
        /// which is what we want... when we want it.
        /// </summary>
        /// <param name="name">Name of field or property</param>
        /// <param name="what">Short description of what is being looked for, for exception message</param>
        /// <returns></returns>
        public MemberInfo GetMember(string name, string what = null)
        {
            var member = ColumnNameToMemberInfo.Values.Where(m => m.Member.Name == name).FirstOrDefault();
            if (member == null)
            {
                throw new InvalidOperationException(
                    string.Format(
                        "Cannot find field or property named {0}{1} in {2} (must be exact match, including case)",
                        name,
                        what == null ? "" : $"for {what}",
                        Key.DataItemType.FullName));
            }
            return member.Member;
        }

        /// <summary>
        /// Return database column name from field or property name
        /// </summary>
        /// <param name="name">The field or property name</param>
        /// <returns>The database column name</returns>
        public string Map(string name)
        {
            if (Key.NullContract)
            {
                throw new InvalidOperationException($"It is not possible to map field names to column names in a non-auto-mapped dynamic instance {nameof(MightyOrm)} without a columns spec (specify the `columns` parameter in the constructor)");
            }
            string mapped;
            if (!MemberNameToColumnName.TryGetValue(name, out mapped))
            {
                throw new InvalidOperationException($"Cannot map field or property name {name} to any database column name");
            }
            return mapped;
        }
    }
}
