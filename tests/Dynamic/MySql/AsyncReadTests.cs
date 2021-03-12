#if !NET40
#pragma warning disable IDE0079
#pragma warning disable IDE0063
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
using Mighty.Dynamic.Tests.MySql.TableClasses;
using NUnit.Framework;

namespace Mighty.Dynamic.Tests.MySql
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
        public async Task Use_GlobalConnectionString()
        {
            MightyOrm.GlobalConnectionString = WhenDevart.AddLicenseKey(ProviderName, string.Format(TestConstants.ReadTestConnection, ProviderName));
            dynamic film = new MightyOrm(tableName: "sakila.film");
            var singleInstance = await film.SingleAsync(new { film_id = 43 });
            Assert.AreEqual(43, singleInstance.film_id);
        }


        [Test]
        public async Task Guid_Arg()
        {
            // MySQL has native Guid parameter support, but the SELECT output is a string
            var db = new MightyOrm(WhenDevart.AddLicenseKey(ProviderName, string.Format(TestConstants.ReadTestConnection, ProviderName)));
            var guid = Guid.NewGuid();
            dynamic item;
            using (var command = db.CreateCommand("SELECT @0 AS val", null, guid))
            {
                Assert.AreEqual(DbType.Guid, command.Parameters[0].DbType);
                item = await db.SingleAsync(command);
            }
            Assert.AreEqual(typeof(string), item.val.GetType());
            Assert.AreEqual(guid, new Guid(item.val));
        }


        [Test]
        public async Task Max_SingleArg()
        {
            var soh = new Film(ProviderName);
            var result = await soh.MaxAsync(columns: "film_id", where: "rental_duration > @0", args: 6);
            Assert.AreEqual(988, result);
        }


        [Test]
        public async Task Max_TwoArgs()
        {
            var soh = new Film(ProviderName);
            var result = await soh.MaxAsync(columns: "film_id", where: "rental_duration > @0 AND rental_duration < @1", args: new object[] { 6, 100 });
            Assert.AreEqual(988, result);
        }


        [Test]
        public async Task Max_NameValuePair()
        {
            var film = new Film(ProviderName);
            var result = await film.MaxAsync(columns: "film_id", whereParams: new { rental_duration = 6 });
            Assert.AreEqual(998, result);
        }


        [Test]
        public void EmptyElement_ProtoType()
        {
            var film = new Film(ProviderName);
            dynamic defaults = film.New();
            Assert.IsTrue(defaults.last_update > DateTime.MinValue);
        }


        [Test]
        public void SchemaTableMetaDataRetrieval()
        {
            var film = new Film(ProviderName);
            var metaData = film.TableMetaData;
            Assert.IsNotNull(metaData);
            Assert.AreEqual(13, metaData.Count());
            Assert.IsTrue(metaData.All(v => v.TABLE_NAME == film.BareTableName));
        }


        [Test]
        public async Task All_NoParameters()
        {
            var film = new Film(ProviderName);
            var allRows = await (await film.AllAsync()).ToListAsync();
            Assert.AreEqual(1000, allRows.Count);
        }


        [Test]
        public async Task All_NoParameters_Streaming()
        {
            var film = new Film(ProviderName);
            var allRows = await film.AllAsync();
            var count = 0;
#if NETCOREAPP3_0 || NETCOREAPP3_1
            await foreach (var r in allRows )
            {
                count++;
                Assert.AreEqual(13, ((IDictionary<string, object>)r).Count);        // # of fields fetched should be 13
            }
#else
            await allRows.ForEachAsync(r => {
                count++;
                Assert.AreEqual(13, ((IDictionary<string, object>)r).Count);        // # of fields fetched should be 13
            });
#endif
            Assert.AreEqual(1000, count);
        }


        [Test]
        public async Task All_LimitSpecification()
        {
            var film = new Film(ProviderName);
            var allRows = await (await film.AllAsync(limit: 10)).ToListAsync();
            Assert.AreEqual(10, allRows.Count);
        }


        [Test]
        public async Task All_ColumnSpecification()
        {
            // specify columns a different way to cause a cache miss (but this does cache wrt sync version)
            var film = new Film(ProviderName, columns: "film_id as FILMID, description, language_id");
            var allRows = await (await film.AllAsync()).ToListAsync();
            Assert.AreEqual(1000, allRows.Count);
            var firstRow = (IDictionary<string, object>)allRows[0];
            Assert.AreEqual(3, firstRow.Count);
            Assert.IsTrue(firstRow.ContainsKey("FILMID"));
            Assert.IsTrue(firstRow.ContainsKey("description"));
            Assert.IsTrue(firstRow.ContainsKey("language_id"));
        }


        [Test]
        public async Task All_OrderBySpecification()
        {
            var film = new Film(ProviderName);
            var allRows = await (await film.AllAsync(orderBy: "rental_duration DESC")).ToListAsync();
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
        public async Task All_WhereClause()
        {
            var film = new Film(ProviderName);
            var allRows = await (await film.AllAsync(where: "WHERE rental_duration=@0", args: 5)).ToListAsync();
            Assert.AreEqual(191, allRows.Count);
        }


        [Test]
        public async Task All_WhereClause_OrderBy()
        {
            var film = new Film(ProviderName);
            var allRows = await (await film.AllAsync(orderBy: "film_id DESC", where: "WHERE rental_duration=@0", args: 5)).ToListAsync();
            Assert.AreEqual(191, allRows.Count);
            int previous = int.MaxValue;
            foreach (var r in allRows)
            {
                int current = r.film_id;
                Assert.IsTrue(current <= previous);
                previous = current;
            }
        }


        [Test]
        public async Task All_WhereClause_Columns()
        {
            var film = new Film(ProviderName);
            var allRows = await (await film.AllAsync(columns: "film_id as FILMID, description, language_id", where: "WHERE rental_duration=@0", args: 5)).ToListAsync();
            Assert.AreEqual(191, allRows.Count);
            var firstRow = (IDictionary<string, object>)allRows[0];
            Assert.AreEqual(3, firstRow.Count);
            Assert.IsTrue(firstRow.ContainsKey("FILMID"));
            Assert.IsTrue(firstRow.ContainsKey("description"));
            Assert.IsTrue(firstRow.ContainsKey("language_id"));
        }


        [Test]
        public async Task All_WhereClause_Columns_Limit()
        {
            var film = new Film(ProviderName);
            var allRows = await (await film.AllAsync(limit: 2, columns: "film_id as FILMID, description, language_id", where: "WHERE rental_duration=@0", args: 5)).ToListAsync();
            Assert.AreEqual(2, allRows.Count);
            var firstRow = (IDictionary<string, object>)allRows[0];
            Assert.AreEqual(3, firstRow.Count);
            Assert.IsTrue(firstRow.ContainsKey("FILMID"));
            Assert.IsTrue(firstRow.ContainsKey("description"));
            Assert.IsTrue(firstRow.ContainsKey("language_id"));
        }


        [Test]
        public async Task All_WhereParams()
        {
            var film = new Film(ProviderName);
            var allRows = await (await film.AllAsync(new { rental_duration = 5})).ToListAsync();
            Assert.AreEqual(191, allRows.Count);
        }


        [Test]
        public async Task All_WhereParams_OrderBy()
        {
            var film = new Film(ProviderName);
            var allRows = await (await film.AllAsync(orderBy: "film_id DESC", whereParams: new { rental_duration = 5 })).ToListAsync();
            Assert.AreEqual(191, allRows.Count);
            int previous = int.MaxValue;
            foreach (var r in allRows)
            {
                int current = r.film_id;
                Assert.IsTrue(current <= previous);
                previous = current;
            }
        }


        [Test]
        public async Task All_WhereParams_Columns()
        {
            var film = new Film(ProviderName);
            var allRows = await (await film.AllAsync(columns: "film_id as FILMID, description, language_id", whereParams: new { rental_duration = 5 })).ToListAsync();
            Assert.AreEqual(191, allRows.Count);
            var firstRow = (IDictionary<string, object>)allRows[0];
            Assert.AreEqual(3, firstRow.Count);
            Assert.IsTrue(firstRow.ContainsKey("FILMID"));
            Assert.IsTrue(firstRow.ContainsKey("description"));
            Assert.IsTrue(firstRow.ContainsKey("language_id"));
        }


        [Test]
        public async Task All_WhereParams_Columns_Limit()
        {
            var film = new Film(ProviderName);
            var allRows = await (await film.AllAsync(limit: 2, columns: "film_id as FILMID, description, language_id", whereParams: new { rental_duration = 5 })).ToListAsync();
            Assert.AreEqual(2, allRows.Count);
            var firstRow = (IDictionary<string, object>)allRows[0];
            Assert.AreEqual(3, firstRow.Count);
            Assert.IsTrue(firstRow.ContainsKey("FILMID"));
            Assert.IsTrue(firstRow.ContainsKey("description"));
            Assert.IsTrue(firstRow.ContainsKey("language_id"));
        }


        [Test]
#pragma warning disable CS1998
        public async Task All_WhereParamsKey_ThrowsInvalidOperationException()
        {
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => {
                var film = new Film(ProviderName);
                var allRows = await (await film.AllAsync(limit: 2, columns: "film_id as FILMID, description, language_id", whereParams: 5)).ToListAsync();
            });
            Assert.AreEqual("whereParams in AllAsync(...) should contain names and values but it contained values only. If you want to get a single item by its primary key use SingleAsync(...) instead.", ex.Message);
        }
