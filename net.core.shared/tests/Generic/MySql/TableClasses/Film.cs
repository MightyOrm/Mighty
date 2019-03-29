using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mighty.Validation;

namespace Mighty.Generic.Tests.MySql.TableClasses
{
	public class Film
	{
		public int film_id { get; set; }
		public int rental_duration { get; set; }
		public DateTime last_update { get; set; }
		public string description { get; set; }
		public int language_id { get; set; }
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
			override public void ValidateForAction(dynamic item, OrmAction action, List<object> Errors)
			{
				// bogus validation: isn't valid if rental_duration > 5

				if (item.rental_duration > 5)
				{
					Errors.Add("rental_duration > 5");
				}
			}
		}
	}
}
