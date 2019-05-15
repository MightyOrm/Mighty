using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Mighty
{
    /// <summary>
    /// Class to return multiple result sets; can be used as <see cref="IEnumerable"/> of <see cref="IEnumerable{T}"/>,
    /// but can also be accessed using <see cref="NextResultSet"/> and <see cref="CurrentResultSet"/>
    /// to allow variable strongly typed multiple result sets.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MultipleResultSets<T> : IEnumerable<EnumerableResultSet<T>>, IDisposable where T : class, new()
    {
        private IEnumerable<EnumerableResultSet<T>> _wrapped;

        /// <summary>
        /// Has this been disposed (or not yet accessed)?
        /// </summary>
        public bool IsDisposed { get { return _wrapped == null; } }

        private IEnumerator<EnumerableResultSet<T>> _enumerator;
        private IEnumerator<EnumerableResultSet<T>> enumerator
        {
            get
            {
                if (_enumerator == null)
                    _enumerator = _wrapped.GetEnumerator();
                return _enumerator;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="wrapped"></param>
        internal MultipleResultSets(IEnumerable<EnumerableResultSet<T>> wrapped)
        {
            if (!(wrapped is IDisposable))
            {
                throw new Exception($"{nameof(MultipleResultSets<T>)} cannot wrap non-disposable {nameof(IEnumerable<T>)}");
            }
            _wrapped = wrapped;
        }

        /// <summary>
        /// Get enumerator for contained result sets
        /// </summary>
        /// <returns></returns>
        public IEnumerator<EnumerableResultSet<T>> GetEnumerator()
        {
            return enumerator;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return enumerator;
        }

        /// <summary>
        /// Access the next result set; remember to call this before accessing each result set.
        /// </summary>
        /// <returns></returns>
        public bool NextResultSet()
        {
            return enumerator.MoveNext();
        }

        /// <summary>
        /// Clean up and dispose of everything
        /// </summary>
        public void Dispose()
        {
            if (_wrapped != null)
            {
                ((IDisposable)_wrapped).Dispose();
                _wrapped = null;
            }
        }

        /// <summary>
        /// Access the current result set; remember to call <see cref="NextResultSet"/>() before accessing each result set.
        /// </summary>
        public EnumerableResultSet<T> CurrentResultSet
        {
            get
            {
                if (enumerator == null)
                {
                    throw new InvalidOperationException($"Call {nameof(NextResultSet)}() before accessing {nameof(CurrentResultSet)}");
                }
                return enumerator.Current;
            }
        }
    }
}
