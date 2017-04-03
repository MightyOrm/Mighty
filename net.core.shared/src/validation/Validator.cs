using System;

namespace Mighty
{
	// override this class to make a validator for your table items
	public abstract class Validator
	{
		// Set this to true if you want validation to stop after the first item with an error , rather than
		// gathering up all the errors for all items.
		public bool Lazy { get; protected set; } = false;

		// all errors encountered in current run
		public List<string> Errors;

		// Reset to empty error list
		virtual public void Init()
		{
			Errors = new List<string>();
		}

		// Add as many errors as you need to the Errors list while processing this.
		// If this returns false for any item or items which are to be inserted/updated/deleted then none of them will be.
		abstract public bool Validate(dynamic item);

		// Get all the errors in the error list
		virtual public List<string> GetErrors()
		{
			return Errors;
		}
	}
}