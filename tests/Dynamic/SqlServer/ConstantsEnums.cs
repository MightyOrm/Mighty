using System;

namespace Mighty.Dynamic.Tests.SqlServer
{
    public static class TestConstants
    {
        public static readonly string ProviderName = "System.Data.SqlClient";

#if NETCOREAPP
        public static readonly string ReadTestConnection = "data source=sqlserver.test.local;initial catalog=AdventureWorks;User Id=mightytests;Password=testpassword;persist security info=False;packet size=4096;ProviderName={0};";
        public static readonly string WriteTestConnection = "data source=sqlserver.test.local;initial catalog=MassiveWriteTests;User Id=mightytests;Password=testpassword;persist security info=False;packet size=4096;ProviderName={0};";
#else
        public static readonly string ReadTestConnection = "AdventureWorks.ConnectionString.SQL Server ({0})";
        public static readonly string WriteTestConnection = "MassiveWriteTests.ConnectionString.SQL Server ({0})";
#endif
    }
}
