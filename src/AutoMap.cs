using System;
using System.Collections.Generic;
using System.Text;

using Mighty.DataContracts;

namespace Mighty
{
    /// <summary>
    /// Controls whether `keys`, `columns` and `orderBy` inputs are automatically mapped, if any fields or
    /// properties have been renamed by column mapping settings (see Mighty documentation).
    /// (If you are not using auto-mapping, you can manually use <see cref="MightyOrm"/>.<see cref="DataContract.Map(string, AutoMap)"/>
    /// to conveniently re-map one or more field or property names to column names for SQL fragments which
    /// you are passing in to Mighty.)
    /// </summary>
    [Flags]
    public enum AutoMap
    {
        /// <summary>
        /// Do not remap anything
        /// </summary>
        Off = 0,

        /// <summary>
        /// Remap `keyNames` parameter (for <see cref="MightyOrm"/> and <see cref="MightyOrm{T}"/> constructors only)
        /// </summary>
        Keys = 1 << 0,

        /// <summary>
        /// Remap `columns` parameter (for <see cref="MightyOrm"/> and <see cref="MightyOrm{T}"/> constructors and all other methods which accept it)
        /// </summary>
        Columns = 1 << 1,

        /// <summary>
        /// Remap `orderBy` parameter (for all methods which accept it)
        /// </summary>
        OrderBy = 1 << 2,

#if KEY_VALUES
        /// <summary>
        /// Remap `valueName` parameter (for <see cref="MightyOrm"/> and <see cref="MightyOrm{T}"/> constructors only)
        /// </summary>
        Value = 1 << 3,
#endif

        /// <summary>
        /// Remap everything (i.e. `keys`, `columns` and `orderBy` parameters to <see cref="MightyOrm"/>
        /// and <see cref="MightyOrm{T}"/> constructors and all other methods which accept them)
        /// </summary>
        On =  Keys
            | Columns
            | OrderBy
#if KEY_VALUES
            | Value
#endif
    }
}
