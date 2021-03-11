#if !NET40
using System;
using System.Collections;
using Dasync.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Mighty.Generic.Tests.SqlServer.TableClasses;
using NUnit.Framework;

namespace Mighty.Generic.Tests.SqlServer
{
    [TestFixture]
    public class AsyncWriteTests
    {
        [Test]
        public async Task Insert_SingleRow()
        {
            var categories = new Categories();
            var inserted = await categories.InsertAsync(new {CategoryName = "Cool stuff", Description = "You know... cool stuff! Cool. n. stuff."});
            int insertedCategoryID = inserted.CategoryID;
            Assert.IsTrue(insertedCategoryID > 0);
        }


        [Test]
        public async Task Insert_MultipleRows()
        {
            var categories = new Categories();
            var toInsert = new List<dynamic>();
            var CategoryName = "Cat Insert_MR";
            toInsert.Add(new { CategoryName, Description = "cat 1 desc" });
            toInsert.Add(new { CategoryName, Description = "cat 2 desc" });
            var inserted = await categories.InsertAsync(toInsert.ToArray());
            var selected = await (await categories.AllAsync(where: "CategoryName=@0", orderBy: "CategoryID", args: (string)toInsert[0].CategoryName)).ToListAsync();
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
        public async Task Update_SingleRow()
        {
            var categories = new Categories();
            // insert something to update first. 
            Category inserted = await categories.InsertAsync(new { CategoryName = "Cool stuff", Description = "You know... cool stuff! Cool. n. stuff." });
            int insertedCategoryID = inserted.CategoryID;
            Assert.IsTrue(insertedCategoryID > 0);
            // update it, with a better description
            inserted.Description = "This is all jolly marvellous";
            Assert.AreEqual(1, await categories.UpdateAsync(inserted), "Update should have affected 1 row");
            Category updatedRow = await categories.SingleAsync(new { inserted.CategoryID });
            Assert.IsNotNull(updatedRow);
            Assert.AreEqual(inserted.CategoryID, updatedRow.CategoryID);
            Assert.AreEqual(inserted.Description, updatedRow.Description);
            // reset description to NULL
            updatedRow.Description = null;
            Assert.AreEqual(1, await categories.UpdateAsync(updatedRow), "Update should have affected 1 row");
            var newUpdatedRow = await categories.SingleAsync(new { updatedRow.CategoryID });
            Assert.IsNotNull(newUpdatedRow);
            Assert.AreEqual(updatedRow.CategoryID, newUpdatedRow.CategoryID);
            Assert.AreEqual(updatedRow.Description, newUpdatedRow.Description);
        }


        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public async Task Update_MultipleRows(bool explicitConnection)
        {
            // first insert 2 categories and 4 products, one for each category
            var categories = new Categories(explicitConnection);
            DbConnection connection = null;
            if (explicitConnection)
            {
                MightyTests.ConnectionStringUtils.CheckConnectionStringRequiredForOpenConnectionAsync(categories);
                connection = await categories.OpenConnectionAsync(MightyTests.ConnectionStringUtils.GetConnectionString(TestConstants.WriteTestConnection, TestConstants.ProviderName));
            }
            using (connection)
            {
                var insertedCategory1 = await categories.InsertAsync(new { CategoryName = "Category 1", Description = "Cat 1 desc" }, connection);
                int category1ID = insertedCategory1.CategoryID;
                Assert.IsTrue(category1ID > 0);
                var insertedCategory2 = await categories.InsertAsync(new { CategoryName = "Category 2", Description = "Cat 2 desc" }, connection);
                int category2ID = insertedCategory2.CategoryID;
                Assert.IsTrue(category2ID > 0);

                var products = new Products(explicitConnection);
                if (explicitConnection)
                {
                    MightyTests.ConnectionStringUtils.CheckConnectionStringRequiredForOpenConnectionAsync(products);
                }
                for (int i = 0; i < 4; i++)
                {
                    var category = i % 2 == 0 ? insertedCategory1 : insertedCategory2;
                    var p = await products.InsertAsync(new { ProductName = "Prod" + i, category.CategoryID }, connection);
                    Assert.IsTrue(p.ProductID > 0);
                }
                var allCat1Products = await (await products.AllAsync(connection, where: "WHERE CategoryID=@0", args: category1ID)).ToArrayAsync();
                Assert.AreEqual(2, allCat1Products.Length);
                foreach (var p in allCat1Products)
                {
                    Assert.AreEqual(category1ID, p.CategoryID);
                    p.CategoryID = category2ID;
                }
                Assert.AreEqual(2, await products.SaveAsync(connection, allCat1Products));
            }
        }


        [Test]
        public async Task Delete_SingleRow()
        {
            // first insert 2 categories
            var categories = new Categories();
            var insertedCategory1 = await categories.InsertAsync(new { CategoryName = "Cat Delete_SR", Description = "cat 1 desc" });
            int category1ID = insertedCategory1.CategoryID;
            Assert.IsTrue(category1ID > 0);
            var insertedCategory2 = await categories.InsertAsync(new { CategoryName = "Cat Delete_SR", Description = "cat 2 desc" });
            int category2ID = insertedCategory2.CategoryID;
            Assert.IsTrue(category2ID > 0);

            Assert.AreEqual(1, await categories.DeleteAsync(category1ID), "Delete should affect 1 row");
            var categoriesFromDB = await (await categories.AllAsync(where:"CategoryName=@0", args:(string)insertedCategory2.CategoryName)).ToListAsync();
            Assert.AreEqual(1, categoriesFromDB.Count);
            Assert.AreEqual(category2ID, categoriesFromDB[0].CategoryID);
        }


        [Test]
        public async Task Delete_MultiRow()
        {
            // first insert 2 categories
            var categories = new Categories();
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
            var db = new MightyOrm(TestConstants.WriteTestConnection);
            await db.ExecuteProcedureAsync("pr_clearAll");
        }
    }
}
#endif