using System;
using System.Collections.Generic;
using System.Text;

namespace Mighty.MethodSignatures
{
    [Flags]
    public enum MightyVariantType
    {
        Neither = 0,
        DbConnection = 1 << 0,
        CancellationToken = 1 << 1,
        Both = DbConnection | CancellationToken
    }
}
