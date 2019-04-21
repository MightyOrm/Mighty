namespace Mighty.Validation
{
    /// <summary>
    /// List of possible ORM actions.
    /// </summary>
    public enum OrmAction
    {
        /// <summary>Save (insert if new, update if pre-existing)</summary>
        Save,
        /// <summary>Insert (insert new object)</summary>
        Insert,
        /// <summary>Update (update existing object)</summary>
        Update,
        /// <summary>Delete</summary>
        Delete
    }
}