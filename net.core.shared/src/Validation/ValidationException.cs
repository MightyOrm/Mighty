using System;
using System.Collections.Generic;

namespace Mighty.Validation
{
    /// <summary>
	/// This exception is thrown by Mighty when validation fails.
    /// </summary>
	public class ValidationException : Exception
	{
        /// <summary>
        /// List of validation errors
        /// </summary>
		public List<object> ErrorList;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="errorList">List of validation errors</param>
		public ValidationException(List<object> errorList) : base()
		{
			ErrorList = errorList;
		}

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="errorList">List of validation errors</param>
        /// <param name="message">Exception message</param>
        public ValidationException(List<object> errorList, string message) : base(message)
		{
			ErrorList = errorList;
		}

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="errorList">List of validation errors</param>
        /// <param name="message">Exception message</param>
        /// <param name="innerException">Inner exception</param>
		public ValidationException(List<object> errorList, string message, Exception innerException) : base(message, innerException)
		{
			ErrorList = errorList;
		}
	}
}