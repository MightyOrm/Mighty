using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mighty.Generic.Tests.SqlServer.TableClasses
{
    // Test fields instead of properties
    public class Category
    {
        // test non-public field
        internal int CategoryID;

        // public field
        public string CategoryName;

        // and a property, which has non-public backing fields which need ignoring
        public string Description { get; set; }
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
