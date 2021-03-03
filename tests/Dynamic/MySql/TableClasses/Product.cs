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
            base(string.Format(WhenDevart.AddLicenseKey(providerName, TestConstants.WriteTestConnection), providerName), includeSchema ? "MassiveWriteTests.Products" : "Products", "ProductID")
        {
        }
    }
}
