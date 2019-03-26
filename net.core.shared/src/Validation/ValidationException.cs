using System;
using System.Collections.Generic;

namespace MightyOrm.Validation
{
	// this is thrown by Mighty when validation fails
	public class ValidationException : Exception
	{
		public List<object> ErrorList;

		public ValidationException(List<object> errorList) : base()
		{
			ErrorList = errorList;
		}

		public ValidationException(List<object> errorList, string message) : base(message)
		{
			ErrorList = errorList;
		}

		public ValidationException(List<object> errorList, string message, Exception innerException) : base(message, innerException)
		{
			ErrorList = errorList;
		}
	}
}