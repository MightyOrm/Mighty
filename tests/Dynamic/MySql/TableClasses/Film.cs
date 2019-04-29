using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mighty.Validation;

namespace Mighty.Dynamic.Tests.MySql.TableClasses
{
    public class Film : MightyOrm
    {
        public Film(string providerName, bool includeSchema = true, string columns = null) :
            base(string.Format(TestConstants.ReadTestConnection, providerName), includeSchema ? "sakila.film" : "film", "film_id", columns: columns, validator: new FilmValidator())
        {
        }


        public class FilmValidator : Validator
        {
            override public void Validate(dynamic item, Action<object> reportError)
            {
                // bogus validation: isn't valid if rental_duration > 5

                if (item.rental_duration > 5)
                {
                    reportError("rental_duration > 5");
                }
            }
        }
    }
}
