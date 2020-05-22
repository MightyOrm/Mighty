#if !NET40 && !NETCOREAPP2_0 // TO DO: Why is AsyncEnumerator ForEachAsync not available in .NET Core 2.0 (when it is in 1.0, 1.1 and 3.0)?
using System;
using System.Data;
using System.Dynamic;
using Dasync.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if !NETCOREAPP
using System.Transactions;
#endif
using Mighty.Dynamic.Tests.PostgreSql.TableClasses;
using NUnit.Framework;
using System.Threading.Tasks;
using System.Threading;

namespace Mighty.Dynamic.Tests.PostgreSql
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
        public async Task InitialNullIntegerOutputParam()
        {
            var db = new SPTestsDatabase();
            // NB This is PostgreSql specific; Npgsql completely ignores the output parameter type and sets it (sensibly) from the return type.
            dynamic z = new ExpandoObject();
            z.z = null;
            dynamic procResult = await db.ExecuteProcedureAsync("find_min", inParams: new { x = 5, y = 3 }, outParams: z);
            Assert.AreEqual(typeof(int), procResult.z.GetType());
            Assert.AreEqual(3, procResult.z);
        }

        [Test]
        public async Task IntegerReturnParam()
        {
            var db = new SPTestsDatabase();
            // NB Massive is converting all Postgres return params to output params because Npgsql treats all function
            // output and return as output (which is because PostgreSQL itself treats them as the same, really).
            dynamic fnResult = await db.ExecuteProcedureAsync("find_max", inParams: new { x = 6, y = 7 }, returnParams: new { returnValue = true });
            Assert.AreEqual(7, fnResult.returnValue);
        }

        [Test]
        public async Task PostgresAnonymousParametersA()
        {
            var db = new SPTestsDatabase();
            // Only PostgreSQL supports anonymous parameters (AFAIK) - we treat object[] in the context of params differently from
            // how it is treated when it appears in args in the standard Massive API, to provide support for this. (Note, object[]
            // makes no sense in the context of named parameters otherwise, and will throw an exception on the other DBs.)
            dynamic fnResultAnon = await db.ExecuteProcedureAsync("find_max", inParams: new object[] { 12, 7 }, returnParams: new { returnValue = 0 });
            Assert.AreEqual(12, fnResultAnon.returnValue);
        }

        [Test]
        public async Task PostgresAnonymousParametersB()
        {
            var db = new SPTestsDatabase();
            // NB This function can't be called except with anonymous parameters.
            // (I believe you can't even do it with a SQL block, because Postgres anonymous SQL blocks do not accept parameters? May be wrong...)
            dynamic addResult = await db.ExecuteProcedureAsync("add_em", inParams: new object[] { 4, 2 }, returnParams: new { RETURN = 0 });
            Assert.AreEqual(6, addResult.RETURN);
        }

        [Test]
        public async Task InputOutputParam()
        {
            var db = new SPTestsDatabase();
            dynamic squareResult = await db.ExecuteProcedureAsync("square_num", ioParams: new { x = 4 });
            Assert.AreEqual(16, squareResult.x);
        }

        [Test]
        public async Task InitialNullInputOutputParam()
        {
            var db = new SPTestsDatabase();
            dynamic xParam = new ExpandoObject();
            xParam.x = null;
            dynamic squareResult = await db.ExecuteProcedureAsync("square_num", ioParams: xParam);
            Assert.AreEqual(null, squareResult.x);
        }

        [Test]
        public async Task InitialNullDateReturnParamMethod1()
        {
            var db = new SPTestsDatabase();
            // This method will work on any provider
            dynamic dateResult = await db.ExecuteProcedureAsync("get_date", returnParams: new { d = (DateTime?)null });
            Assert.AreEqual(typeof(DateTime), dateResult.d.GetType());
        }

        [Test]
        public async Task InitialNullDateReturnParamMethod2()
        {
            var db = new SPTestsDatabase();
            // NB This is PostgreSql specific; Npgsql completely ignores the output parameter type and sets it (sensibly) from the return type.
            dynamic dParam = new ExpandoObject();
            dParam.d = null;
            dynamic dateResult = await db.ExecuteProcedureAsync("get_date", returnParams: dParam);
            Assert.AreEqual(typeof(DateTime), dateResult.d.GetType());
        }

        [Test]
        public async Task InitialNullDateReturnParamMethod3()
        {
            var db = new SPTestsDatabase();
            // Look - it REALLY ignores the parameter type. This would not work on other ADO.NET providers.
            // (Look at per-DB method: `private static bool IgnoresOutputTypes(this DbParameter p);`)
            dynamic dParam = new ExpandoObject();
            dParam.d = false;
            dynamic dateResult = await db.ExecuteProcedureAsync("get_date", returnParams: dParam);
            Assert.AreEqual(typeof(DateTime), dateResult.d.GetType());
        }

        [Test]
        public async Task DefaultValueFromNullInputOutputParam_Npgsql()
        {
            var db = new SPTestsDatabase();
            // the two lines create a null w param with a no type; on most DB providers this only works
            // for input params, where a null is a null is a null, but not on output params, where
            // we need to know what type the output var should be; but some providers plain ignore
            // the output type - in which case we do not insist that the user provide one
            dynamic wArgs = new ExpandoObject();
            wArgs.w = null;
            // w := w + 2; v := w - 1; x := w + 1
            dynamic testResult = await db.ExecuteProcedureAsync("test_vars", ioParams: wArgs, outParams: new { v = 0, x = 0 });
            Assert.AreEqual(1, testResult.v);
            Assert.AreEqual(2, testResult.w);
            Assert.AreEqual(3, testResult.x);
        }

        [Test]
        public async Task DefaultValueFromNullInputOutputParam_CrossDb()
        {
            // This is the cross-DB compatible way to do it.
            var db = new SPTestsDatabase();
            // w := w + 2; v := w - 1; x := w + 1
            dynamic testResult = await db.ExecuteProcedureAsync("test_vars", ioParams: new { w = (int?)null }, outParams: new { v = 0, x = 0 });
            Assert.AreEqual(1, testResult.v);
            Assert.AreEqual(2, testResult.w);
            Assert.AreEqual(3, testResult.x);
        }

        [Test]
        public async Task ProvideValueToInputOutputParam()
        {
            var db = new SPTestsDatabase();
            // w := w + 2; v := w - 1; x := w + 1
            dynamic testResult = await db.ExecuteProcedureAsync("test_vars", ioParams: new { w = 2 }, outParams: new { v = 0, x = 0 });
            Assert.AreEqual(3, testResult.v);
            Assert.AreEqual(4, testResult.w);
            Assert.AreEqual(5, testResult.x);
        }

        [Test]
        public async Task ReadOutputParamsUsingQuery()
        {
            var db = new SPTestsDatabase();
            // Again this is Postgres specific: output params are really part of data row and can be read that way
            var record = await db.SingleFromProcedureAsync("test_vars", new { w = 2 });
            Assert.AreEqual(3, record.v);
            Assert.AreEqual(4, record.w);
            Assert.AreEqual(5, record.x);
        }

        [Test]
        public async Task QuerySetOfRecordsFromFunction()
        {
            var db = new SPTestsDatabase();
            var setOfRecords = await db.QueryFromProcedureAsync("sum_n_product_with_tab", new { x = 10 });
            int count = 0;
            await setOfRecords.ForEachAsync(innerRecord => {
                Console.WriteLine(innerRecord.sum + "\t|\t" + innerRecord.product);
                count++;
            });
            Assert.AreEqual(4, count);
        }

