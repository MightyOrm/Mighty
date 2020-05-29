using NUnit.Framework.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Mighty.MethodSignatures
{
    /// <summary>
    /// Nested gethering of Mighty methods
    /// </summary>
    /// <typeparam name="K">Must be an enum</typeparam>
    /// <typeparam name="V">Must be <see cref="MethodTreeNode{K, V}"/> or <see cref="List{MethodInfo}"/>, this is verified when the class is constructed</typeparam>
    public class MethodTreeNode<K, V> : IEnumerable<MethodInfo> where K : System.Enum where V : new()
    {
        private Dictionary<K, V> _dictionary;

        public bool isLists;

        private int _methodCount;
        private bool _hasCount;

        /// <summary>
        /// Return the count of all methods at and below this node
        /// </summary>
        public int MethodCount
        {
            get
            {
                if (!_hasCount)
                {
                    var sum = 0;
                    foreach (K type in Enum.GetValues(typeof(K)))
                    {
                        if (isLists) sum += ((ICollection)_dictionary[type]).Count; // List count
                        else sum += ((dynamic)_dictionary[type]).MethodCount; // recursive method count
                    }
                    _methodCount = sum;
                    _hasCount = true;
                }
                return _methodCount;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public MethodTreeNode()
        {
            isLists = typeof(V).GetGenericTypeDefinition() == typeof(List<object>).GetGenericTypeDefinition();

            // confirm that the child class makes a valid tree nesting
            Type required, actual;
            if (isLists)
            {
                required = typeof(List<MethodInfo>);
                actual = typeof(V);
            }
            else
            {
                required = typeof(MethodTreeNode<K, V>).GetGenericTypeDefinition();
                actual = typeof(V).GetGenericTypeDefinition();
            }
            if (actual != required)
                throw new InvalidOperationException($"Type V in {typeof(MethodTreeNode<K, V>).FriendlyName()} must be {required.FriendlyName()} not {actual.FriendlyName()}");

            // initialise all branches
            _dictionary = new Dictionary<K, V>();
            foreach (K type in Enum.GetValues(typeof(K)))
            {
                // V here is either list of methods, or a whole child tree node
                _dictionary[type] = new V();
            }
        }

        /// <summary>
        /// yield return to recursively enumerate all methods anywhere inside this node
        /// </summary>
        /// <returns></returns>
        private IEnumerable<MethodInfo> EnumerateMethodsInNode()
        {
            foreach (K type in Enum.GetValues(typeof(K)))
            {
                foreach (var methodInfo in _dictionary[type] as IEnumerable<MethodInfo>)
                {
                    yield return methodInfo;
                }
            }
        }

        /// <summary>
        /// Implement <see cref="IEnumerable<MethodInfo>"/>
        /// </summary>
        /// <returns></returns>
        public IEnumerator<MethodInfo> GetEnumerator()
        {
            return EnumerateMethodsInNode().GetEnumerator();
        }

        /// <summary>
        /// Implement <see cref="IEnumerable<MethodInfo>"/>
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return EnumerateMethodsInNode().GetEnumerator();
        }

        /// <summary>
        /// The direct branch count at this tree node (as opposed to the nested MethodCount)
        /// </summary>
        public int Count
        {
            get
            {
                return _dictionary.Count;
            }
        }

        /// <summary>
        /// Indexer to add and read items at this tree node
        /// </summary>
        public V this[K key]
        {
            get
            {
                return _dictionary[key];
            }

            set
            {
                _dictionary[key] = value;
            }
        }
    }
}