#pragma warning restore CS1998


#if DYNAMIC_METHODS
        [Test]
        public async Task Find_AllColumns()
        {
            dynamic film = new Film(ProviderName);
            var singleInstance = await film.FindAsync(film_id: 43);
            Assert.AreEqual(43, singleInstance.film_id);
        }


        [Test]
        public async Task Find_OneColumn()
        {
            dynamic film = new Film(ProviderName);
            var singleInstance = await film.FindAsync(film_id: 43, columns: "film_id");
            Assert.AreEqual(43, singleInstance.film_id);
            var siAsDict = (IDictionary<string, object>)singleInstance;
            Assert.AreEqual(1, siAsDict.Count);
        }


        [Test]
        public async Task Get_AllColumns()
        {
            dynamic film = new Film(ProviderName);
            var singleInstance = await film.GetAsync(film_id: 43);
            Assert.AreEqual(43, singleInstance.film_id);
        }


        [Test]
        public async Task First_AllColumns()
        {
            dynamic film = new Film(ProviderName);
            var singleInstance = await film.FirstAsync(film_id: 43);
            Assert.AreEqual(43, singleInstance.film_id);
        }


        [Test]
        public async Task Single_AllColumns()
        {
            dynamic film = new Film(ProviderName);
            var singleInstance = await film.SingleAsync(film_id: 43);
            Assert.AreEqual(43, singleInstance.film_id);
        }


        [Test]
        public async Task Single_AllColumns()
        {
            dynamic film = new Film(ProviderName);
            var singleInstance = await film.SingleAsync(film_id: 43);
            Assert.AreEqual(43, singleInstance.film_id);
        }
