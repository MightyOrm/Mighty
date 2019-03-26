using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MightyOrm.Dynamic.Tests.SqlServer.TableClasses
{
	public class SPTestsDatabase : MightyOrm
	{
		public SPTestsDatabase() : base(TestConstants.ReadTestConnection)
		{
		}
	}
}
