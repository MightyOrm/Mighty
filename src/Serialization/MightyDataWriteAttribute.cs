using System;
using System.Collections.Generic;
using System.Text;

namespace Mighty.Serialization
{
    /// <summary>
    /// Apply to a field or property to specify that Mighty may read data
    /// from this property and write it to the database.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class MightyDataWriteAttribute : Attribute { }
}
