#if !NET40
using System;
using System.Data;
using System.Dynamic;
using System.Collections.Async;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if !(NETCOREAPP || NETSTANDARD)
using System.Transactions;
#endif
using Mighty.Generic.Tests.PostgreSql.TableClasses;
using NUnit.Framework;
using System.Threading.Tasks;

namespace Mighty.Generic.Tests.PostgreSql
{
    /// <summary>
    /// Suite of tests for stored procedures, functions and cursors on PostgreSQL database.
    /// </summary>
    /// <remarks>
    /// Runs against functions and procedures which are created by running SPTests.sql script on the test database.
    /// These objects do not conflict with anything in the Northwind database, and can be added there.
    /// </remarks>
    [TestFixture]
    public class AsyncSPTests
    {
        [Test]
        public async Task DereferenceCursorOutputParameter()
        {
            var db = new Employees();
            // Unlike the Oracle data access layer, Npgsql v3 does not dereference cursor parameters.
            // We have added back the support for this which was previously in Npgsql v2.
            var employees = await db.QueryFromProcedureAsync("cursor_employees", outParams: new { refcursor = new Cursor() });
            int count = 0;
            await employees.ForEachAsync(employee => {
                MDebug.WriteLine(employee.firstname + " " + employee.lastname);
                count++;
            });
            Assert.AreEqual(9, count);
        }


#if SYNC_ONLY && !(NETCOREAPP || NETSTANDARD)
        [Test]
        public async Task DereferenceFromQuery_ManualWrapping()
        {
            var db = new Employees();
            // without a cursor param, nothing will trigger the wrapping transaction support in Massive
            // so in this case we need to add the wrapping transaction manually (with TransactionScope or
            // BeginTransaction, see other examples in this file)
            int count = 0;
            using (var scope = new TransactionScope())
            {
                var employees = await db.QueryAsync("SELECT * FROM cursor_employees()");
                await employees.ForEachAsync(employee => {
                    MDebug.WriteLine(employee.firstname + " " + employee.lastname);
                    count++;
                });
                scope.Complete();
            }
            Assert.AreEqual(9, count);
        }
#endif

        [Test]
        public async Task DereferenceFromQuery_AutoWrapping()
        {
            var db = new Employees();
            // use dummy cursor to trigger wrapping transaction support in Massive
            var employees = await db.QueryWithParamsAsync("SELECT * FROM cursor_employees()", outParams: new { abc = new Cursor() });
            int count = 0;
            await employees.ForEachAsync(employee => {
                MDebug.WriteLine(employee.firstname + " " + employee.lastname);
                count++;
            });
            Assert.AreEqual(9, count);
        }
    }
}
#endif
