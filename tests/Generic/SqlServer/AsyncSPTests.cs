#if !NET40
using System;
using System.Data;
using System.Dynamic;
using Dasync.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mighty.Generic.Tests.SqlServer.TableClasses;
using NUnit.Framework;
using System.Threading.Tasks;

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
    public class AsyncSPTests
    {
        private readonly string ProviderName;

        public AsyncSPTests(string providerName)
        {
            ProviderName = providerName;
        }

        [Test]
        public async Task QueryFromStoredProcedure()
        {
            var db = new People(ProviderName);
            var people = await db.QueryFromProcedureAsync("uspGetEmployeeManagers", new { BusinessEntityID = 35 });
            int count = 0;
            await people.ForEachAsync(person => {
                Console.WriteLine(person.FirstName + " " + person.LastName);
                count++;
            });
            Assert.AreEqual(3, count);
        }

        [Test]
        public async Task SingleRowFromTableValuedFunction()
        {
            var db = new People(ProviderName);
            // Accessing table value functions on SQL Server (different syntax from Postgres, for example)
            var person = await db.SingleFromQueryWithParamsAsync("SELECT * FROM dbo.ufnGetContactInformation(@PersonID)", new { @PersonID = 35 });
            Assert.AreEqual(typeof(string), person.FirstName.GetType());
        }
    }
}
#endif