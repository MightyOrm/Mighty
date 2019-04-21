#if !(NETCOREAPP || NETSTANDARD)
using System;
using System.Data;
using System.Collections.Async;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mighty.Generic.Tests.Oracle.TableClasses;
using NUnit.Framework;
using System.Threading.Tasks;

namespace Mighty.Generic.Tests.Oracle
{
    /// <summary>
    /// Suite of tests for stored procedures, functions and cursors on Oracle database.
    /// </summary>
    /// <remarks>
    /// Runs against functions and procedures which are created by running SPTests.sql script on the test database.
    /// These objects do not conflict with anything in the SCOTT database, and can be added there.
    /// </remarks>
    [TestFixture("Oracle.ManagedDataAccess.Client")]
    [TestFixture("Oracle.DataAccess.Client")]
    public class AsyncSPTests
    {
        private readonly string ProviderName;

        /// <summary>
        /// Initialise tests for given provider
        /// </summary>
        /// <param name="providerName">Provider name</param>
        public AsyncSPTests(string providerName)
        {
            ProviderName = providerName;
        }


        public static IEnumerable<object[]> ProviderNames = new[] {
            new object[] { "Oracle.ManagedDataAccess.Client" },
            new object[] { "Oracle.DataAccess.Client" }
        };


        [Test]
        public async Task NormalWhereCall()
        {
            // Check that things are up and running normally before trying the new stuff
            var db = new Departments(ProviderName);
            var rows = await db.AllAsync(where: "LOC = :0", args: "Nowhere");
            Assert.AreEqual(9, (await rows.ToListAsync()).Count);
        }


        [Test]
        public async Task SingleRowFromTableValuedFunction()
        {
            var db = new Employees(ProviderName);
            var record = await db.SingleFromQueryWithParamsAsync("SELECT * FROM table(GET_EMP(:p_EMPNO))", new { p_EMPNO = 7782 });
            Assert.AreEqual(7782, record.EMPNO);
            Assert.AreEqual("CLARK", record.ENAME);
        }


        [Test]
        public async Task DereferenceCursorValuedFunction()
        {
            var db = new Employees(ProviderName);
            // Oracle function one cursor return value
            var employees = await db.QueryFromProcedureAsync("get_dept_emps", inParams: new { p_DeptNo = 10 }, returnParams: new { v_rc = new Cursor() });
            int count = 0;
            await employees.ForEachAsync(employee => {
                Console.WriteLine(employee.EMPNO + " " + employee.ENAME);
                count++;
            });
            Assert.AreEqual(3, count);
        }


        [Test]
        public async Task DereferenceCursorOutputParameter()
        {
            var db = new Employees(ProviderName);
            // Oracle procedure one cursor output variables
            var moreEmployees = await db.QueryFromProcedureAsync("myproc", outParams: new { prc = new Cursor() });
            int count = 0;
            await moreEmployees.ForEachAsync(employee => {
                Console.WriteLine(employee.EMPNO + " " + employee.ENAME);
                count++;
            });
            Assert.AreEqual(14, count);
        }
    }
}
#endif