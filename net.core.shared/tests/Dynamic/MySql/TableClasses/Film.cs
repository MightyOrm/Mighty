using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MightyOrm.Validation;

namespace MightyOrm.Dynamic.Tests.MySql.TableClasses
{
	public class Film : MightyOrm
	{
		public Film(string providerName) : this(providerName, true)
		{
		}


		public Film(string providerName, bool includeSchema) :
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
