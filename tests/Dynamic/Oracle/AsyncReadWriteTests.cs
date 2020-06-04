#if (NETFRAMEWORK && !NET40) || (NETCOREAPP && !(NETCOREAPP1_0 || NETCOREAPP1_1))
using System;
using System.Data;
using Dasync.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mighty.Dynamic.Tests.Oracle.TableClasses;
using NUnit.Framework;
using System.Threading.Tasks;
using System.Data.Common;

namespace Mighty.Dynamic.Tests.Oracle
{
    /// <summary>
    /// Specific tests for code which is specific to Oracle. This means there are fewer tests than for SQL Server, as logic that's covered there already doesn't have to be
    /// retested again here, as the tests are meant to see whether a feature works. Tests are designed to touch the code in Massive.Oracle. 
    /// </summary>
    /// <remarks>These tests run on x64 by default, as by default ODP.NET installs x64 only. If you have x86 ODP.NET installed, change the build directive to AnyCPU
    /// in the project settings.<br/>
    /// These tests use the SCOTT test DB shipped by Oracle. Your values may vary though. </remarks>
    [TestFixture("Oracle.ManagedDataAccess.Client")]
#if !NETCOREAPP
    [TestFixture("Oracle.DataAccess.Client")]
#endif
    public class AsyncReadWriteTests
    {
        private readonly string ProviderName;

        /// <summary>
        /// Initialise tests for given provider
        /// </summary>
        /// <param name="providerName">Provider name</param>
        public AsyncReadWriteTests(string providerName)
        {
            ProviderName = providerName;
        }


        [Test]
        public async Task Guid_Arg()
        {
            // Oracle has no Guid parameter support, Massive maps Guid to string in Oracle
            var db = new MightyOrm(string.Format(TestConstants.ReadWriteTestConnection, ProviderName));
            var guid = Guid.NewGuid();
            var inParams = new { inval = guid };
            var outParams = new { val = new Guid() };
            dynamic item;
            using (var command = db.CreateCommandWithParams("begin :val := :inval; end;", inParams: inParams, outParams: outParams))
            {
                Assert.AreEqual(DbType.String, command.Parameters[0].DbType);
                await db.ExecuteAsync(command);
                item = db.ResultsAsExpando(command);
            }
            Assert.AreEqual(typeof(string), item.val.GetType());
            Assert.AreEqual(guid, new Guid(item.val));
        }


        [Test]
        public async Task All_NoParameters()
        {
            var depts = new Department(ProviderName);
            var allRows = await (await depts.AllAsync()).ToListAsync();
            Assert.AreEqual(60, allRows.Count);
            foreach(var d in allRows)
            {
                MDebug.WriteLine("{0} {1} {2}", d.DEPTNO, d.DNAME, d.LOC);
            }
        }


        [Test]
        public async Task All_LimitSpecification()
        {
            var depts = new Department(ProviderName);
            var allRows = await (await depts.AllAsync(limit: 10)).ToListAsync();
            Assert.AreEqual(10, allRows.Count);
        }


        [Test]
        public async Task All_WhereSpecification_OrderBySpecification()
        {
            var depts = new Department(ProviderName);
            var allRows = await (await depts.AllAsync(orderBy: "DEPTNO DESC", where: "WHERE LOC=:0", args: "Nowhere")).ToListAsync();
            Assert.AreEqual(9, allRows.Count);
            int previous = int.MaxValue;
            foreach(var r in allRows)
            {
                int current = r.DEPTNO;
                Assert.IsTrue(current <= previous);
                previous = current;
            }
        }


        [Test]
        public async Task All_WhereSpecification_OrderBySpecification_LimitSpecification()
        {
            var depts = new Department(ProviderName);
            var allRows = await (await depts.AllAsync(limit: 6, orderBy: "DEPTNO DESC", where: "WHERE LOC=:0", args: "Nowhere")).ToListAsync();
            Assert.AreEqual(6, allRows.Count);
            int previous = int.MaxValue;
            foreach(var r in allRows)
            {
                int current = r.DEPTNO;
                Assert.IsTrue(current <= previous);
                previous = current;
            }
        }


