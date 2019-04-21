using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mighty.Dynamic.Tests.SqlServer.TableClasses
{
    public class Category : MightyOrm
    {
        public Category() : this(true)
        {
        }


        public Category(bool includeSchema) :
            base(TestConstants.WriteTestConnection, includeSchema ? "dbo.Categories" : "Categories", "CategoryID")
        {
        }
    }
}
