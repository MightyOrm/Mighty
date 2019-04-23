using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mighty.Dynamic.Tests.Oracle
{
    public static class TestConstants
    {
#if NETCOREAPP
        public static readonly string ReadWriteTestConnection = "data source=oravirtualnerd;user id=SCOTT;password=TIGER;persist security info=false;ProviderName={0}";
#else
        public static readonly string ReadWriteTestConnection = "Scott.ConnectionString.Oracle ({0})";
#endif
    }
}
