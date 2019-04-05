using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mighty.Generic.Tests.Oracle
{
	public static class TestConstants
	{
        // On a 64 bit machine remember to set 'Test/Test Settings/Default Processor Architecture' to 'X64'
        // or else the Oracle drivers will appear not to be installed.
#if (NETCOREAPP || NETSTANDARD)
		public static readonly string ReadWriteTestConnection = "data source=oravirtualnerd;user id=SCOTT;password=TIGER;persist security info=false;ProviderName={0}";
#else
		public static readonly string ReadWriteTestConnection = "Scott.ConnectionString.Oracle ({0})";
#endif
	}
}
