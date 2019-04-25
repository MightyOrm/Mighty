using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Mighty.Generic.Tests.SqlServer.TableClasses;
using NUnit.Framework;

namespace Mighty.Generic.Tests.SqlServer
{
    [TestFixture]
    public class WriteTests
    {
        [Test]
        public void Insert_SingleRow()
        {
            var categories = new Categories();
            var inserted = categories.Insert(new { CategoryName = "Cool stuff", Description = "You know... cool stuff! Cool. n. stuff." });
            int insertedCategoryID = inserted.CategoryID;
            Assert.IsTrue(insertedCategoryID > 0);
        }


        [Test]
        public void Insert_FromNew()
        {
            var categories = new Categories();
            var toInsert = categories.New();
            toInsert.CategoryName = "Cool stuff";
            toInsert.Description = "You know... cool stuff! Cool. n. stuff.";
            var inserted = categories.Insert(toInsert);
            int insertedCategoryID = inserted.CategoryID;
            Assert.IsTrue(insertedCategoryID > 0);
        }


        [Test]
        public void Insert_MultipleRows()
        {
            var categories = new Categories();
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
            var categories = new Categories();
            // insert something to update first. 
            Category inserted = categories.Insert(new { CategoryName = "Cool stuff", Description = "You know... cool stuff! Cool. n. stuff." });
            int insertedCategoryID = inserted.CategoryID;
            Assert.IsTrue(insertedCategoryID > 0);
            // update it, with a better description
            inserted.Description = "This is all jolly marvellous";
            Assert.AreEqual(1, categories.Update(inserted), "Update should have affected 1 row");
            Category updatedRow = categories.Single(new { inserted.CategoryID });
            Assert.IsNotNull(updatedRow);
            Assert.AreEqual(inserted.CategoryID, updatedRow.CategoryID);
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
        public void Update_MultipleRows()
        {
            // first insert 2 categories and 4 products, one for each category
            var categories = new Categories();
            var insertedCategory1 = categories.Insert(new {CategoryName = "Category 1", Description = "Cat 1 desc"});
            int category1ID = insertedCategory1.CategoryID;
            Assert.IsTrue(category1ID > 0);
            var insertedCategory2 = categories.Insert(new { CategoryName = "Category 2", Description = "Cat 2 desc" });
            int category2ID = insertedCategory2.CategoryID;
            Assert.IsTrue(category2ID > 0);

            var products = new Products();
            for(int i = 0; i < 4; i++)
            {
                var category = i % 2 == 0 ? insertedCategory1 : insertedCategory2;
                var p = products.Insert(new {ProductName = "Prod" + i, category.CategoryID});
                Assert.IsTrue(p.ProductID > 0);
            }
            var allCat1Products = products.All(where:"WHERE CategoryID=@0", args:category1ID).ToArray();
            Assert.AreEqual(2, allCat1Products.Length);
            foreach(var p in allCat1Products)
            {
                Assert.AreEqual(category1ID, p.CategoryID);
                p.CategoryID = category2ID;
            }
            Assert.AreEqual(2, products.Save(allCat1Products));
        }


        [Test]
        public void Delete_SingleRow()
        {
            // first insert 2 categories
            var categories = new Categories();
            var insertedCategory1 = categories.Insert(new { CategoryName = "Cat Delete_SR", Description = "cat 1 desc" });
            int category1ID = insertedCategory1.CategoryID;
            Assert.IsTrue(category1ID > 0);
            var insertedCategory2 = categories.Insert(new { CategoryName = "Cat Delete_SR", Description = "cat 2 desc" });
            int category2ID = insertedCategory2.CategoryID;
            Assert.IsTrue(category2ID > 0);

            Assert.AreEqual(1, categories.Delete(category1ID), "Delete should affect 1 row");
            var categoriesFromDB = categories.All(where:"CategoryName=@0", args:(string)insertedCategory2.CategoryName).ToList();
            Assert.AreEqual(1, categoriesFromDB.Count);
            Assert.AreEqual(category2ID, categoriesFromDB[0].CategoryID);
        }


        [Test]
        public void Delete_MultiRow()
        {
            // first insert 2 categories
            var categories = new Categories();
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
            var db = new MightyOrm(TestConstants.WriteTestConnection);
            db.ExecuteProcedure("pr_clearAll");
        }
    }
}
