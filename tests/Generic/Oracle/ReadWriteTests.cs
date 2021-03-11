#if NETFRAMEWORK || (NETCOREAPP && !(NETCOREAPP1_0 || NETCOREAPP1_1))
using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using Mighty.Generic.Tests.Oracle.TableClasses;
using NUnit.Framework;

namespace Mighty.Generic.Tests.Oracle
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
    public class ReadWriteTests
    {
        private readonly string ProviderName;

        /// <summary>
        /// Initialise tests for given provider
        /// </summary>
        /// <param name="providerName">Provider name</param>
        public ReadWriteTests(string providerName)
        {
            ProviderName = providerName;
        }


        [Test]
        public void All_NoParameters()
        {
            var depts = new Departments(ProviderName);
            var allRows = depts.All().ToList();
            Assert.AreEqual(60, allRows.Count);
            foreach(var d in allRows)
            {
                Console.WriteLine("{0} {1} {2}", d.DEPTNO, d.DNAME, d.LOC);
            }
        }


        [Test]
        public void All_LimitSpecification()
        {
            var depts = new Departments(ProviderName);
            var allRows = depts.All(limit: 10).ToList();
            Assert.AreEqual(10, allRows.Count);
        }


        [Test]
        public void All_WhereSpecification_OrderBySpecification()
        {
            var depts = new Departments(ProviderName);
            var allRows = depts.All(orderBy: "DEPTNO DESC", where: "WHERE LOC=:0", args: "Nowhere").ToList();
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
        public void All_WhereSpecification_OrderBySpecification_LimitSpecification()
        {
            var depts = new Departments(ProviderName);
            var allRows = depts.All(limit: 6, orderBy: "DEPTNO DESC", where: "WHERE LOC=:0", args: "Nowhere").ToList();
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
        public void Paged_NoSpecification()
        {
            var depts = new Departments(ProviderName);
            // no order by, and paged queries logically must have an order by; this will order on PK
            var page2 = depts.Paged(currentPage: 2, pageSize: 10);
            var pageItems = page2.Items.ToList();
            Assert.AreEqual(10, pageItems.Count);
            Assert.AreEqual(60, page2.TotalRecords);
        }


        [Test]
        public void Paged_WhereSpecification()
        {
            var depts = new Departments(ProviderName);
            var page4 = depts.Paged(currentPage: 4, where: "LOC = :0", args: "Somewhere");
            var pageItems = page4.Items.ToList();
            Assert.AreEqual(0, pageItems.Count); // also testing being after last page
            Assert.AreEqual(47, page4.TotalRecords);
        }


        [Test]
        public void Paged_OrderBySpecification()
        {
            var depts = new Departments(ProviderName);
            var page2 = depts.Paged(orderBy: "DEPTNO DESC", currentPage: 2, pageSize: 10);
            var pageItems = page2.Items.ToList();
            Assert.AreEqual(10, pageItems.Count);
            Assert.AreEqual(60, page2.TotalRecords);
        }


        [Test]
        public void Paged_SqlSpecification()
        {
            var depts = new Departments(ProviderName);
            var page2 = depts.PagedFromSelect("SCOTT.EMP e INNER JOIN SCOTT.DEPT d ON e.DEPTNO = d.DEPTNO", "EMPNO", "EMPNO, ENAME, DNAME", pageSize: 5, currentPage: 2);
            var pageItems = page2.Items.ToList();
            Assert.AreEqual(5, pageItems.Count);
            Assert.AreEqual(14, page2.TotalRecords);
            Assert.AreEqual(3, page2.TotalPages);
        }

        [Test]
        public void PagedStarWithJoin_ThrowsInvalidOperationException()
        {
            var ex = Assert.Throws<InvalidOperationException>(() => {
                var depts = new Departments(ProviderName);
                var page2 = depts.PagedFromSelect("SCOTT.EMP e INNER JOIN SCOTT.DEPT d ON e.DEPTNO = d.DEPTNO", "EMPNO", "*", pageSize: 2, currentPage: 2);
                var pageItems = page2.Items.ToList();
            });
            // Check that it was thrown for the right reason
            Assert.AreEqual("To query from joined tables you have to specify the columns explicitly not with *", ex.Message);
        }

        [Test]
        public void PagedNoOrderBy_ThrowsInvalidOperationException()
        {
            var ex = Assert.Throws<InvalidOperationException>(() => {
                var depts = new Departments(ProviderName);
                var page2 = depts.PagedFromSelect("SCOTT.EMP e INNER JOIN SCOTT.DEPT d ON e.DEPTNO = d.DEPTNO", null, "EMPNO, ENAME, DNAME", pageSize: 2, currentPage: 2);
                var pageItems = page2.Items.ToList();
            });
            // Check that it was thrown for the right reason
            Assert.AreEqual("Cannot complete paged select operation, you must provide an ORDER BY value", ex.Message);
        }

        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void Insert_SingleRow(bool explicitConnection)
        {
            var depts = new Departments(ProviderName, explicitConnection);
            DbConnection connection = null;
            if (explicitConnection)
            {
                MightyTests.ConnectionStringUtils.CheckConnectionStringRequiredForOpenConnection(depts);
                connection = depts.OpenConnection(MightyTests.ConnectionStringUtils.GetConnectionString(TestConstants.ReadWriteTestConnection, ProviderName));
            }
            using (connection)
            {
                var inserted = depts.Insert(new { DNAME = "Massive Dep", LOC = "Beach" }, connection: connection);
                Assert.IsTrue(inserted.DEPTNO > 0);
                Assert.AreEqual(1, depts.Delete(inserted.DEPTNO));
            }
        }


        [Test]
        public void Save_SingleRow()
        {
            var depts = new Departments(ProviderName);
            dynamic toSave = new { DNAME = "Massive Dep", LOC = "Beach" }.ToExpando();
            var result = depts.Save(toSave);
            Assert.AreEqual(1, result);
            Assert.IsTrue(toSave.DEPTNO > 0);
            Assert.AreEqual(1, depts.Delete(toSave.DEPTNO));
        }


        [Test]
        public void UpdateUsing_SingleRow()
        {
            var depts = new Departments(ProviderName);
            dynamic toSave = new { DNAME = "Massive Dep", LOC = "Beach" }.ToExpando();
            var saveResult = depts.Save(toSave);
            Assert.AreEqual(1, saveResult);
            Assert.IsTrue(toSave.DEPTNO > 0);
            var mightyDep = new { DNAME = "Mighty Dep" };

            // Clean down
            // TO DO: Copy to the others
            depts.Delete("DNAME = :0", mightyDep.DNAME);

            // The original test
            Assert.AreEqual(0, depts.All(mightyDep).ToList().Count);
            Assert.AreEqual(1, depts.UpdateUsing(mightyDep, toSave.DEPTNO));

            // Some new tests

            // For reasons unknown to science, this will not make this call
            //Assert.AreEqual(1, depts.UpdateUsing(mightyDep, "DEPTNO = :0", args: Convert.ToInt32(toSave.DEPTNO)));

            // works
            string dn = $"{toSave.DEPTNO}"; // used later too
            var thungy = Convert.ToInt32(dn);
            Assert.AreEqual(1, depts.UpdateUsing(mightyDep, "DEPTNO = :0", args: thungy));

            // This doesn't work either, it's because the conversion is from a field of a dynamic, so the var is an object not a basic type
            //var thingy = Convert.ToInt32(toSave.DEPTNO);
            //var type = $"{thingy.GetType()} {thungy.GetType()}";
            //Assert.AreEqual(1, depts.UpdateUsing(mightyDep, "DEPTNO = :0", args: thingy));

            // Check that these do work, also checks that the where args version is working
            Assert.AreEqual(1, depts.UpdateUsing(mightyDep, "DEPTNO = :0", args: new object[] { Convert.ToInt32(toSave.DEPTNO) }));
            Assert.AreEqual(1, depts.UpdateUsing(mightyDep, "DEPTNO = :0", Convert.ToInt32(toSave.DEPTNO)));

            Assert.AreEqual(1, depts.UpdateUsing(mightyDep, where: "DEPTNO = :0", args: dn));
            Assert.AreEqual(1, depts.UpdateUsing(mightyDep, "DEPTNO = :0", args: (int)toSave.DEPTNO));

            // Check
            Assert.AreEqual(1, depts.All(mightyDep).ToList().Count);

            // Clean up
            Assert.AreEqual(1, depts.Delete(toSave.DEPTNO));
            Assert.AreEqual(0, depts.All(mightyDep).ToList().Count);
        }


        [Test]
        public void Save_MultipleRows()
        {
            var depts = new Departments(ProviderName);
            object[] toSave = new object[]
                                   {
                                       new {DNAME = "Massive Dep", LOC = "Beach"}.ToExpando(),
                                       new {DNAME = "Massive Dep", LOC = "DownTown"}.ToExpando()
                                   };
            var result = depts.Save(toSave);
            Assert.AreEqual(2, result);
            foreach(dynamic o in toSave)
            {
                Assert.IsTrue(o.DEPTNO > 0);
            }

            // read them back, update them, save them again, 
            var savedDeps = depts.All(where: "WHERE DEPTNO=:0 OR DEPTNO=:1", args: new object[] { ((dynamic)toSave[0]).DEPTNO, ((dynamic)toSave[1]).DEPTNO }).ToList();
            Assert.AreEqual(2, savedDeps.Count);
            savedDeps[0].LOC += "C";
            savedDeps[1].LOC += "C";
            result = depts.Save(toSave);
            Assert.AreEqual(2, result);
            Assert.AreEqual(1, depts.Delete(savedDeps[0].DEPTNO));
            Assert.AreEqual(1, depts.Delete(savedDeps[1].DEPTNO));
        }


        [OneTimeTearDown]
        public void CleanUp()
        {
            // delete all rows with department name 'Massive Dep'. 
            var depts = new Departments(ProviderName);
            depts.Delete("DNAME=:0", "Massive Dep");
        }
    }
}
#endif