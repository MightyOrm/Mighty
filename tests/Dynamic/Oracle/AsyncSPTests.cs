#if (NETFRAMEWORK && !NET40) || (NETCOREAPP && !(NETCOREAPP1_0 || NETCOREAPP1_1))
#pragma warning disable IDE0079
#pragma warning disable IDE0063
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Dasync.Collections;

using NUnit.Framework;

using Mighty.Dynamic.Tests.Oracle.TableClasses;

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
            var db = new Department(ProviderName);
            var rows = await db.AllAsync(where: "LOC = :0", args: "Nowhere");
            Assert.AreEqual(9, (await rows.ToListAsync()).Count);
        }


        [Test]
        public async Task IntegerOutputParam()
        {
            var db = new SPTestsDatabase(ProviderName);
            dynamic intResult = await db.ExecuteWithParamsAsync("begin :a := 1; end;", outParams: new { a = 0 });
            Assert.AreEqual(1, intResult.a);
        }



        [Test]
        public async Task InitialNullDateOutputParam()
        {
            var db = new SPTestsDatabase(ProviderName);
            dynamic dateResult = await db.ExecuteWithParamsAsync("begin :d := SYSDATE; end;", outParams: new { d = (DateTime?)null });
            Assert.AreEqual(typeof(DateTime), dateResult.d.GetType());
        }


        [Test]
        public async Task InputAndOutputParams()
        {
            var db = new SPTestsDatabase(ProviderName);
            dynamic procResult = await db.ExecuteProcedureAsync("findMin", inParams: new { x = 1, y = 3 }, outParams: new { z = 0 });
            Assert.AreEqual(1, procResult.z);
        }


        [Test]
        public async Task InputAndReturnParams()
        {
            var db = new SPTestsDatabase(ProviderName);
            dynamic fnResult = await db.ExecuteProcedureAsync("findMax", inParams: new { x = 1, y = 3 }, returnParams: new { returnValue = 0 });
            Assert.AreEqual(3, fnResult.returnValue);
        }


        [Test]
        public async Task InputOutputParam()
        {
            var db = new SPTestsDatabase(ProviderName);
            dynamic squareResult = await db.ExecuteProcedureAsync("squareNum", ioParams: new { x = 4 });
            Assert.AreEqual(16, squareResult.x);
        }


        [Test]
        public async Task InitialNullInputOutputParam()
        {
            var db = new SPTestsDatabase(ProviderName);
            dynamic squareResult = await db.ExecuteProcedureAsync("squareNum", ioParams: new { x = (int?)null });
            Assert.AreEqual(null, squareResult.x);
        }


        [Test]
        public async Task SingleRowFromTableValuedFunction()
        {
            var db = new SPTestsDatabase(ProviderName);
            var record = await db.SingleFromQueryWithParamsAsync("SELECT * FROM table(GET_EMP(:p_EMPNO))", new { p_EMPNO = 7782 });
            Assert.AreEqual(7782, record.EMPNO);
            Assert.AreEqual("CLARK", record.ENAME);
        }


        [Test]
        public async Task DereferenceCursorValuedFunction()
        {
            var db = new SPTestsDatabase(ProviderName);
            // Oracle function one cursor return value
            var employees = await db.QueryFromProcedureAsync("get_dept_emps", inParams: new { p_DeptNo = 10 }, returnParams: new { v_rc = new Cursor() });
            int count = 0;
            await employees.ForEachAsync(employee => {
                MDebug.WriteLine(employee.EMPNO + " " + employee.ENAME);
                count++;
            });
            Assert.AreEqual(3, count);
        }


        [Test]
        public async Task DereferenceCursorOutputParameter()
        {
            var db = new SPTestsDatabase(ProviderName);
            // Oracle procedure one cursor output variables
            var moreEmployees = await db.QueryFromProcedureAsync("myproc", outParams: new { prc = new Cursor() });
            int count = 0;
            await moreEmployees.ForEachAsync(employee => {
                MDebug.WriteLine(employee.EMPNO + " " + employee.ENAME);
                count++;
            });
            Assert.AreEqual(14, count);
        }


        [Test]
        public async Task QueryMultipleFromTwoOutputCursors()
        {
            var db = new SPTestsDatabase(ProviderName);
            // Oracle procedure two cursor output variables
            var twoSets = await db.QueryMultipleFromProcedureAsync("tworesults", outParams: new { prc1 = new Cursor(), prc2 = new Cursor() });
            int sets = 0;
            int[] counts = new int[2];
            await twoSets.ForEachAsync(async set => {
                await set.ForEachAsync(item => {
                    counts[sets]++;
                    if (sets == 0) Assert.AreEqual(typeof(string), item.ENAME.GetType());
                    else Assert.AreEqual(typeof(string), item.DNAME.GetType());
                });
                sets++;
            });
            Assert.AreEqual(2, sets);
            Assert.AreEqual(14, counts[0]);
            Assert.AreEqual(60, counts[1]);
        }


        [Test]
        public async Task NonQueryWithTwoOutputCursors()
        {
            var db = new SPTestsDatabase(ProviderName);
            var twoSetDirect = await db.ExecuteProcedureAsync("tworesults", outParams: new { prc1 = new Cursor(), prc2 = new Cursor() });
            Assert.AreEqual(typeof(Cursor), twoSetDirect.prc1.GetType());
            Assert.AreEqual("OracleRefCursor", ((Cursor)twoSetDirect.prc1).CursorRef.GetType().Name);
            Assert.AreEqual(typeof(Cursor), twoSetDirect.prc2.GetType());
            Assert.AreEqual("OracleRefCursor", ((Cursor)twoSetDirect.prc2).CursorRef.GetType().Name);
        }


        [Test]
        public async Task QueryFromMixedCursorOutput()
        {
            var db = new SPTestsDatabase(ProviderName);
            var mixedSets = await db.QueryMultipleFromProcedureAsync("mixedresults", outParams: new { prc1 = new Cursor(), prc2 = new Cursor(), num1 = 0, num2 = 0 });
            int sets = 0;
            int[] counts = new int[2];
            await mixedSets.ForEachAsync(async set => {
                await set.ForEachAsync(item => {
                    counts[sets]++;
                    if (sets == 0) Assert.AreEqual(typeof(string), item.ENAME.GetType());
                    else Assert.AreEqual(typeof(string), item.DNAME.GetType());
                });
                sets++;
            });
            Assert.AreEqual(2, sets);
            Assert.AreEqual(14, counts[0]);
            Assert.AreEqual(60, counts[1]);
        }


        [Test]
        public async Task NonQueryFromMixedCursorOutput()
        {
            var db = new SPTestsDatabase(ProviderName);
            var mixedDirect = await db.ExecuteProcedureAsync("mixedresults", outParams: new { prc1 = new Cursor(), prc2 = new Cursor(), num1 = 0, num2 = 0 });
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
        public async Task PassingCursorInputParameter(bool explicitConnection)
        {
            var db = new SPTestsDatabase(ProviderName, explicitConnection);
            if (explicitConnection)
            {
                MightyTests.ConnectionStringUtils.CheckConnectionStringRequiredForOpenConnectionAsync(db);
            }
            // To share cursors between commands in Oracle the commands must use the same connection
            using (var conn = await db.OpenConnectionAsync(
                explicitConnection ?
                    MightyTests.ConnectionStringUtils.GetConnectionString(TestConstants.ReadWriteTestConnection, ProviderName) :
                    null))
            {
                var res1 = await db.ExecuteWithParamsAsync("begin open :p_rc for select * from emp where deptno = 10; end;", outParams: new { p_rc = new Cursor() }, connection: conn);
                Assert.AreEqual(typeof(Cursor), res1.p_rc.GetType());
                Assert.AreEqual("OracleRefCursor", ((Cursor)res1.p_rc).CursorRef.GetType().Name);

                await db.ExecuteAsync("delete from processing_result", connection: conn);

                // oracle demo code takes the input cursor and writes the results to `processing_result` table
                var res2 = await db.ExecuteProcedureAsync("cursor_in_out.process_cursor", inParams: new { p_cursor = res1.p_rc }, connection: conn);
                Assert.AreEqual(0, ((IDictionary<string, object>)res2).Count);

                var processedRows = await (await db.QueryAsync("select * from processing_result", connection: conn)).ToListAsync();
                Assert.AreEqual(3, processedRows.Count);
            }
        }
    }
}
#endif
