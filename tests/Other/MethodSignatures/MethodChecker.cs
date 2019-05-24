using System;
using System.Collections.Async;
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

        public readonly List<PropertyInfo> Properties;

        public readonly List<MethodInfo> StaticMethods;
        public readonly List<MethodInfo> SyncOnlyMethods;
        public readonly List<MethodInfo> SyncMethods;
        public readonly List<MethodInfo> AsyncMethods;

        public MethodChecker(bool isAbstract, bool isVirtual)
        {
            mightyType = typeof(M);

            StaticMethods = new List<MethodInfo>();
            SyncMethods = new List<MethodInfo>();
            SyncOnlyMethods = new List<MethodInfo>();
            AsyncMethods = new List<MethodInfo>();

            // no fields expected
            var fields = mightyType.GetFields();
            Assert.AreEqual(0, fields.Length);

            // not sure what we're going to test here
            Properties = mightyType.GetProperties().Where(p => p.DeclaringType == mightyType).ToList();

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
                    out bool isAsync,
                    out bool isSyncOnly,
                    out bool withParams,
                    out bool fromProcedure,
                    method);

                foreach (var param in method.GetParameters())
                {
                    if (param.HasDefaultValue != param.IsOptional)
                    {
                        throw new InvalidOperationException("Unhandled param configuration");
                    }
                    if (param.HasDefaultValue)
                    {
                        int c = 1;
                    }
                    if (param.IsOptional)
                    {
                        int d = 1;
                    }
                }

                if (method.IsStatic) StaticMethods.Add(method);
                else if (isSyncOnly) SyncOnlyMethods.Add(method);
                else if (isAsync) AsyncMethods.Add(method);
                else SyncMethods.Add(method);
            }
        }

        public void CheckMethod(
            out MightyMethodType methodType,
            out bool isAsync,
            out bool isSyncOnly,
            out bool withParams,
            out bool fromProcedure,
            MethodInfo method)
        {
            Type returnType;
            isSyncOnly = false;
            withParams = false;
            fromProcedure = false;

            {
                var words = method.Name.CamelCaseSplit();
                int wordCount = words.Count;
                int lastWord = 0;

                isAsync = (words[words.Count - 1] == "Async");
                if (isAsync)
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

                    if (methodType == MightyMethodType.New ||
                        methodType == MightyMethodType.CreateCommand ||
                        methodType == MightyMethodType.ResultsAsExpando ||
                        methodType == MightyMethodType.GetColumnInfo ||
                        methodType == MightyMethodType.IsValid ||
                        methodType == MightyMethodType.HasPrimaryKey ||
                        methodType == MightyMethodType.GetPrimaryKey ||
                        methodType == MightyMethodType.Factory)
                    {
                        if (isAsync)
                        {
                            throw new InvalidOperationException($"Async {methodType} method {method.Name} illegal");
                        }
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
                switch (methodType)
                {
                    case MightyMethodType.Single:
                        if (isAsync) returnType = typeof(Task<T>);
                        else returnType = typeof(T);
                        break;

                    case MightyMethodType.Query:
                        if (isAsync) returnType = typeof(Task<IAsyncEnumerable<T>>);
                        else returnType = typeof(IEnumerable<T>);
                        break;

                    case MightyMethodType.QueryMultiple:
                        if (isAsync) returnType = typeof(Task<IAsyncEnumerable<IAsyncEnumerable<T>>>); // throw new NotImplementedException("AsyncMultipleResultSets<T> not implemented yet");
                        else returnType = typeof(MultipleResultSets<T>);
                        break;

                    case MightyMethodType.Execute:
                        if (withParams || fromProcedure)
                        {
                            if (isAsync) returnType = typeof(Task<object>);
                            else returnType = typeof(object);
                        }
                        else
                        {
                            if (isAsync) returnType = typeof(Task<int>);
                            else returnType = typeof(int);
                        }
                        break;

                    case MightyMethodType.Scalar:
                        if (isAsync) returnType = typeof(Task<object>);
                        else returnType = typeof(object);
                        break;

                    case MightyMethodType.Aggregate:
                        if (isAsync) returnType = typeof(Task<object>);
                        else returnType = typeof(object);
                        break;

                    case MightyMethodType.Paged:
                        if (isAsync) returnType = typeof(Task<PagedResults<T>>);
                        else returnType = typeof(PagedResults<T>);
                        break;

                    case MightyMethodType.Insert:
                        var methodParams = method.GetParameters();
                        if (methodParams.Matches(new Type[] { typeof(object), typeof(DbConnection) }) ||
                            methodParams.Matches(new Type[] { typeof(CancellationToken), typeof(object), typeof(DbConnection) }))
                        {
                            if (isAsync) returnType = typeof(Task<T>);
                            else returnType = typeof(T);
                        }
                        else
                        {
                            if (isAsync) returnType = typeof(Task<List<T>>);
                            else returnType = typeof(List<T>);
                        }
                        break;

                    case MightyMethodType.Delete:
                    case MightyMethodType.Save:
                    case MightyMethodType.Update:
                        if (isAsync) returnType = typeof(Task<int>);
                        else returnType = typeof(int);
                        break;

#if KEY_VALUES
                    case MightyMethodType.KeyValues:
                        if (isAsync) returnType = typeof(Task<IDictionary<string, string>>);
                        else returnType = typeof(IDictionary<string, string>);
                        break;
#endif

                    case MightyMethodType.OpenConnection:
                        if (isAsync) returnType = typeof(Task<DbConnection>);
                        else returnType = typeof(DbConnection);
                        break;

                    case MightyMethodType.CreateCommand:
                        returnType = typeof(DbCommand);
                        break;

                    case MightyMethodType.ResultsAsExpando:
                        returnType = typeof(object);
                        break;

                    case MightyMethodType.New:
                        returnType = typeof(T);
                        break;

                    case MightyMethodType.GetColumnInfo:
                        returnType = typeof(object);
                        break;

                    case MightyMethodType.IsValid:
                        returnType = typeof(List<object>); // hmmm
                        break;

                    case MightyMethodType.HasPrimaryKey:
                        returnType = typeof(bool);
                        break;

                    case MightyMethodType.GetPrimaryKey:
                        returnType = typeof(object);
                        break;

                    case MightyMethodType.Factory:
                        returnType = mightyType;
                        break;

                    default:
                        throw new Exception($"Unexpected method type {methodType} in {nameof(CheckMethod)}");
                }
                var methodReturnType = method.ReturnType;
                if (methodReturnType != returnType)
                {
                    throw new InvalidOperationException($"Incorrect return type {methodReturnType.FriendlyName()} instead of {returnType.FriendlyName()} for method {method.Name} in {mightyType.FriendlyName()}");
                }
            }
            if (methodType == MightyMethodType._Illegal)
            {
                throw new InvalidOperationException($"Illegal method name {method.Name}");
            }
        }
    }
}
