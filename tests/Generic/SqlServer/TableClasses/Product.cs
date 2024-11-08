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
        public Products(string providerName, bool explicitConnection = false) : this(providerName, true, explicitConnection)
        {
        }


        public Products(string providerName, bool includeSchema, bool explicitConnection = false) :
            base(
                explicitConnection ?
                    $"ProviderName={providerName}" :
                    string.Format(TestConstants.WriteTestConnection, providerName),
                includeSchema ? "dbo.Products" : "Products",
                "ProductID")
        {
        }
    }
}
