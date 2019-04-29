using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Mighty
{
    /// <summary>
    /// Specify that this field or property should be included in database operations.
    /// Optionally provide a database name for the underlying column.
    /// Optionally specify that this field or property should be used for data reads or writes only.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class DatabaseColumnAttribute : Attribute
    {
        /// <summary>
        /// The database column name
        /// </summary>
        public readonly string ColumnName;

        /// <summary>
        /// Whether data should be read from or writtten to the database for this column
        /// </summary>
        /// <remarks>
        /// Zero means no checks and only any restrictions on the underlying data member will apply
        /// </remarks>
        public readonly DataDirection DataDirection;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ColumnName">The database column name</param>
        /// <param name="DataDirection">The database column name</param>
        public DatabaseColumnAttribute(string ColumnName = null, DataDirection DataDirection = default)
        {
            this.ColumnName = ColumnName;
            this.DataDirection = DataDirection;
        }
    }
}
