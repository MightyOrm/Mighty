using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Reflection;

using Mighty.Validation;
using Mighty.DataContracts;

namespace Mighty.Parameters
{
    /// <remarks>
    /// <see cref="NameValueCollection"/> *is* supported in .NET Core 1.1, but got a bit lost:
    /// https://github.com/dotnet/corefx/issues/10338
    /// For folks that hit missing types from one of these packages after upgrading to Microsoft.NETCore.UniversalWindowsPlatform they can reference the packages directly as follows.
    /// "System.Collections.NonGeneric": "4.0.1",
    /// "System.Collections.Specialized": "4.0.1", ****
    /// "System.Threading.Overlapped": "4.0.1",
    /// "System.Xml.XmlDocument": "4.0.1"
    /// </remarks>
    internal class NameValueTypeEnumerator : IEnumerable<LazyNameValueTypeInfo>, IEnumerable
    {
        private readonly object _o;
        private readonly ParameterDirection? _direction;
        private readonly OrmAction? _action;
        private readonly DataContract _dataContract;

        internal ParameterInfo Current { get; set; }

        internal NameValueTypeEnumerator(DataContract dataContract, object o, ParameterDirection? direction = null, OrmAction? action = null)
        {
            _o = o;
            _direction = direction;
            _action = action;
            _dataContract = dataContract;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Determine whether the passed in type can provide names as well as values
        /// </summary>
        /// <remarks>NB The logic of this must match <see cref="GetEnumerator"/> below</remarks>
        public bool HasNames()
        {
            // enumerate empty list if null
            if (_o == null)
            {
                return true;
            }

            var o = _o as ExpandoObject;
            if (o != null)
            {
                return true;
            }

            var nvc = _o as NameValueCollection;
            if (nvc != null)
            {
                return true;
            }

            // possible support for Newtonsoft JObject here...

            // Convert non-class objects to value array
            object[] valueArray = null;
            var type = _o.GetType();
            if (!type
#if !NETFRAMEWORK
                .GetTypeInfo()
#endif
                .IsClass)
            {
                valueArray = new object[] { _o };
            }
            else
            {
                valueArray = _o as object[];
            }

            // This is for adding anonymous parameters (which will cause a different exception later on, in AddParam, if used on
            // a DB which doesn't support it; even on DBs which do support it, it only makes sense on input parameters).
            // NB This is not the same thing as the auto-named parameters added by AddParams(), which also use object[]
            // but with a different meaning (namely the values for the auto-named params @0, @1 or :0, :1 etc.).
            // Value-only processing now also used to support value-only collections of PK values when performing <see cref="OrmAction"/> on an item.
            if (valueArray != null)
            {
                return false;
            }

            // names, values and types from properties of anonymous object (therefore with names and values on its fields!) or POCOs
            return true;
        }

        /// <summary>
        /// Enumerate over names and values, or just values, from passed in object
        /// </summary>
        /// <remarks>NB The logic of this must match <see cref="HasNames"/> above</remarks>
        public IEnumerator<LazyNameValueTypeInfo> GetEnumerator()
        {
            // enumerate empty list if null
            if (_o == null)
            {
                yield break;
            }

            var o = _o as ExpandoObject;
            if (o != null)
            {
                foreach (var pair in o)
                {
                    yield return new LazyNameValueTypeInfo(pair.Key, () => pair.Value);
                }
                yield break;
            }

            var nvc = _o as NameValueCollection;
            if (nvc != null)
            {
                foreach (string name in nvc)
                {
                    yield return new LazyNameValueTypeInfo(name, () =>  nvc[name]);
                }
                yield break;
            }

            // possible support for Newtonsoft JObject here...

            // Convert non-class objects to value array
            object[] valueArray = null;
            var type = _o.GetType();
            if (!type
#if !NETFRAMEWORK
                .GetTypeInfo()
#endif
                .IsClass)
            {
                valueArray = new object[] { _o };
            }
            else
            {
                valueArray = _o as object[];
            }

            // This is for adding anonymous parameters (which will cause a different exception later on, in AddParam, if used on
            // a DB which doesn't support it; even on DBs which do support it, it only makes sense on input parameters).
            // NB This is not the same thing as the auto-named parameters added by AddParams(), which also use object[]
            // but with a different meaning (namely the values for the auto-named params @0, @1 or :0, :1 etc.).
            // Value-only processing now also used to support value-only collections of PK values when performing <see cref="OrmAction"/> on an item.
            if (valueArray != null)
            {
                string msg = null;
                if (_action != null)
                {
                    if (_action != OrmAction.Delete)
                    {
                        msg = "Value-only collections not supported for action " + _action;
                        if (_action == OrmAction.Update)
                        {
                            msg += "; use Update(item), not Update(item, pk)";
                        }
                    }
                }
                else
                {
                    if (_direction != ParameterDirection.Input)
                    {
                        msg = "object[] arguments supported for input parameters only";
                    }
                }
                if (msg != null)
                {
                    throw new InvalidOperationException(msg);
                }
                // anonymous parameters from array
                foreach (var value in valueArray)
                {
                    // string.Empty not null is needed in AddParam
                    yield return new LazyNameValueTypeInfo(string.Empty, () => value);
                }
                yield break;
            }

            // Detect if the object type is T and only if it is do a loop over T's stored set of members instead, which
            // reflects columns and bindingFlags. So values obtained from any POCO type except T will always use public
            // members only, but values obtained from T will use whatever members are managed by Mighty - perfect!
            if (_dataContract != null && _dataContract.IsManagedGenericType(_o))
            {
                foreach (DataContractMemberInfo member in _dataContract.ColumnNameToMemberInfo.Values)
                {
                    yield return new LazyNameValueTypeInfo(member.Name, () => member.GetValue(_o), member.MemberType);
                }
                yield break;
            }

            // names, values and types from fields and properties of anonymous object or other POCO
            foreach (var field in _o.GetType().GetFields())
            {
                 yield return new LazyNameValueTypeInfo(field.Name, () => field.GetValue(_o), field.FieldType);
            }
            foreach (var property in _o.GetType().GetProperties())
            {
                yield return new LazyNameValueTypeInfo(property.Name, () => property.GetValue(_o), property.PropertyType);
            }
        }
    }
}