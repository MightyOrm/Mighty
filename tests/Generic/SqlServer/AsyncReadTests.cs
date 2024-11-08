#if !NET40
using System;
using System.Collections;
using Dasync.Collections;
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
    [TestFixture("System.Data.SqlClient")]
#if NETCOREAPP3_1
    [TestFixture("Microsoft.Data.SqlClient")]
#endif
    public class AsyncReadTests
    {
        private readonly string ProviderName;

        public AsyncReadTests(string providerName)
        {
            ProviderName = providerName;
        }

        [Test]
        public async Task MaxOnFilteredSet()
        {
            var soh = new SalesOrderHeaders(ProviderName);
            var result = await soh.MaxAsync(columns: "SalesOrderID", where: "SalesOrderID < @0", args: 100000);
            Assert.AreEqual(75123, result);
        }


        [Test]
        public async Task MaxOnFilteredSet2()
        {
            var soh = new SalesOrderHeaders(ProviderName);
            var result = await soh.MaxAsync("SalesOrderID", new { TerritoryID = 10 });
            Assert.AreEqual(75117, result);
        }


        [Test]
        public async Task EmptyElement_ProtoType()
        {
            var soh = new SalesOrderHeaders(ProviderName);
            dynamic defaults = await soh.NewAsync();
            Assert.IsTrue(defaults.OrderDate > DateTime.MinValue);
        }


        [Test]
        public async Task SchemaTableMetaDataRetrieval()
        {
            var soh = new SalesOrderHeaders(ProviderName);
            var metaData = await soh.GetTableMetaDataAsync();
            Assert.IsNotNull(metaData);
            Assert.AreEqual(26, metaData.Count());
            Assert.IsTrue(metaData.All(v=>v.TABLE_NAME==soh.BareTableName));
        }


        [Test]
        public async Task All_NoParameters()
        {
            var soh = new SalesOrderHeaders(ProviderName);
            var allRows = await (await soh.AllAsync()).ToListAsync();
            Assert.AreEqual(31465, allRows.Count);
        }


        [Test]
        public async Task All_NoParameters_Streaming()
        {
            var soh = new SalesOrderHeaders(ProviderName);
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
            var soh = new SalesOrderHeaders(ProviderName);
            var allRows = await (await soh.AllAsync(limit: 10)).ToListAsync();
            Assert.AreEqual(10, allRows.Count);
        }
        

        [Test]
        public async Task All_ColumnSpecification()
        {
            var soh = new SalesOrderHeaders(ProviderName);
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
            var soh = new SalesOrderHeaders(ProviderName);
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
            var soh = new SalesOrderHeaders(ProviderName);
            var allRows = await (await soh.AllAsync(where: "WHERE CustomerId=@0", args: 30052)).ToListAsync();
            Assert.AreEqual(4, allRows.Count);
        }


        [Test]
        public async Task All_WhereSpecification_OrderBySpecification()
        {
            var soh = new SalesOrderHeaders(ProviderName);
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
            var soh = new SalesOrderHeaders(ProviderName);
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
            var soh = new SalesOrderHeaders(ProviderName);
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


#if DYNAMIC_METHODS
        [Test]
        public async Task Find_AllColumns()
        {
            dynamic soh = new SalesOrderHeaders(ProviderName);
            var singleInstance = await soh.FindAsync(SalesOrderID: 43666);
            Assert.AreEqual(43666, singleInstance.SalesOrderID);
        }


        [Test]
        public async Task Find_OneColumn()
        {
            dynamic soh = new SalesOrderHeaders(ProviderName);
            SalesOrderHeader singleInstance = await soh.FindAsync(SalesOrderID: 43666, columns: "SalesOrderID");
            Assert.AreEqual(43666, singleInstance.SalesOrderID);
            Assert.AreEqual(0, singleInstance.CustomerID);
        }


        [Test]
        public async Task Get_AllColumns()
        {
            dynamic soh = new SalesOrderHeaders(ProviderName);
            var singleInstance = await soh.GetAsync(SalesOrderID: 43666);
            Assert.AreEqual(43666, singleInstance.SalesOrderID);
        }


        [Test]
        public async Task First_AllColumns()
        {
            dynamic soh = new SalesOrderHeaders(ProviderName);
            var singleInstance = await soh.FirstAsync(SalesOrderID: 43666);
            Assert.AreEqual(43666, singleInstance.SalesOrderID);
        }
#endif


        [Test]
        public async Task Single_Where_AllColumns()
        {
            var soh = new SalesOrderHeaders(ProviderName);
            SalesOrderHeader singleInstance = await soh.SingleAsync(new { SalesOrderID = 43666 });
            Assert.AreEqual(43666, singleInstance.SalesOrderID);
            Assert.Greater(singleInstance.CustomerID, 0);
            Assert.Greater(singleInstance.SalesPersonID, 0);
            Assert.Greater(singleInstance.Status, 0);
            Assert.AreNotEqual(singleInstance.PurchaseOrderNumber, "");
            Assert.Greater(singleInstance.OrderDate, DateTime.MinValue);
        }


        [Test]
        public async Task Single_Key_AllColumns()
        {
            var soh = new SalesOrderHeaders(ProviderName);
            SalesOrderHeader singleInstance = await soh.SingleAsync(43666);
            Assert.AreEqual(43666, singleInstance.SalesOrderID);
            Assert.Greater(singleInstance.CustomerID, 0);
            Assert.Greater(singleInstance.SalesPersonID, 0);
            Assert.Greater(singleInstance.Status, 0);
            Assert.AreNotEqual(singleInstance.PurchaseOrderNumber, "");
            Assert.Greater(singleInstance.OrderDate, DateTime.MinValue);
        }


        [Test]
        public async Task Single_Where_ThreeColumns()
        {
            var soh = new SalesOrderHeaders(ProviderName);
            SalesOrderHeader singleInstance = await soh.SingleAsync(new { SalesOrderID = 43666 }, columns: "SalesOrderID, SalesOrderNumber, OrderDate");
            Assert.AreEqual(43666, singleInstance.SalesOrderID);
            Assert.AreEqual("SO43666", singleInstance.SalesOrderNumber);
            Assert.AreEqual(new DateTime(2011, 5, 31), singleInstance.OrderDate);
            Assert.Null(singleInstance.PurchaseOrderNumber, "");
            Assert.AreEqual(singleInstance.CustomerID, 0);
        }


        [Test]
        public async Task Single_Key_ThreeColumns()
        {
            var soh = new SalesOrderHeaders(ProviderName);
            SalesOrderHeader singleInstance = await soh.SingleAsync(43666, columns: "SalesOrderID, SalesOrderNumber, OrderDate");
            Assert.AreEqual(43666, singleInstance.SalesOrderID);
            Assert.AreEqual("SO43666", singleInstance.SalesOrderNumber);
            Assert.AreEqual(new DateTime(2011, 5, 31), singleInstance.OrderDate);
            Assert.Null(singleInstance.PurchaseOrderNumber, "");
            Assert.AreEqual(singleInstance.CustomerID, 0);
        }


        [Test]
        public async Task Query_AllRows()
        {
            var soh = new SalesOrderHeaders(ProviderName);
            var allRows = await (await soh.QueryAsync("SELECT * FROM Sales.SalesOrderHeader")).ToListAsync();
            Assert.AreEqual(31465, allRows.Count);
        }


        [Test]
        public async Task Query_Filter()
        {
            var soh = new SalesOrderHeaders(ProviderName);
            var filteredRows = await (await soh.QueryAsync("SELECT * FROM Sales.SalesOrderHeader WHERE CustomerID=@0", 30052)).ToListAsync();
            Assert.AreEqual(4, filteredRows.Count);
        }


        [Test]
        public async Task Paged_NoSpecification()
        {
            var soh = new SalesOrderHeaders(ProviderName);
            // no order by, and paged queries logically must have an order by; this will order on PK
            var page2 = await soh.PagedAsync(currentPage:2, pageSize: 30);
            var pageItems = page2.Items.ToList();
            Assert.AreEqual(30, pageItems.Count);
            Assert.AreEqual(31465, page2.TotalRecords);
        }


        [Test]
        public async Task Paged_WhereSpecification()
        {
            var soh = new SalesOrderHeaders(ProviderName);
            var page3 = await soh.PagedAsync(currentPage: 3, where: "SalesOrderNumber LIKE @0", args: "SO4%");
            var pageItems = page3.Items.ToList();
            Assert.AreEqual(20, pageItems.Count);
            Assert.AreEqual(6341, page3.TotalRecords);
        }


        [Test]
        public async Task Paged_OrderBySpecification()
        {
            var soh = new SalesOrderHeaders(ProviderName);
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
            var soh = new SalesOrderHeaders(ProviderName);
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
            var soh = new SalesOrderHeaders(ProviderName);
            var total = await soh.CountAsync();
            Assert.AreEqual(31465, total);
        }


        [Test]
        public async Task Count_WhereSpecification_FromArgs()
        {
            var soh = new SalesOrderHeaders(ProviderName);
            var total = await soh.CountAsync(where: "WHERE CustomerId=@0", args:11212);
            Assert.AreEqual(17, total);
        }


#if DYNAMIC_METHODS
        [Test]
        public async Task Count_WhereSpecification_FromArgsPlusNameValue()
        {
            dynamic soh = new SalesOrderHeaders(ProviderName);
            var total = await soh.CountAsync(where: "WHERE CustomerId=@0", args: 11212, ModifiedDate: new DateTime(2013, 10, 10));
            Assert.AreEqual(2, total);
        }


        [Test]
        public async Task Count_WhereSpecification_FromNameValuePairs()
        {
            dynamic soh = new SalesOrderHeaders(ProviderName);
            var total = await soh.CountAsync(CustomerID: 11212, ModifiedDate: new DateTime(2013, 10, 10));
            Assert.AreEqual(2, total);
        }


        /// <remarks>
        /// With correct brackets round the WHERE condition in the SQL this returns 17, otherwise it returns 31465!
        /// </remarks>
        [Test]
        public async Task Count_TestWhereWrapping()
        {
            dynamic soh = new SalesOrderHeaders(ProviderName);
            var total = await soh.CountAsync(where: "1=1 OR 0=0", CustomerID: 11212);
            Assert.AreEqual(17, total);
        }
#else
        [Test]
        public async Task Count_WhereSpecification_FromNameValuePairs()
        {
            dynamic soh = new SalesOrderHeaders(ProviderName);
            var total = await soh.CountAsync(new { CustomerID = 11212, ModifiedDate = new DateTime(2013, 10, 10) });
            Assert.AreEqual(2, total);
        }
#endif


        [Test]
        public void DefaultValue()
        {
            var soh = new SalesOrderHeaders(ProviderName, false);
            var value = soh.GetColumnDefault("OrderDate");
            Assert.AreEqual(typeof(DateTime), value.GetType());
        }


        [Test]
        public async Task IsValid_SalesPersonIDCheck()
        {
            var soh = new SalesOrderHeaders(ProviderName);
            var toValidate = await soh.SingleAsync(new { SalesOrderID = 45816 });
            // is invalid
            Assert.AreEqual(1, soh.IsValid(toValidate).Count);

            toValidate = await soh.SingleAsync(new { SalesOrderID = 45069 });
            // is valid
            Assert.AreEqual(0, soh.IsValid(toValidate).Count);
        }


        [Test]
        public async Task PrimaryKey_Read_Check()
        {
            var soh = new SalesOrderHeaders(ProviderName);
            var toValidate = await soh.SingleAsync(new { SalesOrderID = 45816 });

            Assert.IsTrue(soh.HasPrimaryKey(toValidate));

            var pkValue = soh.GetPrimaryKey(toValidate);
            Assert.AreEqual(45816, pkValue);
        }


#if KEY_VALUES
        [Test]
        public async Task KeyValues()
        {
            var contactTypes = new ContactTypes(ProviderName);
            var keyValues = await contactTypes.KeyValuesAsync();
            int count = 0;
            int oldId = 0;
            string oldName = null;
            foreach (var keyValue in keyValues)
            {
                int id = int.Parse(keyValue.Key);
                Assert.Greater(id, oldId);
                oldId = id;
                Assert.False(string.IsNullOrEmpty(keyValue.Value));
                Assert.AreNotEqual(oldName, keyValue.Value);
                oldName = keyValue.Value;
                count++;
            }
            Assert.AreEqual(20, count);
        }
#endif
    }
}
#endif