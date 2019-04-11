using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mighty.Dynamic.Tests.Oracle.TableClasses
{
	public class Department : MightyOrm
	{
		public Department(string providerName) : this(providerName, true)
		{
		}


		public Department(string providerName, bool includeSchema) 
			: base(string.Format(TestConstants.ReadWriteTestConnection, providerName), includeSchema ? "SCOTT.DEPT" : "DEPT", "DEPTNO",
#if KEY_VALUES
                  string.Empty,
#endif
                  includeSchema ? "SCOTT.DEPT_SEQ" : "DEPT_SEQ")
		{
			
		}
	}
}
