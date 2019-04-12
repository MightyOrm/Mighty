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
        /// Passing validation (i.e. just do not modify the error list)
        /// </summary>
        /// <param name="action">The ORM action for which validation is being performed</param>
        /// <param name="item">The item to be validated</param>
        /// <param name="Errors">A pre-existing list of errors, which should be added to in the case that any errors are detected</param>
        override public void Validate(OrmAction action, dynamic item, List<object> Errors) { }
    }
}
