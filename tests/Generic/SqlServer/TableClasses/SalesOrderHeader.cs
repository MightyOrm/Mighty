using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mighty.Validation;

namespace Mighty.Generic.Tests.SqlServer.TableClasses
{
    public class SalesOrderHeader
    {
        public int SalesOrderID { get; set; }
        public int CustomerID { get; set; }
        public int? SalesPersonID { get; set; }
        public byte Status { get; set; }
        public string PurchaseOrderNumber { get; set; }
        public string SalesOrderNumber { get; set; }
        public DateTime? OrderDate { get; set; }
    }

    public class SalesOrderHeaders : MightyOrm<SalesOrderHeader>
    {
        public SalesOrderHeaders(bool includeSchema = true, string columns = null) :
            base(TestConstants.ReadTestConnection, includeSchema ? "Sales.SalesOrderHeader" : "SalesOrderHeader", "SalesOrderID", columns: columns, validator: new SalesOrderHeaderValidator())
        {
        }


        public class SalesOrderHeaderValidator : Validator
        {
            override public void Validate(dynamic item, Action<object> reportError)
            {
                // bogus validation: isn't valid if sales person is null. 

                if (item.SalesPersonID == null)
                {
                    reportError("SalesPersonID is null");
                }
            }
        }
    }
}
