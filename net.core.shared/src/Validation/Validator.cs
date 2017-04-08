using System.Collections.Generic;

namespace Mighty.Validation
{
	public enum Action
	{
		Save,
		Insert,
		Update,
		Delete
	}

	// Override this class to make a validator for your table items.
	// You may well just want to make T be string, but you have the option for more complex types.
	// TO DO: How do I send errors back; can I send a true object somewhere in the exception?
	public abstract class Validator
	{
		// If true (default) the prevalidator will stop after the first item which gives an errors.
		// If false, errors will be collected all items, then the process will stop.
		public bool PrevalidationIsLazy { get; protected set; } = true;

		// <see cref="PevalidateActions"> calls this one item at a time before any real actions are done.
		// If any item fails, no real actions are done for any item.
		// See also <see cref="PrevalidationIsLazy"/>.
		// Add as many error objects as you need to the <see cref="Errors"/> while processing this.
		// If this returns false for any item or items which are to be inserted/updated/deleted then none of them will be.
		abstract public bool PrevalidateAction(Action action, dynamic item, List<object> Errors);

		// This is called one item at time, just before the processing for that specific item.
		// Action is performed iff this returns true. If false is returned, no processing
		// is done for this item, but processing still continues for all remaining items.
		abstract public bool PerformAction(Action action, dynamic item);

		// This is called one item at time, after processing for that specific item.
		abstract public void PerformedAction(Action action, dynamic item);

		// TO DO: accessibility?
		virtual public void PrevalidateActions(Action action, object[] items)
		{
			// Intention of non-shared error list is thread safety
			List<object> Errors = new List<object>();
			bool valid = true;
			foreach (var item in items)
			{
				if (!PrevalidateAction(action, item, Errors))
				{
					valid = false;
					if (PrevalidationIsLazy) break;
				}
			}
			if (valid == false || Errors.Count> 0)
			{
				throw new ValidationException(Errors, "Prevalidation failed for one or more items for " + action.ToString());
			}
		}
	}
}