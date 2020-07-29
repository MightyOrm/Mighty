#if !NET40
using System;
using System.Collections;
using Dasync.Collections;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Mighty.Generic.Tests.MySql.TableClasses;
using NUnit.Framework;

namespace Mighty.Generic.Tests.MySql
{
    [TestFixture("MySql.Data.MySqlClient")]
#if !DISABLE_DEVART
    [TestFixture("Devart.Data.MySql")]
#endif
    public class AsyncReadTests
    {
        private readonly string ProviderName;

        /// <summary>
        /// Initialise tests for given provider
        /// </summary>
        /// <param name="providerName">Provider name</param>
        public AsyncReadTests(string providerName)
        {
            ProviderName = providerName;
        }


        [Test]
        public async Task Max_SingleArg()
        {
            var soh = new Films(ProviderName);
            var result = await soh.MaxAsync(columns: "film_id", where: "rental_duration > @0", args: 6);
            Assert.AreEqual(988, result);
        }


        [Test]
        public async Task Max_TwoArgs()
        {
            var soh = new Films(ProviderName);
            var result = await soh.MaxAsync(columns: "film_id", where: "rental_duration > @0 AND rental_duration < @1", args: new object[] { 6, 100 });
            Assert.AreEqual(988, result);
        }


        [Test]
        public async Task Max_NameValuePair()
        {
            var films = new Films(ProviderName);
            var result = await films.MaxAsync("film_id", new { rental_duration = 6 });
            Assert.AreEqual(998, result);
        }


        [Test]
        public void EmptyElement_ProtoType()
        {
            var films = new Films(ProviderName);
            var defaults = films.New();
            Assert.IsTrue(defaults.last_update > DateTime.MinValue);
        }


        [Test]
        public void SchemaTableMetaDataRetrieval()
        {
            var films = new Films(ProviderName);
            var metaData = films.TableMetaData;
            Assert.IsNotNull(metaData);
            Assert.AreEqual(13, metaData.Count());
            Assert.IsTrue(metaData.All(v => v.TABLE_NAME == films.BareTableName));
        }


        [Test]
        public async Task All_NoParameters()
        {
            var films = new Films(ProviderName);
            var allRows = await (await films.AllAsync()).ToListAsync();
            Assert.AreEqual(1000, allRows.Count);
        }


        [Test]
        public async Task All_NoParameters_Streaming()
        {
            var films = new Films(ProviderName);
            var allRows = await films.AllAsync();
            var count = 0;
            await allRows.ForEachAsync(r => {
                count++;
                Assert.Greater(r.film_id, 0);
                Assert.Greater(r.last_update, DateTime.MinValue);
                Assert.Greater(r.language_id, 0);
                Assert.Greater(r.rental_duration, 0);
                Assert.AreNotEqual(r.description, "");
            });
            Assert.AreEqual(1000, count);
        }


        [Test]
        public async Task All_LimitSpecification()
        {
            var films = new Films(ProviderName);
            var allRows = await (await films.AllAsync(limit: 10)).ToListAsync();
            Assert.AreEqual(10, allRows.Count);
        }


        [Test]
        public async Task All_ColumnSpecification()
        {
            var films = new Films(ProviderName);
            var allRows = await (await films.AllAsync(columns: "film_id, description, language_id")).ToListAsync();
            Assert.AreEqual(1000, allRows.Count);
            var firstRow = allRows[0];
            Assert.Greater(firstRow.film_id, 0);
            Assert.AreNotEqual("", firstRow.description);
            Assert.Greater(firstRow.language_id, 0);
        }


        [Test]
        public async Task All_OrderBySpecification()
        {
            var films = new Films(ProviderName);
            var allRows = await (await films.AllAsync(orderBy: "rental_duration DESC")).ToListAsync();
            Assert.AreEqual(1000, allRows.Count);
            int previous = int.MaxValue;
            foreach(var r in allRows)
            {
                int current = r.rental_duration;
                Assert.IsTrue(current <= previous);
                previous = current;
            }
        }


        [Test]
        public async Task All_WhereSpecification()
        {
            var films = new Films(ProviderName);
            var allRows = await (await films.AllAsync(where: "WHERE rental_duration=@0", args: 5)).ToListAsync();
            Assert.AreEqual(191, allRows.Count);
        }


