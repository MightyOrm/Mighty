﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Mighty.Dynamic.Tests.SqlServer.TableClasses;
using NUnit.Framework;

namespace Mighty.Dynamic.Tests.SqlServer
{
    [TestFixture("System.Data.SqlClient")]
#if NETCOREAPP3_1
    [TestFixture("Microsoft.Data.SqlClient")]
#endif
    public class ReadTests
    {
        private readonly string ProviderName;

        public ReadTests(string providerName)
        {
            ProviderName = providerName;
        }

#if NETCOREAPP
        [Test]
        public void WorksWithSpacedConnectionString()
        {
            var providerName = "ProviderName";
            var from = $"{providerName}=";
            var to = $" {providerName} = ";
            Assert.That(TestConstants.ReadTestConnection.Contains(from));
            var db = new MightyOrm(string.Format(TestConstants.ReadTestConnection.Replace(from, to), ProviderName));
            var item = db.SingleFromQuery("SELECT 1 AS a");
            Assert.That(item.a, Is.EqualTo(1));
        }
#endif

        [Test]
        public void Guid_Arg()
        {
            // SQL Server has true Guid type support
            var db = new MightyOrm(string.Format(TestConstants.ReadTestConnection, ProviderName));
            var guid = Guid.NewGuid();
            dynamic item;
            using (var command = db.CreateCommand("SELECT @0 AS val", null, guid))
            {
                Assert.AreEqual(DbType.Guid, command.Parameters[0].DbType);
                item = db.Single(command);
            }
            Assert.AreEqual(guid, item.val);
        }


        [Test]
        public void MaxOnFilteredSet()
        {
            var soh = new SalesOrderHeader(ProviderName);
            var result = soh.Max(columns: "SalesOrderID", where: "SalesOrderID < @0", args: 100000);
            Assert.AreEqual(75123, result);
        }


        [Test]
        public void MaxOnFilteredSet2()
        {
            var soh = new SalesOrderHeader(ProviderName);
            var result = soh.Max("SalesOrderID", new { TerritoryID = 10 });
            Assert.AreEqual(75117, result);
        }


        [Test]
        public void EmptyElement_ProtoType()
        {
            var soh = new SalesOrderHeader(ProviderName);
            dynamic defaults = soh.New();
            Assert.IsTrue(defaults.OrderDate > DateTime.MinValue);
        }


        [Test]
        public void SchemaTableMetaDataRetrieval()
        {
            var soh = new SalesOrderHeader(ProviderName);
            var metaData = soh.TableMetaData;
            Assert.IsNotNull(metaData);
            Assert.AreEqual(26, metaData.Count());
            Assert.IsTrue(metaData.All(v=>v.TABLE_NAME==soh.BareTableName));
        }


        [Test]
        public void All_NoParameters()
        {
            var soh = new SalesOrderHeader(ProviderName);
            var allRows = soh.All().ToList();
            Assert.AreEqual(31465, allRows.Count);
        }


        [Test]
        public void All_NoParameters_Streaming()
        {
            var soh = new SalesOrderHeader(ProviderName);
            var allRows = soh.All();
            var count = 0;
            foreach(var r in allRows)
            {
                count++;
                Assert.AreEqual(26, ((IDictionary<string, object>)r).Count);        // # of fields fetched should be 26
            }
            Assert.AreEqual(31465, count);
        }


        [Test]
        public void All_LimitSpecification()
        {
            var soh = new SalesOrderHeader(ProviderName);
            var allRows = soh.All(limit: 10).ToList();
            Assert.AreEqual(10, allRows.Count);
        }
        

        [Test]
        public void All_ColumnSpecification()
        {
            var soh = new SalesOrderHeader(ProviderName);
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
            var soh = new SalesOrderHeader(ProviderName);
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
            var soh = new SalesOrderHeader(ProviderName);
            var allRows = soh.All(where: "WHERE CustomerId=@0", args: 30052).ToList();
            Assert.AreEqual(4, allRows.Count);
        }


