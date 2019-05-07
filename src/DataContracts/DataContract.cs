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
    public class DataContract
    {
        /// <summary>
        /// The info about what this is a data contract for
        /// </summary>
        public DataContractKey Key { get; protected set; }

        /// <summary>
        /// All data read columns in one string (mapping, if any, already applied), or null
        /// </summary>
        public string ReadColumns { get; protected set; }

        /// <summary>
        /// All primary key fields in one string, or null
        /// </summary>
        public string KeyColumns { get; protected set; }

        /// <summary>
        /// The final auto-map setting, after taking into account whether any columns were in fact renamed.
        /// Left at zero if nothing was renamed, since the remap in that case is the identity, and not
        /// re-mapping allows other hacky but useful uses of all the inputs.
        /// </summary>
        public AutoMap AutoMapSettings { get; protected set; }

        /// <summary>
        /// The reflected <see cref="MemberInfo"/> corresponding to all specified columns in the database table
        /// </summary>
        public Dictionary<string, DataContractMemberInfo> ColumnNameToMemberInfo;

        /// <summary>
        /// The reverse mapping for all specified columns in the database table
        /// </summary>
        public Dictionary<string, string> MemberNameToColumnName;

        /// <summary>
        /// Create a new data contract corresponding to the values in the key
        /// </summary>
        /// <param name="Key">All the items on which the contract depends</param>
        public DataContract(DataContractKey Key)
        {
            this.Key = Key;
            if (!Key.DynamicNullContract)
            {
                var ReadColumnList = new List<string>();
                var KeyColumnsList = new List<string>();
                bool foundControlledColumn = false;
                bool foundRenamedColumn = false;

                ColumnNameToMemberInfo = new Dictionary<string, DataContractMemberInfo>(Key.DatabaseTableSettings.CaseSensitiveColumnMapping ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase);
                MemberNameToColumnName = new Dictionary<string, string>(Key.DatabaseTableSettings.CaseSensitiveColumnMapping ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase);

                if (Key.IsGeneric)
                {
                    AddReflectedColumns(out bool fc1, out bool fr1, ReadColumnList, KeyColumnsList, Key, BindingFlags.Instance | BindingFlags.Public);
                    AddReflectedColumns(out bool fc2, out bool fr2, ReadColumnList, KeyColumnsList, Key, BindingFlags.Instance | BindingFlags.NonPublic);
                    foundControlledColumn = fc1 || fc2;
                    foundRenamedColumn = fr1 || fr2;
                }
                else
                {
                    foreach (var column in Key.DynamicColumnSpec.Split(','))
                    {
                        AddReflectedColumn(out bool fc, out bool fr, ReadColumnList, KeyColumnsList, Key, null, column, true);
                    }
                    foundControlledColumn = true;
                    foundRenamedColumn = true;
                }

                if (foundControlledColumn)
                {
                    // We have a read column list if there are any controlled columns (including e.g. ignored columns) in the contract
                    ReadColumns = string.Join(", ", ReadColumnList);
                }

                if (foundRenamedColumn)
                {
                    // This switches on auto-mapping by defaut if there are any actually renamed columns
                    AutoMapSettings = Key.DatabaseTableSettings.AutoMap;
                }

                if (KeyColumnsList.Count > 0)
                {
                    KeyColumns = string.Join(", ", KeyColumnsList);
                }
            }
        }

        /// <summary>
        /// Include reflected columns
        /// </summary>
        /// <param name="foundControlledColumn"></param>
        /// <param name="foundRenamedColumn"></param>
        /// <param name="ReadColumnList"></param>
        /// <param name="KeyColumnsList"></param>
        /// <param name="key"></param>
        /// <param name="bindingFlags"></param>
        /// <returns>Whether a controlled column (<see cref="DatabaseColumnAttribute"/> or <see cref="DatabaseIgnoreAttribute"/>) was found</returns>
        protected void AddReflectedColumns(
            out bool foundControlledColumn, out bool foundRenamedColumn,
            List<string> ReadColumnList, List<string> KeyColumnsList, DataContractKey key, BindingFlags bindingFlags)
        {
            foundControlledColumn = false;
            foundRenamedColumn = false;
            foreach (var member in key.DataItemType.GetMembers(bindingFlags)
                .Where(m => m is FieldInfo || m is PropertyInfo))
            {
                AddReflectedColumn(
                    out bool fc, out bool fr,
                    ReadColumnList, KeyColumnsList, key, member, null, (bindingFlags & BindingFlags.Public) != 0);
                foundControlledColumn = fc || foundControlledColumn;
                foundRenamedColumn = fr || foundRenamedColumn;
            }
        }

        /// <summary>
        /// Add a reflected field to the column list
        /// </summary>
        /// <param name="foundControlledColumn"></param>
        /// <param name="foundRenamedColumn"></param>
        /// <param name="ReadColumnList"></param>
        /// <param name="KeyColumnsList"></param>
        /// <param name="key"></param>
        /// <param name="member"></param>
        /// <param name="name"></param>
        /// <param name="include">The initial default include status (depending on public, non-public or columns-driven)</param>
        /// <returns>Whether a controlled column (<see cref="DatabaseColumnAttribute"/> or <see cref="DatabaseIgnoreAttribute"/>) was found</returns>
        protected void AddReflectedColumn(
            out bool foundControlledColumn, out bool foundRenamedColumn,
            List<string> ReadColumnList, List<string> KeyColumnsList, DataContractKey key, MemberInfo member, string name, bool include)
        {
            foundControlledColumn = false;
            foundRenamedColumn = false;
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
                    if (attr is DatabasePrimaryKeyAttribute)
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
                if (sqlColumnName != name)
                {
                    foundRenamedColumn = true;
                }
                ColumnNameToMemberInfo.Add(sqlColumnName, new DataContractMemberInfo(key.DataItemType, member, name, dataDirection));
                MemberNameToColumnName.Add(name, sqlColumnName);
                ReadColumnList.Add($"{(transformSql != null ? $"{transformSql} AS " : "")}{sqlColumnName}");
                if (isKey)
                {
                    KeyColumnsList.Add(sqlColumnName);
                }
            }
        }


        /// <summary>
        /// Is this column managed by Mighty?
        /// </summary>
        /// <param name="columnName">The database column name</param>
        /// <returns></returns>
        public bool IsMightyColumn(string columnName)
        {
            return Key.DynamicNullContract || ColumnNameToMemberInfo.ContainsKey(columnName);
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
        /// Try to get data member name for column name.
        /// </summary>
        /// <param name="columnName">The database column name</param>
        /// <param name="memberName">The data member name</param>
        /// <param name="dataDirection">The required data direction (only non-zero is tested)</param>
        /// <returns></returns>
        public bool TryGetDataMemberName(string columnName, out string memberName, DataDirection dataDirection = 0)
        {
            if (Key.DynamicNullContract)
            {
                memberName = columnName;
                return true;
            }
            if (TryGetDataMemberInfo(columnName, out DataContractMemberInfo memberInfo, dataDirection))
            {
                memberName = memberInfo.Name;
                return true;
            }
            memberName = null;
            return false;
        }

        /// <summary>
        /// Try to get data member info for column name.
        /// </summary>
        /// <param name="columnName">The database column name</param>
        /// <param name="memberInfo">The data member info</param>
        /// <param name="dataDirection">The required data direction (only non-zero is tested)</param>
        /// <returns></returns>
        public bool TryGetDataMemberInfo(string columnName, out DataContractMemberInfo memberInfo, DataDirection dataDirection = 0)
        {
            if (!TryGetDataMemberInfo(columnName, out memberInfo)) return false;
            if (dataDirection != 0 && memberInfo.DataDirection != 0 && (memberInfo.DataDirection | dataDirection) == 0) memberInfo = null;
            return memberInfo != null;
        }

        /// <summary>
        /// Look up <see cref="DataContractMemberInfo"/> from database column name
        /// </summary>
        /// <param name="columnName">The database column name</param>
        /// <param name="memberInfo">The data member info</param>
        /// <returns></returns>
        private bool TryGetDataMemberInfo(string columnName, out DataContractMemberInfo memberInfo)
        {
            return ColumnNameToMemberInfo.TryGetValue(columnName, out memberInfo);
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

        /// <summary>
        /// Get field or property based on C# field or property name.
        /// Not a hashed lookup, so should not be used frequently.
        /// This will search in all and only managed data members, including managed non-public members,
        /// which is what we want.
        /// </summary>
        /// <param name="fieldNname">Name of field or property</param>
        /// <param name="what">Short description of what is being looked for, for exception message</param>
        /// <returns></returns>
        public MemberInfo GetMember(string fieldNname, string what = null)
        {
            var member = ColumnNameToMemberInfo.Values.Where(m => m.Member.Name == fieldNname).FirstOrDefault();
            if (member == null)
            {
                throw new InvalidOperationException(
                    string.Format(
                        "Cannot find field or property named {0}{1} in {2} (must be exact match, including case)",
                        fieldNname,
                        what == null ? "" : $" for {what}",
                        Key.DataItemType.FullName));
            }
            return member.Member;
        }

        /// <summary>
        /// Return database column name from field or property name
        /// </summary>
        /// <param name="fieldName">The field or property name</param>
        /// <returns>The database column name</returns>
        public string Map(string fieldName)
        {
            if (Key.DynamicNullContract)
            {
                return fieldName;
            }
            string mapped;
            if (!MemberNameToColumnName.TryGetValue(fieldName, out mapped))
            {
                throw new InvalidOperationException($"Field or property name {fieldName} does not exist in {Key.DataItemType.FullName}, or exists but has been excluded from database column mapping");
            }
            return mapped;
        }

        /// <summary>
        /// Map a comma-separated list of field names to column names,
        /// or leave alone if the mapping implies that they are already column names.
        /// </summary>
        /// <param name="which">Which type of thing is this? Checked against the current columns data contract
        /// to decide whether to actually map. Or send <see cref="AutoMap.On"/> to map unconditionally.</param>
        /// <param name="fieldNames">The incoming field names</param>
        /// <returns></returns>
        public string Map(AutoMap which, string fieldNames)
        {
            if (fieldNames == null)
            {
                return null;
            }
            if (which == AutoMap.On || (which & AutoMapSettings) != 0)
            {
                return string.Join(", ", fieldNames.Split(',').Select(n => n.Trim()).Select(n => Map(n)));
            }
            else
            {
                // 'field names' are or should be already column names, in this case
                return fieldNames;
            }
        }

        /// <summary>
        /// Return field or property name from database column name
        /// </summary>
        /// <param name="columnName">The database column name</param>
        /// <returns>The field or property name</returns>
        public string ReverseMap(string columnName)
        {
            if (Key.DynamicNullContract)
            {
                return columnName;
            }
            DataContractMemberInfo member;
            if (!ColumnNameToMemberInfo.TryGetValue(columnName, out member))
            {
                throw new InvalidOperationException($"Cannot map database column name {columnName} to any field or property name");
            }
            return member.Name;
        }

        /// <summary>
        /// Map a comma-separated list of column names to field names,
        /// or leave alone if the mapping implies that they are already field names.
        /// </summary>
        /// <param name="which">Which type of thing is this? Checked against the current columns data contract
        /// to decide whether to actually map. Or send <see cref="AutoMap.On"/> to map unconditionally.</param>
        /// <param name="columnNames">The incoming column names</param>
        /// <returns></returns>
        public string ReverseMap(AutoMap which, string columnNames)
        {
            if (columnNames == null)
            {
                return null;
            }
            if (which == AutoMap.On || (which & AutoMapSettings) == 0)
            {
                return string.Join(", ", columnNames.Split(',').Select(n => n.Trim()).Select(n => ReverseMap(n)));
            }
            else
            {
                // They are (or should be) already field names
                return columnNames;
            }
        }

        /// <summary>
        /// Returns true if this is an instance or sublass of <see cref="MightyOrm{T}"/> and <paramref name="item"/> is of type T.
        /// </summary>
        /// <param name="item">The object to check</param>
        /// <returns></returns>
        public bool IsManagedGenericType(object item)
        {
            return Key.IsGeneric && item.GetType() == Key.DataItemType;
        }
    }
}
