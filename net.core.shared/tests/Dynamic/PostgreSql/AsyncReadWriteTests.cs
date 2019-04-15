using System;
using System.Collections.Async;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Mighty.Dynamic.Tests.PostgreSql.TableClasses;
using NUnit.Framework;

namespace Mighty.Dynamic.Tests.PostgreSql
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
		public async Task Guid_Arg()
		{
			// PostgreSQL has true Guid type support
			var db = new MightyOrm(TestConstants.ReadWriteTestConnection);
			var guid = Guid.NewGuid();
            dynamic item;
            using (var command = db.CreateCommand("SELECT @0 AS val", null, guid))
            {
                Assert.AreEqual(DbType.Guid, command.Parameters[0].DbType);
                item = await db.SingleAsync(command);
            }
			Assert.AreEqual(guid, item.val);
		}


		[Test]
		public async Task All_NoParameters()
		{
			var customers = new Customer();
			var allRows = await (await customers.AllAsync()).ToListAsync();
			Assert.AreEqual(91, allRows.Count);
			foreach(var c in allRows)
			{
				Console.WriteLine("{0} {1}", c.customerid, c.companyname);
			}
		}


		/// <summary>
		/// This is documenting a bug in Npgsql; if it changes, we can remove the extra code we've added to
		/// <see cref="Mighty.Npgsql.NpgsqlDereferencingReader"/> to make it respond to cancellations even though the Npgsql objects don't.
		/// </summary>
		/// <remarks>
		/// Note the similar tests for all other supported drivers, which pass.
		/// </remarks>
		[Test]
		public async Task All_NoParameters_NpgsqlDoesNotRespondToCancellation()
		{
			using (CancellationTokenSource cts = new CancellationTokenSource())
			{
				var customers = new Customer();
				var allRows = await customers.AllAsync(cts.Token);
				int count = 0;
				// does not throw TaskCanceledException
				Assert.DoesNotThrowAsync(async () => {
					await allRows.ForEachAsync(c => {
						Console.WriteLine("{0} {1}", c.customerid, c.companyname);
						count++;
						if (count == 11)
						{
							cts.Cancel();
						}
					});
				});
				// is not 11
				Assert.AreEqual(91, count);
			}
		}


		[Test]
		public async Task All_LimitSpecification()
		{
            // TO DO: When the DB user does not exist, this is throwing the wrong exception
            // (even though ONE of those thrown while running is the user does not exist exception)
			var customers = new Customer();
			var allRows = await (await customers.AllAsync(limit: 10)).ToListAsync();
			Assert.AreEqual(10, allRows.Count);
		}


		[Test]
		public async Task All_WhereSpecification_OrderBySpecification()
		{
			var customers = new Customer();
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
			var customers = new Customer();
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
			var customers = new Customer();
			// no order by, so in theory this is useless. It will order on PK though
			var page2 = await customers.PagedAsync(currentPage: 2, pageSize: 10);
			var pageItems = ((IEnumerable<dynamic>)page2.Items).ToList();
			Assert.AreEqual(10, pageItems.Count);
			Assert.AreEqual(91, page2.TotalRecords);
		}


		[Test]
		public async Task Paged_OrderBySpecification()
		{
			var customers = new Customer();
			var page2 = await customers.PagedAsync(orderBy: "companyname DESC", currentPage: 2, pageSize: 10);
			var pageItems = ((IEnumerable<dynamic>)page2.Items).ToList();
			Assert.AreEqual(10, pageItems.Count);
			Assert.AreEqual(91, page2.TotalRecords);
		}


		[Test]
		public async Task Insert_SingleRow()
		{
			var products = new Product();
			var inserted = await products.InsertAsync(new { productname = "Massive Product" });
			Assert.IsTrue(inserted.productid > 0);
		}


		[OneTimeTearDown]
		public async Task CleanUp()
		{
			// delete all rows with ProductName 'Massive Product'. 
			var products = new Product();
			await products.DeleteAsync("productname=:0", "Massive Product");
		}
	}
}
