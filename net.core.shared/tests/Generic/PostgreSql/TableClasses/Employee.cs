using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mighty.Generic.Tests.PostgreSql.TableClasses
{
	public class Employee
	{
		public int employeeid { get; set; }
		public string companyname { get; set; }
		public string firstname { get; set; }
		public string lastname { get; set; }
	}

	public class Employees : MightyOrm<Employee>
	{
		public Employees()
			: this(includeSchema: true)
		{
		}


		public Employees(bool includeSchema) :
			base(TestConstants.ReadWriteTestConnection, includeSchema ? "public.employees" : "employees", "employeeid")
		{
		}
	}
}
