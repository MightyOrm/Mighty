using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Mighty.Dynamic.Tests.MySql.TableClasses;
using NUnit.Framework;

namespace Mighty.Dynamic.Tests.MySql
{
    [TestFixture("MySql.Data.MySqlClient")]
#if !DISABLE_DEVART // Devart works fine on .NET Core, but I want to get a version to test with without paying $100 p/a!
    [TestFixture("Devart.Data.MySql")]
#endif
    public class ReadTests
    {
        private readonly string ProviderName;

        /// <summary>
        /// Initialise tests for given provider
        /// </summary>
        /// <param name="providerName">Provider name</param>
        public ReadTests(string providerName)
        {
            ProviderName = providerName;
        }


        [Test]
        public void Guid_Arg()
        {
            // MySQL has native Guid parameter support, but the SELECT output is a string
            var db = new MightyOrm(string.Format(TestConstants.ReadTestConnection, ProviderName));
            var guid = Guid.NewGuid();
            dynamic item;
            using (var command = db.CreateCommand("SELECT @0 AS val", null, guid))
            {
                Assert.AreEqual(DbType.Guid, command.Parameters[0].DbType);
                item = db.Single(command);
            }
            Assert.AreEqual(typeof(string), item.val.GetType());
            Assert.AreEqual(guid, new Guid(item.val));
        }


        [Test]
        public void Max_SingleArg()
        {
            var soh = new Film(ProviderName);
            var result = soh.Max(columns: "film_id", where: "rental_duration > @0", args: 6);
            Assert.AreEqual(988, result);
        }


        [Test]
        public void Max_TwoArgs()
        {
            var soh = new Film(ProviderName);
            var result = soh.Max(columns: "film_id", where: "rental_duration > @0 AND rental_duration < @1", args: new object[] { 6, 100 });
            Assert.AreEqual(988, result);
        }


