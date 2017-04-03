using System;

namespace Mighty
{
	// this is thrown by Mighty when validation fails
	public class ValidationException : Exception
	{
		public ValidationException() : base()
		{
			throw new Exception()
		}

		public ValidationException(string message) : base(message)
		{
		}

		public ValidationException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}