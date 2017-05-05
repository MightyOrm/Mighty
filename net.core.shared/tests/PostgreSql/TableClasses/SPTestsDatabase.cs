using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mighty.Tests.PostgreSql.TableClasses
{
	public class SPTestsDatabase : MightyORM
	{
		public SPTestsDatabase() : base(TestConstants.ReadWriteTestConnection)
		{
		}
	}
}
