using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mighty.Generic.Tests.PostgreSql.TableClasses
{
    public class Product
    {
        public int productid { get; set; }
    }

    public class Products : MightyOrm<Product>
    {
        public Products()
            : this(includeSchema: true)
        {
        }


        public Products(bool includeSchema) :
            base(TestConstants.ReadWriteTestConnection, includeSchema ? "public.products" : "products", "productid",
#if KEY_VALUES
                string.Empty,
#endif
                "products_productid_seq")
        {
        }
    }
}
