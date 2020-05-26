using System;
#if !NET40 && !NETCOREAPP3_0 || NETCOREAPP3_1
using Dasync.Collections;
#endif
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

namespace Mighty.MethodSignatures
{
    public class MethodChecker<M, T> where T : class, new()
    {
        private Type mightyType;

        /// <summary>
        /// TO DO: we're not doing any checks on these yet...
        /// </summary>
        public readonly List<PropertyInfo> Properties;

        /// <summary>
        /// Static methods on this instance
        /// </summary>
        public readonly List<MethodInfo> StaticMethods;

        /// <summary>
        /// Methods which, from their name, are expected to be sync only (no async variant)
        /// </summary>
        public readonly List<MethodInfo> SyncOnlyMethods;

        /// <summary>
        /// Sync methods on this instace
        /// </summary>
        public readonly List<MethodInfo> SyncMethods;
#if !NET40
        /// <summary>
        /// Async methods on this instace
        /// </summary>
        public readonly List<MethodInfo> AsyncMethods;
#endif

        /// <summary>
        /// Check all fields, properties and methods on a given Mighty class/interface.
        /// Populate the public lists of methods according to what is found.
        /// (Some sanity check tests are carried out while this is being doing.)
        /// </summary>
        /// <param name="isAbstract">Is this an abstract interface?</param>
        /// <param name="isVirtual">Is this is a virtual class?</param>
        public MethodChecker(bool isAbstract, bool isVirtual)
        {
            mightyType = typeof(M);

            StaticMethods = new List<MethodInfo>();
            SyncMethods = new List<MethodInfo>();
            SyncOnlyMethods = new List<MethodInfo>();
#if !NET40
            AsyncMethods = new List<MethodInfo>();
#endif

            // enforce no public fields present
            var fields = mightyType.GetFields();
            Assert.AreEqual(0, fields.Length);

            // populating the public property info, but not yet testing it
            Properties = mightyType.GetProperties().Where(p => p.DeclaringType == mightyType).ToList();

            // populate the method info lists...
            foreach (var method in mightyType.GetMethods().Where(m => m.DeclaringType == mightyType && !m.IsSpecialName))
            {
                if (method.IsAbstract != isAbstract)
                {
                    throw new InvalidOperationException($"Methods in {mightyType.Name} must {(isAbstract ? "" : "not ")}be abstract (method: {method.Name})");
                }
                if (!method.IsStatic && method.IsVirtual != isVirtual)
                {
                    throw new InvalidOperationException($"Methods in {mightyType.Name} must {(isVirtual ? "" : "not ")}be virtual (method: {method.Name})");
                }

                CheckMethod(
                    out MightyMethodType methodType,
#if !NET40
                    out bool isAsync,
#endif
                    out bool isSyncOnly,
                    out bool withParams,
                    out bool fromProcedure,
                    method);

                foreach (var param in method.GetParameters())
                {
#if !NET40
                    if (param.HasDefaultValue != param.IsOptional)
                    {
                        throw new InvalidOperationException("Unhandled param configuration");
                    }
                    if (param.HasDefaultValue)
                    {
                        //int c = 1;
                    }
#endif
                    if (param.IsOptional)
                    {
                        //int d = 1;
                    }
                }

                if (method.IsStatic) StaticMethods.Add(method);
                else if (isSyncOnly) SyncOnlyMethods.Add(method);
#if !NET40
                else if (isAsync) AsyncMethods.Add(method);
#endif
                else SyncMethods.Add(method);
            }

            // and sort the lists, because it's going to be very useful for comparing one list to another...
            // I think...
            // If we do sort them, then sort by method name then by parameter type short names, that will be
            // enough.
            // ...
        }

