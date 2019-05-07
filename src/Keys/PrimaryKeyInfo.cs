using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;

using Mighty.DataContracts;
using Mighty.Mapping;
using Mighty.Parameters;
using Mighty.Plugins;

namespace Mighty.Keys
{
    /// <summary>
    /// Store key and sequence info.
    /// </summary>
    /// <remarks>
    /// And value info on .NET Framework only.
    /// </remarks>
    public class PrimaryKeyInfo
    {
        /// <summary>
        /// Primary key database column name (or multiple columns as a comma separated list)
        /// </summary>
        public string PrimaryKeyColumn { get; private set; }

        /// <summary>
        /// Primary key field or property name (or multiple names as a comma separated list)
        /// </summary>
        /// <remarks>
        /// Mighty does not (currently) use this internally, it is made available as a convenience to users
        /// </remarks>
        public string PrimaryKeyField { get; private set; }

        /// <summary>
        /// Number of primary keys
        /// </summary>
        public int Count { get { return PrimaryKeyColumnList.Count; } }

        /// <summary>
        /// Separated, primary key columns (note: these are database column names not class field or property names if these are different)
        /// </summary>
        public List<string> PrimaryKeyColumnList;

        /// <summary>
        /// Sequence name or identity retrieval function (always null for compound PK)
        /// </summary>
        public string SequenceNameOrIdentityFunction { get; private set; }

        /// <summary>
        /// For a single primary key only, on generic versions of Mighty only, the reflected
        /// <see cref="MemberInfo"/> corresponding to the primary key field in the generic type.
        /// </summary>
        internal MemberInfo PrimaryKeyMemberInfo { get; private set; }

        /// <summary>
        /// For a single primary key only, on dynamic versions of Mighty only, the reflected
        /// member name corresponding to the primary key field in the dynamic type.
        /// </summary>
        internal string PrimaryKeyMemberName { get; private set; }

        private readonly DataContract DataContract;

        private readonly PluginBase Plugin;
        private readonly SqlNamingMapper SqlNamingMapper;
        private readonly Type DataItemType;

        /// <summary>
        /// Manage key(s) and sequence or identity.
        /// </summary>
        /// <param name="IsGeneric"></param>
        /// <param name="dataContract"></param>
        /// <param name="xplugin"></param>
        /// <param name="dataMappingType"></param>
        /// <param name="xmapper"></param>
        /// <param name="keyNames"></param>
        /// <param name="sequence"></param>
        internal PrimaryKeyInfo(
            bool IsGeneric, DataContract dataContract, PluginBase xplugin, Type dataMappingType, SqlNamingMapper xmapper,
            string keyNames, string sequence)
        {
            Plugin = xplugin;
            SqlNamingMapper = xmapper;
            DataItemType = dataContract.Key.DataItemType;
            DataContract = dataContract;
            SetKeys(keyNames, xmapper, dataMappingType);
            SetSequence(xplugin, xmapper, sequence);
            SetPkMemberInfo(IsGeneric, dataContract);
        }

        private void SetKeys(string keys, SqlNamingMapper mapper, Type dataMappingType)
        {
            if (keys != null)
            {
                // from constructor
                keys = DataContract.Map(AutoMap.Keys, keys);
            }
            else if (DataContract.KeyColumns != null)
            {
                // from attributes
                keys = DataContract.KeyColumns;
            }
            else
            {
                // from mapper
                PrimaryKeyField = mapper.GetPrimaryKeyFieldNames(dataMappingType);
                keys = DataContract.Map(AutoMap.On, PrimaryKeyField);
            }

            if (keys == null)
            {
                PrimaryKeyColumnList = new List<string>();
            }
            else
            {
                PrimaryKeyColumn = keys;
                if (PrimaryKeyField == null) PrimaryKeyField = DataContract.ReverseMap(PrimaryKeyColumn);
                PrimaryKeyColumnList = keys.Split(',').Select(k => k.Trim()).ToList();
            }
        }

        private void SetSequence(PluginBase plugin, SqlNamingMapper mapper, string sequence)
        {
            // At the end of this next block of code, SequenceNameOrIdentityFunction should only be non-null if we
            // are actually expecting to use it later (which entails a simple (single column) PK).

            // It makes no sense to attempt to retrieve an auto-generated value for a compund primary key.
            if (PrimaryKeyColumnList.Count != 1)
            {
                // No exception here if database is identity-based since we want to allow, e.g., an override
                // of `sequence: "@@IDENTITY"` on all instances of Mighty, even if some instances are then
                // used for tables with no or compound primary keys.
                if (plugin.IsSequenceBased && !string.IsNullOrEmpty(sequence))
                {
                    throw new InvalidOperationException($"It is not possible to specify a sequence name for a table with {(PrimaryKeyColumnList.Count > 1 ? "a compound (multi-column)" : "no")} primary key");
                }
                SequenceNameOrIdentityFunction = null;
            }
            else
            {
                if (sequence == "")
                {
                    // empty string specifies that PK is manually controlled
                    SequenceNameOrIdentityFunction = null;
                }
                else
                {
                    if (plugin.IsSequenceBased)
                    {
                        // sequence-based, non-null, non-empty specifies sequence name
                        SequenceNameOrIdentityFunction = sequence ?? mapper.GetQuotedDatabaseIdentifier(sequence);
                    }
                    else
                    {
                        // identity-based, non-null, non-empty specifies non-default identity retrieval function (e.g. use "@@IDENTITY" on SQL CE)
                        SequenceNameOrIdentityFunction = sequence != null ? sequence : plugin.IdentityRetrievalFunction;
                    }
                }
            }
        }

