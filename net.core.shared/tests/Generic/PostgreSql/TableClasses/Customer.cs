using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MightyOrm.Generic.Tests.PostgreSql.TableClasses
{
	public class Customer
	{
		public string customerid { get; set; }
		public string companyname { get; set; }
	}

	public class Customers : MightyOrm<Customer>
	{
		public Customers()
			: this(includeSchema: true)
		{
		}


		public Customers(bool includeSchema) :
			base(TestConstants.ReadWriteTestConnection, includeSchema ? "public.customers" : "customers", "customerid")
		{
		}
	}
}
