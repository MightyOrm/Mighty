using System;
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
    [TestFixture]
    public class SPTests
    {
        [Test]
        public void NormalSingleCall()
        {
            // Check that things are up and running normally before trying the new stuff
            var soh = new SalesOrderHeader();
            var item = soh.Single("SalesOrderID=@0", args: 43659);
            Assert.AreEqual("PO522145787", item.PurchaseOrderNumber);
        }

        [Test]
        public void InitialNullBooleanOutputParam()
        {
            var db = new SPTestsDatabase();
            dynamic boolResult = db.ExecuteWithParams("set @a = 1", outParams: new { a = (bool?)null });
            Assert.AreEqual(typeof(bool), boolResult.a.GetType());
        }

        [Test]
        public void InitialNullIntegerOutputParam()
        {
            var db = new SPTestsDatabase();
            dynamic intResult = db.ExecuteWithParams("set @a = 1", outParams: new { a = (int?)null });
            Assert.AreEqual(typeof(int), intResult.a.GetType());
        }

        [Test]
        public void QueryFromStoredProcedure()
        {
            var db = new SPTestsDatabase();
            var people = db.QueryFromProcedure("uspGetEmployeeManagers", new { BusinessEntityID = 35 });
            int count = 0;
            foreach(var person in people)
            {
                MDebug.WriteLine(person.FirstName + " " + person.LastName);
                count++;
            }
            Assert.AreEqual(3, count);
        }

        [Test]
        public void SingleRowFromTableValuedFunction()
        {
            var db = new SPTestsDatabase();
            // Accessing table value functions on SQL Server (different syntax from Postgres, for example)
            var person = db.SingleFromQueryWithParams("SELECT * FROM dbo.ufnGetContactInformation(@PersonID)", new { @PersonID = 35 });
            Assert.AreEqual(typeof(string), person.FirstName.GetType());
        }

        [Test]
        public void DateReturnParameter()
        {
            var db = new SPTestsDatabase();
            dynamic d = new ExpandoObject();
            d.d = true; // NB the type is ignored (by the underlying driver)
            var dResult = db.ExecuteProcedure("ufnGetAccountingEndDate", returnParams: d);
            Assert.AreEqual(typeof(DateTime), dResult.d.GetType());
        }

        [Test]
        public void QueryMultipleFromTwoResultSets()
        {
            var db = new SPTestsDatabase();
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
            var db = new SPTestsDatabase();
            // w := w + 2; v := w - 1; x := w + 1
            dynamic testResult = db.ExecuteProcedure("TestVars", ioParams: new { w = (int?)null }, outParams: new { v = 0, x = 0 });
            Assert.AreEqual(1, testResult.v);
            Assert.AreEqual(2, testResult.w);
            Assert.AreEqual(3, testResult.x);
        }

        [Test]
        public void ProvideValueToInputOutputParam()
        {
            var db = new SPTestsDatabase();
            // w := w + 2; v := w - 1; x := w + 1
            dynamic testResult = db.ExecuteProcedure("TestVars", ioParams: new { w = 2 }, outParams: new { v = 0, x = 0 });
            Assert.AreEqual(3, testResult.v);
            Assert.AreEqual(4, testResult.w);
            Assert.AreEqual(5, testResult.x);
        }

        [Test]
        public void DereferenceCursor()
        {
            // Split the results into multiple result sets for a test
            var db = new SPTestsDatabase();
            var SQL = @"DECLARE @MyCursor CURSOR;
EXEC dbo.uspCurrencyCursor @CurrencyCursor = @MyCursor OUTPUT;
WHILE(@@FETCH_STATUS = 0)
BEGIN;
    FETCH NEXT FROM @MyCursor;
END;
CLOSE @MyCursor;
DEALLOCATE @MyCursor;";
            int totalRows = 0;
            int totalResultSets = 0;
            using (var results = db.QueryMultiple(SQL))
            {
                while (results.NextResultSet())
                {
                    totalResultSets++;
                    int count = 0;
                    foreach (var item in results.CurrentResultSet)
                    {
                        Assert.AreEqual(typeof(string), item.CurrencyCode.GetType());
                        Assert.AreEqual(typeof(string), item.Name.GetType());
                        count++;
                        totalRows++;
                    }
                    // query really does return empty last resultset for some reason, it's not Mighty!
                    Assert.AreEqual(totalResultSets <= 105 ? 1 : 0, count);
                }
            }
            Assert.AreEqual(105, totalRows);
            Assert.AreEqual(106, totalResultSets);
        }

        [Test]
        public void ScalarFromProcedure()
        {
            var db = new SPTestsDatabase();
            var value = db.ScalarFromProcedure("uspCurrencySelect");
            Assert.AreEqual("AFA", value);
        }
    }
}