        [Test]
        public async Task All_WhereSpecification_OrderBySpecification()
        {
            var films = new Films(ProviderName);
            var allRows = await (await films.AllAsync(orderBy: "film_id DESC", where: "WHERE rental_duration=@0", args: 5)).ToListAsync();
            Assert.AreEqual(191, allRows.Count);
            int previous = int.MaxValue;
            foreach(var r in allRows)
            {
                int current = r.film_id;
                Assert.IsTrue(current <= previous);
                previous = current;
            }
        }


        [Test]
        public async Task All_WhereSpecification_ColumnsSpecification()
        {
            var films = new Films(ProviderName);
            var allRows = await (await films.AllAsync(columns: "film_id, description, language_id", where: "WHERE rental_duration=@0", args: 5)).ToListAsync();
            Assert.AreEqual(191, allRows.Count);
            var firstRow = allRows[0];
            Assert.Greater(firstRow.film_id, 0);
            Assert.AreNotEqual(firstRow.description, "");
            Assert.Greater(firstRow.language_id, 0);
        }


        [Test]
        public async Task All_WhereSpecification_ColumnsSpecification_LimitSpecification()
        {
            var films = new Films(ProviderName);
            var allRows = await (await films.AllAsync(limit: 2, columns: "film_id, description, language_id", where: "WHERE rental_duration=@0", args: 5)).ToListAsync();
            Assert.AreEqual(2, allRows.Count);
            var firstRow = allRows[0];
            Assert.Greater(firstRow.film_id, 0);
            Assert.AreNotEqual(firstRow.description, "");
            Assert.Greater(firstRow.language_id, 0);
        }


#if DYNAMIC_METHODS
        [Test]
        public async Task Find_AllColumns()
        {
            dynamic films = new Films(ProviderName);
            var singleInstance = await films.FindAsync(film_id: 43);
            Assert.AreEqual(43, singleInstance.film_id);
        }


        [Test]
        public async Task Find_OneColumn()
        {
            dynamic films = new Films(ProviderName);
            var singleInstance = await films.FindAsync(film_id: 43, columns: "film_id");
            Assert.AreEqual(43, singleInstance.film_id);
            Assert.AreEqual(DateTime.MinValue, singleInstance.last_update);
        }


        [Test]
        public async Task Get_AllColumns()
        {
            dynamic films = new Films(ProviderName);
            var singleInstance = await films.GetAsync(film_id: 43);
            Assert.AreEqual(43, singleInstance.film_id);
            Assert.Greater(singleInstance.last_update, DateTime.MinValue);
        }


        [Test]
        public async Task First_AllColumns()
        {
            dynamic films = new Films(ProviderName);
            var singleInstance = await films.FirstAsync(film_id: 43);
            Assert.AreEqual(43, singleInstance.film_id);
        }
#endif


        [Test]
        public async Task Single_Where_AllColumns()
        {
            var films = new Films(ProviderName);
            var singleInstance = await films.SingleAsync(new { film_id = 43 });
            Assert.AreEqual(43, singleInstance.film_id);
        }


        [Test]
        public async Task Single_Key_AllColumns()
        {
            var films = new Films(ProviderName);
            var singleInstance = await films.SingleAsync(43);
            Assert.AreEqual(43, singleInstance.film_id);
        }


        [Test]
        public async Task Query_AllRows()
        {
            var films = new Films(ProviderName);
            var allRows = await (await films.QueryAsync("SELECT * FROM sakila.film")).ToListAsync();
            Assert.AreEqual(1000, allRows.Count);
        }


        [Test]
        public async Task Query_AllRows_RespondsToCancellation()
        {
            using (CancellationTokenSource cts = new CancellationTokenSource())
            {
                var films = new Films(ProviderName);
                var allRows = await films.QueryAsync(cts.Token, "SELECT * FROM sakila.film");
                int count = 0;
                Assert.ThrowsAsync<TaskCanceledException>(async () =>
                {
                    await allRows.ForEachAsync(async row => {
                        await Task.Delay(0);
                        MDebug.WriteLine($"{row.film_id}");
                        count++;
                        if (count == 10)
                        {
                            cts.Cancel();
                        }
                    });
                });
                Assert.AreEqual(10, count);
            }
        }


