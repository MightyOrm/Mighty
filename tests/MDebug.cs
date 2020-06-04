using System;
using System.Diagnostics;

namespace Mighty
{
    public class MDebug
    {
        /// <summary>
        /// We can't call <see cref="System.Diagnostics.Debug.WriteLine(string, object[])"/> directly because
        /// it is a conditional method, which interacts badly (gives warnings) when passed dynamic arguments.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public static void WriteLine(string format, params object[] args)
        {
            Debug.WriteLine(format, args);
        }
    }
}
