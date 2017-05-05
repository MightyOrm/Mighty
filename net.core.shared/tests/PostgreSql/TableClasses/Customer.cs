using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mighty.Tests.PostgreSql.TableClasses
{
	public class Customer : MightyORM
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
