using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mighty.Validation;

namespace Mighty.Dynamic.Tests.SqlServer.TableClasses
{
    public class SalesOrderHeader : MightyOrm
    {
        public SalesOrderHeader() : this(true)
        {
        }


        public SalesOrderHeader(bool includeSchema) :
            base(TestConstants.ReadTestConnection, includeSchema ? "Sales.SalesOrderHeader" : "SalesOrderHeader", "SalesOrderID", validator: new SalesOrderHeaderValidator())
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
