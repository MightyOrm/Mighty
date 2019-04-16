using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mighty.Generic.Tests.MySql
{
    public static class TestConstants
    {
#if (NETCOREAPP || NETSTANDARD)
        public static readonly string ReadTestConnection = "data source=mysqltest;database=sakila;user id=Massive;password=mt123;persist security info=false;providerName={0}";
        public static readonly string WriteTestConnection = "data source=mysqltest;database=massivewritetests;user id=Massive;password=mt123;persist security info=false;providerName={0}";
#else
        public static readonly string ReadTestConnection = "Sakila.ConnectionString.MySql ({0})";
        public static readonly string WriteTestConnection = "MassiveWriteTests.ConnectionString.MySql ({0})";
#endif
    }
}
