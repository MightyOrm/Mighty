using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mighty;
using Mighty.Dynamic.Tests;

namespace Mighty.Dynamic.Tests.Sqlite.TableClasses
{
	public class Playlist : MightyOrm
	{
		public Playlist()
			: this(includeSchema: false)
		{
		}


		public Playlist(bool includeSchema) :
			base(TestConstants.ReadWriteTestConnection, includeSchema ? "Playlist" : "Playlist", "PlaylistId",
#if KEY_VALUES
                string.Empty,
#endif
                "last_insert_rowid()")
		{
		}
	}
}
