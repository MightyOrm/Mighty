using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mighty.Dynamic.Tests.MySql.TableClasses
{
    public class Product : MightyOrm
    {
        public Product(string providerName) : this(providerName, true)
        {
        }


        public Product(string providerName, bool includeSchema) :
            base(WhenDevart.AddLicenseKey(providerName, string.Format(TestConstants.WriteTestConnection, providerName)), includeSchema ? "MassiveWriteTests.Products" : "Products", "ProductID")
        {
        }
    }
}
