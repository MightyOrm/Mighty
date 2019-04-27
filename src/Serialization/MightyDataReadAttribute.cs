using System;
using System.Collections.Generic;
using System.Text;

namespace Mighty.Serialization
{
    /// <summary>
    /// Apply to a field or property to specify that Mighty may read data
    /// from the database and write it to this property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class MightyDataReadAttribute : Attribute { }
}
