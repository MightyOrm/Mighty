using System;
using System.Collections;
using System.Collections.Async;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Mighty.Generic.Tests.SqlServer.TableClasses;
using NUnit.Framework;

namespace Mighty.Generic.Tests.SqlServer
{
	[TestFixture]
	public class ReadTests
	{
		[Test]
		public void MaxOnFilteredSet()
		{
			var soh = new SalesOrderHeaders();
			var result = ((dynamic)soh).Max(columns: "SalesOrderID", where: "SalesOrderID < @0", args: 100000);
			Assert.AreEqual(75123, result);
		}


		[Test]
		public void MaxOnFilteredSet2()
		{
			var soh = new SalesOrderHeaders();
			var result = ((dynamic)soh).Max(columns: "SalesOrderID", TerritoryID: 10);
			Assert.AreEqual(75117, result);
		}


		[Test]
		public void EmptyElement_ProtoType()
		{
			var soh = new SalesOrderHeaders();
			dynamic defaults = soh.New();
			Assert.IsTrue(defaults.OrderDate > DateTime.MinValue);
		}


		[Test]
		public void SchemaTableMetaDataRetrieval()
		{
			var soh = new SalesOrderHeaders();
			var metaData = soh.TableMetaData;
			Assert.IsNotNull(metaData);
			Assert.AreEqual(26, metaData.Count());
			Assert.IsTrue(metaData.All(v=>v.TABLE_NAME==soh.BareTableName));
		}


		[Test]
		public async Task All_NoParameters()
		{
			var soh = new SalesOrderHeaders();
			var allRows = await (await soh.AllAsync()).ToListAsync();
			Assert.AreEqual(31465, allRows.Count);
		}


		[Test]
		public async Task All_NoParameters_Streaming()
		{
			var soh = new SalesOrderHeaders();
			var allRows = await soh.AllAsync();
			var count = 0;
			int nonZeroSalesPersonID = 0;
			await allRows.ForEachAsync(r => {
				count++;
				Assert.Greater(r.SalesOrderID, 0);
				if (r.SalesPersonID > 0)
				{
					nonZeroSalesPersonID++;
					Assert.AreNotEqual("", r.PurchaseOrderNumber);
				}
				else
				{
					Assert.Null(r.PurchaseOrderNumber);
				}
				Assert.Greater(r.CustomerID, 0);
				Assert.Greater(r.Status, 0);
			});
			Assert.AreEqual(31465, count);
			Assert.AreEqual(3806, nonZeroSalesPersonID);
		}


		[Test]
		public async Task All_LimitSpecification()
		{
			var soh = new SalesOrderHeaders();
			var allRows = await (await soh.AllAsync(limit: 10)).ToListAsync();
			Assert.AreEqual(10, allRows.Count);
		}
		

		[Test]
		public async Task All_ColumnSpecification()
		{
			var soh = new SalesOrderHeaders();
			var allRows = await (await soh.AllAsync(columns: "SalesOrderID, Status, SalesPersonID")).ToListAsync();
			Assert.AreEqual(31465, allRows.Count);
			var firstRow = allRows[0];
			Assert.Greater(firstRow.SalesOrderID, 0);
			Assert.AreNotEqual(0, firstRow.Status);
			Assert.Greater(firstRow.SalesPersonID, 0);
		}


