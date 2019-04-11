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
		public SalesOrderHeaders() : this(true)
		{
		}


		public SalesOrderHeaders(bool includeSchema) :
			base(TestConstants.ReadTestConnection, includeSchema ? "Sales.SalesOrderHeader" : "SalesOrderHeader", "SalesOrderID", validator: new SalesOrderHeaderValidator())
		{
		}


		public class SalesOrderHeaderValidator : Validator
		{
			override public void Validate(OrmAction action, dynamic item, List<object> Errors)
			{
				// bogus validation: isn't valid if sales person is null. 

				if (item.SalesPersonID == null)
				{
					Errors.Add("SalesPersonID is null");
				}
			}
		}
	}
}
