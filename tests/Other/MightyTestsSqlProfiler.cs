using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Mighty.Profiling;
using SqlProfiler.Simple;

namespace MightyTests.Profiling
{
    public class MightyTestsSqlProfiler : DataProfiler
    {
        public Dictionary<DbCommandMethod, int> DbCommandMethodCounts;

        /// <summary>
        /// Constructor
        /// </summary>
        public MightyTestsSqlProfiler()
        {
            DbCommandMethodCounts = new Dictionary<DbCommandMethod, int>();
            DbCommandMethodCounts[DbCommandMethod.ExecuteDbDataReader] = 0;
            DbCommandMethodCounts[DbCommandMethod.ExecuteNonQuery] = 0;
            DbCommandMethodCounts[DbCommandMethod.ExecuteScalar] = 0;

            CommandWrapping = wrapped => new SimpleCommandProfiler(
                wrapped,
                (method, command, behavior) =>
                {
#if DEBUG
#if NET40
                    Console.WriteLine("-----");
                    Console.WriteLine(command.CommandText);
#else
                    Debug.WriteLine("-----");
                    Debug.WriteLine(command.CommandText);
#endif
#endif
                    DbCommandMethodCounts[method]++;
                });
        }
    }
}
