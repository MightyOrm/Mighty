using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mighty.MethodSignatures
{
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
