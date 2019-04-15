using System;
using System.Collections;
using System.Collections.Async;
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
	[TestFixture("MySql.Data.MySqlClient")]
#if !(NETCOREAPP || NETSTANDARD)
	[TestFixture("Devart.Data.MySql")]
#endif
	public class AsyncSPTests
	{
		private string ProviderName;

		/// <summary>
		/// Initialise tests for given provider
		/// </summary>
		/// <param name="providerName">Provider name</param>
		public AsyncSPTests(string providerName)
		{
			ProviderName = providerName;
		}


		[Test]
		public async Task Procedure_Call()
		{
			var db = new SPTestsDatabase(ProviderName);
			var result = await db.ExecuteProcedureAsync("rewards_report_for_date", inParams: new { min_monthly_purchases = 3, min_dollar_amount_purchased = 20, report_date = new DateTime(2005, 5, 1) }, outParams: new { count_rewardees = 0 });
			Assert.AreEqual(27, result.count_rewardees);
		}


		/// <remarks>
		/// There's some non-trivial work behind the scenes in Massive.MySql.cs to make the two 
		/// providers return a bool when we expect them to.
		/// </remarks>
		[Test]
		public async Task Function_Call_Bool()
		{
			var db = new SPTestsDatabase(ProviderName);
			var result = await db.ExecuteProcedureAsync("inventory_in_stock",
											   inParams: new { p_inventory_id = 5 },
											   returnParams: new { retval = false });
			Assert.AreEqual(true, result.retval);
		}


		/// <remarks>
		/// Devart doesn't have an unsigned byte type, so has to put 0-255 into a short
		/// </remarks>
		[Test]
		public async Task Function_Call_Byte()
		{
			var db = new SPTestsDatabase(ProviderName);
			var result = await db.ExecuteProcedureAsync("inventory_in_stock",
											   inParams: new { p_inventory_id = 5 },
											   returnParams: new { retval = (byte)1 });
			if(ProviderName == "Devart.Data.MySql")
			{
				Assert.AreEqual(typeof(short), result.retval.GetType());
			}
			else
			{
				Assert.AreEqual(typeof(byte), result.retval.GetType());
			}
			Assert.AreEqual(1, result.retval);
		}


		/// <remarks>
		/// Again there's some non-trivial work behind the scenes in Massive.MySql.cs to make both 
		/// providers return a signed byte when we expect them to.
		/// </remarks>
		[Test]
		public async Task Function_Call_SByte()
		{
			var db = new SPTestsDatabase(ProviderName);
			var result = await db.ExecuteProcedureAsync("inventory_in_stock",
											   inParams: new { p_inventory_id = 5 },
											   returnParams: new { retval = (sbyte)1 });
			Assert.AreEqual(typeof(sbyte), result.retval.GetType());
			Assert.AreEqual(1, result.retval);
		}


		/// <summary>
		/// Now we can ask Massive to read the query results AND get the param values. Cool.
		/// 
		/// Because of yield return execution, results are definitely not available until at least one item has been read back.
		/// Becasue of the ADO.NET driver, results may not be available until all of the values have been read back (REF).
		/// </summary>
		[Test]
		public async Task Procedure_Call_Query_Plus_Results()
		{
			var db = new SPTestsDatabase(ProviderName);

            int count = 0;
            dynamic results;
            using (var command = db.CreateCommandWithParams("rewards_report_for_date",
                                                     inParams: new
                                                     {
                                                         min_monthly_purchases = 3,
                                                         min_dollar_amount_purchased = 20,
                                                         report_date = new DateTime(2005, 5, 1)
                                                     },
                                                     outParams: new
                                                     {
                                                         count_rewardees = 0
                                                     },
                                                     isProcedure: true))
            {
                var resultset = await db.QueryAsync(command);

                // read the result set
                await resultset.ForEachAsync(item =>
                {
                    count++;
                    Assert.AreEqual(typeof(string), item.last_name.GetType());
                    Assert.AreEqual(typeof(DateTime), item.create_date.GetType());
                });

                results = db.ResultsAsExpando(command);
            }

			Assert.Greater(results.count_rewardees, 0);
			Assert.AreEqual(count, results.count_rewardees);
		}


		// Massive style calls to some examples from https://www.devart.com/dotconnect/mysql/docs/Parameters.html#inoutparams
		#region Devart Examples
		[Test]
		public async Task In_Out_Params_SQL()
		{
			var _providerName = ProviderName;
			if(ProviderName == "MySql.Data.MySqlClient")
			{
				// this must be added to access user variables on the Oracle/MySQL driver
				_providerName += ";AllowUserVariables=true";
			}
			var db = new SPTestsDatabase(_providerName);
			// old skool SQL
			// this approach only works on the Oracle/MySQL driver if "AllowUserVariables=true" is included in the connection string
			var result = await db.ScalarAsync("CALL testproc_in_out(10, @param2); SELECT @param2");
			Assert.AreEqual((long)20, result);
		}


		[Test]
		public async Task In_Out_Params_SP()
		{
			var db = new SPTestsDatabase(ProviderName);
			// new skool
			var result = await db.ExecuteProcedureAsync("testproc_in_out", inParams: new { param1 = 10 }, outParams: new { param2 = 0 });
			Assert.AreEqual(20, result.param2);
		}


		[Test]
		public async Task InOut_Param_SP()
		{
			var db = new SPTestsDatabase(ProviderName);
			var result = await db.ExecuteProcedureAsync("testproc_inout", ioParams: new { param1 = 10 });
			Assert.AreEqual(20, result.param1);
		}
		#endregion
	}
}
