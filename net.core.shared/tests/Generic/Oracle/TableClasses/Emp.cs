using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mighty.Generic.Tests.Oracle.TableClasses
{
	public class EMP
	{
		public int EMPNO { get; set; }
		public string ENAME { get; set; }
		public string LOC { get; set; }
	}

	public class Employees : MightyOrm<EMP>
	{
		public Employees(string providerName) : this(providerName, true)
		{
		}


		public Employees(string providerName, bool includeSchema)
			: base(string.Format(TestConstants.ReadWriteTestConnection, providerName), includeSchema ? "SCOTT.EMP" : "EMP", "EMPNO",
#if KEY_VALUES
                  string.Empty,
#endif
                  includeSchema ? "SCOTT.EMP_SEQ" : "EMP_SEQ")
		{

		}
	}
}
