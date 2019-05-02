using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

using Mighty.Mapping;
using Mighty.Plugins;

namespace Mighty.DataContracts
{
    /// <summary>
    /// A data contract key (unique identifier);
    /// all of the values on which a <see cref="ColumnsContract"/> depends.
    /// </summary>
    public class ColumnsContractKey
    {
        /// <summary>
        /// The data item type (which will always be null for the unique columns contract for dynamic instances of <see cref="MightyOrm"/>
        /// </summary>
        public readonly Type DataItemType;

        /// <summary>
        /// The case sensitivity
        /// </summary>
        public readonly Func<Type, bool> CaseSensitiveColumnMapping;

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
        /// Constructor
        /// </summary>
        /// <param name="IsDynamic"></param>
        /// <param name="Type"></param>
        /// <param name="Mapper"></param>
        internal ColumnsContractKey(bool IsDynamic, Type Type, SqlNamingMapper Mapper)
        {
            if (IsDynamic != (Type == null))
            {
                // should not happen
                // (sub-class type, if present, is NOT sent in and not wanted for dynamic Mighty columns contract)
                throw new Exception("Dynamic-Type mismatch in data contract");
            }

            if (IsDynamic)
            {
                if (Mapper.CaseSensitiveColumnMapping != SqlNamingMapper.CaseInsensitiveColumnMapping ||
                    Mapper.ColumnName != SqlNamingMapper.IdentityColumnMapping ||
                    Mapper.ColumnDataDirection != SqlNamingMapper.ColumnDataDirectionUnspecified ||
                    Mapper.IgnoreColumn != SqlNamingMapper.NeverIgnoreColumn)
                {
                    throw new InvalidOperationException($"You cannot override any aspect of column mapping or case sensitivity with {nameof(SqlNamingMapper)} for dynamic instances of {nameof(MightyOrm)} (but you can use e.g. `columns: \"film_id AS FilmID, description AS Description\"` in the constructor instead)");
                }
            }

            // Enforces a unique null key for all dynamic instances of Mighty
            if (!IsDynamic)
            {
                this.DataItemType = Type;
                this.CaseSensitiveColumnMapping = Mapper.CaseSensitiveColumnMapping;
                this.ColumnName = Mapper.ColumnName;
                this.ColumnDataDirection = Mapper.ColumnDataDirection;
                this.IgnoreColumn = Mapper.IgnoreColumn;
            }
        }

        /// <summary>
        /// Get the hash code for this key
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            var h =
                (DataItemType?.GetHashCode() ?? 0) ^
                (CaseSensitiveColumnMapping?.GetHashCode() ?? 0) ^
                (ColumnName?.GetHashCode() ?? 0) ^
                (ColumnDataDirection?.GetHashCode() ?? 0) ^
                (IgnoreColumn?.GetHashCode() ?? 0);
            return h;
        }

        /// <summary>
        /// Define equality between keys
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            var other = obj as ColumnsContractKey;
            if (other == null) return false;
            var y =
                DataItemType == other.DataItemType &&
                CaseSensitiveColumnMapping == other.CaseSensitiveColumnMapping &&
                ColumnName == other.ColumnName &&
                ColumnDataDirection == other.ColumnDataDirection &&
                IgnoreColumn == other.IgnoreColumn;
            return y;
        }
    }
}
