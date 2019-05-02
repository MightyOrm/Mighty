using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Mighty.DataContracts;
using Mighty.Mapping;
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
        /// Primary key field or fields (exact C# names, no mapping applied) as a comma separated list
        /// </summary>
        public string FieldNames { get; private set; }

        /// <summary>
        /// Number of primary key fields
        /// </summary>
        public int Count { get { return PrimaryKeyList.Count; } }

        /// <summary>
        /// The primary keys as an <see cref="IEnumerable{T}"/>
        /// </summary>
        public IEnumerable<string> Keys { get { return PrimaryKeyList; } }

        /// <summary>
        /// Separated, lowered primary key fields (exact C# names, no mapping applied)
        /// </summary>
        private List<string> PrimaryKeyList;

        /// <summary>
        /// Sequence name or identity retrieval function (always null for compound PK)
        /// </summary>
        public string SequenceNameOrIdentityFunction { get; private set; }

        /// <summary>
        /// For a single primary key only, on generic versions of Mighty only, the reflected
        /// <see cref="MemberInfo"/> corresponding to the primary key field in the generic type.
        /// </summary>
        internal MemberInfo PrimaryKeyMemberInfo { get; private set; }

#if KEY_VALUES
        internal string PrimaryKeyColumn { get; private set; }
#endif

        private readonly PluginBase Plugin;
        private readonly SqlNamingMapper SqlNamingMapper;
        private readonly Type DataItemType;

        /// <summary>
        /// Manage key(s) and sequence or identity.
        /// </summary>
        /// <param name="IsDynamic"></param>
        /// <param name="ColumnsContract"></param>
        /// <param name="xplugin"></param>
        /// <param name="dataMappingType"></param>
        /// <param name="xmapper"></param>
        /// <param name="keys"></param>
        /// <param name="sequence"></param>
        internal PrimaryKeyInfo(
            bool IsDynamic, ColumnsContract ColumnsContract, PluginBase xplugin, Type dataMappingType, SqlNamingMapper xmapper,
            string keys, string sequence)
        {
            Plugin = xplugin;
            SqlNamingMapper = xmapper;
            DataItemType = ColumnsContract.Key.DataItemType;
            SetKeys(keys, xmapper, dataMappingType);
            SetSequence(xplugin, xmapper, sequence);
            SetPkMemberInfo(IsDynamic, ColumnsContract);
        }

        private void SetKeys(string keys, SqlNamingMapper mapper, Type dataMappingType)
        {
            if (keys == null && dataMappingType != null)
            {
                keys = mapper.PrimaryKeyFieldNames(dataMappingType);
            }
            FieldNames = keys;
            if (keys == null)
            {
                PrimaryKeyList = new List<string>();
            }
            else
            {
                PrimaryKeyList = keys.Split(',').Select(k => k.Trim()).ToList();
            }
        }

        private void SetSequence(PluginBase plugin, SqlNamingMapper mapper, string sequence)
        {
            // At the end of this next block of code, SequenceNameOrIdentityFunction should only be non-null if we
            // are actually expecting to use it later (which entails a simple (single column) PK).

            // It makes no sense to attempt to retrieve an auto-generated value for a compund primary key.
            if (PrimaryKeyList.Count != 1)
            {
                // No exception here if database is identity-based since we want to allow, e.g., an override
                // of `sequence: "@@IDENTITY"` on all instances of Mighty, even if some instances are then
                // used for tables with no or compound primary keys.
                if (plugin.IsSequenceBased && !string.IsNullOrEmpty(sequence))
                {
                    throw new InvalidOperationException($"It is not possible to specify a sequence name for a table with {(PrimaryKeyList.Count > 1 ? "a compound (multi-column)" : "no")} primary key");
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
                        SequenceNameOrIdentityFunction = sequence ?? mapper.QuotedDatabaseIdentifier(sequence);
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
        private void SetPkMemberInfo(bool IsDynamic, ColumnsContract ColumnsContract)
        {
            // SequenceNameOrIdentityFunction is only left at non-null when there is a single PK,
            // and we only want to write to the PK when there is a SequenceNameOrIdentityFunction
            if (SequenceNameOrIdentityFunction != null)
            {
                if (IsDynamic)
                {
#if KEY_VALUES
                    PrimaryKeyColumn = FieldNames;
#endif
                }
                else
                {
                    PrimaryKeyMemberInfo = ColumnsContract.GetMember(FieldNames, "primary key");
#if KEY_VALUES
                    //// PrimaryKeyColumn is only ever used on dynamic version
                    //PrimaryKeyColumn = ColumnsContract.Key.ColumnName(ColumnsContract.Key.DataItemType, FieldNames);
#endif
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
            if (PrimaryKeyColumn == null)
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
            if (i >= PrimaryKeyList.Count)
            {
                throw new InvalidOperationException(message);
            }
            return PrimaryKeyList[i];
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
                if (PrimaryKeyList == null || PrimaryKeyList.Count == 0)
                {
                    throw new InvalidOperationException("No primary key field(s) have been specified");
                }
                int i = 0;
                _whereForKeys = string.Join(" AND ", PrimaryKeyList.Select(k => $"{k}  = {Plugin.PrefixParameterName(i++.ToString())}"));
            }
            return _whereForKeys;
        }

        /// <summary>
        /// Return comma-separated list of primary key fields, raising an exception if there are none.
        /// </summary>
        /// <returns></returns>
        internal string CheckGetPrimaryKeyFields()
        {
            if (string.IsNullOrEmpty(FieldNames))
            {
                throw new InvalidOperationException("No primary key field(s) have been specified");
            }
            return FieldNames;
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
            foreach (var key in PrimaryKeyList)
            {
                if (key.Equals(fieldName, SqlNamingMapper.CaseSensitiveColumnMapping(DataItemType) ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase))
                {
                    canonicalKeyName = key;
                    return true;
                }
            }
            return false;
        }
    }
}
