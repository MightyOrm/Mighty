using System;

using Mighty.Validation;

namespace Mighty.Dynamic.Tests.SqlServer.TableClasses
{
    public class SalesOrderHeader : MightyOrm
    {
        public SalesOrderHeader(string providerName, bool includeSchema = true, string columns = null) :
            base(string.Format(TestConstants.ReadTestConnection, providerName), includeSchema ? "Sales.SalesOrderHeader" : "SalesOrderHeader", "SalesOrderID", columns: columns, validator: new SalesOrderHeaderValidator())
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
