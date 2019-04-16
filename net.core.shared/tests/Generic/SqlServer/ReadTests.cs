using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
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
            var result = soh.Max(columns: "SalesOrderID", where: "SalesOrderID < @0", args: 100000);
            Assert.AreEqual(75123, result);
        }


        [Test]
        public void MaxOnFilteredSet2()
        {
            var soh = new SalesOrderHeaders();
            var result = soh.Max("SalesOrderID", new { TerritoryID = 10 });
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
        public void All_NoParameters()
        {
            var soh = new SalesOrderHeaders();
            var allRows = soh.All().ToList();
            Assert.AreEqual(31465, allRows.Count);
        }


        [Test]
        public void All_NoParameters_Streaming()
        {
            var soh = new SalesOrderHeaders();
            var allRows = soh.All();
            var count = 0;
            int nonZeroSalesPersonID = 0;
            foreach (var r in allRows)
            {
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
            }
            Assert.AreEqual(31465, count);
            Assert.AreEqual(3806, nonZeroSalesPersonID);
        }


        [Test]
        public void All_LimitSpecification()
        {
            var soh = new SalesOrderHeaders();
            var allRows = soh.All(limit: 10).ToList();
            Assert.AreEqual(10, allRows.Count);
        }
        

        [Test]
        public void All_ColumnSpecification()
        {
            var soh = new SalesOrderHeaders();
            var allRows = soh.All(columns: "SalesOrderID, Status, SalesPersonID").ToList();
            Assert.AreEqual(31465, allRows.Count);
            var firstRow = allRows[0];
            Assert.Greater(firstRow.SalesOrderID, 0);
            Assert.AreNotEqual(0, firstRow.Status);
            Assert.Greater(firstRow.SalesPersonID, 0);
        }


        [Test]
        public void All_OrderBySpecification()
        {
            var soh = new SalesOrderHeaders();
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
        public void All_WhereClause()
        {
            var soh = new SalesOrderHeaders();
            var allRows = soh.All(where: "WHERE CustomerId=@0", args: 30052).ToList();
            Assert.AreEqual(4, allRows.Count);
        }


        [Test]
        public void All_WhereClause_OrderBy()
        {
            var soh = new SalesOrderHeaders();
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
        public void All_WhereClause_Columns()
        {
            var soh = new SalesOrderHeaders();
            var allRows = soh.All(columns: "SalesOrderID, Status, SalesPersonID", where: "WHERE CustomerId=@0", args: 30052);
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
            Assert.AreEqual(4, count);
        }


        [Test]
        public void All_WhereClause_Columns_Limit()
        {
            var soh = new SalesOrderHeaders();
            var allRows = soh.All(limit: 2, columns: "SalesOrderID, Status, SalesPersonID", where: "WHERE CustomerId=@0", args: 30052).ToList();
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
        public void All_WhereParams()
        {
            var soh = new SalesOrderHeaders();
            var allRows = soh.All(new { CustomerId = 30052 }).ToList();
            Assert.AreEqual(4, allRows.Count);
        }


        [Test]
        public void All_WhereParams_OrderBy()
        {
            var soh = new SalesOrderHeaders();
            var allRows = soh.All(orderBy: "SalesOrderID DESC", whereParams: new { CustomerId = 30052 }).ToList();
            Assert.AreEqual(4, allRows.Count);
            int previous = int.MaxValue;
            foreach (var r in allRows)
            {
                int current = r.SalesOrderID;
                Assert.IsTrue(current <= previous);
                previous = current;
            }
        }


        [Test]
        public void All_WhereParams_Columns()
        {
            var soh = new SalesOrderHeaders();
            var allRows = soh.All(columns: "SalesOrderID, Status, SalesPersonID", whereParams: new { CustomerId = 30052 });
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
            Assert.AreEqual(4, count);
        }


        [Test]
        public void All_WhereParams_Columns_Limit()
        {
            var soh = new SalesOrderHeaders();
            var allRows = soh.All(limit: 2, columns: "SalesOrderID, Status, SalesPersonID", whereParams: new { CustomerId = 30052 }).ToList();
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
        public void All_WhereParamsKey_ThrowsInvalidOperationException()
        {
            var ex = Assert.Throws<InvalidOperationException>(() => {
                var soh = new SalesOrderHeaders();
                var allRows = soh.All(limit: 2, columns: "SalesOrderID, Status, SalesPersonID", whereParams: 30052).ToList();
            });
            Assert.AreEqual("whereParams in All(...) should contain names and values but it contained values only. If you want to get a single item by its primary key use Single(...) instead.", ex.Message);
        }


#if DYNAMIC_METHODS
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
#endif


        [Test]
        public void Single_Where_AllColumns()
        {
            var soh = new SalesOrderHeaders();
            SalesOrderHeader singleInstance = soh.Single(new { SalesOrderID = 43666 });
            Assert.AreEqual(43666, singleInstance.SalesOrderID);
            Assert.Greater(singleInstance.CustomerID, 0);
            Assert.Greater(singleInstance.SalesPersonID, 0);
            Assert.Greater(singleInstance.Status, 0);
            Assert.AreNotEqual(singleInstance.PurchaseOrderNumber, "");
            Assert.Greater(singleInstance.OrderDate, DateTime.MinValue);
        }


        [Test]
        public void Single_Key_AllColumns()
        {
            var soh = new SalesOrderHeaders();
            SalesOrderHeader singleInstance = soh.Single(43666);
            Assert.AreEqual(43666, singleInstance.SalesOrderID);
            Assert.Greater(singleInstance.CustomerID, 0);
            Assert.Greater(singleInstance.SalesPersonID, 0);
            Assert.Greater(singleInstance.Status, 0);
            Assert.AreNotEqual(singleInstance.PurchaseOrderNumber, "");
            Assert.Greater(singleInstance.OrderDate, DateTime.MinValue);
        }


        [Test]
        public void Single_Where_ThreeColumns()
        {
            var soh = new SalesOrderHeaders();
            SalesOrderHeader singleInstance = soh.Single(new { SalesOrderID = 43666 }, columns: "SalesOrderID, SalesOrderNumber, OrderDate");
            Assert.AreEqual(43666, singleInstance.SalesOrderID);
            Assert.AreEqual("SO43666", singleInstance.SalesOrderNumber);
            Assert.AreEqual(new DateTime(2011, 5, 31), singleInstance.OrderDate);
            Assert.Null(singleInstance.PurchaseOrderNumber, "");
            Assert.AreEqual(singleInstance.CustomerID, 0);
        }


        [Test]
        public void Single_Key_ThreeColumns()
        {
            var soh = new SalesOrderHeaders();
            SalesOrderHeader singleInstance = soh.Single(43666, columns: "SalesOrderID, SalesOrderNumber, OrderDate");
            Assert.AreEqual(43666, singleInstance.SalesOrderID);
            Assert.AreEqual("SO43666", singleInstance.SalesOrderNumber);
            Assert.AreEqual(new DateTime(2011, 5, 31), singleInstance.OrderDate);
            Assert.Null(singleInstance.PurchaseOrderNumber, "");
            Assert.AreEqual(singleInstance.CustomerID, 0);
        }


        [Test]
        public void Query_AllRows()
        {
            var soh = new SalesOrderHeaders();
            var allRows = soh.Query("SELECT * FROM Sales.SalesOrderHeader").ToList();
            Assert.AreEqual(31465, allRows.Count);
        }


        [Test]
        public void Query_Filter()
        {
            var soh = new SalesOrderHeaders();
            var filteredRows = soh.Query("SELECT * FROM Sales.SalesOrderHeader WHERE CustomerID=@0", 30052).ToList();
            Assert.AreEqual(4, filteredRows.Count);
        }


        [Test]
        public void Paged_NoSpecification()
        {
            var soh = new SalesOrderHeaders();
            // no order by, so in theory this is useless. It will order on PK though
            var page2 = soh.Paged(currentPage:2, pageSize: 30);
            var pageItems = page2.Items.ToList();
            Assert.AreEqual(30, pageItems.Count);
            Assert.AreEqual(31465, page2.TotalRecords);
        }


        [Test]
        public void Paged_OrderBySpecification()
        {
            var soh = new SalesOrderHeaders();
            var page2 = soh.Paged(orderBy: "CustomerID DESC", currentPage: 2, pageSize: 30);
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
        public void Paged_OrderBySpecification_ColumnsSpecification()
        {
            var soh = new SalesOrderHeaders();
            var page2 = soh.Paged(columns: "CustomerID, SalesOrderID", orderBy: "CustomerID DESC", currentPage: 2, pageSize: 30);
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
        public void Count_NoSpecification()
        {
            var soh = new SalesOrderHeaders();
            var total = soh.Count();
            Assert.AreEqual(31465, total);
        }


        [Test]
        public void Count_WhereSpecification_FromArgs()
        {
            var soh = new SalesOrderHeaders();
            var total = soh.Count(where: "WHERE CustomerId=@0", args:11212);
            Assert.AreEqual(17, total);
        }


#if DYNAMIC_METHODS
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
#else
        [Test]
        public void Count_WhereSpecification_FromNameValuePairs()
        {
            var soh = new SalesOrderHeaders();
            var total = soh.Count(new { CustomerID = 11212, ModifiedDate = new DateTime(2013, 10, 10) });
            Assert.AreEqual(2, total);
        }
#endif


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
            var soh = new SalesOrderHeaders();
            var toValidate = soh.Single(new { SalesOrderID = 45816 });
            // is invalid
            Assert.AreEqual(1, soh.IsValid(toValidate).Count);

            toValidate = soh.Single(new { SalesOrderID = 45069 });
            // is valid
            Assert.AreEqual(0, soh.IsValid(toValidate).Count);
        }


        [Test]
        public void PrimaryKey_Read_Check()
        {
            var soh = new SalesOrderHeaders();
            var toValidate = soh.Single(new { SalesOrderID = 45816 });

            Assert.IsTrue(soh.HasPrimaryKey(toValidate));

            var pkValue = soh.GetPrimaryKey(toValidate);
            Assert.AreEqual(45816, pkValue);
        }
    }
}
