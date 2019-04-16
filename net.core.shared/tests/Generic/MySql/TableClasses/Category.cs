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
        public string Description { get; set; }

        public override bool Equals(object obj)
        {
            var c = obj as Category;
            if (c == null) return false;
            return (
                CategoryID == c.CategoryID &&
                CategoryName == c.CategoryName &&
                Description == c.Description
            );
        }
    }

    public class Categories : MightyOrm<Category>
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
