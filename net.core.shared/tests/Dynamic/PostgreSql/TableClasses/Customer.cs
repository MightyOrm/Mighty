using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mighty.Dynamic.Tests.PostgreSql.TableClasses
{
	public class Customer : MightyOrm
	{
		public Customer()
			: this(includeSchema: true)
		{
		}


		public Customer(bool includeSchema) :
			base(TestConstants.ReadWriteTestConnection, includeSchema ? "public.customers" : "customers", "customerid")
		{
		}
	}
}
