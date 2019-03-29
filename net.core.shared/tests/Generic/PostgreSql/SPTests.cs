using System;
using System.Data;
using System.Dynamic;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if !COREFX
using System.Transactions;
#endif
using Mighty.Generic.Tests.PostgreSql.TableClasses;
using NUnit.Framework;

namespace Mighty.Generic.Tests.PostgreSql
{
	/// <summary>
	/// Suite of tests for stored procedures, functions and cursors on PostgreSQL database.
	/// </summary>
	/// <remarks>
	/// Runs against functions and procedures which are created by running SPTests.sql script on the test database.
	/// These objects do not conflict with anything in the Northwind database, and can be added there.
	/// </remarks>
	[TestFixture]
	public class SPTests
	{
		[Test]
		public void DereferenceCursorOutputParameter()
		{
			var db = new Employees();
			// Unlike the Oracle data access layer, Npgsql v3 does not dereference cursor parameters.
			// We have added back the support for this which was previously in Npgsql v2.
			var employees = db.QueryFromProcedure("cursor_employees", outParams: new { refcursor = new Cursor() });
			int count = 0;
			foreach(var employee in employees)
			{
				Console.WriteLine(employee.firstname + " " + employee.lastname);
				count++;
			}
			Assert.AreEqual(9, count);
		}


#if !COREFX
		[Test]
		public void DereferenceFromQuery_ManualWrapping()
		{
			var db = new Employees();
			// without a cursor param, nothing will trigger the wrapping transaction support in Massive
			// so in this case we need to add the wrapping transaction manually (with TransactionScope or
			// BeginTransaction, see other examples in this file)
			int count = 0;
			using(var scope = new TransactionScope())
			{
				var employees = db.Query("SELECT * FROM cursor_employees()");
				foreach(var employee in employees)
				{
					Console.WriteLine(employee.firstname + " " + employee.lastname);
					count++;
				}
				scope.Complete();
			}
			Assert.AreEqual(9, count);
		}
#endif

		[Test]
		public void DereferenceFromQuery_AutoWrapping()
		{
			var db = new Employees();
			// use dummy cursor to trigger wrapping transaction support in Massive
			var employees = db.QueryWithParams("SELECT * FROM cursor_employees()", outParams: new { abc = new Cursor() });
			int count = 0;
			foreach(var employee in employees)
			{
				Console.WriteLine(employee.firstname + " " + employee.lastname);
				count++;
			}
			Assert.AreEqual(9, count);
		}
	}
}