#region Dereferencing tests
        [Test]
        public async Task DereferenceCursorOutputParameter()
        {
            var db = new SPTestsDatabase();
            // Unlike the Oracle data access layer, Npgsql v3 does not dereference cursor parameters.
            // We have added back the support for this which was previously in Npgsql v2.
            var employees = await db.QueryFromProcedureAsync("cursor_employees", outParams: new { refcursor = new Cursor() });
            int count = 0;
            await employees.ForEachAsync(employee => {
                Console.WriteLine(employee.firstname + " " + employee.lastname);
                count++;
            });
            Assert.AreEqual(9, count);
        }


        [Test]
        public async Task Dereferencing_RespondsToCancellation()
        {
            using (CancellationTokenSource cts = new CancellationTokenSource())
            {
                var db = new SPTestsDatabase();
                // Unlike the Oracle data access layer, Npgsql v3 does not dereference cursor parameters.
                // We have added back the support for this which was previously in Npgsql v2.
                var employees = await db.QueryFromProcedureAsync(cts.Token, "cursor_employees", outParams: new { refcursor = new Cursor() });
                int count = 0;
                Assert.ThrowsAsync<TaskCanceledException>(async () =>
                {
                    await employees.ForEachAsync(employee => {
                        Console.WriteLine(employee.firstname + " " + employee.lastname);
                        count++;
                        if (count == 5)
                        {
                            cts.Cancel();
                        }
                    });
                });
                Assert.AreEqual(5, count);
            }
        }


        [Test]
        public async Task DereferenceFromQuery_ManualWrapping()
        {
            var db = new SPTestsDatabase();
            // without a cursor param, nothing will trigger the wrapping transaction support in Massive
            // so in this case we need to add the wrapping transaction manually (with TransactionScope or
            // BeginTransaction, see other examples in this file)
            int count = 0;
            using (var conn = await db.OpenConnectionAsync())
            {
                using (var trans = conn.BeginTransaction())
                {
                    var employees = await db.QueryAsync("SELECT * FROM cursor_employees()", conn);
                    await employees.ForEachAsync(employee =>
                    {
                        Console.WriteLine(employee.firstname + " " + employee.lastname);
                        count++;
                    });
                    //scope.Complete();
                }
            }
            Assert.AreEqual(9, count);
        }


        [Test]
        public async Task DereferenceFromQuery_AutoWrapping()
        {
            var db = new SPTestsDatabase();
            // use dummy cursor to trigger wrapping transaction support in Massive
            // TO DO: document this (the idea is that this requires a wrapping transaction, but Mighty will do this for us if we do this;
            // still to remind exactly why and document)
            var employees = await db.QueryWithParamsAsync("SELECT * FROM cursor_employees()", outParams: new { abc = new Cursor() });
            int count = 0;
            await employees.ForEachAsync(employee => {
                Console.WriteLine(employee.firstname + " " + employee.lastname);
                count++;
            });
            Assert.AreEqual(9, count);
        }

        // Test various dereferencing patters (more relevant since we are coding this ourselves)
        private async Task CheckMultiResultSetStructureAsync(IAsyncEnumerable<IAsyncEnumerable<dynamic>> results, int count0 = 1, int count1 = 1, bool breakTest = false, bool idTest = false)
        {
            int sets = 0;
            int[] counts = new int[2];

            await results.ForEachAsync(async set => {
                try
                {
                    await set.ForEachAsync(item => {
                        counts[sets]++;
                        if (idTest) Assert.AreEqual(typeof(int), item.id.GetType());
                        else if (sets == 0) Assert.AreEqual(typeof(int), item.a.GetType());
                        else Assert.AreEqual(typeof(int), item.c.GetType());
                        if (breakTest) throw new AsyncEnumerationCanceledException();
                    });
                }
                catch (AsyncEnumerationCanceledException)
                {
                }
                sets++;
            });

            Assert.AreEqual(2, sets);
            Assert.AreEqual(breakTest ? 1 : count0, counts[0]);
            Assert.AreEqual(breakTest ? 1 : count1, counts[1]);
        }

        [Test]
        public async Task DereferenceOneByNFromProcedure()
        {
            var db = new SPTestsDatabase();
            var resultSetOneByN = await db.QueryMultipleFromProcedureAsync("cursorOneByN", outParams: new { xyz = new Cursor() });
            await CheckMultiResultSetStructureAsync(resultSetOneByN);
        }

        [Test]
        public async Task DereferenceNByOneFromProcedure()
        {
            var db = new SPTestsDatabase();
            // For small result sets, this actually saves one round trip to the database
            //db.AutoDereferenceFetchSize = -1;
            var resultSetNByOne = await db.QueryMultipleFromProcedureAsync("cursorNByOne", outParams: new { c1 = new Cursor(), c2 = new Cursor() });
            await CheckMultiResultSetStructureAsync(resultSetNByOne);
        }

        [Test]
        public async Task DereferenceOneByNFromQuery()
        {
            var db = new SPTestsDatabase();
            var resultSetOneByNSQL = await db.QueryMultipleWithParamsAsync("SELECT * FROM cursorOneByN()",
                                                                outParams: new { anyname = new Cursor() });
            await CheckMultiResultSetStructureAsync(resultSetOneByNSQL);
        }

        [Test]
        public async Task DereferenceNByOneFromQuery()
        {
            var db = new SPTestsDatabase();
            var resultSetNByOneSQL = await db.QueryMultipleWithParamsAsync("SELECT * FROM cursorNByOne()",
                                                                outParams: new { anyname = new Cursor() });
            await CheckMultiResultSetStructureAsync(resultSetNByOneSQL);
        }

        [Test]
        public async Task QueryMultipleWithBreaks()
        {
            var db = new SPTestsDatabase();
            var resultCTestFull = await db.QueryMultipleFromProcedureAsync("cbreaktest", outParams: new { c1 = new Cursor(), c2 = new Cursor() });
            await CheckMultiResultSetStructureAsync(resultCTestFull, 10, 11);
            var resultCTestToBreak = await db.QueryMultipleFromProcedureAsync("cbreaktest", ioParams: new { c1 = new Cursor(), c2 = new Cursor() });
            await CheckMultiResultSetStructureAsync(resultCTestToBreak, breakTest: true);
        }
