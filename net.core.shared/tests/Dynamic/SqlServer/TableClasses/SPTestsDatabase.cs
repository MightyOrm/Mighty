using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mighty.Dynamic.Tests.SqlServer.TableClasses
{
	public class SPTestsDatabase : MightyORM
	{
		public SPTestsDatabase() : base(TestConstants.ReadTestConnection)
		{
		}
	}
}
