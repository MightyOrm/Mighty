﻿using System;
using System.Data;
using System.Dynamic;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mighty.Dynamic.Tests.SqlServer.TableClasses;
using NUnit.Framework;

namespace Mighty.Dynamic.Tests.SqlServer
{
    /// <summary>
    /// Suite of tests for stored procedures and functions on SQL Server database.
    /// </summary>
    /// <remarks>
    /// Runs against functions and procedures which are already in the AdventureWorks test database.
    /// </remarks>
    [TestFixture("System.Data.SqlClient")]
#if NETCOREAPP3_1
    [TestFixture("Microsoft.Data.SqlClient")]
#endif
    public class SPTests
    {
        private readonly string ProviderName;

        public SPTests(string providerName)
        {
            ProviderName = providerName;
        }

        [Test]
        public void NormalSingleCall()
        {
            // Check that things are up and running normally before trying the new stuff
            var soh = new SalesOrderHeader(ProviderName);
            var item = soh.Single("SalesOrderID=@0", args: 43659);
            Assert.AreEqual("PO522145787", item.PurchaseOrderNumber);
        }

        [Test]
        public void InitialNullBooleanOutputParam()
        {
            var db = new SPTestsDatabase(ProviderName);
            dynamic boolResult = db.ExecuteWithParams("set @a = 1", outParams: new { a = (bool?)null });
            Assert.AreEqual(typeof(bool), boolResult.a.GetType());
        }

        [Test]
        public void InitialNullIntegerOutputParam()
        {
            var db = new SPTestsDatabase(ProviderName);
            dynamic intResult = db.ExecuteWithParams("set @a = 1", outParams: new { a = (int?)null });
            Assert.AreEqual(typeof(int), intResult.a.GetType());
        }

        [Test]
        public void QueryFromStoredProcedure()
        {
            var db = new SPTestsDatabase(ProviderName);
            var people = db.QueryFromProcedure("uspGetEmployeeManagers", new { BusinessEntityID = 35 });
            int count = 0;
            foreach(var person in people)
            {
                Console.WriteLine(person.FirstName + " " + person.LastName);
                count++;
            }
            Assert.AreEqual(3, count);
        }

        [Test]
        public void SingleRowFromTableValuedFunction()
        {
            var db = new SPTestsDatabase(ProviderName);
            // Accessing table value functions on SQL Server (different syntax from Postgres, for example)
            var person = db.SingleFromQueryWithParams("SELECT * FROM dbo.ufnGetContactInformation(@PersonID)", new { @PersonID = 35 });
            Assert.AreEqual(typeof(string), person.FirstName.GetType());
        }

        [Test]
        public void DateReturnParameter()
        {
            var db = new SPTestsDatabase(ProviderName);
            dynamic d = new ExpandoObject();
            d.d = true; // NB the type is ignored (by the underlying driver)
            var dResult = db.ExecuteProcedure("ufnGetAccountingEndDate", returnParams: d);
            Assert.AreEqual(typeof(DateTime), dResult.d.GetType());
        }

        [Test]
        public void QueryMultipleFromTwoResultSets()
        {
            var db = new SPTestsDatabase(ProviderName);
            var twoSets = db.QueryMultiple("select 1 as a, 2 as b; select 3 as c, 4 as d;");
            int sets = 0;
            int[] counts = new int[2];
            foreach(var set in twoSets)
            {
                foreach(var item in set)
                {
                    counts[sets]++;
                    if(sets == 0) Assert.AreEqual(typeof(int), item.b.GetType());
                    else Assert.AreEqual(typeof(int), item.c.GetType());
                }
                sets++;
            }
            Assert.AreEqual(2, sets);
            Assert.AreEqual(1, counts[0]);
            Assert.AreEqual(1, counts[1]);
        }

        [Test]
        public void DefaultValueFromNullInputOutputParam()
        {
            var db = new SPTestsDatabase(ProviderName);
            // w := w + 2; v := w - 1; x := w + 1
            dynamic testResult = db.ExecuteProcedure("TestVars", ioParams: new { w = (int?)null }, outParams: new { v = 0, x = 0 });
            Assert.AreEqual(1, testResult.v);
            Assert.AreEqual(2, testResult.w);
            Assert.AreEqual(3, testResult.x);
        }

        [Test]
        public void ProvideValueToInputOutputParam()
        {
            var db = new SPTestsDatabase(ProviderName);
            // w := w + 2; v := w - 1; x := w + 1
            dynamic testResult = db.ExecuteProcedure("TestVars", ioParams: new { w = 2 }, outParams: new { v = 0, x = 0 });
            Assert.AreEqual(3, testResult.v);
            Assert.AreEqual(4, testResult.w);
            Assert.AreEqual(5, testResult.x);
        }

        /// <remarks>
        /// See comments on IsCursor() in Massive.SqlServer.cs
        /// </remarks>
        [Test]
        public void DereferenceCursor()
        {
            // There is probably no situation in which it would make sense to do this (a procedure returning a cursor should be for use by another
            // procedure only - if at all); the remarks above and the example immediately below document why this is the wrong thing to do.
            var db = new SPTestsDatabase(ProviderName);
            var SQL = "DECLARE @MyCursor CURSOR;\r\n" +
                      "EXEC dbo.uspCurrencyCursor @CurrencyCursor = @MyCursor OUTPUT;\r\n" +
                      "WHILE(@@FETCH_STATUS = 0)\r\n" +
                      "BEGIN;\r\n" +
                      "\tFETCH NEXT FROM @MyCursor;\r\n" +
                      "END;\r\n" +
                      "CLOSE @MyCursor;\r\n" +
                      "DEALLOCATE @MyCursor;\r\n";
            dynamic resultSets = db.QueryMultiple(SQL);
            int count = 0;
            foreach(var results in resultSets)
            {
                foreach(var item in results)
                {
                    count++;
                    Assert.AreEqual(typeof(string), item.CurrencyCode.GetType());
                    Assert.AreEqual(typeof(string), item.Name.GetType());
                }
            }
            Assert.AreEqual(105, count);

            // An example of the correct way to do it
            dynamic fastResults = db.QueryFromProcedure("uspCurrencySelect");
            int fastCount = 0;
            foreach(var item in fastResults)
            {
                fastCount++;
                Assert.AreEqual(typeof(string), item.CurrencyCode.GetType());
                Assert.AreEqual(typeof(string), item.Name.GetType());
            }
            Assert.AreEqual(105, fastCount);
        }

        [Test]
        public void ScalarFromProcedure()
        {
            var db = new SPTestsDatabase(ProviderName);
            var value = db.ScalarFromProcedure("uspCurrencySelect");
            Assert.AreEqual("AFA", value);
        }
    }
}