        [Test]
        public async Task Query_Filter()
        {
            var films = new Films(ProviderName);
            var filteredRows = await (await films.QueryAsync("SELECT * FROM sakila.film WHERE rental_duration=@0", 5)).ToListAsync();
            Assert.AreEqual(191, filteredRows.Count);
        }


        [Test]
        public async Task Paged_NoSpecification()
        {
            var films = new Films(ProviderName);
            // no order by, and paged queries logically must have an order by; this will order on PK
            var page2 = await films.PagedAsync(currentPage: 2, pageSize: 30);
            Assert.AreEqual(30, page2.Items.Count);
            Assert.AreEqual(1000, page2.TotalRecords);
            Assert.AreEqual(2, page2.CurrentPage);
            Assert.AreEqual(30, page2.PageSize);
        }

        [Test]
        public async Task Paged_WhereSpecification()
        {
            var films = new Films(ProviderName);
            // no order by, and paged queries logically must have an order by; this will order on PK
            var page11 = await films.PagedAsync(currentPage: 11, where: "description LIKE @0", args: "%the%");
            var pageItems = page11.Items.ToList();
            Assert.AreEqual(1, pageItems.Count); // also testing being on last page
            Assert.AreEqual(201, page11.TotalRecords);
        }


        [Test]
        public async Task Paged_WhereSpecification_WithParams()
        {
            var films = new Films(ProviderName);
            // no order by, and paged queries logically must have an order by; this will order on PK
            var page11 = await films.PagedWithParamsAsync(currentPage: 11, where: "description LIKE @description", inParams: new { description = "%the%" });
            var pageItems = page11.Items.ToList();
            Assert.AreEqual(1, pageItems.Count); // also testing being on last page
            Assert.AreEqual(201, page11.TotalRecords);
        }


        [Test]
        public async Task Paged_OrderBySpecification()
        {
            var films = new Films(ProviderName);
            var page2 = await films.PagedAsync(orderBy: "rental_duration DESC", currentPage: 2, pageSize: 30);
            var pageItems = page2.Items.ToList();
            Assert.AreEqual(30, pageItems.Count);
            Assert.AreEqual(1000, page2.TotalRecords);

            int previous = int.MaxValue;
            foreach(var r in pageItems)
            {
                int current = r.rental_duration;
                Assert.IsTrue(current <= previous);
                previous = current;
            }
        }


        [Test]
        public async Task Paged_OrderBySpecification_ColumnsSpecification()
        {
            var films = new Films(ProviderName);
            var page2 = await films.PagedAsync(columns: "rental_duration, film_id", orderBy: "rental_duration DESC", currentPage: 2, pageSize: 30);
            var pageItems = page2.Items.ToList();
            Assert.AreEqual(30, pageItems.Count);
            Assert.AreEqual(1000, page2.TotalRecords);
            int previous = int.MaxValue;
            foreach(var r in pageItems)
            {
                int current = r.rental_duration;
                Assert.IsTrue(current <= previous);
                previous = current;
            }
        }


        [Test]
        public async Task Count_NoSpecification()
        {
            var films = new Films(ProviderName);
            var total = await films.CountAsync();
            Assert.AreEqual(1000, total);
        }


        [Test]
        public async Task Count_WhereSpecification()
        {
            var films = new Films(ProviderName);
            var total = await films.CountAsync(where: "WHERE rental_duration=@0", args: 5);
            Assert.AreEqual(191, total);
        }


        [Test]
        public void DefaultValue()
        {
            var films = new Films(ProviderName, false);
            var value = films.GetColumnDefault("last_update");
            Assert.AreEqual(typeof(DateTime), value.GetType());
        }


        [Test]
        public async Task IsValid_FilmIDCheck()
        {
            var films = new Films(ProviderName);
            var toValidate = await films.SingleAsync(new { film_id = 72 });
            // is invalid
            Assert.AreEqual(1, films.IsValid(toValidate).Count);

            toValidate = await films.SingleAsync(new { film_id = 2 });
            // is valid
            Assert.AreEqual(0, films.IsValid(toValidate).Count);
        }


        [Test]
        public async Task PrimaryKey_Read_Check()
        {
            var films = new Films(ProviderName);
            var toValidate = await films.SingleAsync(new { film_id = 45 });

            Assert.IsTrue(films.HasPrimaryKey(toValidate));

            var pkValue = films.GetPrimaryKey(toValidate);
            Assert.AreEqual(45, pkValue);
        }
    }
}
#endif