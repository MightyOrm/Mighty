using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mighty;
using Mighty.Generic.Tests;

namespace Mighty.Generic.Tests.Sqlite.TableClasses
{
	public class Playlist
	{
		public int PlaylistId { get; set; }
	}

	public class Playlists : MightyOrm<Playlist>
	{
		public Playlists()
			: this(includeSchema: false)
		{
		}


		public Playlists(bool includeSchema) :
			base(TestConstants.ReadWriteTestConnection, includeSchema ? "Playlist" : "Playlist", "PlaylistId", string.Empty, "last_insert_rowid()")
		{
		}
	}
}
