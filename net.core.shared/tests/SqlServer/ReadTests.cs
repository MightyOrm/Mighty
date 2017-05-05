using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Mighty.Tests.SqlServer.TableClasses;
using NUnit.Framework;

namespace Mighty.Tests.SqlServer
{
	[TestFixture]
	public class ReadTests
    {
		[Test]
		public void Guid_Arg()
		{
			// SQL Server has true Guid type support
			var db = new MightyORM(TestConstants.ReadTestConnection);
			var guid = Guid.NewGuid();
			var command = db.CreateCommand("SELECT @0 AS val", null, guid);
			Assert.AreEqual(DbType.Guid, command.Parameters[0].DbType);
			var item = db.Query(command).FirstOrDefault();
			Assert.AreEqual(guid, item.val);
		}


		[Test]
		public void MaxOnFilteredSet()
		{
			var soh = new SalesOrderHeader();
			var result = ((dynamic)soh).Max(columns: "SalesOrderID", where: "SalesOrderID < @0", args: 100000);
			Assert.AreEqual(75123, result);
		}


		[Test]
		public void MaxOnFilteredSet2()
		{
			var soh = new SalesOrderHeader();
			var result = ((dynamic)soh).Max(columns: "SalesOrderID", TerritoryID: 10);
			Assert.AreEqual(75117, result);
		}


		[Test]
		public void EmptyElement_ProtoType()
		{
			var soh = new SalesOrderHeader();
			dynamic defaults = soh.New();
			Assert.IsTrue(defaults.OrderDate > DateTime.MinValue);
		}


		[Test]
		public void SchemaMetaDataRetrieval()
		{
			var soh = new SalesOrderHeader();
			var metaData = soh.TableInfo;
			Assert.IsNotNull(metaData);
			Assert.AreEqual(26, metaData.Count());
			Assert.IsTrue(metaData.All(v=>v.TABLE_NAME==soh.BareTableName));
		}


		[Test]
		public void All_NoParameters()
		{
			var soh = new SalesOrderHeader();
			var allRows = soh.All().ToList();
			Assert.AreEqual(31465, allRows.Count);
		}


		[Test]
		public void All_NoParameters_Streaming()
		{
			var soh = new SalesOrderHeader();
			var allRows = soh.All();
			var count = 0;
			foreach(var r in allRows)
			{
				count++;
				Assert.AreEqual(26, ((IDictionary<string, object>)r).Count);		// # of fields fetched should be 26
			}
			Assert.AreEqual(31465, count);
		}


		[Test]
		public void All_LimitSpecification()
		{
			var soh = new SalesOrderHeader();
			var allRows = soh.All(limit: 10).ToList();
			Assert.AreEqual(10, allRows.Count);
		}
		

		[Test]
		public void All_ColumnSpecification()
		{
			var soh = new SalesOrderHeader();
			var allRows = soh.All(columns: "SalesOrderID as SOID, Status, SalesPersonID").ToList();
			Assert.AreEqual(31465, allRows.Count);
			var firstRow = (IDictionary<string, object>)allRows[0];
			Assert.AreEqual(3, firstRow.Count);
			Assert.IsTrue(firstRow.ContainsKey("SOID"));
			Assert.IsTrue(firstRow.ContainsKey("Status"));
			Assert.IsTrue(firstRow.ContainsKey("SalesPersonID"));
		}


		[Test]
		public void All_OrderBySpecification()
		{
			var soh = new SalesOrderHeader();
			var allRows = soh.All(orderBy: "CustomerID DESC").ToList();
			Assert.AreEqual(31465, allRows.Count);
			int previous = int.MaxValue;
			foreach(var r in allRows)
			{
				int current = r.CustomerID;
				Assert.IsTrue(current <= previous);
				previous = current;
			}
		}


		[Test]
		public void All_WhereSpecification()
		{
			var soh = new SalesOrderHeader();
			var allRows = soh.All(where: "WHERE CustomerId=@0", args: 30052).ToList();
			Assert.AreEqual(4, allRows.Count);
		}


		[Test]
		public void All_WhereSpecification_OrderBySpecification()
		{
			var soh = new SalesOrderHeader();
			var allRows = soh.All(orderBy: "SalesOrderID DESC", where: "WHERE CustomerId=@0", args: 30052).ToList();
			Assert.AreEqual(4, allRows.Count);
			int previous = int.MaxValue;
			foreach(var r in allRows)
			{
				int current = r.SalesOrderID;
				Assert.IsTrue(current <= previous);
				previous = current;
			}
		}
		

