using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mighty.Dynamic.Tests.PostgreSql.TableClasses
{
	public class SPTestsDatabase : MightyOrm
	{
		public SPTestsDatabase() : base(TestConstants.ReadWriteTestConnection)
		{
		}
	}
}
