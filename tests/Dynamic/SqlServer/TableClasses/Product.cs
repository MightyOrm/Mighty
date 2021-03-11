using System;

namespace Mighty.Dynamic.Tests.SqlServer.TableClasses
{
    public class Product : MightyOrm
    {
        public Product(bool explicitConnection = false) : this(includeSchema: true, explicitConnection: explicitConnection)
        {
        }


        public Product(bool includeSchema, bool explicitConnection = false) :
            base(
                explicitConnection ?
                    $"providerName={TestConstants.ProviderName}" :
                    string.Format(TestConstants.WriteTestConnection, TestConstants.ProviderName),
                includeSchema ? "dbo.Products" : "Products",
                "ProductID")
        {
        }
    }
}
