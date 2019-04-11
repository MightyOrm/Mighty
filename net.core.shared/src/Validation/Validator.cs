using System.Collections.Generic;

namespace Mighty.Validation
{
    /// <summary>
    /// Validator which passes all tests.
    /// </summary>
	public class NullValidator : Validator
	{
        /// <summary>
        /// Passing validation (just don't change the error list)
        /// </summary>
        /// <param name="action"></param>
        /// <param name="item"></param>
        /// <param name="Errors"></param>
        override public void Validate(OrmAction action, dynamic item, List<object> Errors) { }
    }

    /// <summary>
    /// Override this class to make a validator for your table items.
    /// Note that because almost any type of item can be passed into most Mighty commands, this validator is NOT strongly typed.
    /// If you know you care only going to pass items of type T, you can just add a cast to your validation methods.
    /// </summary>
    /// <remarks>
    /// The objects passed to the validation callbacks are whatever was passed in to Save, Insert, Delete, etc., which can always be any reasonable object
    /// or collection to contain the data needed for the task at hand,  even when used with the generically typed version of MightyOrm&lt;T&gt;.
    /// (If you know you care only going to pass items of type T, you can just add a cast to your validation methods.)
    /// </remarks>
    abstract public class Validator
	{
		/// <summary>
		/// Determine whether and how to pre-validate lists of items before performing any action.
		/// Default is no pre-validation. Other options are to stop after the first item which gives any error,
		/// or to continue and collect all errors before stopping.
		/// </summary>
		/// <returns></returns>
		virtual public Prevalidation Prevalidation { get; set; } = Prevalidation.Off;

        /// <summary>
        /// If <see cref="Prevalidation" /> is enabled <see cref="MightyOrm"/> calls this one item at a time before *any* real actions are done.
        /// If any item fails, no actions are done for any item.
        /// You might well just want to add strings as your error objects, but it is up to you.
        /// Adding one or more errors counts as failing validation.
        /// </summary>
        /// <param name="action">You can choose to ignore this and do the same validation for every action.</param>
        /// <param name="item">The item to validate. NB this can be whatever you pass in as input objects, it is not restricted to items of type T
        /// even for the generically typed MightyOrm&lt;T&gt;.</param>
        /// <param name="Errors">Append your errors to this list. You may choose to append strings, or a more complex object if you wish.
        /// NB Adding one or more errors indicates that the item fails, adding nothing to the errors indicates success.</param>
        /// <returns></returns>
        /// <remarks>
        /// Item is not necessarily a representation of the item for action: for delete only, it might be a representation of just the PK depending on how .Delete was called.
        /// Despite this, you can write fairly stright-forward validators; have a look at the table classes in the generic tests in the source code.
        /// </remarks>
        abstract public void Validate(OrmAction action, dynamic item, List<object> Errors);

        /// <summary>
        /// This is called one item at time, just before the processing for that specific item.
        /// ORMAction is performed iff this returns true. If false is returned, no processing
        /// is done for this item, but processing still continues for all remaining items.
        /// </summary>
        /// <param name="item">The item for which the action is about to be performed.
        /// The type of this is NOT normalised, and depends on what you pass in.</param>
        /// <param name="action">The ORM action</param>
        /// <returns></returns>
        virtual public bool ShouldPerformAction(dynamic item, OrmAction action) { return true; }

        /// <summary>
        /// This is called one item at time, after processing for that specific item.
        /// </summary>
        /// <param name="item">The item for which the action is about to be performed.
        /// The type of this is NOT normalised, and depends on what you pass in.</param>
        /// <param name="action">The ORM action</param>
        /// <returns></returns>
        virtual public void HasPerformedAction(dynamic item, OrmAction action) { }
	}
}