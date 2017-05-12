using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mighty.Generic.Tests.Oracle.TableClasses
{
	public class DEPT
	{
		public int DEPTNO { get; set; }
		public string DNAME { get; set; }
		public string LOC { get; set; }
	}

	public class Departments : MightyORM<DEPT>
	{
		public Departments(string providerName) : this(providerName, true)
		{
		}


		public Departments(string providerName, bool includeSchema) 
			: base(string.Format(TestConstants.ReadWriteTestConnection, providerName), includeSchema ? "SCOTT.DEPT" : "DEPT", "DEPTNO", string.Empty, includeSchema ? "SCOTT.DEPT_SEQ" : "DEPT_SEQ")
		{
			
		}
	}
}