        [Test]
        public async Task Paged_NoSpecification()
        {
            var depts = new Department(ProviderName);
            // no order by, and paged queries logically must have an order by; this will order on PK
            var page2 = await depts.PagedAsync(currentPage: 2, pageSize: 10);
            Assert.AreEqual(10, page2.Items.Count);
            Assert.AreEqual(60, page2.TotalRecords);
            Assert.AreEqual(2, page2.CurrentPage);
            Assert.AreEqual(10, page2.PageSize);
        }


        [Test]
        public async Task Paged_WhereSpecification()
        {
            var depts = new Department(ProviderName);
            var page4 = await depts.PagedAsync(currentPage: 4, where: "LOC = :0", args: "Somewhere");
            var pageItems = ((IEnumerable<dynamic>)page4.Items).ToList();
            Assert.AreEqual(0, pageItems.Count); // also testing being after last page
            Assert.AreEqual(47, page4.TotalRecords);
        }


        [Test]
        public async Task Paged_WhereSpecification_WithParams()
        {
            var depts = new Department(ProviderName);
            var page4 = await depts.PagedWithParamsAsync(currentPage: 4, where: "LOC = :loc", inParams: new { loc = "Somewhere" });
            var pageItems = ((IEnumerable<dynamic>)page4.Items).ToList();
            Assert.AreEqual(0, pageItems.Count); // also testing being after last page
            Assert.AreEqual(47, page4.TotalRecords);
        }


        [Test]
        public async Task Paged_OrderBySpecification()
        {
            var depts = new Department(ProviderName);
            var page2 = await depts.PagedAsync(orderBy: "DEPTNO DESC", currentPage: 2, pageSize: 10);
            var pageItems = ((IEnumerable<dynamic>)page2.Items).ToList();
            Assert.AreEqual(10, pageItems.Count);
            Assert.AreEqual(60, page2.TotalRecords);
        }


        [Test]
        public async Task Paged_SqlSpecification()
        {
            var depts = new Department(ProviderName);
            var page2 = await depts.PagedFromSelectAsync("SCOTT.EMP e INNER JOIN SCOTT.DEPT d ON e.DEPTNO = d.DEPTNO", "EMPNO", "EMPNO, ENAME, DNAME", pageSize: 5, currentPage: 2);
            var pageItems = ((IEnumerable<dynamic>)page2.Items).ToList();
            Assert.AreEqual(5, pageItems.Count);
            Assert.AreEqual(14, page2.TotalRecords);
            Assert.AreEqual(3, page2.TotalPages);
        }

        [Test]
#pragma warning disable CS1998
        public async Task PagedStarWithJoin_ThrowsInvalidOperationException()
        {
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => {
                var depts = new Department(ProviderName);
                var page2 = await depts.PagedFromSelectAsync("SCOTT.EMP e INNER JOIN SCOTT.DEPT d ON e.DEPTNO = d.DEPTNO", "EMPNO", "*", pageSize: 2, currentPage: 2);
                var pageItems = ((IEnumerable<dynamic>)page2.Items).ToList();
            });
            // Check that it was thrown for the right reason
            Assert.AreEqual("To query from joined tables you have to specify the columns explicitly not with *", ex.Message);
        }
#pragma warning restore CS1998

        [Test]
#pragma warning disable CS1998
        public async Task PagedNoOrderBy_ThrowsInvalidOperationException()
        {
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => {
                var depts = new Department(ProviderName);
                var page2 = await depts.PagedFromSelectAsync("SCOTT.EMP e INNER JOIN SCOTT.DEPT d ON e.DEPTNO = d.DEPTNO", null, "EMPNO, ENAME, DNAME", pageSize: 2, currentPage: 2);
                var pageItems = ((IEnumerable<dynamic>)page2.Items).ToList();
            });
            // Check that it was thrown for the right reason
            Assert.AreEqual("Cannot complete paged select operation, you must provide an ORDER BY value", ex.Message);
        }
