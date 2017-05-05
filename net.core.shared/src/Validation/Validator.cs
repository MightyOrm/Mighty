using System.Collections.Generic;

namespace Mighty.Validation
{
	// Override this class to make a validator for your table items.
	public class Validator
	{
		/// <summary>
		/// If true (default) the prevalidator will stop after the first item which gives an errors.
		/// If false, errors will be collected for all items, then the process will stop.
		/// </summary>
		/// <returns></returns>
		virtual public bool LazyPrevalidation { get; set; } = true;

		/// <summary>
		/// <see cref="Prevalidate" /> calls this one item at a time before any real actions are done.
		/// If any item fails, no real actions are done for any item.
		/// See also <see cref="LazyPrevalidation" />.
		/// If this returns false for any item or items which are to be inserted/updated/deleted then none of them will be.
		/// You may well just want to add strings as your error objects... but it is up to you!
		/// </summary>
		/// <param name="action">You could choose to ignore this and do the same validation for every action... or not. Up to you!</param>
		/// <param name="item">The item to validate. NB this can be whatever you pass in as input objects.</param>
		/// <param name="Errors">Append your errors to this list. You may choose to append strings, or a more complex object if you wish. NB Adding one or more errors indicates that the item fails, adding nothing to the errors indicates success.</param>
		/// <returns></returns>
		virtual public void ValidateForAction(dynamic item, ORMAction action, List<object> Errors) { }

		/// <summary>
		/// This is called one item at time, just before the processing for that specific item.
		/// ORMAction is performed iff this returns true. If false is returned, no processing
		/// is done for this item, but processing still continues for all remaining items.
		/// </summary>
		/// <param name="action">The ORM action</param>
		/// <param name="item">The item for which the action is about to be performed.
		/// The type of this is NOT normalised, and depends on what you pass in.</param>
		/// <returns></returns>
		virtual public bool PerformingAction(dynamic item, ORMAction action) { return true; }

		// This is called one item at time, after processing for that specific item.
		virtual public void PerformedAction(dynamic item, ORMAction action) { }
	}
}