#else
        [Test]
        public async Task Single_Where_AllColumns()
        {
            var film = new Film(ProviderName);
            var singleInstance = await film.SingleAsync(new { film_id = 43 });
            Assert.AreEqual(43, singleInstance.film_id);
        }


        [Test]
        public async Task Single_Key_AllColumns()
        {
            var film = new Film(ProviderName);
            var singleInstance = await film.SingleAsync(43);
            Assert.AreEqual(43, singleInstance.film_id);
        }


        [Test]
        public async Task Single_Where_OneColumn()
        {
            var film = new Film(ProviderName);
            var singleInstance = await film.SingleAsync(new { film_id = 43 }, columns: "film_id");
            Assert.AreEqual(43, singleInstance.film_id);
            var siAsDict = (IDictionary<string, object>)singleInstance;
            Assert.AreEqual(1, siAsDict.Count);
        }


        [Test]
        public async Task Single_Key_OneColumn()
        {
            var film = new Film(ProviderName);
            var singleInstance = await film.SingleAsync(43, columns: "film_id");
            Assert.AreEqual(43, singleInstance.film_id);
            var siAsDict = (IDictionary<string, object>)singleInstance;
            Assert.AreEqual(1, siAsDict.Count);
        }


        //[Test]
        //public async Task First_AllColumns()
        //{
        //    // TO DO: Maybe support non-dynamic First method?
        //    dynamic film = new Film(ProviderName);
        //    var singleInstance = await film.FirstAsync(film_id: 43);
        //    Assert.AreEqual(43, singleInstance.film_id);
        //}
