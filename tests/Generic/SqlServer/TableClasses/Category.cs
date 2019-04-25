using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mighty.Generic.Tests.SqlServer.TableClasses
{
    // Test fields instead of properties
    public class Category
    {
        internal int CategoryID; // test non-public field
        public string CategoryName; // leave as public
        internal string Description; // test non-public property
    }

    public class Categories : MightyOrm<Category>
    {
        public Categories() : this(true)
        {
        }


        public Categories(bool includeSchema) :
            base(TestConstants.WriteTestConnection, includeSchema ? "dbo.Categories" : "Categories", "CategoryID", bindingFlags: System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)
        {
        }
    }
}
