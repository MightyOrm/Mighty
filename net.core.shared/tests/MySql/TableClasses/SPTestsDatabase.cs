using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mighty.Tests.MySql.TableClasses
{
	public class SPTestsDatabase : MightyORM
	{
		public SPTestsDatabase(string providerName) : base(string.Format(TestConstants.ReadTestConnection, providerName))
		{
		}
	}
}