        [Test]
        public void All_WhereSpecification_OrderBySpecification()
        {
            var soh = new SalesOrderHeader(ProviderName);
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
            var soh = new SalesOrderHeader(ProviderName);
            var allRows = soh.All(columns: "SalesOrderID as SOID, Status, SalesPersonID", where: "WHERE CustomerId=@0", args: 30052).ToList();
            Assert.AreEqual(4, allRows.Count);
            var firstRow = (IDictionary<string, object>)allRows[0];
            Assert.AreEqual(3, firstRow.Count);
            Assert.IsTrue(firstRow.ContainsKey("SOID"));
            Assert.IsTrue(firstRow.ContainsKey("Status"));
            Assert.IsTrue(firstRow.ContainsKey("SalesPersonID"));
        }


#if DYNAMIC_METHODS
        [Test]
        public void All_WhereSpecification_ColumnsSpecification_LimitSpecification()
        {
            var soh = new SalesOrderHeader(ProviderName);
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
            dynamic soh = new SalesOrderHeader(ProviderName);
            var singleInstance = soh.Find(SalesOrderID: 43666);
            Assert.AreEqual(43666, singleInstance.SalesOrderID);
        }


        [Test]
        public void Find_OneColumn()
        {
            dynamic soh = new SalesOrderHeader(ProviderName);
            var singleInstance = soh.Find(SalesOrderID: 43666, columns:"SalesOrderID");
            Assert.AreEqual(43666, singleInstance.SalesOrderID);
            var siAsDict = (IDictionary<string, object>)singleInstance;
            Assert.AreEqual(1, siAsDict.Count);
        }


        [Test]
        public void Get_AllColumns()
        {
            dynamic soh = new SalesOrderHeader(ProviderName);
            var singleInstance = soh.Get(SalesOrderID: 43666);
            Assert.AreEqual(43666, singleInstance.SalesOrderID);
        }


        [Test]
        public void First_AllColumns()
        {
            dynamic soh = new SalesOrderHeader(ProviderName);
            var singleInstance = soh.First(SalesOrderID: 43666);
            Assert.AreEqual(43666, singleInstance.SalesOrderID);
        }
#endif


        [Test]
        public void Single_Where_AllColumns()
        {
            var soh = new SalesOrderHeader(ProviderName);
            var singleInstance = soh.Single(new { SalesOrderID = 43666 });
            Assert.AreEqual(43666, singleInstance.SalesOrderID);
            Assert.AreEqual(26, ((ExpandoObject)singleInstance).ToDictionary().Count);
        }


        [Test]
        public void Single_Key_AllColumns()
        {
            var soh = new SalesOrderHeader(ProviderName);
            var singleInstance = soh.Single(43666);
            Assert.AreEqual(43666, singleInstance.SalesOrderID);
            Assert.AreEqual(26, ((ExpandoObject)singleInstance).ToDictionary().Count);
        }


        [Test]
        public void Single_Where_ThreeColumns()
        {
            var soh = new SalesOrderHeader(ProviderName);
            var singleInstance = soh.Single(new { SalesOrderID = 43666 }, columns: "SalesOrderID, SalesOrderNumber, OrderDate");
            Assert.AreEqual(43666, singleInstance.SalesOrderID);
            Assert.AreEqual("SO43666", singleInstance.SalesOrderNumber);
            Assert.AreEqual(new DateTime(2011, 5, 31), singleInstance.OrderDate);
            Assert.AreEqual(3, ((ExpandoObject)singleInstance).ToDictionary().Count);
        }


        [Test]
        public void Single_Key_ThreeColumns()
        {
            var soh = new SalesOrderHeader(ProviderName);
            var singleInstance = soh.Single(43666, columns: "SalesOrderID, SalesOrderNumber, OrderDate");
            Assert.AreEqual(43666, singleInstance.SalesOrderID);
            Assert.AreEqual("SO43666", singleInstance.SalesOrderNumber);
            Assert.AreEqual(new DateTime(2011, 5, 31), singleInstance.OrderDate);
            Assert.AreEqual(3, ((ExpandoObject)singleInstance).ToDictionary().Count);
        }


        [Test]
        public void Query_AllRows()
        {
            var soh = new SalesOrderHeader(ProviderName);
            var allRows = soh.Query("SELECT * FROM Sales.SalesOrderHeader").ToList();
            Assert.AreEqual(31465, allRows.Count);
        }


        [Test]
        public void Query_Filter()
        {
            var soh = new SalesOrderHeader(ProviderName);
            var filteredRows = soh.Query("SELECT * FROM Sales.SalesOrderHeader WHERE CustomerID=@0", 30052).ToList();
            Assert.AreEqual(4, filteredRows.Count);
        }


        [Test]
        public void Paged_NoSpecification()
        {
            var soh = new SalesOrderHeader(ProviderName);
            // no order by, and paged queries logically must have an order by; this will order on PK
            var page2 = soh.Paged(currentPage:2, pageSize: 30);
            Assert.AreEqual(30, page2.Items.Count);
            Assert.AreEqual(31465, page2.TotalRecords);
            Assert.AreEqual(2, page2.CurrentPage);
            Assert.AreEqual(30, page2.PageSize);
        }


