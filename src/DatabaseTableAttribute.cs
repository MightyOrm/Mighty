using System;
using System.Collections.Generic;
using System.Text;

namespace Mighty
{
    /// <summary>
    /// Specify the database name for the underlying table.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class DatabaseTableAttribute : Attribute
    {
        /// <summary>
        /// The underlying table name.
        /// </summary>
        public string TableName { get; protected set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="TableName">Database name for the underlying table</param>
        public DatabaseTableAttribute(string TableName)
        {
            this.TableName = TableName;
        }
    }
}
