#if NET40 //|| COREFX
using NUnit.Common;
// NUnitLite (the console runner) isn't available for .NETCoreApp 1.1 (it should be for 1.0 and 2.0, via .NETStandard 1.3 and 1.6)
using NUnitLite;
using System;
using System.Data.Common;
using System.Reflection;

using Mighty.Profiling;

#if NET40
// NB This CAN be loaded from NuGet pre-release package by name (currently SqlProfiler v0.0.1-alpha3),
// or from co-loaded project.
using SqlProfiler;
#endif

using MightyTests;
using MightyTests.SOTests;

namespace Mighty.Generic.Tests.NUnit.ConsoleRunner
{
#if NET40
	/// <summary>
	/// Make a simple link between <see cref="Mighty.Profiling.SqlProfiler"/> and <see cref="SqlProfiler.SimpleCommandWrapper"/>.
	/// </summary>
	/// <remarks>
	/// Unless we want Mighty to depend on our specific profiling library, or the profiling library to depend on Mighty, then I
	/// think we can't avoid something like this.
	/// </remarks>
	class MyProfiler : Mighty.Profiling.SqlProfiler
	{
		override public DbCommand Wrap(DbCommand command)
		{
			return new SqlProfiler.SimpleCommandWrapper(command);
		}
	}
#endif

	class Program
	{
#if NET40
		static public void SOSubTest(dynamic da, dynamic db)
		{
			Console.WriteLine(da.Foo());
			Console.WriteLine(da.Bar());
			Console.WriteLine(db.Foo());
			Console.WriteLine(db.Bar());
		}
		static public void SOTest()
		{
			dynamic d1 = new AStaticComponent();
			dynamic d2 = new AStaticComponent();
			dynamic d3 = new AStaticComponent();
			dynamic d4 = new AStaticComponent();
			Console.WriteLine(d1.Foo());
			Console.WriteLine(d1.Bar());
			Console.WriteLine(d2.Foo());
			Console.WriteLine(d2.Bar());
			Console.WriteLine(d3.Foo());
			Console.WriteLine(d3.Bar());
			Console.WriteLine(d4.Foo());
			Console.WriteLine(d4.Bar());
			SOSubTest(d1, d2);
			SOSubTest(d3, d4);
		}
		static public void MyTest(object o)
		{
			var tc = new TestClass(o);
			string r;

			r = tc.A();
			r = tc.C();
			r = ((dynamic)tc).A();
			r = ((dynamic)tc).B();
			r = ((dynamic)tc).C();
			r = ((dynamic)tc).D();

			r = tc.W;
			r = tc.Y;
			r = ((dynamic)tc).W;
			r = ((dynamic)tc).X;
			r = ((dynamic)tc).Y;
			r = ((dynamic)tc).Z;
		}
		static public void MyTest2(dynamic o)
		{
			Console.WriteLine(o.W);
		}
#endif

		static int Main(string[] args)
		{
#if false //NET40
			// Test the delegating meta-object
			//SOTest();
			//return 0;

			//MyTest(new TestDynamic());
			//MyTest(new TestDynamic());
			//MyTest(new TestPOCO());
			//return 0;

			MyTest2(new TestDynamic());
			MyTest2(new TestPOCO());
			return 0;
#endif

#if false //NET40
			// Test the profiler ... seems to be working now that we've fixed the DelegatingMetaObject (**NB** & TODO: Mighty itself is still using the broken one).
			// Passing all tests on .NET 4.0, with or without the profiler, which is good.
			MightyORM.GlobalSqlProfiler = new MyProfiler();
			//Mighty.Generic.Tests.Oracle.TableClasses.Departments.GlobalSqlProfiler = new MyProfiler();
#endif

#if false
			// This is a test which was failing, just because it's an Oracle test which actually uses the dynamic properties thing.
			// The fix was simple (needed to add BindingInstance.Public flag for a public Wrapped property).
			new Mighty.Dynamic.Tests.Oracle.ReadWriteTests("Oracle.DataAccess.Client").Guid_Arg();

			// Still some binding site confusion: okay, the binding I've written requires that once a SqlProfiler, always a SqlProfiler - but the GlobalSqlProfiler is only set for the dynamic version.
			new Mighty.Generic.Tests.Oracle.ReadWriteTests("Oracle.DataAccess.Client").All_LimitSpecification();

			return 0;
#endif

			// Okay, this runs all the tests in the NUnit test runner.
#if NET40 || COREFX
			return new AutoRun(
#if NETFRAMEWORK
				typeof(Program).Assembly
#else
				typeof(Program).GetTypeInfo().Assembly
#endif
				)
				.Execute(args, new ExtendedTextWrapper(Console.Out), Console.In);
#else
			return 0;
#endif
		}
	}
}
#endif
