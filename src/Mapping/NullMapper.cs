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
        /// Returns false, equals not case sensitive.
        /// </summary>
        /// <returns></returns>
        override public bool UseCaseSensitiveMapping() { return false; }

        /// <summary>
        /// Returns <paramref name="type"/>.Name.
        /// </summary>
        /// <param name="type">Class type</param>
        /// <returns></returns>
        override public string GetTableName(Type type) { return type.Name; }

        /// <summary>
        /// Returns <paramref name="name"/>.
        /// </summary>
        /// <param name="type">Class type</param>
        /// <param name="member">The field or property</param>
        /// <param name="name">The property name</param>
        /// <returns></returns>
        /// <remarks>The field can be from an ExpandoObject, so it might not have a PropertyInfo - which probably means we need typed and untyped mappers</remarks>
        override public string GetColumnName(Type type, MemberInfo member, string name) { return name; }

        /// <summary>
        /// Returns null, meaning no primary key(s).
        /// </summary>
        /// <param name="type">Class type</param>
        /// <returns></returns>
        override public string GetPrimaryKeysFromClassType(Type type) { return null; }

        /// <summary>
        /// Returns null, meaning no sequence.
        /// </summary>
        /// <param name="type">Class type</param>
        /// <returns></returns>
        public override string GetSequenceFromClassType(Type type) { return null; }

        /// <summary>
        /// Returns <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The identifier to quote</param>
        /// <returns></returns>
        override public string QuoteDatabaseIdentifier(string id) { return id; }
    }
}