#endregion

        [Test]
        public async Task QueryFromMixedCursorOutput()
        {
            var db = new SPTestsDatabase();
            // Following the Oracle pattern this WILL dereference; so we need some more interesting result sets in there now.
            var firstItemCursorMix = await db.SingleFromProcedureAsync("cursor_mix", outParams: new { anyname = new Cursor(), othername = 0 });
            Assert.AreEqual(11, firstItemCursorMix.a);
            Assert.AreEqual(22, firstItemCursorMix.b);
        }

        [Test]
        public async Task NonQueryFromMixedCursorOutput()
        {
            var db = new SPTestsDatabase();
            // Following the Oracle pattern this will not dereference: we get a variable value and a cursor ref.
            var itemCursorMix = await db.ExecuteProcedureAsync("cursor_mix", outParams: new { anyname = new Cursor(), othername = 0 });
            Assert.AreEqual(42, itemCursorMix.othername);
            Assert.AreEqual(typeof(Cursor), itemCursorMix.anyname.GetType());
            Assert.AreEqual(typeof(string), ((Cursor)itemCursorMix.anyname).CursorRef.GetType()); // NB PostgreSql ref cursors return as string
        }

        [Test]
        public async Task InputCursors_BeginTransaction()
        {
            var db = new SPTestsDatabase();
            using (var conn = await db.OpenConnectionAsync())
            {
                // cursors in PostgreSQL must share a transaction (not just a connection, as in Oracle)
                using (var trans = conn.BeginTransaction())
                {
                    var cursors = await db.ExecuteProcedureAsync("cursorNByOne", outParams: new { c1 = new Cursor(), c2 = new Cursor() }, connection: conn);
                    var cursor1 = await db.QueryFromProcedureAsync("fetch_next_ints_from_cursor", new { mycursor = cursors.c1 }, connection: conn);
                    int count1 = 0;
                    await cursor1.ForEachAsync(item => {
                        Assert.AreEqual(11, item.myint1);
                        Assert.AreEqual(22, item.myint2);
                        count1++;
                    });
                    Assert.AreEqual(1, count1);
                    var cursor2 = await db.QueryFromProcedureAsync("fetch_next_ints_from_cursor", new { mycursor = cursors.c2 }, connection: conn);
                    int count2 = 0;
                    await cursor2.ForEachAsync(item => {
                        Assert.AreEqual(33, item.myint1);
                        Assert.AreEqual(44, item.myint2);
                        count2++;
                    });
                    Assert.AreEqual(1, count2);
                    trans.Commit();
                }
            }
        }

        [Test]
        public async Task InputCursors_TransactionScope()
        {
            var db = new SPTestsDatabase();

            // cursors in PostgreSQL must share a transaction (not just a connection, as in Oracle)
            using (var conn = await db.OpenConnectionAsync())
            {
                using (var trans = conn.BeginTransaction())
                {
                    var cursors = await db.ExecuteProcedureAsync("cursorNByOne", outParams: new { c1 = new Cursor(), c2 = new Cursor() }, connection: conn);
                    var cursor1 = await db.QueryFromProcedureAsync("fetch_next_ints_from_cursor", new { mycursor = cursors.c1 }, connection: conn);
                    int count1 = 0;
                    await cursor1.ForEachAsync(item =>
                    {
                        Assert.AreEqual(11, item.myint1);
                        Assert.AreEqual(22, item.myint2);
                        count1++;
                    });
                    Assert.AreEqual(1, count1);
                    var cursor2 = await db.QueryFromProcedureAsync("fetch_next_ints_from_cursor", new { mycursor = cursors.c2 }, connection: conn);
                    int count2 = 0;
                    await cursor2.ForEachAsync(item =>
                    {
                        Assert.AreEqual(33, item.myint1);
                        Assert.AreEqual(44, item.myint2);
                        count2++;
                    });
                    Assert.AreEqual(1, count2);
                    trans.Commit();
                }
            }
        }

        /// <summary>
        /// For NX1 cursors as above Execute can get the raw cursors.
        /// For 1XN as here we have to use Query with automatic dereferencing turned off.
        /// </summary>
        [Test]
        public async Task InputCursors_1XN()
        {
            var db = new SPTestsDatabase();
            db.NpgsqlAutoDereferenceCursors = false; // for this instance only

            // cursors in PostgreSQL must share a transaction (not just a connection, as in Oracle)
            using (var conn = await db.OpenConnectionAsync())
            {

                using (var trans = conn.BeginTransaction())
                {
                    // Including a cursor param is optional and makes no difference, because Npgsql/PostgreSQL is lax about such things
                    // and we don't need to hint to Massive to do anything special 
                    var cursors = await db.QueryFromProcedureAsync("cursorOneByN", connection: conn); //, outParams: new { abcdef = new Cursor() });
                    string[] cursor = new string[2];
                    int i = 0;
                    await cursors.ForEachAsync(item =>
                    {
                        cursor[i++] = item.cursoronebyn;
                    });
                    Assert.AreEqual(2, i);
                    var cursor1 = await db.QueryFromProcedureAsync("fetch_next_ints_from_cursor", new { mycursor = new Cursor(cursor[0]) }, connection: conn);
                    int count1 = 0;
                    await cursor1.ForEachAsync(item =>
                    {
                        Assert.AreEqual(1, item.myint1);
                        Assert.AreEqual(2, item.myint2);
                        count1++;
                    });
                    Assert.AreEqual(1, count1);
                    var cursor2 = await db.QueryFromProcedureAsync("fetch_next_ints_from_cursor", new { mycursor = new Cursor(cursor[1]) }, connection: conn);
                    int count2 = 0;
                    await cursor2.ForEachAsync(item =>
                    {
                        Assert.AreEqual(3, item.myint1);
                        Assert.AreEqual(4, item.myint2);
                        count2++;
                    });
                    Assert.AreEqual(1, count2);
                    trans.Commit();
                }
            }
        }


        // Temporarily commenting out large cursor tests
