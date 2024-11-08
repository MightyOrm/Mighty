using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mighty.Dynamic.Tests.SqlServer.TableClasses
{
    public class Product : MightyOrm
    {
        public Product(string providerName) : this(providerName, true)
        {
        }


        public Product(string providerName, bool includeSchema) :
            base(string.Format(TestConstants.WriteTestConnection, providerName), includeSchema ? "dbo.Products" : "Products", "ProductID")
        {
        }
    }
}
