using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mighty.Validation;

namespace Mighty.Generic.Tests.MySql.TableClasses
{
    // Test fields
    public class Film
    {
#pragma warning disable IDE1006
        public int film_id;
        public int rental_duration;
        public DateTime last_update;
        public string description;
        public int language_id;
#pragma warning restore IDE1006
    }

    public class Films : MightyOrm<Film>
    {
        public Films(string providerName) : this(providerName, true)
        {
        }

        public Films(string providerName, bool includeSchema) :
            base(string.Format(TestConstants.ReadTestConnection, providerName), includeSchema ? "sakila.film" : "film", "film_id", validator : new FilmValidator())
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