#if true
        readonly int LargeCursorSize = 1000000;

        /// <summary>
        /// Explicit dereferencing is more fiddly to do, but you still have the choice to do it (even when automatic dereferencing is on).
        /// </summary>
        [Test]
        public async Task LargeCursor_ExplicitFetch()
        {
            int FetchSize = 20000;
            int count = 0;
            int batchCount = 0;
            var db = new SPTestsDatabase();
            using (var conn = await db.OpenConnectionAsync())
            {
                // cursors in PostgreSQL must share a transaction (not just a connection, as in Oracle)
                using (var trans = conn.BeginTransaction())
                {
                    var result = await db.ExecuteProcedureAsync("lump", returnParams: new { cursor = new Cursor() }, connection: conn);
                    while(true)
                    {
                        var fetchTest = await db.QueryAsync($@"FETCH {FetchSize} FROM ""{result.cursor.CursorRef}""", connection: conn);
                        int subcount = 0;
                        await fetchTest.ForEachAsync(item => {
                            count++;
                            subcount++;
                            // there is no ORDER BY (it would not be sensible on such a huge data set) - this only sometimes works...
                            //Assert.AreEqual(count, item.id);
                        });
                        if(subcount == 0)
                        {
                            break;
                        }
                        batchCount++;
                    }
                    await db.ExecuteAsync($@"CLOSE ""{result.cursor.CursorRef}""", connection: conn);
                    trans.Commit();
                }
            }
            Assert.AreEqual((LargeCursorSize + FetchSize - 1) / FetchSize, batchCount);
            Assert.AreEqual(LargeCursorSize, count);
        }

        /// <remarks>
        /// Implicit dereferencing is much easier to do, but also safe even for large or huge cursors.
        /// PostgreSQL specific settings; the following are the defaults which should work fine for most situations:
        ///     db.AutoDereferenceCursors = true;
        ///     db.AutoDereferenceFetchSize = 10000;
        /// </remarks>
        [Test]
        public async Task LargeCursor_AutomaticDereferencing()
        {
            var db = new SPTestsDatabase();
            // Either of these will show big server-side buffers in PostrgeSQL logs (but will still pass)
            //db.AutoDereferenceFetchSize = -1; // FETCH ALL
            //db.AutoDereferenceFetchSize = 400000;
            var fetchTest = await db.QueryFromProcedureAsync("lump", returnParams: new { cursor = new Cursor() });
            int count = 0;
            await fetchTest.ForEachAsync(item => {
                count++;
                // there is no ORDER BY (it would not be sensible on such a huge data set) - this only sometimes works...
                //Assert.AreEqual(count, item.id);
            });
            Assert.AreEqual(LargeCursorSize, count);
        }

        [Test]
        public async Task LargeCursorX2_AutomaticDereferencing()
        {
            var db = new SPTestsDatabase();
            // Either of these will show big server-side buffers in PostrgeSQL logs (but will still pass)
            //db.AutoDereferenceFetchSize = -1; // FETCH ALL
            //db.AutoDereferenceFetchSize = 400000;
            var results = await db.QueryMultipleFromProcedureAsync("lump2", returnParams: new { cursor = new Cursor() });
            int rcount = 0;
            await results.ForEachAsync(async result => {
                rcount++;
                int count = 0;
                await result.ForEachAsync(item => {
                    count++;
                    // there is no ORDER BY (it would not be sensible on such a huge data set) - this only sometimes works...
                    //Assert.AreEqual(count, item.id);
                });
                Assert.AreEqual(LargeCursorSize, count);
            });
            Assert.AreEqual(2, rcount);
        }
