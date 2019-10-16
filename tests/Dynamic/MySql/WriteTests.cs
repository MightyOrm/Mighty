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

using Mighty.Mapping;

namespace Mighty.Dynamic.Tests.MySql
{
    [TestFixture("MySql.Data.MySqlClient")]
#if !DISABLE_DEVART // Devart works fine on .NET Core, but I want to get a version to test with without paying $100 p/a!
    [TestFixture("Devart.Data.MySql")]
#endif
    public class WriteTests
    {
        private readonly string ProviderName;

        /// <summary>
        /// Initialise tests for given provider
        /// </summary>
        /// <param name="providerName">Provider name</param>
        public WriteTests(string providerName)
        {
            ProviderName = providerName;
        }


        [Test]
        public void Insert_SingleRow()
        {
            var categories = new Category(ProviderName);
            var inserted = categories.Insert(new { CategoryName = "Cool stuff", Description = "You know... cool stuff! Cool. n. stuff." });
            int insertedCategoryID = inserted.CategoryID;
            Assert.IsTrue(insertedCategoryID > 0);
        }


        [Test]
        public void Insert_MultipleRows()
        {
            var categories = new Category(ProviderName);
            var toInsert = new List<dynamic>();
            var CategoryName = "Cat Insert_MR";
            toInsert.Add(new { CategoryName, Description = "cat 1 desc" });
            toInsert.Add(new { CategoryName, Description = "cat 2 desc" });
            var inserted = categories.Insert(toInsert.ToArray());
            var selected = categories.All(where: "CategoryName=@0", orderBy: "CategoryID", args: CategoryName).ToList();
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
        public void Update_SingleRow()
        {
            var categories = new Category(ProviderName);
            // insert something to update first. 
            var inserted = categories.Insert(new { CategoryName = "Cool stuff", Description = "You know... cool stuff! Cool. n. stuff." });
            int insertedCategoryID = inserted.CategoryID;
            Assert.IsTrue(insertedCategoryID > 0);
            // update it, with a better description
            inserted.Description = "This is all jolly marvellous";
            Assert.AreEqual(1, categories.Update(inserted), "Update should have affected 1 row");
            var updatedRow = categories.Single(new { inserted.CategoryID });
            Assert.IsNotNull(updatedRow);
            Assert.AreEqual(inserted.CategoryID, Convert.ToInt32(updatedRow.CategoryID)); // convert from uint
            Assert.AreEqual(inserted.Description, updatedRow.Description);
            // reset description to NULL
            updatedRow.Description = null;
            Assert.AreEqual(1, categories.Update(updatedRow), "Update should have affected 1 row");
            var newUpdatedRow = categories.Single(new { updatedRow.CategoryID });
            Assert.IsNotNull(newUpdatedRow);
            Assert.AreEqual(updatedRow.CategoryID, newUpdatedRow.CategoryID);
            Assert.AreEqual(updatedRow.Description, newUpdatedRow.Description);
        }


        [Test]
        public void Update_SingleRow_MappedExpando()
        {
            // Apply some quick crazy-ass mapping... to an ExpandoObject :-)
            // Remember, we're mapping from crazy fake 'class' names to the sensible underlying column names
            var categories = new MightyOrm(
                string.Format(TestConstants.WriteTestConnection, ProviderName),
                "MassiveWriteTests.Categories",
                primaryKeys: "MYCATEGORYID",
                columns: "MYCATEGORYID, TheName, ItsADescription",
                mapper: new SqlNamingMapper(columnNameMapping: (t, c) => c
                    // 'class' names should come first
                    .Map("MYCATEGORYID", "CategoryID")
                    .Map("TheName", "CategoryName")
                    .Map("ItsADescription", "Description")));
            // insert something to update first. 
            var inserted = categories.Insert(new { TheName = "Cool stuff", ItsADescription = "You know... cool stuff! Cool. n. stuff." });
            int insertedCategoryID = inserted.MYCATEGORYID;
            Assert.IsTrue(insertedCategoryID > 0);
            // update it, with a better description
            inserted.ItsADescription = "This is all jolly marvellous";
            Assert.AreEqual(1, categories.Update(inserted), "Update should have affected 1 row");
            var updatedRow = categories.Single(new { inserted.MYCATEGORYID });
            Assert.IsNotNull(updatedRow);
            Assert.AreEqual(inserted.MYCATEGORYID, Convert.ToInt32(updatedRow.MYCATEGORYID)); // convert from uint
            Assert.AreEqual(inserted.ItsADescription, updatedRow.ItsADescription);
            // reset description to NULL
            updatedRow.ItsADescription = null;
            Assert.AreEqual(1, categories.Update(updatedRow), "Update should have affected 1 row");
            var newUpdatedRow = categories.Single(new { updatedRow.MYCATEGORYID });
            Assert.IsNotNull(newUpdatedRow);
            Assert.AreEqual(updatedRow.MYCATEGORYID, newUpdatedRow.MYCATEGORYID);
            Assert.AreEqual(updatedRow.ItsADescription, newUpdatedRow.ItsADescription);
        }


        [Test]
        public void Update_MultipleRows()
        {
            // first insert 2 categories and 4 products, one for each category
            var categories = new Category(ProviderName);
            var insertedCategory1 = categories.Insert(new { CategoryName = "Category 1", Description = "Cat 1 desc" });
            int category1ID = insertedCategory1.CategoryID;
            Assert.IsTrue(category1ID > 0);
            var insertedCategory2 = categories.Insert(new { CategoryName = "Category 2", Description = "Cat 2 desc" });
            int category2ID = insertedCategory2.CategoryID;
            Assert.IsTrue(category2ID > 0);

            var products = new Product(ProviderName);
            for(int i = 0; i < 4; i++)
            {
                var category = i % 2 == 0 ? insertedCategory1 : insertedCategory2;
                var p = products.Insert(new { ProductName = "Prod" + i, category.CategoryID });
                Assert.IsTrue(p.ProductID > 0);
            }
            var allCat1Products = products.All(where: "WHERE CategoryID=@0", args: category1ID).ToArray();
            Assert.AreEqual(2, allCat1Products.Length);
            foreach(var p in allCat1Products)
            {
                Assert.AreEqual(category1ID, Convert.ToInt32(p.CategoryID)); // convert from uint
                p.CategoryID = category2ID;
            }
            Assert.AreEqual(2, products.Save(allCat1Products));
        }


        [Test]
        public void Delete_SingleRow()
        {
            // first insert 2 categories
            var categories = new Category(ProviderName);
            var insertedCategory1 = categories.Insert(new { CategoryName = "Cat Delete_SR", Description = "cat 1 desc" });
            int category1ID = insertedCategory1.CategoryID;
            Assert.IsTrue(category1ID > 0);
            var insertedCategory2 = categories.Insert(new { CategoryName = "Cat Delete_SR", Description = "cat 2 desc" });
            int category2ID = insertedCategory2.CategoryID;
            Assert.IsTrue(category2ID > 0);

            Assert.AreEqual(1, categories.Delete(category1ID), "Delete should affect 1 row");
            var categoriesFromDB = categories.All(where: "CategoryName=@0", args: (string)insertedCategory2.CategoryName).ToList();
            Assert.AreEqual((long)1, categoriesFromDB.Count);
            Assert.AreEqual(category2ID, Convert.ToInt32(categoriesFromDB[0].CategoryID)); // convert from uint
        }


        [Test]
        public void Delete_MultiRow()
        {
            // first insert 2 categories
            var categories = new Category(ProviderName);
            var insertedCategory1 = categories.Insert(new { CategoryName = "Cat Delete_MR", Description = "cat 1 desc" });
            int category1ID = insertedCategory1.CategoryID;
            Assert.IsTrue(category1ID > 0);
            var insertedCategory2 = categories.Insert(new { CategoryName = "Cat Delete_MR", Description = "cat 2 desc" });
            int category2ID = insertedCategory2.CategoryID;
            Assert.IsTrue(category2ID > 0);

            Assert.AreEqual(2, categories.Delete(where: "CategoryName=@0", args: (string)insertedCategory1.CategoryName), "Delete should affect 2 rows");
            var categoriesFromDB = categories.All(where: "CategoryName=@0", args: (string)insertedCategory2.CategoryName).ToList();
            Assert.AreEqual(0, categoriesFromDB.Count);
        }


        [OneTimeTearDown]
        public void CleanUp()
        {
            var db = new MightyOrm(string.Format(TestConstants.WriteTestConnection, ProviderName));
            db.ExecuteProcedure("pr_clearAll");
        }
    }
}
