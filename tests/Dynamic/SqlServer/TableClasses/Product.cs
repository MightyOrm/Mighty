using System;

namespace Mighty.Dynamic.Tests.SqlServer.TableClasses
{
    public class Product : MightyOrm
    {
        public Product(string providerName, bool explicitConnection = false) : this(providerName, true, explicitConnection)
        {
        }


        public Product(string providerName, bool includeSchema, bool explicitConnection = false) :
            base(
                explicitConnection ?
                    $"providerName={providerName}" :
                    string.Format(TestConstants.WriteTestConnection, providerName),
                includeSchema ? "dbo.Products" : "Products",
                "ProductID")
        {
        }
    }
}
