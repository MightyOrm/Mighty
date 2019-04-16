using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mighty.Generic.Tests.MySql.TableClasses
{
    public class Product
    {
        public int ProductID { get; set; }
        public int CategoryID { get; set; }
    }

    public class Products : MightyOrm<Product>
    {
        public Products(string providerName) : this(providerName, true)
        {
        }


        public Products(string providerName, bool includeSchema) :
            base(string.Format(TestConstants.WriteTestConnection, providerName), includeSchema ? "MassiveWriteTests.Products" : "Products", "ProductID")
        {
        }
    }
}
