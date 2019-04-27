using System;
using System.Collections.Generic;
using System.Text;

namespace Mighty.Serialization
{
    /// <summary>
    /// All of the values on which a <see cref="MightyDataContract"/> depends.
    /// </summary>
    public class MightyStoreKey
    {
        /// <summary>
        /// Table name (database name, after mapping if any)
        /// </summary>
        public string tableName;

        /// <summary>
        /// Primary key(s) (C# property names, before mapping if any)
        /// </summary>
        public string keys;

#if KEY_VALUES
        /// <summary>
        /// Value field (C# property name, before mapping if any)
        /// </summary>
        public string valueField;
#endif

        /// <summary>
        /// Sequence or identity
        /// </summary>
        /// <remarks>TO DO: Is this needed?</remarks>
        public string sequence;

        /// <summary>
        /// Columns (database column names, after mapping if any)
        /// </summary>
        public string columns;

        /// <summary>
        /// The C# data item type.
        /// </summary>
        public Type type;
    }
}
