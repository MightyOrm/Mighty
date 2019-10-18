#if !NET40
using System;
using Dasync.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Mighty.Generic.Tests.Sqlite.TableClasses;
using System.Threading.Tasks;

namespace Mighty.Generic.Tests.Sqlite
{
    /// <summary>
    /// Specific tests for code which is specific to Sqlite. This means there are fewer tests than for SQL Server, as logic that's covered there already doesn't have to be
    /// retested again here, as the tests are meant to see whether a feature works. Tests are designed to touch the code in Massive.Sqlite. 
    /// </summary>
    /// <remarks>Tests use the Chinook example DB (https://chinookdatabase.codeplex.com/releases/view/55681), autonumber variant. 
    /// Writes are done on Playlist, reads on other tables.</remarks>
    [TestFixture]
    public class AsyncReadWriteTests
    {
        [Test]
        public async Task All_NoParameters()
        {
            var albums = new Albums();
            var allRows = await (await albums.AllAsync()).ToListAsync();
            Assert.AreEqual(347, allRows.Count);
            foreach(var a in allRows)
            {
                Console.WriteLine("{0} {1}", a.AlbumId, a.Title);
            }
        }

        [Test]
        public async Task All_LimitSpecification()
        {
            var albums = new Albums();
            var allRows = await (await albums.AllAsync(limit: 10)).ToListAsync();
            Assert.AreEqual(10, allRows.Count);
        }


        [Test]
        public async Task All_WhereSpecification_OrderBySpecification()
        {
            var albums = new Albums();
            var allRows = await (await albums.AllAsync(orderBy: "Title DESC", where: "WHERE ArtistId=@0", args: 90)).ToListAsync();
            Assert.AreEqual(21, allRows.Count);
            string previous = string.Empty;
            foreach(var r in allRows)
            {
                string current = r.Title;
                Assert.IsTrue(string.IsNullOrEmpty(previous) || string.Compare(previous, current) > 0);
                previous = current;
            }
        }


        [Test]
        public async Task All_WhereSpecification_OrderBySpecification_LimitSpecification()
        {
            var albums = new Albums();
            var allRows = await (await albums.AllAsync(limit: 6, orderBy: "Title DESC", where: "ArtistId=@0", args: 90)).ToListAsync();
            Assert.AreEqual(6, allRows.Count);
            string previous = string.Empty;
            foreach(var r in allRows)
            {
                string current = r.Title;
                Assert.IsTrue(string.IsNullOrEmpty(previous) || string.Compare(previous, current) > 0);
                previous = current;
            }
        }


        [Test]
        public async Task Paged_NoSpecification()
        {
            var albums = new Albums();
            // no order by, and paged queries logically must have an order by; this will order on PK
            var page3 = await albums.PagedAsync(currentPage: 3, pageSize: 13);
            var pageItems = page3.Items.ToList();
            Assert.AreEqual(13, pageItems.Count);
            Assert.AreEqual(27, pageItems[0].AlbumId);
            Assert.AreEqual(347, page3.TotalRecords);
        }


        [Test]
        public async Task Paged_WhereSpecification()
        {
            var albums = new Albums();
            var page2 = await albums.PagedAsync(currentPage: 2, where: "Title LIKE @0", args: "%the%");
            var pageItems = page2.Items.ToList();
            Assert.AreEqual(20, pageItems.Count);
            Assert.AreEqual(105, pageItems[0].AlbumId);
            Assert.AreEqual(80, page2.TotalRecords);
        }


        [Test]
        public async Task Paged_WhereSpecification_WithParams()
        {
            var albums = new Albums();
            var page2 = await albums.PagedWithParamsAsync(currentPage: 2, where: "Title LIKE @Title", inParams: new { Title = "%the%" });
            var pageItems = page2.Items.ToList();
            Assert.AreEqual(20, pageItems.Count);
            Assert.AreEqual(105, pageItems[0].AlbumId);
            Assert.AreEqual(80, page2.TotalRecords);
        }


        [Test]
        public async Task Paged_OrderBySpecification()
        {
            var albums = new Albums();
            var page2 = await albums.PagedAsync(orderBy: "Title DESC", currentPage: 3, pageSize: 13);
            var pageItems = page2.Items.ToList();
            Assert.AreEqual(13, pageItems.Count);
            Assert.AreEqual(174, pageItems[0].AlbumId);
            Assert.AreEqual(347, page2.TotalRecords);
        }


        [Test]
        public async Task Insert_SingleRow()
        {
            var playlists = new Playlists();
            var inserted = await playlists.InsertAsync(new { Name = "MassivePlaylist" });
            Assert.IsTrue(inserted.PlaylistId > 0);
        }


        [OneTimeTearDown]
        public async Task CleanUp()
        {
            // delete all rows with ProductName 'Massive Product'. 
            var playlists = new Playlists();
            await playlists.DeleteAsync("Name=@0", "MassivePlaylist");
        }
    }
}
#endif