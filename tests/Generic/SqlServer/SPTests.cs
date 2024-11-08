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
        public void QueryFromStoredProcedure()
        {
            var db = new People(ProviderName);
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
            var db = new People(ProviderName);
            // Accessing table value functions on SQL Server (different syntax from Postgres, for example)
            var person = db.SingleFromQueryWithParams("SELECT * FROM dbo.ufnGetContactInformation(@PersonID)", new { @PersonID = 35 });
            Assert.AreEqual(typeof(string), person.FirstName.GetType());
        }
    }
}
