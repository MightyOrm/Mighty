using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;

using Mighty.Mapping;
using Mighty.Plugins;

namespace Mighty.DataContracts
{
    /// <summary>
    /// A data contract key (unique identifier);
    /// all of the values on which a <see cref="DataContract"/> depends.
    /// </summary>
    public class DataContractKey
    {
        /// <summary>
        /// Is this a dynamic instance?
        /// </summary>
        public readonly bool IsGeneric;

        /// <summary>
        /// The data item type (which will always be null for the unique columns contract for dynamic instances of <see cref="MightyOrm"/>
        /// </summary>
        public readonly Type DataItemType;

        /// <summary>
        /// The columns spec, but only when this is actually driving the columns to use on a dynamic object
        /// </summary>
        public readonly string DynamicColumnSpec;

        /// <summary>
        /// The auto-map settings for the type
        /// </summary>
        /// <remarks>
        /// Not needed as part of this key's hashcode, since it is derived
        /// </remarks>
        public readonly bool HasColumnMapping;

        /// <summary>
        /// The user-supplied <see cref="DatabaseTableAttribute"/> settings for the data item type;
        /// or the same settings but derived by calling the user-supplied mapper if no such user-supplied attribute;
        /// or the same settings but from the default mapper if no such user-supplied mapper!
        /// </summary>
        public readonly DatabaseTableAttribute DatabaseTableSettings;

        /// <summary>
        /// The column name mapping
        /// </summary>
        public readonly Func<Type, string, string> ColumnName;

        /// <summary>
        /// The column data-direction mapping
        /// </summary>
        public readonly Func<Type, string, DataDirection> ColumnDataDirection;

        /// <summary>
        /// The column ignmore mapping
        /// </summary>
        public readonly Func<Type, string, bool> IgnoreColumn;

        /// <summary>
        /// Will the key result in a null columns contract?
        /// </summary>
        public bool NullContract { get { return !IsGeneric && DynamicColumnSpec == null; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="isGeneric"></param>
        /// <param name="type"></param>
        /// <param name="columns"></param>
        /// <param name="mapper"></param>
        internal DataContractKey(bool isGeneric, Type type, string columns, SqlNamingMapper mapper)
        {
            IsGeneric = isGeneric;

            foreach (var attr in type
#if !NETFRAMEWORK
                .GetTypeInfo()
#endif
                .GetCustomAttributes(false))
            {
                if (attr is DatabaseTableAttribute)
                {
                    DatabaseTableSettings = (DatabaseTableAttribute)attr;
                }
            }

            if (DatabaseTableSettings == null)
            {
                // we don't ever need to look up the mapper values if they have been overridden by the user-attribute;
                // these will be from the user-mapper if defined, or the default mapper if not
                DatabaseTableSettings = new DatabaseTableAttribute(
                    mapper.TableNameMapping(type),
                    mapper.CaseSensitiveColumns(type),
                    mapper.AutoMapAfterColumnRename(type));
            }

            HasColumnMapping =
                mapper.ColumnNameMapping != SqlNamingMapper.IdentityColumnMapping ||
                mapper.ColumnDataDirection != SqlNamingMapper.ColumnDataDirectionUnspecified ||
                mapper.IgnoreColumn != SqlNamingMapper.NeverIgnoreColumn;

            if (!IsGeneric && HasColumnMapping)
            {
                // If we are trying to map column names in a dynamic instance of Mighty, then we must have a columns spec and we must use columns auto-mapping
                if (columns == null || columns == "*")
                {
                    throw new InvalidOperationException($"You must provide an explicit `columns` specification to any dynamic instance of {nameof(MightyOrm)} with column name mapping");
                }
                if ((DatabaseTableSettings.AutoMapAfterColumnRename & AutoMap.Columns) == 0)
                {
                    throw new InvalidOperationException($"You must enable {nameof(AutoMap)}.{nameof(AutoMap.Columns)} in your {nameof(DatabaseTableSettings.AutoMapAfterColumnRename)} settings for any dynamic instance of {nameof(MightyOrm)} with column name mapping");
                }
                // Columns is not needed in the data contract except if we're here;
                // where needed, normalise it to improve caching
                DynamicColumnSpec = NormaliseColumns(columns);
            }

            ColumnName = mapper.ColumnNameMapping;
            ColumnDataDirection = mapper.ColumnDataDirection;
            IgnoreColumn = mapper.IgnoreColumn;

            this.DataItemType = type;
        }

        /// <summary>
        /// Normalise - trim and sort - column names to improve caching
        /// </summary>
        /// <param name="columns"></param>
        /// <returns></returns>
        private string NormaliseColumns(string columns)
        {
            // do not include space after comma here, since we assume it's not present in order to run slightly faster
            // (with no .Trim()) when reading these back, and since this column list is never included directly in SQL output
            return string.Join(",", columns.Split(',').Select(c => c.Trim()).OrderBy(c => c));
        }

        /// <summary>
        /// Get the hash code for this key
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            int h = 0;

            if (!IsGeneric)
            {
                // For dynamic types the only things that can affect the mapping are the column mapping, if any,
                // and the final DatabaseTableSettings, but not the item type per se.
                if (HasColumnMapping)
                {
                    h = DynamicColumnSpec?.GetHashCode() ?? 0;
                }
            }
            else
            {
                // And the contract for generic instances does not depend on the column spec
                h = DataItemType?.GetHashCode() ?? 0;
            }

            h ^= DatabaseTableSettings.GetHashCode();

            if (HasColumnMapping)
            {
                h ^= (ColumnName?.GetHashCode() ?? 0) ^
                     (ColumnDataDirection?.GetHashCode() ?? 0) ^
                     (IgnoreColumn?.GetHashCode() ?? 0);
            }

            return h;
        }

        /// <summary>
        /// Define equality between keys
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (!(obj is DataContractKey)) return false;
            var other = (DataContractKey)obj;

            bool y = true;

            if (!IsGeneric)
            {
                // For dynamic types the only things that can affect the mapping are the column mapping, if any,
                // and the final DatabaseTableSettings, but not the item type per se.
                if (HasColumnMapping)
                {
                    y = DynamicColumnSpec == other.DynamicColumnSpec;
                }
            }
            else
            {
                // And the contract for generic instances does not depend on the column spec
                y = DataItemType == other.DataItemType;
            }

            y = y &&
                DatabaseTableSettings.Equals(other.DatabaseTableSettings);

            if (HasColumnMapping)
            {
                y = y &&
                    ColumnName == other.ColumnName &&
                    ColumnDataDirection == other.ColumnDataDirection &&
                    IgnoreColumn == other.IgnoreColumn;
            }

            return y;
        }
    }
}