		[Test]
		public void All_WhereSpecification_ColumnsSpecification()
		{
			var soh = new SalesOrderHeader();
			var allRows = soh.All(columns: "SalesOrderID as SOID, Status, SalesPersonID", where: "WHERE CustomerId=@0", args: 30052).ToList();
			Assert.AreEqual(4, allRows.Count);
			var firstRow = (IDictionary<string, object>)allRows[0];
			Assert.AreEqual(3, firstRow.Count);
			Assert.IsTrue(firstRow.ContainsKey("SOID"));
			Assert.IsTrue(firstRow.ContainsKey("Status"));
			Assert.IsTrue(firstRow.ContainsKey("SalesPersonID"));
		}


		[Test]
		public void All_WhereSpecification_ColumnsSpecification_LimitSpecification()
		{
			var soh = new SalesOrderHeader();
			var allRows = soh.All(limit: 2, columns: "SalesOrderID as SOID, Status, SalesPersonID", where: "WHERE CustomerId=@0", args: 30052).ToList();
			Assert.AreEqual(2, allRows.Count);
			var firstRow = (IDictionary<string, object>)allRows[0];
			Assert.AreEqual(3, firstRow.Count);
			Assert.IsTrue(firstRow.ContainsKey("SOID"));
			Assert.IsTrue(firstRow.ContainsKey("Status"));
			Assert.IsTrue(firstRow.ContainsKey("SalesPersonID"));
		}


		[Test]
		public void Find_AllColumns()
		{
			dynamic soh = new SalesOrderHeader();
			var singleInstance = soh.Find(SalesOrderID: 43666);
			Assert.AreEqual(43666, singleInstance.SalesOrderID);
		}


		[Test]
		public void Find_OneColumn()
		{
			dynamic soh = new SalesOrderHeader();
			var singleInstance = soh.Find(SalesOrderID: 43666, columns:"SalesOrderID");
			Assert.AreEqual(43666, singleInstance.SalesOrderID);
			var siAsDict = (IDictionary<string, object>)singleInstance;
			Assert.AreEqual(1, siAsDict.Count);
		}


		[Test]
		public void Get_AllColumns()
		{
			dynamic soh = new SalesOrderHeader();
			var singleInstance = soh.Get(SalesOrderID: 43666);
			Assert.AreEqual(43666, singleInstance.SalesOrderID);
		}


		[Test]
		public void First_AllColumns()
		{
			dynamic soh = new SalesOrderHeader();
			var singleInstance = soh.First(SalesOrderID: 43666);
			Assert.AreEqual(43666, singleInstance.SalesOrderID);
		}


		[Test]
		public void Single_AllColumns()
		{
			dynamic soh = new SalesOrderHeader();
			var singleInstance = soh.Single(SalesOrderID: 43666);
			Assert.AreEqual(43666, singleInstance.SalesOrderID);
			Assert.AreEqual(26, ((ExpandoObject)singleInstance).AsDictionary().Count);
		}


		[Test]
		public void Single_ThreeColumns()
		{
			dynamic soh = new SalesOrderHeader();
			var singleInstance = soh.Single(SalesOrderID: 43666, columns: "SalesOrderID, SalesOrderNumber, OrderDate");
			Assert.AreEqual(43666, singleInstance.SalesOrderID);
			Assert.AreEqual("SO43666", singleInstance.SalesOrderNumber);
			Assert.AreEqual(new DateTime(2011, 5, 31), singleInstance.OrderDate);
			Assert.AreEqual(3, ((ExpandoObject)singleInstance).AsDictionary().Count);
		}


		[Test]
		public void Query_AllRows()
		{
			var soh = new SalesOrderHeader();
			var allRows = soh.Query("SELECT * FROM Sales.SalesOrderHeader").ToList();
			Assert.AreEqual(31465, allRows.Count);
		}


		[Test]
		public void Query_Filter()
		{
			var soh = new SalesOrderHeader();
			var filteredRows = soh.Query("SELECT * FROM Sales.SalesOrderHeader WHERE CustomerID=@0", 30052).ToList();
			Assert.AreEqual(4, filteredRows.Count);
		}


