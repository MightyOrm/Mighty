using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mighty.Dynamic.Tests.MySql.TableClasses
{
    public class Category : MightyOrm
    {
        public Category(string providerName) : this(providerName, true)
        {
        }


        public Category(string providerName, bool includeSchema) :
            base(WhenDevart.AddLicenseKey(TestConstants.WriteTestConnection, providerName), includeSchema ? "MassiveWriteTests.Categories" : "Categories", "CategoryID")
        {
        }
    }
}
