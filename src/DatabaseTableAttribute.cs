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
        public string Name { get; protected set; }

        /// <summary>
        /// The auto-map setting
        /// </summary>
        public AutoMap AutoMapAfterColumnRename { get; protected set; }

        /// <summary>
        /// The case-sensitivity setting
        /// </summary>
        public bool CaseSensitiveColumnMapping { get; protected set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Database name for the underlying table</param>
        /// <param name="caseSensitiveColumnMapping">Should Mighty be case sensitive when mapping from field and property names to database names?</param>
        /// <param name="autoMapAfterColumnRename">Should Mighty automatically remap any `keys`, `columns` or `orderBy` inputs it receives, if one or more column names have been remapped?</param>
        public DatabaseTableAttribute(string name, bool caseSensitiveColumnMapping = false, AutoMap autoMapAfterColumnRename = AutoMap.On)
        {
            Name = name;
            AutoMapAfterColumnRename = autoMapAfterColumnRename;
            CaseSensitiveColumnMapping = caseSensitiveColumnMapping;
        }
    }
}
