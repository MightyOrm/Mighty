using System;

namespace Mighty
{
	// override this class to make a validator for your table items
	public abstract class Validator<T> where T : new()
	{
		// make this true if you want validation to stop on the first error, rather than gathering up all the errors
		public bool Lazy { get; protected set; } = false;

		// all errors encountered in current run
		public T Errors;

		// initialise your own way of storing validation errors
		virtual public void Init()
		{
			Errors = new T();
		}

		// if this returns true for any of the items to be inserted/updated/deleted, then none of them will be
		// add Errors as you got to Errors list
		abstract public bool Validate(dynamic item);

		virtual public T GetErrors()
		{
			return Errors;
		}
	}
}