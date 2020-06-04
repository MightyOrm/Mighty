#if !(NETCOREAPP || NETSTANDARD)
using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mighty.Generic.Tests.Oracle.TableClasses;
using NUnit.Framework;

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
    public class SPTests
    {
        private readonly string ProviderName;

        /// <summary>
        /// Initialise tests for given provider
        /// </summary>
        /// <param name="providerName">Provider name</param>
        public SPTests(string providerName)
        {
            ProviderName = providerName;
        }


        public static IEnumerable<object[]> ProviderNames = new[] {
            new object[] { "Oracle.ManagedDataAccess.Client" },
            new object[] { "Oracle.DataAccess.Client" }
        };


        [Test]
        public void NormalWhereCall()
        {
            // Check that things are up and running normally before trying the new stuff
            var db = new Departments(ProviderName);
            var rows = db.All(where: "LOC = :0", args: "Nowhere");
            Assert.AreEqual(9, rows.ToList().Count);
        }


        [Test]
        public void SingleRowFromTableValuedFunction()
        {
            var db = new Employees(ProviderName);
            var record = db.SingleFromQueryWithParams("SELECT * FROM table(GET_EMP(:p_EMPNO))", new { p_EMPNO = 7782 });
            Assert.AreEqual(7782, record.EMPNO);
            Assert.AreEqual("CLARK", record.ENAME);
        }


        [Test]
        public void DereferenceCursorValuedFunction()
        {
            var db = new Employees(ProviderName);
            // Oracle function one cursor return value
            var employees = db.QueryFromProcedure("get_dept_emps", inParams: new { p_DeptNo = 10 }, returnParams: new { v_rc = new Cursor() });
            int count = 0;
            foreach(var employee in employees)
            {
                MDebug.WriteLine(employee.EMPNO + " " + employee.ENAME);
                count++;
            }
            Assert.AreEqual(3, count);
        }


        [Test]
        public void DereferenceCursorOutputParameter()
        {
            var db = new Employees(ProviderName);
            // Oracle procedure one cursor output variables
            var moreEmployees = db.QueryFromProcedure("myproc", outParams: new { prc = new Cursor() });
            int count = 0;
            foreach(var employee in moreEmployees)
            {
                MDebug.WriteLine(employee.EMPNO + " " + employee.ENAME);
                count++;
            }
            Assert.AreEqual(14, count);
        }
    }
}
#endif