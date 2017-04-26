using NUnit.Common;
using NUnitLite;
using System;
using System.Reflection;

namespace Mighty.Tests.NUnit.ConsoleRunner
{
	class Program
	{
		static int Main(string[] args)
		{
			return new AutoRun(
#if NETFRAMEWORK
				typeof(Program).Assembly
#else
				typeof(Program).GetTypeInfo().Assembly
#endif
				)
				.Execute(args, new ExtendedTextWrapper(Console.Out), Console.In);
		}
	}
}