        [Test]
        public void Max_NameValuePair()
        {
            var film = new Film(ProviderName);
            var result = film.Max("film_id", new { rental_duration = 6 });
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
        public void All_NoParameters()
        {
            var film = new Film(ProviderName);
            var allRows = film.All().ToList();
            Assert.AreEqual(1000, allRows.Count);
        }


        [Test]
        public void All_NoParameters_Streaming()
        {
            var film = new Film(ProviderName);
            var allRows = film.All();
            var count = 0;
            foreach(var r in allRows)
            {
                count++;
                Assert.AreEqual(13, ((IDictionary<string, object>)r).Count);        // # of fields fetched should be 13
            }
            Assert.AreEqual(1000, count);
        }


        [Test]
        public void All_LimitSpecification()
        {
            var film = new Film(ProviderName);
            var allRows = film.All(limit: 10).ToList();
            Assert.AreEqual(10, allRows.Count);
        }


        [Test]
        public void All_ColumnSpecification()
        {
            // specify columns a different way to cause a cache miss (but this does cache wrt async version)
            var film = new Film(ProviderName, columns: "film_id as FILMID, description, language_id");
            var allRows = film.All().ToList();
            Assert.AreEqual(1000, allRows.Count);
            var firstRow = (IDictionary<string, object>)allRows[0];
            Assert.AreEqual(3, firstRow.Count);
            Assert.IsTrue(firstRow.ContainsKey("FILMID"));
            Assert.IsTrue(firstRow.ContainsKey("description"));
            Assert.IsTrue(firstRow.ContainsKey("language_id"));
        }


        [Test]
        public void All_OrderBySpecification()
        {
            var film = new Film(ProviderName);
            var allRows = film.All(orderBy: "rental_duration DESC").ToList();
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
        public void All_WhereSpecification()
        {
            var film = new Film(ProviderName);
            var allRows = film.All(where: "WHERE rental_duration=@0", args: 5).ToList();
            Assert.AreEqual(191, allRows.Count);
        }


        [Test]
        public void All_WhereSpecification_OrderBySpecification()
        {
            var film = new Film(ProviderName);
            var allRows = film.All(orderBy: "film_id DESC", where: "WHERE rental_duration=@0", args: 5).ToList();
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
        public void All_WhereSpecification_ColumnsSpecification()
        {
            var film = new Film(ProviderName);
            var allRows = film.All(columns: "film_id as FILMID, description, language_id", where: "WHERE rental_duration=@0", args: 5).ToList();
            Assert.AreEqual(191, allRows.Count);
            var firstRow = (IDictionary<string, object>)allRows[0];
            Assert.AreEqual(3, firstRow.Count);
            Assert.IsTrue(firstRow.ContainsKey("FILMID"));
            Assert.IsTrue(firstRow.ContainsKey("description"));
            Assert.IsTrue(firstRow.ContainsKey("language_id"));
        }


        [Test]
        public void All_WhereSpecification_ColumnsSpecification_LimitSpecification()
        {
            var film = new Film(ProviderName);
            var allRows = film.All(limit: 2, columns: "film_id as FILMID, description, language_id", where: "WHERE rental_duration=@0", args: 5).ToList();
            Assert.AreEqual(2, allRows.Count);
            var firstRow = (IDictionary<string, object>)allRows[0];
            Assert.AreEqual(3, firstRow.Count);
            Assert.IsTrue(firstRow.ContainsKey("FILMID"));
            Assert.IsTrue(firstRow.ContainsKey("description"));
            Assert.IsTrue(firstRow.ContainsKey("language_id"));
        }


        [Test]
        public void Single_Where_OneColumn()
        {
            var film = new Film(ProviderName);
            var singleInstance = film.Single(new { film_id = 43 }, columns: "film_id");
            Assert.AreEqual(43, singleInstance.film_id);
            var siAsDict = (IDictionary<string, object>)singleInstance;
            Assert.AreEqual(1, siAsDict.Count);
        }


        [Test]
        public void Single_Key_OneColumn()
        {
            var film = new Film(ProviderName);
            var singleInstance = film.Single(43, columns: "film_id");
            Assert.AreEqual(43, singleInstance.film_id);
            var siAsDict = (IDictionary<string, object>)singleInstance;
            Assert.AreEqual(1, siAsDict.Count);
        }


#if DYNAMIC_METHODS
        [Test]
        public void Get_AllColumns()
        {
            dynamic film = new Film(ProviderName);
            var singleInstance = film.Get(film_id: 43);
            Assert.AreEqual(43, singleInstance.film_id);
        }


        [Test]
        public void First_AllColumns()
        {
            dynamic film = new Film(ProviderName);
            var singleInstance = film.First(film_id: 43);
            Assert.AreEqual(43, singleInstance.film_id);
        }


        [Test]
        public void Single_AllColumns()
        {
            dynamic film = new Film(ProviderName);
            var singleInstance = film.Single(film_id: 43);
            Assert.AreEqual(43, singleInstance.film_id);
        }
#else
        [Test]
        public void Single_Where_AllColumns()
        {
            var film = new Film(ProviderName);
            var singleInstance = film.Single(new { film_id = 43});
            Assert.AreEqual(43, singleInstance.film_id);
        }

        [Test]
        public void Single_Key_AllColumns()
        {
            var film = new Film(ProviderName);
            var singleInstance = film.Single(43);
            Assert.AreEqual(43, singleInstance.film_id);
        }
#endif


        [Test]
        public void Query_AllRows()
        {
            var film = new Film(ProviderName);
            var allRows = film.Query("SELECT * FROM sakila.film").ToList();
            Assert.AreEqual(1000, allRows.Count);
        }


        [Test]
        public void Query_Filter()
        {
            var film = new Film(ProviderName);
            var filteredRows = film.Query("SELECT * FROM sakila.film WHERE rental_duration=@0", 5).ToList();
            Assert.AreEqual(191, filteredRows.Count);
        }


        [Test]
        public void Paged_NoSpecification()
        {
            var film = new Film(ProviderName);
            // no order by, and paged queries logically must have an order by; this will order on PK
            var page2 = film.Paged(currentPage: 2, pageSize: 30);
            Assert.AreEqual(30, page2.Items.Count);
            Assert.AreEqual(1000, page2.TotalRecords);
            Assert.AreEqual(2, page2.CurrentPage);
            Assert.AreEqual(30, page2.PageSize);
        }


        [Test]
        public void Paged_WhereSpecification()
        {
            var film = new Film(ProviderName);
            var page11 = film.Paged(currentPage: 11, where: "description LIKE @0", args: "%the%");
            var pageItems = ((IEnumerable<dynamic>)page11.Items).ToList();
            Assert.AreEqual(1, pageItems.Count); // also testing being on last page
            Assert.AreEqual(201, page11.TotalRecords);
        }


        [Test]
        public void Paged_WhereSpecification_WithParams()
        {
            var film = new Film(ProviderName);
            var page11 = film.PagedWithParams(currentPage: 11, where: "description LIKE @description", inParams: new { description = "%the%" });
            var pageItems = ((IEnumerable<dynamic>)page11.Items).ToList();
            Assert.AreEqual(1, pageItems.Count); // also testing being on last page
            Assert.AreEqual(201, page11.TotalRecords);
        }


        [Test]
        public void Paged_OrderBySpecification()
        {
            var film = new Film(ProviderName);
            var page2 = film.Paged(orderBy: "rental_duration DESC", currentPage: 2, pageSize: 30);
            var pageItems = ((IEnumerable<dynamic>)page2.Items).ToList();
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
        public void Paged_OrderBySpecification_ColumnsSpecification()
        {
            var film = new Film(ProviderName);
            var page2 = film.Paged(columns: "rental_duration, film_id", orderBy: "rental_duration DESC", currentPage: 2, pageSize: 30);
            var pageItems = ((IEnumerable<dynamic>)page2.Items).ToList();
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
        public void Count_NoSpecification()
        {
            var film = new Film(ProviderName);
            var total = film.Count();
            Assert.AreEqual(1000, total);
        }


        [Test]
        public void Count_WhereSpecification()
        {
            var film = new Film(ProviderName);
            var total = film.Count(where: "WHERE rental_duration=@0", args: 5);
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
        public void IsValid_FilmIDCheck()
        {
            var film = new Film(ProviderName);
            var toValidate = film.Single(new { film_id = 72 });
            // is invalid
            Assert.AreEqual(1, film.IsValid(toValidate).Count);

            toValidate = film.Single(new { film_id = 2 });
            // is valid
            Assert.AreEqual(0, film.IsValid(toValidate).Count);
        }


        [Test]
        public void PrimaryKey_Read_Check()
        {
            var film = new Film(ProviderName);
            var toValidate = film.Single(new { film_id = 45 });

            Assert.IsTrue(film.HasPrimaryKey(toValidate));

            var pkValue = film.GetPrimaryKey(toValidate);
            Assert.AreEqual(45, pkValue);
        }
    }
}
