using System;
using System.Collections.Generic;
using System.Text;

namespace Mighty.Serialization
{
    /// <summary>
    /// Specify that this data member should be managed by Mighty, and optionally
    /// provides a database name mapping for the underlyimh column.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class MightyColumnAttribute : Attribute
    {
        /// <summary>
        /// The underlying column name.
        /// </summary>
        public string columnName { get; protected set; }

        /// <summary>
        /// Specify that this data member should be managed by Mighty, and optionally
        /// provides a database name mapping for the underlying column.
        /// </summary>
        /// <param name="columnName">Database name for the underlying column</param>
        public MightyColumnAttribute(string columnName = null)
        {
            this.columnName = columnName;
        }
    }
}
