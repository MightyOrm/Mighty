using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mighty.MethodSignatures
{
    /// <summary>
    /// Enum of known Mighty method types. It's not the only way to split these up, just one reasonably useful way
    /// for doing the automated, reflection-based method checking.
    /// </summary>
    public enum MightyMethodType
    {
        _Illegal,
        Single,
        Query,
        QueryMultiple,
        Execute,
        Scalar,
        Aggregate,
        Paged,
        Save,
        Delete,
        Update,
        Insert,
        New,
#if KEY_VALUES
        KeyValues,
#endif
        OpenConnection,
        CreateCommand,
        ResultsAsExpando,
        GetColumnInfo,
        IsValid,
        HasPrimaryKey,
        GetPrimaryKey,
        Factory,
    }
}
