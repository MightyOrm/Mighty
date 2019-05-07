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
        /// <param name="name">Database table name for the underlying table</param>
        /// <param name="caseSensitiveColumnMapping">Should Mighty be case sensitive when mapping from field and property names to database names?</param>
        /// <param name="autoMap">Should Mighty automatically remap any `keys`, `columns` or `orderBy` inputs it receives, if one or more column names have been remapped?</param>
        public DatabaseTableAttribute(string name = null, bool caseSensitiveColumnMapping = false, AutoMap autoMap = AutoMap.On)
        {
            TableName = name;
            AutoMapAfterColumnRename = autoMap;
            CaseSensitiveColumnMapping = caseSensitiveColumnMapping;
        }

        /// <summary>
        /// Override the hash code for the class
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            var h =
                (TableName?.GetHashCode() ?? 0) ^
                (int)AutoMapAfterColumnRename ^
                (CaseSensitiveColumnMapping ? 1 : 0);

            return h;
        }

        /// <summary>
        /// Override equality for the class
        /// </summary>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (!(obj is DatabaseTableAttribute)) return false;

            var other = (DatabaseTableAttribute)obj;

            var y =
                TableName == other.TableName &&
                AutoMapAfterColumnRename == other.AutoMapAfterColumnRename &&
                CaseSensitiveColumnMapping == other.CaseSensitiveColumnMapping;

            return y;
        }
    }
}
