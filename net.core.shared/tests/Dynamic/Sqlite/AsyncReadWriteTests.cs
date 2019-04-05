using System;
using System.Collections.Async;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Mighty.Dynamic.Tests.Sqlite.TableClasses;
using System.Threading.Tasks;
using System.Threading;

namespace Mighty.Dynamic.Tests.Sqlite
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
		public async Task Guid_Arg()
		{
			var db = new MightyOrm(TestConstants.ReadWriteTestConnection);
			var guid = Guid.NewGuid();
			var command = db.CreateCommand("SELECT @0 AS val", null, guid);
#if (NETCOREAPP || NETSTANDARD)
			// For some reason .NET Core provider doesn't have DbType.Guid support even though .NET Framework provider does
			Assert.AreEqual(DbType.String, command.Parameters[0].DbType);
#else
			Assert.AreEqual(DbType.Guid, command.Parameters[0].DbType);
#endif
			var item = await db.SingleAsync(command);
			// The output from the provider is a bunch of bytes either way, so we stick with the provider
			// default here (especially since it is the same in both cases).
			Assert.AreEqual(typeof(byte[]), item.val.GetType());
			Assert.AreEqual(guid, new Guid(item.val));
		}


		[Test]
		public async Task All_NoParameters()
		{
			var albums = new Album();
			var allRows = await (await albums.AllAsync()).ToListAsync();
			Assert.AreEqual(347, allRows.Count);
			foreach(var a in allRows)
			{
				Console.WriteLine("{0} {1}", a.AlbumId, a.Title);
			}
		}


		[Test]
		public async Task All_NoParameters_RespondsToCancellation()
		{
			using (CancellationTokenSource cts = new CancellationTokenSource())
			{
				var albums = new Album();
				var allRows = await albums.AllAsync(cts.Token);
				int count = 0;
				Assert.ThrowsAsync<TaskCanceledException>(async () => {
					await allRows.ForEachAsync(a => {
						Console.WriteLine("{0} {1}", a.AlbumId, a.Title);
						count++;
						if (count == 12)
						{
							cts.Cancel();
						}
					});
				});
				Assert.AreEqual(12, count);
			}
		}


		[Test]
		public async Task All_LimitSpecification()
		{
			var albums = new Album();
			var allRows = await (await albums.AllAsync(limit: 10)).ToListAsync();
			Assert.AreEqual(10, allRows.Count);
		}


		[Test]
		public async Task All_WhereSpecification_OrderBySpecification()
		{
			var albums = new Album();
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
			var albums = new Album();
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
			var albums = new Album();
			// no order by, so in theory this is useless. It will order on PK though
			var page2 = await albums.PagedAsync(currentPage: 3, pageSize: 13);
			var pageItems = ((IEnumerable<dynamic>)page2.Items).ToList();
			Assert.AreEqual(13, pageItems.Count);
			Assert.AreEqual(27, pageItems[0].AlbumId);
			Assert.AreEqual(347, page2.TotalRecords);
		}


		[Test]
		public async Task Paged_OrderBySpecification()
		{
			var albums = new Album();
			var page2 = await albums.PagedAsync(orderBy: "Title DESC", currentPage: 3, pageSize: 13);
			var pageItems = ((IEnumerable<dynamic>)page2.Items).ToList();
			Assert.AreEqual(13, pageItems.Count);
			Assert.AreEqual(174, pageItems[0].AlbumId);
			Assert.AreEqual(347, page2.TotalRecords);
		}


		[Test]
		public async Task Insert_SingleRow()
		{
			var playlists = new Playlist();
			var inserted = await playlists.InsertAsync(new { Name = "MassivePlaylist" });
			Assert.IsTrue(inserted.PlaylistId > 0);
		}


		[OneTimeTearDown]
		public async Task CleanUp()
		{
			// delete all rows with ProductName 'Massive Product'. 
			var playlists = new Playlist();
			await playlists.DeleteAsync("Name=@0", "MassivePlaylist");
		}
	}
}
