using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mighty.Generic.Tests.SqlServer.TableClasses
{
	public class Category
	{
		public int CategoryID { get; set; }
		public string CategoryName { get; set; }
	}

	public class Categories : MightyORM<Category>
	{
		public Categories() : this(true)
		{
		}


		public Categories(bool includeSchema) :
			base(TestConstants.WriteTestConnection, includeSchema ? "dbo.Categories" : "Categories", "CategoryID")
		{
		}
	}
}
