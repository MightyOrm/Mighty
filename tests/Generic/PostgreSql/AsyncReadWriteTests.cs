using System;
using System.Collections.Async;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mighty.Generic.Tests.PostgreSql.TableClasses;
using NUnit.Framework;

namespace Mighty.Generic.Tests.PostgreSql
{
    /// <summary>
    /// Specific tests for code which is specific to Postgresql. This means there are fewer tests than for SQL Server, as logic that's covered there already doesn't have to be
    /// retested again here, as the tests are meant to see whether a feature works. Tests are designed to touch the code in Massive.PostgreSql. 
    /// </summary>
    /// <remarks>Tests use the northwind DB clone for Postgresql. Writes are done on Product, reads on other tables. Tests are compiled against x64 as npgsql installs itself in 
    /// x64's machine.config file by default. Change if required for your setup. </remarks>
    [TestFixture]
    public class AsyncReadWriteTests
    {
        [Test]
        public async Task All_NoParameters()
        {
            var customers = new Customers();
            var allRows = await (await customers.AllAsync()).ToListAsync();
            Assert.AreEqual(91, allRows.Count);
            foreach(var c in allRows)
            {
                Console.WriteLine("{0} {1}", c.customerid, c.companyname);
            }
        }

        [Test]
        public async Task All_LimitSpecification()
        {
            var customers = new Customers();
            var allRows = await (await customers.AllAsync(limit: 10)).ToListAsync();
            Assert.AreEqual(10, allRows.Count);
        }


        [Test]
        public async Task All_WhereSpecification_OrderBySpecification()
        {
            var customers = new Customers();
            var allRows = await (await customers.AllAsync(orderBy: "companyname DESC", where: "WHERE country=:0", args: "USA")).ToListAsync();
            Assert.AreEqual(13, allRows.Count);
            string previous = string.Empty;
            foreach(var r in allRows)
            {
                string current = r.companyname;
                Assert.IsTrue(string.IsNullOrEmpty(previous) || string.Compare(previous, current) > 0);
                previous = current;
            }
        }


        [Test]
        public async Task All_WhereSpecification_OrderBySpecification_LimitSpecification()
        {
            var customers = new Customers();
            var allRows = await (await customers.AllAsync(limit: 6, orderBy: "companyname DESC", where: "WHERE country=:0", args: "USA")).ToListAsync();
            Assert.AreEqual(6, allRows.Count);
            string previous = string.Empty;
            foreach(var r in allRows)
            {
                string current = r.companyname;
                Assert.IsTrue(string.IsNullOrEmpty(previous) || string.Compare(previous, current) > 0);
                previous = current;
            }
        }


        [Test]
        public async Task Paged_NoSpecification()
        {
            var customers = new Customers();
            // no order by, and paged queries logically must have an order by; this will order on PK
            var page2 = await customers.PagedAsync(currentPage: 2, pageSize: 10);
            var pageItems = page2.Items.ToList();
            Assert.AreEqual(10, pageItems.Count);
            Assert.AreEqual(91, page2.TotalRecords);
        }


        [Test]
        public async Task Paged_WhereSpecification()
        {
            var customers = new Customers();
            var page3 = await customers.PagedAsync(currentPage: 3, where: "companyname LIKE :0", args: "%a%");
            var pageItems = page3.Items.ToList();
            Assert.AreEqual(20, pageItems.Count);
            Assert.AreEqual(72, page3.TotalRecords);
        }


        [Test]
        public async Task Paged_OrderBySpecification()
        {
            var customers = new Customers();
            var page2 = await customers.PagedAsync(orderBy: "companyname DESC", currentPage: 2, pageSize: 10);
            var pageItems = page2.Items.ToList();
            Assert.AreEqual(10, pageItems.Count);
            Assert.AreEqual(91, page2.TotalRecords);
        }


        [Test]
        public async Task Insert_SingleRow()
        {
            var products = new Products();
            var inserted = await products.InsertAsync(new { productname = "Massive Product" });
            Assert.IsTrue(inserted.productid > 0);
        }


        [OneTimeTearDown]
        public async Task CleanUp()
        {
            // delete all rows with ProductName 'Massive Product'. 
            var products = new Products();
            await products.DeleteAsync("productname=:0", "Massive Product");
        }
    }
}
