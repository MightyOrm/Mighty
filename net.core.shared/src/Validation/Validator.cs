using System.Collections.Generic;

namespace MightyOrm.Validation
{
	public class NullValidator : Validator
	{

	}

	/// <summary>
	/// Override this class to make a validator for your table items.
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
		virtual public PrevalidationSetting PrevalidationSetting { get; set; } = PrevalidationSetting.Off;

		/// <summary>
		/// <see cref="Prevalidate" /> calls this one item at a time before any real actions are done.
		/// If any item fails, no real actions are done for any item.
		/// See also <see cref="PrevalidationSetting" />.
		/// If this returns false for any item or items which are to be inserted/updated/deleted then none of them will be.
		/// You might well just want to add strings as your error objects... but it is up to you.
		/// </summary>
		/// <param name="action">You could choose to ignore this and do the same validation for every action... or not.</param>
		/// <param name="item">The item to validate. NB this can be whatever you pass in as input objects, it is not restricted to items of type T
		/// even for the generically typed MightyOrm&lt;T&gt;.</param>
		/// <param name="Errors">Append your errors to this list. You may choose to append strings, or a more complex object if you wish.
		/// NB Adding one or more errors indicates that the item fails, adding nothing to the errors indicates success.</param>
		/// <returns></returns>
		/// <remarks>
		/// Item is not necessarily a representation of the item for action: for delete only, it might be a representation of just the PK depending on how .Delete was called.
		/// Despite this, you can write fairly stright-forward validators; have a look at the table classes in the generic tests in the source code.
		/// </remarks>
		virtual public void ValidateForAction(dynamic item, OrmAction action, List<object> Errors) { }

		/// <summary>
		/// This is called one item at time, just before the processing for that specific item.
		/// ORMAction is performed iff this returns true. If false is returned, no processing
		/// is done for this item, but processing still continues for all remaining items.
		/// </summary>
		/// <param name="action">The ORM action</param>
		/// <param name="item">The item for which the action is about to be performed.
		/// The type of this is NOT normalised, and depends on what you pass in.</param>
		/// <returns></returns>
		virtual public bool PerformingAction(dynamic item, OrmAction action) { return true; }

		// This is called one item at time, after processing for that specific item.
		virtual public void PerformedAction(dynamic item, OrmAction action) { }
	}
}