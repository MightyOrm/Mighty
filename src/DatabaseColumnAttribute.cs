using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Mighty
{
    /// <summary>
    /// Specify that this field or property should be included in database operations even if non-public.
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
        /// <param name="name">
        /// The database column name;
        /// setting this turns on auto-mapping of `keys`, `columns` and `orderBy` inputs to Mighty by default;
        /// to disable that apply [<see cref="DatabaseTableAttribute"/>(autoMap: <see cref="AutoMap.Off"/>)] to the class.
        /// </param>
        /// <param name="direction">Experimental: The database column direction</param>
        /// <param name="sqlTransform">In Mighty-generated SQL the column will selected as "{<paramref name="sqlTransform"/>} AS {<paramref name="name"/>}" (can only work when Mighty is generating the SQL to fetch the column from a table or view; cannot transform a column returned from a stored procedure)</param>
        /// <remarks>
        /// TO DO: Given the above, do we want to do <paramref name="sqlTransform"/> this way? (Or at all?)
        /// Probably still yes, noting that we can *rename* with name anyway.
        /// </remarks>
        public DatabaseColumnAttribute(string name = null, DataDirection direction = default, string sqlTransform = null)
        {
            this.Name = name;
            this.Direction = direction;
            this.Transform = sqlTransform;
        }
    }
}
