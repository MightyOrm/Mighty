#if NETFRAMEWORK || (NETCOREAPP && !(NETCOREAPP1_0 || NETCOREAPP1_1))
using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mighty.Dynamic.Tests.Oracle.TableClasses;
using NUnit.Framework;

namespace Mighty.Dynamic.Tests.Oracle
{
    /// <summary>
    /// Suite of tests for stored procedures, functions and cursors on Oracle database.
    /// </summary>
    /// <remarks>
    /// Runs against functions and procedures which are created by running SPTests.sql script on the test database.
    /// These objects do not conflict with anything in the SCOTT database, and can be added there.
    /// </remarks>
    [TestFixture("Oracle.ManagedDataAccess.Client")]
#if !NETCOREAPP
    [TestFixture("Oracle.DataAccess.Client")]
#endif
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
            var db = new Department(ProviderName);
            var rows = db.All(where: "LOC = :0", args: "Nowhere");
            Assert.AreEqual(9, rows.ToList().Count);
        }


        [Test]
        public void IntegerOutputParam()
        {
            var db = new SPTestsDatabase(ProviderName);
            dynamic intResult = db.ExecuteWithParams("begin :a := 1; end;", outParams: new { a = 0 });
            Assert.AreEqual(1, intResult.a);
        }



        [Test]
        public void InitialNullDateOutputParam()
        {
            var db = new SPTestsDatabase(ProviderName);
            dynamic dateResult = db.ExecuteWithParams("begin :d := SYSDATE; end;", outParams: new { d = (DateTime?)null });
            Assert.AreEqual(typeof(DateTime), dateResult.d.GetType());
        }


        [Test]
        public void InputAndOutputParams()
        {
            var db = new SPTestsDatabase(ProviderName);
            dynamic procResult = db.ExecuteProcedure("findMin", inParams: new { x = 1, y = 3 }, outParams: new { z = 0 });
            Assert.AreEqual(1, procResult.z);
        }


        [Test]
        public void InputAndReturnParams()
        {
            var db = new SPTestsDatabase(ProviderName);
            dynamic fnResult = db.ExecuteProcedure("findMax", inParams: new { x = 1, y = 3 }, returnParams: new { returnValue = 0 });
            Assert.AreEqual(3, fnResult.returnValue);
        }


        [Test]
        public void InputOutputParam()
        {
            var db = new SPTestsDatabase(ProviderName);
            dynamic squareResult = db.ExecuteProcedure("squareNum", ioParams: new { x = 4 });
            Assert.AreEqual(16, squareResult.x);
        }


        [Test]
        public void InitialNullInputOutputParam()
        {
            var db = new SPTestsDatabase(ProviderName);
            dynamic squareResult = db.ExecuteProcedure("squareNum", ioParams: new { x = (int?)null });
            Assert.AreEqual(null, squareResult.x);
        }


        [Test]
        public void SingleRowFromTableValuedFunction()
        {
            var db = new SPTestsDatabase(ProviderName);
            var record = db.SingleFromQueryWithParams("SELECT * FROM table(GET_EMP(:p_EMPNO))", new { p_EMPNO = 7782 });
            Assert.AreEqual(7782, record.EMPNO);
            Assert.AreEqual("CLARK", record.ENAME);
        }


        [Test]
        public void DereferenceCursorValuedFunction()
        {
            var db = new SPTestsDatabase(ProviderName);
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
            var db = new SPTestsDatabase(ProviderName);
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


        [Test]
        public void QueryMultipleFromTwoOutputCursors()
        {
            var db = new SPTestsDatabase(ProviderName);
            // Oracle procedure two cursor output variables
            var twoSets = db.QueryMultipleFromProcedure("tworesults", outParams: new { prc1 = new Cursor(), prc2 = new Cursor() });
            int sets = 0;
            int[] counts = new int[2];
            foreach(var set in twoSets)
            {
                foreach(var item in set)
                {
                    counts[sets]++;
                    if(sets == 0) Assert.AreEqual(typeof(string), item.ENAME.GetType());
                    else Assert.AreEqual(typeof(string), item.DNAME.GetType());
                }
                sets++;
            }
            Assert.AreEqual(2, sets);
            Assert.AreEqual(14, counts[0]);
            Assert.AreEqual(60, counts[1]);
        }


        [Test]
        public void NonQueryWithTwoOutputCursors()
        {
            var db = new SPTestsDatabase(ProviderName);
            var twoSetDirect = db.ExecuteProcedure("tworesults", outParams: new { prc1 = new Cursor(), prc2 = new Cursor() });
            Assert.AreEqual(typeof(Cursor), twoSetDirect.prc1.GetType());
            Assert.AreEqual("OracleRefCursor", ((Cursor)twoSetDirect.prc1).CursorRef.GetType().Name);
            Assert.AreEqual(typeof(Cursor), twoSetDirect.prc2.GetType());
            Assert.AreEqual("OracleRefCursor", ((Cursor)twoSetDirect.prc2).CursorRef.GetType().Name);
        }


        [Test]
        public void QueryFromMixedCursorOutput()
        {
            var db = new SPTestsDatabase(ProviderName);
            var mixedSets = db.QueryMultipleFromProcedure("mixedresults", outParams: new { prc1 = new Cursor(), prc2 = new Cursor(), num1 = 0, num2 = 0 });
            int sets = 0;
            int[] counts = new int[2];
            foreach(var set in mixedSets)
            {
                foreach(var item in set)
                {
                    counts[sets]++;
                    if(sets == 0) Assert.AreEqual(typeof(string), item.ENAME.GetType());
                    else Assert.AreEqual(typeof(string), item.DNAME.GetType());
                }
                sets++;
            }
            Assert.AreEqual(2, sets);
            Assert.AreEqual(14, counts[0]);
            Assert.AreEqual(60, counts[1]);
        }


        [Test]
        public void NonQueryFromMixedCursorOutput()
        {
            var db = new SPTestsDatabase(ProviderName);
            var mixedDirect = db.ExecuteProcedure("mixedresults", outParams: new { prc1 = new Cursor(), prc2 = new Cursor(), num1 = 0, num2 = 0 });
            Assert.AreEqual(typeof(Cursor), mixedDirect.prc1.GetType());
            Assert.AreEqual("OracleRefCursor", ((Cursor)mixedDirect.prc1).CursorRef.GetType().Name);
            Assert.AreEqual(typeof(Cursor), mixedDirect.prc2.GetType());
            Assert.AreEqual("OracleRefCursor", ((Cursor)mixedDirect.prc2).CursorRef.GetType().Name);
            Assert.AreEqual(1, mixedDirect.num1);
            Assert.AreEqual(2, mixedDirect.num2);
        }


        /// <remarks>
        /// This is based on Oracle demo code: https://blogs.oracle.com/oraclemagazine/cursor-in-cursor-out
        /// </remarks>
        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void PassingCursorInputParameter(bool explicitConnection)
        {
            var db = new SPTestsDatabase(ProviderName, explicitConnection);
            if (explicitConnection)
            {
                MightyTests.ConnectionStringUtils.CheckConnectionStringRequiredForOpenConnection(db);
            }
            // To share cursors between commands in Oracle the commands must use the same connection
            using(var conn = db.OpenConnection(
                explicitConnection ?
                    MightyTests.ConnectionStringUtils.GetConnectionString(TestConstants.ReadWriteTestConnection, ProviderName) :
                    null
                ))
            {
                var res1 = db.ExecuteWithParams("begin open :p_rc for select * from emp where deptno = 10; end;", outParams: new { p_rc = new Cursor() }, connection: conn);
                Assert.AreEqual(typeof(Cursor), res1.p_rc.GetType());
                Assert.AreEqual("OracleRefCursor", ((Cursor)res1.p_rc).CursorRef.GetType().Name);

                db.Execute("delete from processing_result", connection: conn);

                // oracle demo code takes the input cursor and writes the results to `processing_result` table
                var res2 = db.ExecuteProcedure("cursor_in_out.process_cursor", inParams: new { p_cursor = res1.p_rc }, connection: conn);
                Assert.AreEqual(0, ((IDictionary<string, object>)res2).Count);

                var processedRows = db.Query("select * from processing_result", connection: conn).ToList();
                Assert.AreEqual(3, processedRows.Count);
            }
        }
    }
}
#endif
