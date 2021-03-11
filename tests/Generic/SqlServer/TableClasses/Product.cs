using System;

namespace Mighty.Generic.Tests.SqlServer.TableClasses
{
    public class Product
    {
        public int ProductID { get; set; }
        public int CategoryID { get; set; }
        public string ProductName { get; set; }
    }

    public class Products : MightyOrm<Product>
    {
        public Products(bool explicitConnection = false) : this(includeSchema: true, explicitConnection: explicitConnection)
        {
        }


        public Products(bool includeSchema, bool explicitConnection = false) :
            base(
                explicitConnection ?
                    $"ProviderName={TestConstants.ProviderName}" :
                    string.Format(TestConstants.WriteTestConnection, TestConstants.ProviderName),
                includeSchema ? "dbo.Products" : "Products",
                "ProductID")
        {
        }
    }
}
