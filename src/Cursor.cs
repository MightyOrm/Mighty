namespace Mighty
{
    /// <summary>
    /// Mighty-specific support for database cursors (only for use on those providers on which passing a cursor out to external code makes sense).
    /// </summary>
    public class Cursor
    {
        /// <summary>
        /// The cursor ref for the underlying ADO.NET database provider
        /// </summary>
        public object CursorRef { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="value">
        /// When the cursor is an input parameter, this should be the database-specific cursor reference obtained from a previous database command.
        /// When the cursor is an output parameter, omit this parameter.
        /// </param>
        public Cursor(object value = null)
        {
            CursorRef = value;
        }

        /// <summary>
        /// String representation of the type of this object and what it contains
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{base.ToString()}: {(CursorRef == null ? "null" : (CursorRef is string ? $@"""{(string)CursorRef}""" : CursorRef.ToString()))}";
        }
    }
}