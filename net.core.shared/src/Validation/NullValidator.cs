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
        /// <param name="item">The item to be validated</param>
        /// <param name="reportError">Call <paramref name="reportError"/>(object) to add errors to the error list. You may choose to add strings, or a more complex object if you wish.
        /// NB Adding one or more errors indicates that the item fails, adding no errors indicates success.</param>
        override public void Validate(dynamic item, Action<object> reportError) { }
    }
}
