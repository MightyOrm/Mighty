using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mighty.Validation
{
    /// <summary>
    /// Validator which passes all tests.
    /// </summary>
	internal class NullValidator : Validator
    {
        /// <summary>
        /// Passing validation (just don't change the error list)
        /// </summary>
        /// <param name="action"></param>
        /// <param name="item"></param>
        /// <param name="Errors"></param>
        override public void Validate(OrmAction action, dynamic item, List<object> Errors) { }
    }
}
