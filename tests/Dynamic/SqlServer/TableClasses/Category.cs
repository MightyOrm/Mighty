using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mighty.Dynamic.Tests.SqlServer.TableClasses
{
    public class Category : MightyOrm
    {
        public Category(string providerName) : this(providerName, true)
        {
        }


        public Category(string providerName, bool includeSchema) :
            base(string.Format(TestConstants.WriteTestConnection, providerName), includeSchema ? "dbo.Categories" : "Categories", "CategoryID")
        {
        }
    }
}
