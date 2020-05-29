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
using System.CodeDom;

namespace Mighty.MethodSignatures
{
    public class MethodChecker<M, T> where T : class, new()
    {
        private static readonly Type dbConnectionType = typeof(DbConnection);
        private static readonly Type cancellationTokenType = typeof(CancellationToken);

        private Type mightyType;

        /// <summary>
        /// Mighty methods indexable by various types
        /// </summary>
        public readonly MightyMethodList methodList;

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

            // enforce no public fields present
            var fields = mightyType.GetFields();
            Assert.AreEqual(0, fields.Length);

            // TO DO: test some stuff on the properties
            //Properties = mightyType.GetProperties().Where(p => p.DeclaringType == mightyType).ToList();

            // populate the method info lists...
            methodList = new MightyMethodList(
                mightyType
                .GetMethods()
                .Where(m => m.DeclaringType == mightyType && !m.IsSpecialName)
                .Select(method =>
                    {
                        if (method.IsAbstract != isAbstract)
                        {
                            throw new InvalidOperationException($"Methods in {mightyType.Name} must {(isAbstract ? "" : "not ")}be abstract (method: {method.Name})");
                        }

                        if (!method.IsStatic && method.IsVirtual != isVirtual)
                        {
                            throw new InvalidOperationException($"Methods in {mightyType.Name} must {(isVirtual ? "" : "not ")}be virtual (method: {method.Name})");
                        }

                        foreach (var param in method.GetParameters())
                        {
#if !NET40
                            // this is just a sanity check that both of these flags are the same for everything we're doing
                            // (AFAIK they can't be different in C#, but if they can be, we need to understand why and work
                            // out which one to use)
                            if (param.HasDefaultValue != param.IsOptional)
                            {
                                throw new InvalidOperationException("Unhandled param configuration");
                            }
#endif
                        }

                        return ParseMethod(method);
                    }
                )
            );
        }

