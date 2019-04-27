using System;
using System.Collections.Generic;
using System.Text;

namespace Mighty.Serialization
{
    /// <summary>
    /// Specify that this data member should be managed by Mighty, and optionally
    /// provides a database name mapping for the underlyimh table.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class MightyTableAttribute : Attribute
    {
        /// <summary>
        /// The underlying table name.
        /// </summary>
        public string tableName { get; protected set; }

        /// <summary>
        /// Specify that this data member should be managed by Mighty, and optionally
        /// provides a database name mapping for the underlying table.
        /// </summary>
        /// <param name="tableName">Database name for the underlying table</param>
        public MightyTableAttribute(string tableName = null)
        {
            this.tableName = tableName;
        }
    }
}
