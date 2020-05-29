using System;
using System.Collections.Generic;
using System.Text;

namespace Mighty.MethodSignatures
{
    public enum MightySyncType
    {
        Static,     // static methods (only expecting sync methods here)
        SyncOnly,   // methods intended to be sync-only (no async variant is intended to exists)
        Sync,       // sync-async methods, sync variant
        Async,      // sync-async methods, async variant
    }
}