        /// <summary>
        /// Parse an individual method, determining which known <see cref="MightyMethodType"/> it has.
        /// Exception - test fail - if any method doesn't have a known method type.
        /// Also already does various checks, including that all the method return types make sense
        /// for the known method types.
        /// </summary>
        /// <param name="method"></param>
        private MightyMethodInfo ParseMethod(MethodInfo method)
        {
            var methodInfo = DetermineMethodInfo(method);

            // throw exception if we haven't been able to parse this as a known method type
            if (methodInfo.methodType == MightyMethodType._Illegal)
            {
                throw new InvalidOperationException($"Illegal method name {method.Name}");
            }

            var expectedReturnType = DetermineReturnType(method, methodInfo);
            if (expectedReturnType != method.ReturnType)
            {
                throw new InvalidOperationException($"Incorrect return type {method.ReturnType.FriendlyName()} instead of {expectedReturnType.FriendlyName()} for method {method.Name} in {mightyType.FriendlyName()}");
            }

            return methodInfo;
        }
        private MightyMethodInfo DetermineMethodInfo(MethodInfo method)
        {
            MightyMethodType methodType = MightyMethodType._Illegal;
            MightyParamsType paramsType = MightyParamsType.Normal;
            bool isSyncOnly = false;
            bool isAsync = false;

            var words = method.Name.CamelCaseSplit();
            int wordCount = words.Count;
            int lastWord = 0;

            if (words[wordCount - 1] == "Async")
            {
                isAsync = true;
                wordCount--;
            }

            // set the method type from word[0] if possible (may still be overridden);
            // if not, identify it by hand
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
                        if (lastWord + 2 == wordCount && words[lastWord] == "With" && words[lastWord + 1] == "Params") paramsType = MightyParamsType.WithParams;
                        else if (lastWord + 1 == wordCount && words[lastWord] == "Procedure") paramsType = MightyParamsType.FromProcedure;
                        else methodType = MightyMethodType._Illegal;
                    }
                    else
                    {
                        if (lastWord + 2 == wordCount && words[lastWord] == "With" && words[lastWord + 1] == "Params") paramsType = MightyParamsType.WithParams;
                        else if (lastWord + 2 == wordCount && words[lastWord] == "From" && words[lastWord + 1] == "Procedure") paramsType = MightyParamsType.FromProcedure;
                        else methodType = MightyMethodType._Illegal;
                    }
                }
            }

            MightyVariantType variantType =
                (method.ContainsParamType(typeof(DbConnection)) ? MightyVariantType.DbConnection : MightyVariantType.None) |
                (method.ContainsParamType(typeof(CancellationToken)) ? MightyVariantType.CancellationToken : MightyVariantType.None);

            MightySyncType syncType;
            if (method.IsStatic) syncType = MightySyncType.Static;
            else if (isSyncOnly) syncType = MightySyncType.SyncOnly;
            else if (isAsync) syncType = MightySyncType.Async;
            else syncType = MightySyncType.Sync;

            return new MightyMethodInfo() {
                method = method,
                syncType = syncType,
                methodType = methodType,
                paramsType = paramsType,
                variantType = variantType
            };
        }

        public Type DetermineReturnType(MethodInfo method, MightyMethodInfo methodInfo)
        {
            Type expectedReturnType;

            switch (methodInfo.methodType)
            {
                case MightyMethodType.Single:
                    if (methodInfo.syncType == MightySyncType.Async) expectedReturnType = typeof(Task<T>);
                    else expectedReturnType = typeof(T);
                    break;

                case MightyMethodType.Query:
#if !NET40
                    if (methodInfo.syncType == MightySyncType.Async)
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
                    if (methodInfo.syncType == MightySyncType.Async)
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
                    if (methodInfo.paramsType != MightyParamsType.Normal)
                    {
                        if (methodInfo.syncType == MightySyncType.Async) expectedReturnType = typeof(Task<object>);
                        else expectedReturnType = typeof(object);
                    }
                    else
                    {
                        if (methodInfo.syncType == MightySyncType.Async) expectedReturnType = typeof(Task<int>);
                        else expectedReturnType = typeof(int);
                    }
                    break;

                case MightyMethodType.Scalar:
                    if (methodInfo.syncType == MightySyncType.Async) expectedReturnType = typeof(Task<object>);
                    else expectedReturnType = typeof(object);
                    break;

                case MightyMethodType.Aggregate:
                    if (methodInfo.syncType == MightySyncType.Async) expectedReturnType = typeof(Task<object>);
                    else expectedReturnType = typeof(object);
                    break;

                case MightyMethodType.Paged:
                    if (methodInfo.syncType == MightySyncType.Async) expectedReturnType = typeof(Task<PagedResults<T>>);
                    else expectedReturnType = typeof(PagedResults<T>);
                    break;

                case MightyMethodType.Insert:
                    var methodParams = method.GetParameters();
                    if (methodParams.Matches(new Type[] { typeof(object), typeof(DbConnection) }) ||
                        methodParams.Matches(new Type[] { typeof(CancellationToken), typeof(object), typeof(DbConnection) }))
                    {
                        if (methodInfo.syncType == MightySyncType.Async) expectedReturnType = typeof(Task<T>);
                        else expectedReturnType = typeof(T);
                    }
                    else
                    {
                        if (methodInfo.syncType == MightySyncType.Async) expectedReturnType = typeof(Task<List<T>>);
                        else expectedReturnType = typeof(List<T>);
                    }
                    break;

                case MightyMethodType.Delete:
                case MightyMethodType.Save:
                case MightyMethodType.Update:
                    if (methodInfo.syncType == MightySyncType.Async) expectedReturnType = typeof(Task<int>);
                    else expectedReturnType = typeof(int);
                    break;

#if KEY_VALUES
                case MightyMethodType.KeyValues:
                    if (methodInfo.syncType == MightySyncType.Async) expectedReturnType = typeof(Task<IDictionary<string, string>>);
                    else expectedReturnType = typeof(IDictionary<string, string>);
                    break;
#endif

                case MightyMethodType.OpenConnection:
                    if (methodInfo.syncType == MightySyncType.Async) expectedReturnType = typeof(Task<DbConnection>);
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
                    expectedReturnType = typeof(List<object>); // hmmm - TO DO: why?
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
                    throw new Exception($"Unexpected method type {methodInfo.methodType} in {nameof(ParseMethod)}");
            }

            return expectedReturnType;
        }

        public MightyMethodList this[MightyMethodType type]
        {
            get
            {
                return methodList[type];
            }
        }

        public MightyMethodList this[MightySyncType type]
        {
            get
            {
                return methodList[type];
            }
        }

        public MightyMethodList this[MightyParamsType type]
        {
            get
            {
                return methodList[type];
            }
        }

        public MightyMethodList this[MightyVariantType type]
        {
            get
            {
                return methodList[type];
            }
        }

        public MightyMethodList this[Func<MightyMethodInfo, bool> filter]
        {
            get
            {
                return methodList[filter];
            }
        }
    }
}
