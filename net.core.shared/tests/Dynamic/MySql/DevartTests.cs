#if !COREFX
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Mighty.Dynamic.Tests.MySql.TableClasses;
using NUnit.Framework;

namespace Mighty.Dynamic.Tests.MySql
{
	[TestFixture]
	public class DevartTests
	{
		private string ProviderName = "Devart.Data.MySql";

		// Massive style calls to some examples from https://www.devart.com/dotconnect/mysql/docs/Parameters.html#inoutparams
		#region Devart Examples
		
		/// <remarks>
		/// Demonstrates that this Devart-specific syntax is possible in Massive;
		/// although it pretty much stops looking much like Massive when used like this.
		/// </remarks>
		[Test]
		public async Task Devart_ParameterCheck()
		{
			var db = new SPTestsDatabase(ProviderName);
			var connection = await db.OpenConnectionAsync();
			var command = db.CreateCommandWithParams("testproc_in_out", isProcedure: true, connection: connection);
			((dynamic)command).ParameterCheck = true; // dynamic trick to set the underlying property
			command.Prepare(); // makes a round-trip to the database
			command.Parameters["param1"].Value = 10;
			await db.ExecuteAsync(command);
			var result = db.ResultsAsExpando(command);
			Assert.AreEqual(20, result.param2);
		}
		#endregion
	}
}
#endif
