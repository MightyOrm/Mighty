using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mighty.Tests.Oracle.TableClasses
{
	public class SPTestsDatabase : MightyORM
	{
		public SPTestsDatabase(string providerName) : base(string.Format(TestConstants.ReadWriteTestConnection, providerName))
		{
		}
	}
}
