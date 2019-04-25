using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Mighty.Mapping
{
    /// <summary>
    /// A <see cref="SqlNamingMapper"/> for Mighty's default behaviour.
    /// </summary>
    internal class NullMapper : SqlNamingMapper
    {
        override public bool UseCaseSensitiveMapping() { return false; }

        override public string GetTableNameFromClassType(Type classType) { return classType.Name; }

        override public string GetColumnNameFromField(Type classType, string fieldName) { return fieldName; }

        override public string GetPrimaryKeysFromClassType(Type classType) { return null; }

        override public string QuoteDatabaseIdentifier(string id) { return id; }
    }
}
