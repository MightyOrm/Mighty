using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mighty.Generic.Tests.SqlServer
{
    public static class TestConstants
    {
#if (NETCOREAPP || NETSTANDARD)
        public static readonly string ReadTestConnection = "data source=sqlserver.test.local;initial catalog=AdventureWorks;integrated security=SSPI;persist security info=False;packet size=4096;ProviderName=System.Data.SqlClient;";
        public static readonly string WriteTestConnection = "data source=sqlserver.test.local;initial catalog=MassiveWriteTests;integrated security=SSPI;persist security info=False;packet size=4096;ProviderName=System.Data.SqlClient;";
#else
        public static readonly string ReadTestConnection = "AdventureWorks.ConnectionString.SQL Server (SqlClient)";
        public static readonly string WriteTestConnection = "MassiveWriteTests.ConnectionString.SQL Server (SqlClient)";
#endif
    }
}
