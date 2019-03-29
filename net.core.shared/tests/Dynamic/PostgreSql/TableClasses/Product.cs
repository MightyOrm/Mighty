using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mighty.Dynamic.Tests.PostgreSql.TableClasses
{
	public class Product : MightyOrm
	{
		public Product()
			: this(includeSchema: true)
		{
		}


		public Product(bool includeSchema) :
			base(TestConstants.ReadWriteTestConnection, includeSchema ? "public.products" : "products", "productid", string.Empty, "products_productid_seq")
		{
		}
	}
}
