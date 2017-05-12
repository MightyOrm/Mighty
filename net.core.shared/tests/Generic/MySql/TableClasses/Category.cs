using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mighty.Generic.Tests.MySql.TableClasses
{
	public class Category
	{
		public int CategoryID { get; set; }
		public string CategoryName { get; set; }
	}

	public class Categories : MightyORM<Category>
	{
		public Categories(string providerName) : this(providerName, true)
		{
		}


		public Categories(string providerName, bool includeSchema) :
			base(string.Format(TestConstants.WriteTestConnection, providerName), includeSchema ? "MassiveWriteTests.Categories" : "Categories", "CategoryID")
		{
		}
	}
}
