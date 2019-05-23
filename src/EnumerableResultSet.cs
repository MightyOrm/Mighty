using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading;
using Mighty.ConnectionProviders;
using Mighty.DataContracts;
using Mighty.Mapping;
using Mighty.Profiling;
using Mighty.Validation;

namespace Mighty
{
    /// <summary>
    /// Mighty result set of type <typeparamref name="T"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    /// When thinking about disposing all disposable items potentiall in use here, remember that it is
    /// the Enumerator and not the initial Enumerable from a yield return which implements IDisposable.
    /// https://stackoverflow.com/q/4982396
    /// </remarks>
    public sealed class EnumerableResultSet<T> : IEnumerable<T> where T : class, new()
    {
        private readonly DbConnection _connection;
        private readonly DbDataReader _outerReader;

        /// <summary>
        /// The parent instance of <see cref="MightyOrm{T}"/>
        /// </summary>
        public MightyOrm<T> MightyInstance { get; }

        /// <summary>
        /// Construct enumerable result set, with output type not yet specified;
        /// call <see cref="ResultsAs{X}(string, string, string, string, string, Validator, SqlNamingMapper, DataProfiler)"/>
        /// to get the results as a different type from the parent instance of <see cref="MightyOrm{T}"/>.
        /// </summary>
        /// <param name="mightyInstance"></param>
        /// <param name="connection"></param>
        /// <param name="outerReader"></param>
        internal EnumerableResultSet(
            MightyOrm<T> mightyInstance,
            DbConnection connection = null,
            DbDataReader outerReader = null)
        {
            MightyInstance = mightyInstance;
            _connection = connection;
            _outerReader = outerReader;
        }

        /// <summary>
        /// Return the enumerator for the results in this result set as type <typeparamref name="T"/>.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator()
        {
            return Results().GetEnumerator();
        }

        /// <summary>
        /// Return the results in this result set as type <typeparamref name="T"/>.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<T> Results()
        {
            return MightyInstance.QueryNWithParams<T>(null, (CommandBehavior)(-1), _connection, _outerReader);
        }

        /// <summary>
        /// Return the results in this result set as type <typeparamref name="T2"/>.
        /// </summary>
        /// <typeparam name="T2"></typeparam>
        /// <param name="tableName">Optional</param>
        /// <param name="primaryKeys">Optional</param>
        /// <param name="valueField">Optional</param>
        /// <param name="sequence">Optional</param>
        /// <param name="columns">Optional</param>
        /// <param name="validator">Optional</param>
        /// <param name="mapper">Optional - inherits the mapper from the parent instance of Mighty if null</param>
        /// <param name="profiler">Optional - inherits the SQL profiler from the parent instance of Mighty if null</param>
        /// <returns></returns>
        public IEnumerable<T2> ResultsAs<T2>(
            string tableName = null,
            string primaryKeys = null,
            string valueField = null,
            string sequence = null,
            string columns = null,
            Validator validator = null,
            SqlNamingMapper mapper = null,
            DataProfiler profiler = null) where T2 : class, new()
        {
            var mightyT2 = new MightyOrm<T2>(
                        null,
                        tableName,
                        primaryKeys,
#if KEY_VALUES
                        valueField,
#endif
                        sequence,
                        columns,
                        validator,
                        mapper ?? MightyInstance.SqlNamingMapper,
                        profiler ?? MightyInstance.DataProfiler,
                        new PresetsConnectionProvider(
                            MightyInstance.ConnectionString,
                            MightyInstance.Factory,
                            MightyInstance.Plugin.GetType()));
            return mightyT2.QueryNWithParams<T2>(null, (CommandBehavior)(-1), _connection, _outerReader);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Results().GetEnumerator();
        }
    }
}