        /// <summary>
        /// Set the primary key member info
        /// </summary>
        private void SetPkMemberInfo(bool isGeneric, DataContract dataContract)
        {
            // SequenceNameOrIdentityFunction is only left at non-null when there is a single PK,
            // and we only want to write to the PK when there is a SequenceNameOrIdentityFunction
            if (SequenceNameOrIdentityFunction != null)
            {
                PrimaryKeyMemberName = dataContract.ReverseMap(PrimaryKeyColumn);
                if (isGeneric)
                {
                    PrimaryKeyMemberInfo = dataContract.GetMember(PrimaryKeyMemberName, "primary key");
                }
            }
        }

#if KEY_VALUES
        /// <summary>
        /// Return the single (non-compound) primary key name, or throw <see cref="InvalidOperationException"/> with the provided message if there isn't one.
        /// </summary>
        /// <param name="partialMessage">Exception message to use on failure</param>
        /// <returns></returns>
        internal string CheckGetKeyColumn(string partialMessage)
        {
            if (PrimaryKeyColumnList.Count != 1)
            {
                throw new InvalidOperationException($"A single primary key must be specified{partialMessage}");
            }
            return PrimaryKeyColumn;
        }
#endif

        /// <summary>
        /// Return ith primary key name, with meaningful exception if too many requested.
        /// </summary>
        /// <param name="i">i</param>
        /// <param name="message">Meaningful exception message</param>
        /// <returns></returns>
        internal string CheckGetKeyName(int i, string message)
        {
            if (i >= PrimaryKeyColumnList.Count)
            {
                throw new InvalidOperationException(message);
            }
            return PrimaryKeyColumnList[i];
        }

        /// <summary>
        /// Return array of key values from passed in key values.
        /// Raise exception if the wrong number of keys are provided.
        /// The wrapping of a single item into an array which this does would happen automatically anyway
        /// in C# params handling, so this code is only required for the exception checking.
        /// </summary>
        /// <param name="key">The key value or values</param>
        /// <returns></returns>
        internal object[] KeyValuesFromKey(object key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            object[] keyArray;
            if (key is object[])
            {
                keyArray = (object[])key;
            }
            else
            {
                keyArray = new object[] { key };
            }
            if (keyArray.Length != Count)
            {
                throw new InvalidOperationException(keyArray.Length + " key values provided, " + Count + "expected");
            }
            return keyArray;
        }

        private string _whereForKeys;

        /// <summary>
        /// Return a WHERE clause with auto-named parameters for the primary keys
        /// </summary>
        /// <returns></returns>
        internal string WhereForKeys()
        {
            if (_whereForKeys == null)
            {
                if (PrimaryKeyColumnList == null || PrimaryKeyColumnList.Count == 0)
                {
                    throw new InvalidOperationException("No primary key field(s) have been specified");
                }
                int i = 0;
                _whereForKeys = string.Join(" AND ", PrimaryKeyColumnList.Select(k => $"{k}  = {Plugin.PrefixParameterName(i++.ToString())}"));
            }
            return _whereForKeys;
        }

        /// <summary>
        /// Return comma-separated list of primary key fields, raising an exception if there are none.
        /// </summary>
        /// <returns></returns>
        internal string CheckGetPrimaryKeyFields()
        {
            if (string.IsNullOrEmpty(PrimaryKeyColumn))
            {
                throw new InvalidOperationException("No primary key field(s) have been specified");
            }
            return PrimaryKeyColumn;
        }

#pragma warning disable IDE0059 // Value assigned is never used
        /// <summary>
        /// Is this the name of a PK field?
        /// </summary>
        /// <param name="fieldName">The name to check</param>
        /// <returns></returns>
        internal bool IsKey(string fieldName)
        {
            string canonicalKeyName;
            return IsKey(fieldName, out canonicalKeyName);
        }
#pragma warning restore IDE0059

        /// <summary>
        /// Is this the name of a PK field?
        /// </summary>
        /// <param name="fieldName">The name to check</param>
        /// <param name="canonicalKeyName">Returns the canonical key name, i.e. as specified in <see cref="MightyOrm"/> constructor</param>
        /// <returns></returns>
        internal bool IsKey(string fieldName, out string canonicalKeyName)
        {
            canonicalKeyName = null;
            foreach (var key in PrimaryKeyColumnList)
            {
                if (key.Equals(fieldName, SqlNamingMapper.CaseSensitiveColumns(DataItemType) ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase))
                {
                    canonicalKeyName = key;
                    return true;
                }
            }
            return false;
        }

        internal bool HasPrimaryKey(object item)
        {
            int count = 0;
            foreach (var info in new NameValueTypeEnumerator(DataContract, item))
            {
                if (IsKey(info.Name)) count++;
            }
            return count == Count;
        }

        internal object GetPrimaryKey(object item, bool alwaysArray = false)
        {
            var pks = new ExpandoObject();
            var pkDictionary = pks.ToDictionary();
            foreach (var info in new NameValueTypeEnumerator(DataContract, item))
            {
                string canonicalKeyName;
                if (IsKey(info.Name, out canonicalKeyName)) pkDictionary.Add(canonicalKeyName, info.Value);
            }
            if (pkDictionary.Count != Count)
            {
                throw new InvalidOperationException("PK field(s) not present in object");
            }
            // re-arrange to specified order
            var retval = new List<object>();
            foreach (var key in PrimaryKeyColumnList)
            {
                retval.Add(pkDictionary[key]);
            }
            var array = retval.ToArray();
            if (array.Length == 1 && !alwaysArray)
            {
                return array[0];
            }
            return array;
        }
    }
}
