namespace MightyOrm.Interfaces
{
	// Abstract class 'interface' for Npgsql cursor control additions.
	// These should ideally be contributed back to Npgsql ([ref]()), but for now are added to MightyOrm.
	// (Note: unfortunately it looks far from trivial to set up a full Npgsql build environment in order to create
	// a properly constructed and tested PR for that project. Which is not to say it won't be done at some point.)
	abstract public partial class MicroOrm<T> // NpgslCursorController
	{
		virtual public bool NpgsqlAutoDereferenceCursors { get; set; } = true;
		virtual public int NpgsqlAutoDereferenceFetchSize { get; set; } = 10000;
	}
}