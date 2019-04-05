using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mighty.Generic.Tests.PostgreSql
{
	public static class TestConstants
	{
#if (NETCOREAPP || NETSTANDARD)
		public static readonly string ReadWriteTestConnection = "Database=northwind;Server=windows2008r2.sd.local;Port=5432;User Id=postgres;Password=123;providerName=Npgsql";
#else
		public static readonly string ReadWriteTestConnection = "Northwind.ConnectionString.PostgreSql (Npgsql)";
#endif
	}
}
