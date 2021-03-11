#if !NET40
using System;
using System.Collections;
using Dasync.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Mighty.Generic.Tests.MySql.TableClasses;
using NUnit.Framework;

namespace Mighty.Generic.Tests.MySql
{
    [TestFixture("MySql.Data.MySqlClient")]
#if !DISABLE_DEVART
    [TestFixture("Devart.Data.MySql")]
#endif
    public class AsyncWriteTests
    {
        private readonly string ProviderName;

        /// <summary>
        /// Initialise tests for given provider
        /// </summary>
        /// <param name="providerName">Provider name</param>
        public AsyncWriteTests(string providerName)
        {
            ProviderName = providerName;
        }


        [Test]
        public async Task Insert_SingleRow()
        {
            var categories = new Categories(ProviderName);
            var inserted = await categories.InsertAsync(new { CategoryName = "Cool stuff", Description = "You know... cool stuff! Cool. n. stuff." });
            int insertedCategoryID = inserted.CategoryID;
            Assert.IsTrue(insertedCategoryID > 0);
        }


        [Test]
        public async Task Insert_FromNew()
        {
            Categories categories = new Categories(ProviderName);
            Category toInsert = categories.New();
            toInsert.CategoryName = "Cool stuff";
            toInsert.Description = "You know... cool stuff! Cool. n. stuff.";
            var inserted = await categories.InsertAsync(toInsert);
            int insertedCategoryID = inserted.CategoryID;
            Assert.IsTrue(insertedCategoryID > 0);
        }


        [Test]
        public async Task Insert_MultipleRows()
        {
            var categories = new Categories(ProviderName);
            var toInsert = new List<dynamic>();
            var CategoryName = "Cat Insert_MR";
            toInsert.Add(new { CategoryName, Description = "cat 1 desc" });
            toInsert.Add(new { CategoryName, Description = "cat 2 desc" });
            var inserted = await categories.InsertAsync(toInsert.ToArray());
            var selected = await (await categories.AllAsync(where: "CategoryName=@0", orderBy: "CategoryID", args: CategoryName)).ToListAsync();
            Assert.AreEqual(2, inserted.Count());
            Assert.AreEqual(2, selected.Count);
            var both = inserted.Zip(selected, (insertedItem, selectedItem) => new { insertedItem, selectedItem });
            foreach (var combined in both)
            {
                Assert.AreEqual(combined.insertedItem.CategoryID, combined.selectedItem.CategoryID);
                Assert.AreEqual(combined.insertedItem.CategoryName, combined.selectedItem.CategoryName);
                Assert.AreEqual(combined.insertedItem.Description, combined.selectedItem.Description);
            }
        }


        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public async Task Update_SingleRow(bool explicitConnection)
        {
            var categories = new Categories(ProviderName, explicitConnection);
            DbConnection connection = null;
            if (explicitConnection)
            {
                MightyTests.ConnectionStringUtils.CheckConnectionStringRequiredForOpenConnectionAsync(categories);
                connection = categories.OpenConnection(MightyTests.ConnectionStringUtils.GetConnectionString(TestConstants.WriteTestConnection, ProviderName));
            }
            using (connection)
            {
                // insert something to update first. 
                var inserted = await categories.InsertAsync(new { CategoryName = "Cool stuff", Description = "You know... cool stuff! Cool. n. stuff." }, connection);
                int insertedCategoryID = inserted.CategoryID;
                Assert.IsTrue(insertedCategoryID > 0);
                // update it, with a better description
                inserted.Description = "This is all jolly marvellous";
                Assert.AreEqual(1, await categories.UpdateAsync(inserted, connection), "Update should have affected 1 row");
                var updatedRow = await categories.SingleAsync(new { inserted.CategoryID }, connection: connection);
                Assert.IsNotNull(updatedRow);
                Assert.AreEqual(inserted.CategoryID, Convert.ToInt32(updatedRow.CategoryID)); // convert from uint
                Assert.AreEqual(inserted.Description, updatedRow.Description);
                // reset description to NULL
                updatedRow.Description = null;
                Assert.AreEqual(1, await categories.UpdateAsync(updatedRow, connection), "Update should have affected 1 row");
                var newUpdatedRow = await categories.SingleAsync(new { updatedRow.CategoryID }, connection: connection);
                Assert.IsNotNull(newUpdatedRow);
                Assert.AreEqual(updatedRow.CategoryID, newUpdatedRow.CategoryID);
                Assert.AreEqual(updatedRow.Description, newUpdatedRow.Description);
            }
        }


