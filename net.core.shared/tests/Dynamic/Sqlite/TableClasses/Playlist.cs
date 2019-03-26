using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MightyOrm;
using MightyOrm.Dynamic.Tests;

namespace MightyOrm.Dynamic.Tests.Sqlite.TableClasses
{
	public class Playlist : MightyOrm
	{
		public Playlist()
			: this(includeSchema: false)
		{
		}


		public Playlist(bool includeSchema) :
			base(TestConstants.ReadWriteTestConnection, includeSchema ? "Playlist" : "Playlist", "PlaylistId", string.Empty, "last_insert_rowid()")
		{
		}
	}
}
