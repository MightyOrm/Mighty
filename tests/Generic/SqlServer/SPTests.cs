using System;
using System.Data;
using System.Dynamic;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mighty.Generic.Tests.SqlServer.TableClasses;
using NUnit.Framework;

namespace Mighty.Generic.Tests.SqlServer
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
        public void QueryFromStoredProcedure()
        {
            var db = new People();
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
            var db = new People();
            // Accessing table value functions on SQL Server (different syntax from Postgres, for example)
            var person = db.SingleFromQueryWithParams("SELECT * FROM dbo.ufnGetContactInformation(@PersonID)", new { @PersonID = 35 });
            Assert.AreEqual(typeof(string), person.FirstName.GetType());
        }

        public class myAb
        {
            public int a;
            public int b;
        }

        public class myCd
        {
            public int c;
            public int d;
        }

        [Test]
        public void QueryMultipleFromTwoResultSets_FullResultSetSupport()
        {
            var db = new MightyOrm(TestConstants.ReadTestConnection);
            int[] counts = new int[2];
            using (var twoSets = db.QueryMultiple("select 1 as a, 2 as b; select 3 as c, 4 as d;"))
            {
                twoSets.NextResultSet();
                foreach (var item in twoSets.CurrentResultSet.ResultsAs<myAb>())
                {
                    Assert.AreEqual(1, item.a);
                    Assert.AreEqual(2, item.b);
                    counts[0]++;
                }
                twoSets.NextResultSet();
                foreach (var item in twoSets.CurrentResultSet.ResultsAs<myCd>())
                {
                    Assert.AreEqual(3, item.c);
                    Assert.AreEqual(4, item.d);
                    counts[1]++;
                }
                Assert.False(twoSets.NextResultSet());
            }
            Assert.AreEqual(1, counts[0]);
            Assert.AreEqual(1, counts[1]);
        }

        [Test]
        public void QueryMultiple_OnlyFirstResultSet_DisposesCorrectly()
        {
            var db = new MightyOrm(TestConstants.ReadTestConnection);
            int[] counts = new int[2];
            MultipleResultSets<dynamic> twoSets;
            EnumerableResultSet<dynamic> set1;
            using (twoSets = db.QueryMultiple("select 1 as a, 2 as b; select 3 as c, 4 as d;"))
            {
                Assert.True(twoSets.NextResultSet());
                set1 = twoSets.CurrentResultSet;
                foreach (var item in set1.ResultsAs<myAb>())
                {
                    Assert.AreEqual(1, item.a);
                    Assert.AreEqual(2, item.b);
                    counts[0]++;
                }
            }
            Assert.True(twoSets.IsDisposed);
            Assert.AreEqual(1, counts[0]);
            Assert.AreEqual(0, counts[1]);
        }

        [Test]
        public void QueryMultiple_OnlyFirstRow_DisposesCorrectly()
        {
            var db = new MightyOrm(TestConstants.ReadTestConnection);
            int[] counts = new int[2];
            MultipleResultSets<dynamic> twoSets;
            EnumerableResultSet<dynamic> set1;
            using (twoSets = db.QueryMultiple("select 1 as a, 2 as b; select 3 as c, 4 as d;"))
            {
                Assert.True(twoSets.NextResultSet());
                set1 = twoSets.CurrentResultSet;
                foreach (var item in set1.ResultsAs<myAb>())
                {
                    Assert.AreEqual(1, item.a);
                    Assert.AreEqual(2, item.b);
                    counts[0]++;
                    break;
                }
            }
            Assert.True(twoSets.IsDisposed);
            Assert.AreEqual(1, counts[0]);
            Assert.AreEqual(0, counts[1]);
        }

        [Test]
        public void QueryMultipleFromTwoResultSets_SemiEnumerable()
        {
            var db = new MightyOrm(TestConstants.ReadTestConnection);
            var twoSets = db.QueryMultiple("select 1 as a, 2 as b; select 3 as c, 4 as d;");
            int sets = 0;
            int[] counts = new int[2];
            foreach (var set in twoSets)
            {
                if (sets == 0)
                {
                    foreach (var item in set.ResultsAs<myAb>())
                    {
                        Assert.AreEqual(1, item.a);
                        Assert.AreEqual(2, item.b);
                        counts[sets]++;
                    }
                }
                else
                {
                    foreach (var item in set.ResultsAs<myCd>())
                    {
                        Assert.AreEqual(3, item.c);
                        Assert.AreEqual(4, item.d);
                        counts[sets]++;
                    }
                }
                sets++;
            }
            Assert.AreEqual(2, sets);
            Assert.AreEqual(1, counts[0]);
            Assert.AreEqual(1, counts[1]);
        }
    }
}