        /// <summary>
        /// Parse an individual method, determining which known <see cref="MightyMethodType"/> it has.
        /// Exception - test fail - if any method doesn't have a known method type.
        /// Also already does various checks, including that all the method return types make sense
        /// for the known method types.
        /// </summary>
        /// <param name="methodType"></param>
        /// <param name="isAsync"></param>
        /// <param name="isSyncOnly"></param>
        /// <param name="withParams"></param>
        /// <param name="fromProcedure"></param>
        /// <param name="method"></param>
        private void CheckMethod(
            out MightyMethodType methodType,
#if !NET40
            out bool isAsync,
#endif
            out bool isSyncOnly,
            out bool withParams,
            out bool fromProcedure,
            MethodInfo method)
        {
#if NET40
            bool isAsync;
#endif
            isAsync = false;
            isSyncOnly = false;
            withParams = false;
            fromProcedure = false;

            {
                var words = method.Name.CamelCaseSplit();
                int wordCount = words.Count;
                int lastWord = 0;

                if (words[wordCount - 1] == "Async")
                {
                    isAsync = true;
                    wordCount--;
                }

                if (Enum.TryParse(words[0], out methodType))
                {
                    if (methodType == MightyMethodType.Query && wordCount > 1 && words[1] == "Multiple")
                    {
                        methodType = MightyMethodType.QueryMultiple;
                        lastWord++;
                    }
                    else if (methodType == MightyMethodType.Update && wordCount > 1 && words[1] == "Using")
                    {
                        lastWord++;
                    }
                    else if (methodType == MightyMethodType.New && wordCount > 1 && words[1] == "From")
                    {
                        lastWord++;
                    }
                    else if (methodType == MightyMethodType.Single && wordCount > 2 && words[1] == "From" && words[2] == "Query")
                    {
                        lastWord += 2;
                    }
                    else if (methodType == MightyMethodType.Paged && wordCount > 2 && words[1] == "From" && words[2] == "Select")
                    {
                        lastWord += 2;
                    }
                }
                else if (words[0] == "All")
                {
                    methodType = MightyMethodType.Query;
                }
                else if (
                    words[0] == "Count" ||
                    words[0] == "Avg" ||
                    words[0] == "Max" ||
                    words[0] == "Min" ||
                    words[0] == "Sum")
                {
                    methodType = MightyMethodType.Aggregate;
                }
#if KEY_VALUES
                else if (wordCount == 2 && words[0] == "Key" && words[1] == "Values")
                {
                    methodType = MightyMethodType.KeyValues;
                    lastWord++;
                }
#endif
                else if (wordCount == 1 && words[0] == "Open")
                {
                    methodType = MightyMethodType.Factory;
                }
                else if (wordCount == 2 && words[0] == "Open" && words[1] == "Connection")
                {
                    methodType = MightyMethodType.OpenConnection;
                    lastWord++;
                }
                else if (wordCount == 2 && words[0] == "Is" && words[1] == "Valid")
                {
                    methodType = MightyMethodType.IsValid;
                    lastWord++;
                }
                else if (wordCount >= 2 && words[0] == "Create" && words[1] == "Command")
                {
                    methodType = MightyMethodType.CreateCommand;
                    lastWord++;
                }
                else if (wordCount == 3 && words[0] == "Results" && words[1] == "As" && words[2] == "Expando")
                {
                    methodType = MightyMethodType.ResultsAsExpando;
                    lastWord += 2;
                }
                else if (wordCount == 3 && words[0] == "Has" && words[1] == "Primary" && words[2] == "Key")
                {
                    methodType = MightyMethodType.HasPrimaryKey;
                    lastWord += 2;
                }
                else if (wordCount == 3 && words[0] == "Get" && words[1] == "Primary" && words[2] == "Key")
                {
                    methodType = MightyMethodType.GetPrimaryKey;
                    lastWord += 2;
                }
                else if (wordCount == 3 && words[0] == "Get" && words[1] == "Column" && (words[2] == "Info" || words[2] == "Default"))
                {
                    methodType = MightyMethodType.GetColumnInfo;
                    lastWord += 2;
                }

                if (methodType != MightyMethodType._Illegal)
                {
                    if (method.IsStatic != (methodType == MightyMethodType.Factory))
                        throw new InvalidOperationException($"{(methodType == MightyMethodType.Factory ? "" : "Only ")}Mighty factory method must be static at method {method.Name} in {mightyType.FriendlyName()}");

#if NET40
                    if (isAsync)
                    {
                        throw new InvalidOperationException($"Async {methodType} method {method.Name} always illegal in .NET Framework 4.0");
                    }
#endif
                    if (methodType == MightyMethodType.New ||
                        methodType == MightyMethodType.CreateCommand ||
                        methodType == MightyMethodType.ResultsAsExpando ||
                        methodType == MightyMethodType.GetColumnInfo ||
                        methodType == MightyMethodType.IsValid ||
                        methodType == MightyMethodType.HasPrimaryKey ||
                        methodType == MightyMethodType.GetPrimaryKey ||
                        methodType == MightyMethodType.Factory)
                    {
#if !NET40
                        if (isAsync)
                        {
                            throw new InvalidOperationException($"Async {methodType} method {method.Name} illegal");
                        }
#endif
                        isSyncOnly = true;
                    }

                    lastWord++;
                    if (wordCount > lastWord)
                    {
                        if (methodType == MightyMethodType.Execute)
                        {
                            if (lastWord + 2 == wordCount && words[lastWord] == "With" && words[lastWord + 1] == "Params") withParams = true;
                            else if (lastWord + 1 == wordCount && words[lastWord] == "Procedure") fromProcedure = true;
                            else methodType = MightyMethodType._Illegal;
                        }
                        else
                        {
                            if (lastWord + 2 == wordCount && words[lastWord] == "With" && words[lastWord + 1] == "Params") withParams = true;
                            else if (lastWord + 2 == wordCount && words[lastWord] == "From" && words[lastWord + 1] == "Procedure") fromProcedure = true;
                            else methodType = MightyMethodType._Illegal;
                        }
                    }
                }
            }

            if (methodType != MightyMethodType._Illegal)
            {
                Type expectedReturnType;

                switch (methodType)
                {
                    case MightyMethodType.Single:
                        if (isAsync) expectedReturnType = typeof(Task<T>);
                        else expectedReturnType = typeof(T);
                        break;

                    case MightyMethodType.Query:
#if !NET40
                        if (isAsync)
#if NETCOREAPP3_0 || NETCOREAPP3_1
                            expectedReturnType = typeof(Task<IAsyncEnumerable<T>>);
#else
                            // this is ambiguous without the namespace in netcoreapp2_0
                            expectedReturnType = typeof(Task<Dasync.Collections.IAsyncEnumerable<T>>);
#endif
                        else
#endif
                            expectedReturnType = typeof(IEnumerable<T>);
                        break;

                    case MightyMethodType.QueryMultiple:
#if !NET40
                        if (isAsync)
                            // TO DO: AsyncMultipleResultSets<T>
#if NETCOREAPP3_0 || NETCOREAPP3_1
                            expectedReturnType = typeof(Task<IAsyncEnumerable<IAsyncEnumerable<T>>>); // throw new NotImplementedException("AsyncMultipleResultSets<T> not implemented yet");
#else
                            // this is ambiguous without the namespace in netcoreapp2_0
                            expectedReturnType = typeof(Task<Dasync.Collections.IAsyncEnumerable<Dasync.Collections.IAsyncEnumerable<T>>>); // throw new NotImplementedException("AsyncMultipleResultSets<T> not implemented yet");
#endif
                        else
#endif
                            expectedReturnType = typeof(MultipleResultSets<T>);
                        break;

                    case MightyMethodType.Execute:
                        if (withParams || fromProcedure)
                        {
                            if (isAsync) expectedReturnType = typeof(Task<object>);
                            else expectedReturnType = typeof(object);
                        }
                        else
                        {
                            if (isAsync) expectedReturnType = typeof(Task<int>);
                            else expectedReturnType = typeof(int);
                        }
                        break;

                    case MightyMethodType.Scalar:
                        if (isAsync) expectedReturnType = typeof(Task<object>);
                        else expectedReturnType = typeof(object);
                        break;

                    case MightyMethodType.Aggregate:
                        if (isAsync) expectedReturnType = typeof(Task<object>);
                        else expectedReturnType = typeof(object);
                        break;

                    case MightyMethodType.Paged:
                        if (isAsync) expectedReturnType = typeof(Task<PagedResults<T>>);
                        else expectedReturnType = typeof(PagedResults<T>);
                        break;

                    case MightyMethodType.Insert:
                        var methodParams = method.GetParameters();
                        if (methodParams.Matches(new Type[] { typeof(object), typeof(DbConnection) }) ||
                            methodParams.Matches(new Type[] { typeof(CancellationToken), typeof(object), typeof(DbConnection) }))
                        {
                            if (isAsync) expectedReturnType = typeof(Task<T>);
                            else expectedReturnType = typeof(T);
                        }
                        else
                        {
                            if (isAsync) expectedReturnType = typeof(Task<List<T>>);
                            else expectedReturnType = typeof(List<T>);
                        }
                        break;

                    case MightyMethodType.Delete:
                    case MightyMethodType.Save:
                    case MightyMethodType.Update:
                        if (isAsync) expectedReturnType = typeof(Task<int>);
                        else expectedReturnType = typeof(int);
                        break;

#if KEY_VALUES
                    case MightyMethodType.KeyValues:
                        if (isAsync) expectedReturnType = typeof(Task<IDictionary<string, string>>);
                        else expectedReturnType = typeof(IDictionary<string, string>);
                        break;
#endif

                    case MightyMethodType.OpenConnection:
                        if (isAsync) expectedReturnType = typeof(Task<DbConnection>);
                        else expectedReturnType = typeof(DbConnection);
                        break;

                    case MightyMethodType.CreateCommand:
                        expectedReturnType = typeof(DbCommand);
                        break;

                    case MightyMethodType.ResultsAsExpando:
                        expectedReturnType = typeof(object);
                        break;

                    case MightyMethodType.New:
                        expectedReturnType = typeof(T);
                        break;

                    case MightyMethodType.GetColumnInfo:
                        expectedReturnType = typeof(object);
                        break;

                    case MightyMethodType.IsValid:
                        expectedReturnType = typeof(List<object>); // hmmm
                        break;

                    case MightyMethodType.HasPrimaryKey:
                        expectedReturnType = typeof(bool);
                        break;

                    case MightyMethodType.GetPrimaryKey:
                        expectedReturnType = typeof(object);
                        break;

                    case MightyMethodType.Factory:
                        expectedReturnType = mightyType;
                        break;

                    default:
                        throw new Exception($"Unexpected method type {methodType} in {nameof(CheckMethod)}");
                }

                var actualReturnType = method.ReturnType;
                if (actualReturnType != expectedReturnType)
                {
                    throw new InvalidOperationException($"Incorrect return type {actualReturnType.FriendlyName()} instead of {expectedReturnType.FriendlyName()} for method {method.Name} in {mightyType.FriendlyName()}");
                }
            }

            // throw exception if we haven't been able to parse this as a known method type
            if (methodType == MightyMethodType._Illegal)
            {
                throw new InvalidOperationException($"Illegal method name {method.Name}");
            }
        }
    }
}