		[Test]
		public void Paged_NoSpecification()
		{
			var soh = new SalesOrderHeader();
			// no order by, so in theory this is useless. It will order on PK though
			var page2 = soh.Paged(currentPage:2, pageSize: 30);
			var pageItems = ((IEnumerable<dynamic>)page2.Items).ToList();
			Assert.AreEqual(30, pageItems.Count);
			Assert.AreEqual(31465, page2.TotalRecords);
		}


		[Test]
		public void Paged_OrderBySpecification()
		{
			var soh = new SalesOrderHeader();
			var page2 = soh.Paged(orderBy: "CustomerID DESC", currentPage: 2, pageSize: 30);
			var pageItems = ((IEnumerable<dynamic>)page2.Items).ToList();
			Assert.AreEqual(30, pageItems.Count);
			Assert.AreEqual(31465, page2.TotalRecords);

			int previous = int.MaxValue;
			foreach(var r in pageItems)
			{
				int current = r.CustomerID;
				Assert.IsTrue(current <= previous);
				previous = current;
			}
		}


		[Test]
		public void Paged_OrderBySpecification_ColumnsSpecification()
		{
			var soh = new SalesOrderHeader();
			var page2 = soh.Paged(columns: "CustomerID, SalesOrderID", orderBy: "CustomerID DESC", currentPage: 2, pageSize: 30);
			var pageItems = ((IEnumerable<dynamic>)page2.Items).ToList();
			Assert.AreEqual(30, pageItems.Count);
			Assert.AreEqual(31465, page2.TotalRecords);
			var firstRow = (IDictionary<string, object>)pageItems[0];
			Assert.AreEqual(2, firstRow.Count);
			int previous = int.MaxValue;
			foreach(var r in pageItems)
			{
				int current = r.CustomerID;
				Assert.IsTrue(current <= previous);
				previous = current;
			}
		}


		[Test]
		public void Count_NoSpecification()
		{
			var soh = new SalesOrderHeader();
			var total = soh.Count();
			Assert.AreEqual(31465, total);
		}


		[Test]
		public void Count_WhereSpecification_FromArgs()
		{
			var soh = new SalesOrderHeader();
			var total = soh.Count(where: "WHERE CustomerId=@0", args:11212);
			Assert.AreEqual(17, total);
		}


		[Test]
		public void Count_WhereSpecification_FromArgsPlusNameValue()
		{
			dynamic soh = new SalesOrderHeader();
			var total = soh.Count(where: "WHERE CustomerId=@0", args: 11212, ModifiedDate: new DateTime(2013, 10, 10));
			Assert.AreEqual(2, total);
		}


		[Test]
		public void Count_WhereSpecification_FromNameValuePairs()
		{
			dynamic soh = new SalesOrderHeader();
			var total = soh.Count(CustomerID: 11212, ModifiedDate: new DateTime(2013, 10, 10));
			Assert.AreEqual(2, total);
		}


		/// <remarks>
		/// With correct brackets round the WHERE condition in the SQL this returns 17, otherwise it returns 31465!
		/// </remarks>
		[Test]
		public void Count_TestWhereWrapping()
		{
			dynamic soh = new SalesOrderHeader();
			var total = soh.Count(where: "1=1 OR 0=0", CustomerID: 11212);
			Assert.AreEqual(17, total);
		}


		[Test]
		public void DefaultValue()
		{
			var soh = new SalesOrderHeader(false);
			var value = soh.GetColumnDefault("OrderDate");
			Assert.AreEqual(typeof(DateTime), value.GetType());
		}


		[Test]
		public void IsValid_SalesPersonIDCheck()
		{
			dynamic soh = new SalesOrderHeader();
			var toValidate = soh.Find(SalesOrderID: 45816);
			// is invalid
			Assert.IsFalse(soh.IsValid(toValidate));
			Assert.AreEqual(1, soh.Errors.Count);

			toValidate = soh.Find(SalesOrderID: 45069);
			// is valid
			Assert.IsTrue(soh.IsValid(toValidate));
			Assert.AreEqual(0, soh.Errors.Count);
		}


		[Test]
		public void PrimaryKey_Read_Check()
		{
			dynamic soh = new SalesOrderHeader();
			var toValidate = soh.Find(SalesOrderID: 45816);

			Assert.IsTrue(soh.HasPrimaryKey(toValidate));

			var pkValue = soh.GetPrimaryKey(toValidate);
			Assert.AreEqual(45816, pkValue);
		}
	}
}
