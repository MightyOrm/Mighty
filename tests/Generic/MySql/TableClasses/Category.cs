using System;

namespace Mighty.Generic.Tests.MySql.TableClasses
{
    public class Category
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }
    }

    public class Categories : MightyOrm<Category>
    {
        public Categories(string providerName, bool explicitConnection = false) : this(providerName, true, explicitConnection)
        {
        }


        public Categories(string providerName, bool includeSchema, bool explicitConnection = false) :
            base(
                explicitConnection ?
                    $"providername={providerName}" :
                    string.Format(TestConstants.WriteTestConnection, providerName),
                includeSchema ? "MassiveWriteTests.Categories" : "Categories",
                "CategoryID"
                )
        {
        }
    }
}
