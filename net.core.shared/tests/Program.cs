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
#if COREFX
				typeof(Program).GetTypeInfo().Assembly
#else
				typeof(Program).Assembly
#endif
				)
				.Execute(args, new ExtendedTextWrapper(Console.Out), Console.In);
		}
	}
}