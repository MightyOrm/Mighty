using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mighty.Generic.Tests.Sqlite
{
    public static class TestConstants
    {
#if NETCOREAPP
        public static readonly string ReadWriteTestConnection = @"Data Source=C:\Users\mjsbeaton\Documents\ChinookDatabase1.4_Sqlite\Chinook_Sqlite_AutoIncrementPKs.sqlite;providerName=Microsoft.Data.Sqlite";
#else
        public static readonly string ReadWriteTestConnection = "ReadWriteTests.ConnectionString.SQLite";
#endif
    }
}