		[Test]
		public async Task All_OrderBySpecification()
		{
			var soh = new SalesOrderHeaders();
			var allRows = await (await soh.AllAsync(orderBy: "CustomerID DESC")).ToListAsync();
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
		public async Task All_WhereSpecification()
		{
			var soh = new SalesOrderHeaders();
			var allRows = await (await soh.AllAsync(where: "WHERE CustomerId=@0", args: 30052)).ToListAsync();
			Assert.AreEqual(4, allRows.Count);
		}


		[Test]
		public async Task All_WhereSpecification_OrderBySpecification()
		{
			var soh = new SalesOrderHeaders();
			var allRows = await (await soh.AllAsync(orderBy: "SalesOrderID DESC", where: "WHERE CustomerId=@0", args: 30052)).ToListAsync();
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
		public async Task All_WhereSpecification_ColumnsSpecification()
		{
			var soh = new SalesOrderHeaders();
			var allRows = await soh.AllAsync(columns: "SalesOrderID, Status, SalesPersonID", where: "WHERE CustomerId=@0", args: 30052);
			int count = 0;
			await allRows.ForEachAsync(item => {
				Assert.Greater(item.SalesOrderID, 0);
				Assert.Greater(item.Status, 0);
				Assert.Greater(item.SalesPersonID, 0);

				Assert.AreEqual(item.CustomerID, 0);
				Assert.Null(item.PurchaseOrderNumber);
				count++;
			});
			Assert.AreEqual(4, count);
		}


		[Test]
		public async Task All_WhereSpecification_ColumnsSpecification_LimitSpecification()
		{
			var soh = new SalesOrderHeaders();
			var allRows = await (await soh.AllAsync(limit: 2, columns: "SalesOrderID, Status, SalesPersonID", where: "WHERE CustomerId=@0", args: 30052)).ToListAsync();
			int count = 0;
			foreach (var item in allRows)
			{
				Assert.Greater(item.SalesOrderID, 0);
				Assert.Greater(item.Status, 0);
				Assert.Greater(item.SalesPersonID, 0);

				Assert.AreEqual(item.CustomerID, 0);
				Assert.Null(item.PurchaseOrderNumber);
				count++;
			}
			Assert.AreEqual(2, count);
		}


		[Test]
		public void Find_AllColumns()
		{
			dynamic soh = new SalesOrderHeaders();
			var singleInstance = soh.Find(SalesOrderID: 43666);
			Assert.AreEqual(43666, singleInstance.SalesOrderID);
		}


		[Test]
		public void Find_OneColumn()
		{
			dynamic soh = new SalesOrderHeaders();
			SalesOrderHeader singleInstance = soh.Find(SalesOrderID: 43666, columns: "SalesOrderID");
			Assert.AreEqual(43666, singleInstance.SalesOrderID);
			Assert.AreEqual(0, singleInstance.CustomerID);
		}


		[Test]
		public void Get_AllColumns()
		{
			dynamic soh = new SalesOrderHeaders();
			var singleInstance = soh.Get(SalesOrderID: 43666);
			Assert.AreEqual(43666, singleInstance.SalesOrderID);
		}


		[Test]
		public void First_AllColumns()
		{
			dynamic soh = new SalesOrderHeaders();
			var singleInstance = soh.First(SalesOrderID: 43666);
			Assert.AreEqual(43666, singleInstance.SalesOrderID);
		}


		[Test]
		public void Single_AllColumns()
		{
			dynamic soh = new SalesOrderHeaders();
			SalesOrderHeader singleInstance = soh.Single(SalesOrderID: 43666);
			Assert.AreEqual(43666, singleInstance.SalesOrderID);
			Assert.Greater(singleInstance.CustomerID, 0);
			Assert.Greater(singleInstance.SalesPersonID, 0);
			Assert.Greater(singleInstance.Status, 0);
			Assert.AreNotEqual(singleInstance.PurchaseOrderNumber, "");
			Assert.Greater(singleInstance.OrderDate, DateTime.MinValue);
		}


		[Test]
		public void Single_ThreeColumns()
		{
			dynamic soh = new SalesOrderHeaders();
			SalesOrderHeader singleInstance = soh.Single(SalesOrderID: 43666, columns: "SalesOrderID, SalesOrderNumber, OrderDate");
			Assert.AreEqual(43666, singleInstance.SalesOrderID);
			Assert.AreEqual("SO43666", singleInstance.SalesOrderNumber);
			Assert.AreEqual(new DateTime(2011, 5, 31), singleInstance.OrderDate);
			Assert.Null(singleInstance.PurchaseOrderNumber, "");
			Assert.AreEqual(singleInstance.CustomerID, 0);
		}


		[Test]
		public async Task Query_AllRows()
		{
			var soh = new SalesOrderHeaders();
			var allRows = await (await soh.QueryAsync("SELECT * FROM Sales.SalesOrderHeader")).ToListAsync();
			Assert.AreEqual(31465, allRows.Count);
		}


		[Test]
		public async Task Query_Filter()
		{
			var soh = new SalesOrderHeaders();
			var filteredRows = await (await soh.QueryAsync("SELECT * FROM Sales.SalesOrderHeader WHERE CustomerID=@0", 30052)).ToListAsync();
			Assert.AreEqual(4, filteredRows.Count);
		}


		[Test]
		public async Task Paged_NoSpecification()
		{
			var soh = new SalesOrderHeaders();
			// no order by, so in theory this is useless. It will order on PK though
			var page2 = await soh.PagedAsync(currentPage:2, pageSize: 30);
			var pageItems = page2.Items.ToList();
			Assert.AreEqual(30, pageItems.Count);
			Assert.AreEqual(31465, page2.TotalRecords);
		}


		[Test]
		public async Task Paged_OrderBySpecification()
		{
			var soh = new SalesOrderHeaders();
			var page2 = await soh.PagedAsync(orderBy: "CustomerID DESC", currentPage: 2, pageSize: 30);
			var pageItems = page2.Items.ToList();
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
		public async Task Paged_OrderBySpecification_ColumnsSpecification()
		{
			var soh = new SalesOrderHeaders();
			var page2 = await soh.PagedAsync(columns: "CustomerID, SalesOrderID", orderBy: "CustomerID DESC", currentPage: 2, pageSize: 30);
			var pageItems = page2.Items.ToList();
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
		public async Task Count_NoSpecification()
		{
			var soh = new SalesOrderHeaders();
			var total = await soh.CountAsync();
			Assert.AreEqual(31465, total);
		}


		[Test]
		public async Task Count_WhereSpecification_FromArgs()
		{
			var soh = new SalesOrderHeaders();
			var total = await soh.CountAsync(where: "WHERE CustomerId=@0", args:11212);
			Assert.AreEqual(17, total);
		}


		[Test]
		public void Count_WhereSpecification_FromArgsPlusNameValue()
		{
			dynamic soh = new SalesOrderHeaders();
			var total = soh.Count(where: "WHERE CustomerId=@0", args: 11212, ModifiedDate: new DateTime(2013, 10, 10));
			Assert.AreEqual(2, total);
		}


		[Test]
		public void Count_WhereSpecification_FromNameValuePairs()
		{
			dynamic soh = new SalesOrderHeaders();
			var total = soh.Count(CustomerID: 11212, ModifiedDate: new DateTime(2013, 10, 10));
			Assert.AreEqual(2, total);
		}


		/// <remarks>
		/// With correct brackets round the WHERE condition in the SQL this returns 17, otherwise it returns 31465!
		/// </remarks>
		[Test]
		public void Count_TestWhereWrapping()
		{
			dynamic soh = new SalesOrderHeaders();
			var total = soh.Count(where: "1=1 OR 0=0", CustomerID: 11212);
			Assert.AreEqual(17, total);
		}


		[Test]
		public void DefaultValue()
		{
			var soh = new SalesOrderHeaders(false);
			var value = soh.GetColumnDefault("OrderDate");
			Assert.AreEqual(typeof(DateTime), value.GetType());
		}


		[Test]
		public void IsValid_SalesPersonIDCheck()
		{
			dynamic soh = new SalesOrderHeaders();
			var toValidate = soh.Find(SalesOrderID: 45816);
			// is invalid
			Assert.AreEqual(1, soh.IsValid(toValidate).Count);

			toValidate = soh.Find(SalesOrderID: 45069);
			// is valid
			Assert.AreEqual(0, soh.IsValid(toValidate).Count);
		}


		[Test]
		public void PrimaryKey_Read_Check()
		{
			dynamic soh = new SalesOrderHeaders();
			var toValidate = soh.Find(SalesOrderID: 45816);

			Assert.IsTrue(soh.HasPrimaryKey(toValidate));

			var pkValue = soh.GetPrimaryKey(toValidate);
			Assert.AreEqual(45816, pkValue);
		}
	}
}
