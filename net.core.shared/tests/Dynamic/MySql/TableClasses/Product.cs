using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mighty.Dynamic.Tests.MySql.TableClasses
{
	public class Product : MightyORM
	{
		public Product(string providerName) : this(providerName, true)
		{
		}


		public Product(string providerName, bool includeSchema) :
			base(string.Format(TestConstants.WriteTestConnection, providerName), includeSchema ? "MassiveWriteTests.Products" : "Products", "ProductID")
		{
		}
	}
}
