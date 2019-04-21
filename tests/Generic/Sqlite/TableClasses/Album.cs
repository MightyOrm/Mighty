using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mighty;
using Mighty.Generic.Tests;

namespace Mighty.Generic.Tests.Sqlite.TableClasses
{
    public class Album
    {
        public long AlbumId { get; set; }
        public string Title { get; set; }
    }

    public class Albums : MightyOrm<Album>
    {
        public Albums()
            : this(includeSchema: false)
        {
        }


        public Albums(bool includeSchema) :
            base(TestConstants.ReadWriteTestConnection, includeSchema ? "Album" : "Album", "AlbumId",
#if KEY_VALUES
                string.Empty,
#endif
                "last_insert_rowid()")
        {
        }
    }
}
