using System;
using System.Reflection;

#if !NETCOREAPP1_1
// NUnitLite (the console runner) isn't available for .NETCoreApp 1.1 (it should be for 1.0 and 2.0, via .NETStandard 1.3 and 1.6)
using NUnit.Common;
using NUnitLite;
#endif

namespace Mighty
{
    public class Program
    {
        /// <summary>
        /// This runs all the tests in the NUnit test runner.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static int Main(string[] args)
        {
#if NETCOREAPP1_1
            return 0;
#else
            return new AutoRun(
                typeof(Mighty.MDebug)
#if !NETFRAMEWORK
                    .GetTypeInfo()
#endif
                    .Assembly
                )
                .Execute(args, new ExtendedTextWrapper(Console.Out), Console.In);
#endif
        }
    }
}
