using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mighty.Generic.Tests.SqlServer.TableClasses
{
    // Test fields instead of properties
    public class Category
    {
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
        // test non-public field
        [DatabaseColumn]
        internal int CategoryID;
#pragma warning restore CS0649

        // public field
        public string CategoryName;

        // and a property, which has non-public backing fields which need ignoring
        public string Description { get; set; }
    }

    public class Categories : MightyOrm<Category>
    {
        public Categories(string providerName) : this(providerName, true)
        {
        }


        public Categories(string providerName, bool includeSchema) :
            base(string.Format(TestConstants.WriteTestConnection, providerName), includeSchema ? "dbo.Categories" : "Categories", "CategoryID")
        {
        }
    }
}