        [Test]
        public void Paged_WhereSpecification()
        {
            var soh = new SalesOrderHeader(ProviderName);
            var page3 = soh.Paged(currentPage: 3, where: "SalesOrderNumber LIKE @0", args: "SO4%");
            var pageItems = ((IEnumerable<dynamic>)page3.Items).ToList();
            Assert.AreEqual(20, pageItems.Count);
            Assert.AreEqual(6341, page3.TotalRecords);
        }


        [Test]
        public void Paged_WhereSpecification_WithParams()
        {
            var soh = new SalesOrderHeader(ProviderName);
            var page3 = soh.PagedWithParams(currentPage: 3, where: "SalesOrderNumber LIKE @son", inParams: new { son = "SO4%" });
            var pageItems = ((IEnumerable<dynamic>)page3.Items).ToList();
            Assert.AreEqual(20, pageItems.Count);
            Assert.AreEqual(6341, page3.TotalRecords);
        }


        [Test]
        public void Paged_OrderBySpecification()
        {
            var soh = new SalesOrderHeader(ProviderName);
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
            var soh = new SalesOrderHeader(ProviderName);
            var page2 = soh.Paged(columns: "CustomerID, SalesOrderID", orderBy: "CustomerID DESC", currentPage: 2, pageSize: 30);
            var pageItems = ((IEnumerable<dynamic>)page2.Items).ToList();
            Assert.AreEqual(30, pageItems.Count);
            Assert.AreEqual(31465, page2.TotalRecords);
            var firstRow = (IDictionary<string, object>)pageItems[0];
            Assert.AreEqual(3, firstRow.Count);
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
            var soh = new SalesOrderHeader(ProviderName);
            var total = soh.Count();
            Assert.AreEqual(31465, total);
        }


        [Test]
        public void Count_WhereSpecification_FromArgs()
        {
            var soh = new SalesOrderHeader(ProviderName);
            var total = soh.Count(where: "WHERE CustomerId=@0", args:11212);
            Assert.AreEqual(17, total);
        }


#if DYNAMIC_METHODS
        [Test]
        public void Count_WhereSpecification_FromArgsPlusNameValue()
        {
            dynamic soh = new SalesOrderHeader(ProviderName);
            var total = soh.Count(where: "WHERE CustomerId=@0", args: 11212, ModifiedDate: new DateTime(2013, 10, 10));
            Assert.AreEqual(2, total);
        }


        [Test]
        public void Count_WhereSpecification_FromNameValuePairs()
        {
            dynamic soh = new SalesOrderHeader(ProviderName);
            var total = soh.Count(CustomerID: 11212, ModifiedDate: new DateTime(2013, 10, 10));
            Assert.AreEqual(2, total);
        }


        /// <remarks>
        /// With correct brackets round the WHERE condition in the SQL this returns 17, otherwise it returns 31465!
        /// </remarks>
        [Test]
        public void Count_TestWhereWrapping()
        {
            dynamic soh = new SalesOrderHeader(ProviderName);
            var total = soh.Count(where: "1=1 OR 0=0", CustomerID: 11212);
            Assert.AreEqual(17, total);
        }
#else
        [Test]
        public void Count_WhereSpecification_FromNameValuePairs()
        {
            var soh = new SalesOrderHeader(ProviderName);
            var total = soh.Count(new { CustomerID = 11212, ModifiedDate = new DateTime(2013, 10, 10) });
            Assert.AreEqual(2, total);
        }
#endif


        [Test]
        public void DefaultValue()
        {
            var soh = new SalesOrderHeader(ProviderName, false);
            var value = soh.GetColumnDefault("OrderDate");
            Assert.AreEqual(typeof(DateTime), value.GetType());
        }


        [Test]
        public void IsValid_SalesPersonIDCheck()
        {
            var soh = new SalesOrderHeader(ProviderName);
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
            var soh = new SalesOrderHeader(ProviderName);
            var toValidate = soh.Single(new { SalesOrderID = 45816 });

            Assert.IsTrue(soh.HasPrimaryKey(toValidate));

            var pkValue = soh.GetPrimaryKey(toValidate);
            Assert.AreEqual(45816, pkValue);
        }


#if KEY_VALUES
        [Test]
        public void KeyValues()
        {
            var contactTypes = new ContactType(ProviderName);
            var keyValues = contactTypes.KeyValues();
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
