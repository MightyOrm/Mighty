#if RELEASE
using System;

#if !NETCOREAPP1_1
using System.Reflection;

// NUnitLite (the console runner) isn't available for .NETCoreApp 1.1 (it should be for 1.0 and 2.0, via .NETStandard 1.3 and 1.6)
using NUnit.Common;
using NUnitLite;
#endif

using MightyTests;

namespace Mighty.Generic.Tests.NUnit.ConsoleRunner
{
    public class Program
    {
        public static int Main(string[] args)
        {
            // Okay, this runs all the tests in the NUnit test runner.
#if !NETCOREAPP1_1
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