using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MightyOrm.Dynamic.Tests.Oracle.TableClasses
{
	public class SPTestsDatabase : MightyOrm
	{
		public SPTestsDatabase(string providerName) : base(string.Format(TestConstants.ReadWriteTestConnection, providerName))
		{
		}
	}
}