#endif

#if false
        [Test]
        public void HugeCursorTest()
        {
            var db = new SPTestsDatabase();

            //// Huge cursor tests....
            var config = db.SingleFromQuery("SELECT current_setting('work_mem') work_mem, current_setting('log_temp_files') log_temp_files");

            //// huge data from SELECT *
            //var resultLargeSelectTest = await db.QueryWithParamsAsync("SELECT * FROM large");
            //foreach(var item in resultLargeSelectTest)
            //{
            //    int a = 1;
            //}

            //// huge data from (implicit) FETCH ALL
            //// AUTO-DEREFERENCE TWO HUGE, ONLY FETCH FROM ONE
            //var resultLargeProcTest = await db.QueryFromProcedureAsync("lump2", returnParams: new { abc = new Cursor() });
            //foreach (var item in resultLargeProcTest)
            //{
            //    Console.WriteLine(item.id);
            //    break;
            //}

            var results = await db.QueryMultipleFromProcedureAsync("lump2", returnParams: new { abc = new Cursor() });
            db.NpgsqlAutoDereferenceFetchSize = 4000000;
            await CheckMultiResultSetStructureAsync(results, 10000000, 10000000, true, true);

            // one item from cursor
            //using (var conn = db.OpenConnection())
            //{
            //    using (var trans = conn.BeginTransaction())
            //    {
            //        var result = db.ExecuteAsProcedure("lump2", returnParams: new { abc = new Cursor(), def = new Cursor() }, connection: conn);
            //        var singleItemTest = await db.QueryWithParamsAsync($@"FETCH 5000000 FROM ""{result.abc}"";", connection: conn);
            //        foreach (var item in singleItemTest)
            //        {
            //            Console.WriteLine(item.id);
            //            break;
            //        }
            //         NB plain Execute() did NOT take a connection, and changing this MIGHT be an API breaking change??? TEST...!
            //         (This is the first, and so far only, really unwanted side effect of trying to stay 100% non-breaking.)
            //        db.Execute($@"CLOSE ""{result.abc}"";", conn);
            //        trans.Commit();
            //    }
            //}
        }
#endif

// TO DO:
#if false
        public void ToDo()
        {
            var db = new SPTestsDatabase();

            // AFAIK these will never work (you can't assign to vars in SQL block)
            //dynamic intResult = db.Execute(":a := 1", inParams: new aArgs());
            //dynamic dateResult = db.Execute("begin :d := SYSDATE; end;", outParams: new myParamsD());
        }
#endif
    }
}
#endif