using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Mighty.Serialization
{
    public class MightyProperty
    {
        /// <summary>
        /// The field if this is wrapping <see cref="FieldInfo"/>.
        /// </summary>
        public FieldInfo field { get; protected set; }

        /// <summary>
        /// The property if this is wrapping <see cref="PropertyInfo"/>.
        /// </summary>
        public PropertyInfo property { get; protected set; }

        /// <summary>
        /// Can Mighty read from the class member (aka write to this column in the database)?
        /// True by default for fields and for properties with a set method, overridden by
        /// <see cref="MightyDataWriteAttribute"/>.
        /// </summary>
        public bool CanRead { get; protected set; }

        /// <summary>
        /// Can Mighty write to the class member (aka read from this column in the database)?
        /// True by default for fields and for properties with a get method, overridded by
        /// <see cref="MightyDataReadAttribute"/>.
        /// </summary>
        public bool CanWrite { get; protected set; }
    }
}
