namespace Mighty
{
    /// <summary>
    /// Mighty-specific support for database cursors (only for use on those providers on which passing a cursor out to external code makes sense).
    /// </summary>
	public class Cursor
	{
		internal object Value { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="value">
        /// When the cursor is an input parameter, this should be the database-specific cursor reference obtained from a previous database command.
        /// When the cursor is an output parameter, omit this parameter.
        /// </param>
		public Cursor(object value = null)
		{
			Value = value;
		}
	}
}