#pragma warning restore CS1998

        [Test]
        public async Task Insert_SingleRow()
        {
            var depts = new Department(ProviderName);
            var inserted = await depts.InsertAsync(new { DNAME = "Massive Dep", LOC = "Beach" });
            Assert.IsTrue(inserted.DEPTNO > 0);
            Assert.AreEqual(1, await depts.DeleteAsync(inserted.DEPTNO));
        }


        [Test]
        public async Task Save_SingleRow()
        {
            var depts = new Department(ProviderName);
            dynamic toSave = new { DNAME = "Massive Dep", LOC = "Beach" }.ToExpando();
            Assert.AreEqual(1, await depts.SaveAsync(toSave));
            Assert.IsTrue(toSave.DEPTNO > 0);
            Assert.AreEqual(1, await depts.DeleteAsync(toSave.DEPTNO));
        }


        [Test]
#pragma warning disable CS1998
        public async Task Save_NoSequenceNoPk_ThrowsCannotInsertNull()
        {
            var depts = new MightyOrm(string.Format(TestConstants.ReadWriteTestConnection, ProviderName), "SCOTT.DEPT", "DEPTNO");
            dynamic toSave = new { DNAME = "Massive Dep", LOC = "Beach" }.ToExpando();
            var ex = Assert.CatchAsync<DbException>(async () => await depts.SaveAsync(toSave));
            Assert.True(ex.Message.Contains("cannot insert NULL"));
        }
#pragma warning restore CS1998


        [Test]
        public async Task Save_NoSequenceWithPk_CanInsert()
        {
            dynamic toSave = new { DNAME = "Massive Dep", LOC = "Beach" }.ToExpando();
            {
                var depts = new Department(ProviderName);
                var result = await depts.SaveAsync(toSave);
                Assert.AreEqual(1, result);
                Assert.IsTrue(toSave.DEPTNO > 0);
                Assert.AreEqual(1, await depts.DeleteAsync(toSave.DEPTNO));
            }
            {
                // re-insert at the previous, deleted therefore valid, PK value but without using sequence to generate it;
                // actually tests that Oracle can insert user-managed PKs with no sequence
                var depts = new MightyOrm(string.Format(TestConstants.ReadWriteTestConnection, ProviderName), "SCOTT.DEPT", "DEPTNO");
                int oldId = toSave.DEPTNO;
                var result = await depts.InsertAsync(toSave);
                Assert.AreEqual(oldId, result.DEPTNO);
                Assert.AreEqual(1, await depts.DeleteAsync(toSave.DEPTNO));
            }
        }


        [Test]
        public async Task Save_MultipleRows()
        {
            var depts = new Department(ProviderName);
            object[] toSave = new object[]
                                   {
                                       new {DNAME = "Massive Dep", LOC = "Beach"}.ToExpando(),
                                       new {DNAME = "Massive Dep", LOC = "DownTown"}.ToExpando()
                                   };
            var result = await depts.SaveAsync(toSave);
            Assert.AreEqual(2, result);
            foreach(dynamic o in toSave)
            {
                Assert.IsTrue(o.DEPTNO > 0);
            }

            // read them back, update them, save them again, 
            var savedDeps = await (await depts.AllAsync(where: "WHERE DEPTNO=:0 OR DEPTNO=:1", args: new object[] { ((dynamic)toSave[0]).DEPTNO, ((dynamic)toSave[1]).DEPTNO })).ToListAsync();
            Assert.AreEqual(2, savedDeps.Count);
            savedDeps[0].LOC += "C";
            savedDeps[1].LOC += "C";
            result = await depts.SaveAsync(toSave);
            Assert.AreEqual(2, result);
            Assert.AreEqual(1, await depts.DeleteAsync(savedDeps[0].DEPTNO));
            Assert.AreEqual(1, await depts.DeleteAsync(savedDeps[1].DEPTNO));
        }


        [OneTimeTearDown]
        public async Task CleanUp()
        {
            // delete all rows with department name 'Massive Dep'. 
            var depts = new Department(ProviderName);
            await depts.DeleteAsync("DNAME=:0", "Massive Dep");
        }
    }
}
#endif
