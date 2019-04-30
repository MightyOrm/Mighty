namespace Mighty.Validation
{
    /// <summary>
    /// Specifies the type of prevalidation (if any) to use in a <see cref="Validator"/>.
    /// </summary>
    public enum Prevalidation
    {
        /// <summary>
        /// No prevalidation
        /// </summary>
        Off,

        /// <summary>
        /// Stop prevalidation as soon as the first item fails
        /// </summary>
        Lazy,

        /// <summary>
        /// Continue prevalidation for all items (so as to accumulate all errors from all items)
        /// </summary>
        Full
    }
}
