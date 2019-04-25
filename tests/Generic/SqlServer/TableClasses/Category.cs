using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mighty.Generic.Tests.SqlServer.TableClasses
{
    // Test fields instead of properties
    public class Category
    {
        public int CategoryID;
        public string CategoryName;
        public string Description;
    }

    public class Categories : MightyOrm<Category>
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