        [Test]
        public async Task Update_MultipleRows()
        {
            // first insert 2 categories and 4 products, one for each category
            var categories = new Categories(ProviderName);
            var insertedCategory1 = await categories.InsertAsync(new { CategoryName = "Category 1", Description = "Cat 1 desc" });
            int category1ID = insertedCategory1.CategoryID;
            Assert.IsTrue(category1ID > 0);
            var insertedCategory2 = await categories.InsertAsync(new { CategoryName = "Category 2", Description = "Cat 2 desc" });
            int category2ID = insertedCategory2.CategoryID;
            Assert.IsTrue(category2ID > 0);

            var products = new Products(ProviderName);
            for(int i = 0; i < 4; i++)
            {
                var category = i % 2 == 0 ? insertedCategory1 : insertedCategory2;
                var p = await products.InsertAsync(new { ProductName = "Prod" + i, category.CategoryID });
                Assert.IsTrue(p.ProductID > 0);
            }
            var allCat1Products = await (await products.AllAsync(where: "WHERE CategoryID=@0", args: category1ID)).ToArrayAsync();
            Assert.AreEqual(2, allCat1Products.Length);
            foreach(var p in allCat1Products)
            {
                Assert.AreEqual(category1ID, Convert.ToInt32(p.CategoryID)); // convert from uint
                p.CategoryID = category2ID;
            }
            Assert.AreEqual(2, await products.SaveAsync(allCat1Products));
        }


        [Test]
        public async Task Delete_SingleRow()
        {
            // first insert 2 categories
            var categories = new Categories(ProviderName);
            var insertedCategory1 = await categories.InsertAsync(new { CategoryName = "Cat Delete_SR", Description = "cat 1 desc" });
            int category1ID = insertedCategory1.CategoryID;
            Assert.IsTrue(category1ID > 0);
            var insertedCategory2 = await categories.InsertAsync(new { CategoryName = "Cat Delete_SR", Description = "cat 2 desc" });
            int category2ID = insertedCategory2.CategoryID;
            Assert.IsTrue(category2ID > 0);

            Assert.AreEqual(1, await categories.DeleteAsync(category1ID), "Delete should affect 1 row");
            var categoriesFromDB = await (await categories.AllAsync(where: "CategoryName=@0", args: (string)insertedCategory2.CategoryName)).ToListAsync();
            Assert.AreEqual((long)1, categoriesFromDB.Count);
            Assert.AreEqual(category2ID, Convert.ToInt32(categoriesFromDB[0].CategoryID)); // convert from uint
        }


        [Test]
        public async Task Delete_MultiRow()
        {
            // first insert 2 categories
            var categories = new Categories(ProviderName);
            var insertedCategory1 = await categories.InsertAsync(new { CategoryName = "Cat Delete_MR", Description = "cat 1 desc" });
            int category1ID = insertedCategory1.CategoryID;
            Assert.IsTrue(category1ID > 0);
            var insertedCategory2 = await categories.InsertAsync(new { CategoryName = "Cat Delete_MR", Description = "cat 2 desc" });
            int category2ID = insertedCategory2.CategoryID;
            Assert.IsTrue(category2ID > 0);

            Assert.AreEqual(2, await categories.DeleteAsync(where: "CategoryName=@0", args: (string)insertedCategory1.CategoryName), "Delete should affect 2 rows");
            var categoriesFromDB = await (await categories.AllAsync(where: "CategoryName=@0", args: (string)insertedCategory2.CategoryName)).ToListAsync();
            Assert.AreEqual(0, categoriesFromDB.Count);
        }


        [OneTimeTearDown]
        public async Task CleanUp()
        {
            var db = new MightyOrm(string.Format(TestConstants.WriteTestConnection, ProviderName));
            await db.ExecuteProcedureAsync("pr_clearAll");
        }
    }
}
#endif