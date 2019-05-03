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
        public readonly string Name;

        /// <summary>
        /// Whether data should be read from or writtten to the database for this column
        /// </summary>
        /// <remarks>
        /// Zero means no checks and only any restrictions on the underlying data member will apply
        /// </remarks>
        public readonly DataDirection Direction;

        /// <summary>
        /// The transform SQL
        /// </summary>
        public readonly string Transform;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The database column name</param>
        /// <param name="direction">The database column name</param>
        /// <param name="sqlTransform">Experimental: column will selected as "{<paramref name="sqlTransform"/>} AS {<paramref name="name"/>}"</param>
        public DatabaseColumnAttribute(string name = null, DataDirection direction = default, string sqlTransform = null)
        {
            this.Name = name;
            this.Direction = direction;
            this.Transform = sqlTransform;
        }
    }
}
