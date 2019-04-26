using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Mighty.Mapping
{
    /// <summary>
    /// Inject this if you need to inject Mighty's default mapping behaviour.
    /// </summary>
    public class NullMapper : SqlNamingMapper
    {
        /// <summary>
        /// Case sensitive
        /// </summary>
        /// <returns></returns>
        override public bool UseCaseSensitiveMapping() { return false; }

        /// <summary>
        /// Returns <paramref name="classType"/>.Name.
        /// </summary>
        /// <param name="classType">Class type</param>
        /// <returns></returns>
        override public string GetTableNameFromClassType(Type classType) { return classType.Name; }

        /// <summary>
        /// Returns <paramref name="fieldName"/>.
        /// </summary>
        /// <param name="classType">Class type</param>
        /// <param name="fieldName">Field name</param>
        /// <returns></returns>
        override public string GetColumnNameFromField(Type classType, string fieldName) { return fieldName; }

        /// <summary>
        /// Returns null, meaning no primary key(s).
        /// </summary>
        /// <param name="classType">Class type</param>
        /// <returns></returns>
        override public string GetPrimaryKeysFromClassType(Type classType) { return null; }

        /// <summary>
        /// Returns <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The identifier to quote</param>
        /// <returns></returns>
        override public string QuoteDatabaseIdentifier(string id) { return id; }
    }
}