#endif

        [Test]
        public async Task Query_AllRows()
        {
            var film = new Film(ProviderName);
            var allRows = await (await film.QueryAsync("SELECT * FROM sakila.film")).ToListAsync();
            Assert.AreEqual(1000, allRows.Count);
        }


        [Test]
        public async Task Query_AllRows_RespondsToCancellation()
        {
            using (CancellationTokenSource cts = new CancellationTokenSource())
            {
                var film = new Film(ProviderName);
                var allRows = await film.QueryAsync("SELECT * FROM sakila.film", cts.Token);
                int count = 0;
                Assert.ThrowsAsync<TaskCanceledException>(async () =>
                {
                    await allRows.ForEachAsync(async row => {
                        await Console.Out.WriteLineAsync($"{row.film_id}");
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
            var film = new Film(ProviderName);
            var filteredRows = await (await film.QueryAsync("SELECT * FROM sakila.film WHERE rental_duration=@0", 5)).ToListAsync();
            Assert.AreEqual(191, filteredRows.Count);
        }


        [Test]
        public async Task Paged_NoSpecification()
        {
            var film = new Film(ProviderName);
            // no order by, and paged queries logically must have an order by; this will order on PK
            var page2 = await film.PagedAsync(currentPage: 2, pageSize: 30);
            var pageItems = page2.Items.ToList();
            Assert.AreEqual(30, pageItems.Count);
            Assert.AreEqual(1000, page2.TotalRecords);
        }


        [Test]
        public async Task Paged_WhereSpecification()
        {
            var film = new Film(ProviderName);
            var page11 = await film.PagedAsync(currentPage: 11, where: "description LIKE @0", args: "%the%");
            var pageItems = page11.Items.ToList();
            Assert.AreEqual(1, pageItems.Count); // also testing being on last page
            Assert.AreEqual(201, page11.TotalRecords);
        }


        [Test]
        public async Task Paged_OrderBySpecification()
        {
            var film = new Film(ProviderName);
            var page2 = await film.PagedAsync(orderBy: "rental_duration DESC", currentPage: 2, pageSize: 30);
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
            var film = new Film(ProviderName);
            var page2 = await film.PagedAsync(columns: "rental_duration, film_id", orderBy: "rental_duration DESC", currentPage: 2, pageSize: 30);
            var pageItems = page2.Items.ToList();
            Assert.AreEqual(30, pageItems.Count);
            Assert.AreEqual(1000, page2.TotalRecords);
            var firstRow = (IDictionary<string, object>)pageItems[0];
            Assert.AreEqual(2, firstRow.Count);
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
            var film = new Film(ProviderName);
            var total = await film.CountAsync();
            Assert.AreEqual(1000, total);
        }


        [Test]
        public async Task Count_WhereSpecification()
        {
            var film = new Film(ProviderName);
            var total = await film.CountAsync(where: "WHERE rental_duration=@0", args: 5);
            Assert.AreEqual(191, total);
        }


        [Test]
        public void DefaultValue()
        {
            var film = new Film(ProviderName, false);
            var value = film.GetColumnDefault("last_update");
            Assert.AreEqual(typeof(DateTime), value.GetType());
        }


        [Test]
        public async Task IsValid_FilmIDCheck()
        {
            var film = new Film(ProviderName);
            var toValidate = await film.SingleAsync(new { film_id = 72 });
            // is invalid
            Assert.AreEqual(1, film.IsValid(toValidate).Count);

            toValidate = await film.SingleAsync(new { film_id = 2 });
            // is valid
            Assert.AreEqual(0, film.IsValid(toValidate).Count);
        }


        [Test]
        public async Task PrimaryKey_Read_Check()
        {
            var film = new Film(ProviderName);
            var toValidate = await film.SingleAsync(new { film_id = 45 });

            Assert.IsTrue(film.HasPrimaryKey(toValidate));

            var pkValue = film.GetPrimaryKey(toValidate);
            Assert.AreEqual(45, pkValue);
        }
    }
}
#endif