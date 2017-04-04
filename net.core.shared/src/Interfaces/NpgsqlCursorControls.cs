using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace Mighty.Interfaces
{
	// Abstract class 'interface' for Npgsql cursor control additions.
    // These should ideally be contributed back to Npgsql ([ref]()), but for now are added to MightyORM, as it's far from
    // trivial to set up a full Npgsql build environment in order to create a properly constructed and tested PR for them.
	public abstract class NpgslCursorControls
	{
		public bool NpgsqlAutoDereferenceCursors { get; set; } = true;
		public int NpgsqlAutoDereferenceFetchSize { get; set; } = 10000;
	}
}