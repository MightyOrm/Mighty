namespace Mighty.Interfaces
{
	// Abstract class 'interface' for Npgsql cursor control additions.
	// These should ideally be contributed back to Npgsql ([ref]()), but for now are added to MightyORM.
	// (Note: it unfortunately does look far from trivial to set up a full Npgsql build environment in order to create
	// a properly constructed and tested PR for that project. Which is not to say it won't be done at some point.)
	abstract public class NpgslCursorController
	{
		public bool NpgsqlAutoDereferenceCursors { get; set; } = true;
		public int NpgsqlAutoDereferenceFetchSize { get; set; } = 10000;
	}
}