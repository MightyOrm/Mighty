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
    public static partial class ObjectExtensions
    {
        public static List<string> CamelCaseSplit(this string str, bool enforceFirstUpper = true)
        {
            if (str == null) return null;
            var results = new List<string>();
            if (str == string.Empty) return results;
            int o = 0, p = 0, w = 0;
            bool firstWord = true;
            do
            {
                bool add;
                if (p == str.Length) add = true;
                else
                {
                    char c = str[p];
                    bool isUpper = (c == c.ToString().ToUpperInvariant()[0]);
                    if (firstWord)
                    {
                        if (enforceFirstUpper && !isUpper)
                        {
                            throw new InvalidOperationException($"{str}: first char of all public member names must be upper case");
                        }
                        firstWord = false;
                    }
                    add = (w > 0 && isUpper);
                }
                if (add)
                {
                    results.Add(str.Substring(o, p - o));
                    w = 0;
                    o = p;
                }
                else
                {
                    w++;
                }
                p++;
            }
            while (p <= str.Length);
            return results;
        }

        public static string FriendlyName(this Type type)
        {
            StringBuilder sb = new StringBuilder();
            if (type.GenericTypeArguments.Length == 0)
            {
                sb.Append(type.Name);
            }
            else
            {
                sb.Append(type.Name.Remove(type.Name.IndexOf('`')));
                sb.Append('<');
                bool first = true;
                foreach (var generic in type.GenericTypeArguments)
                {
                    if (first) first = false;
                    else sb.Append(',');
                    sb.Append(generic.FriendlyName());
                }
                sb.Append('>');
            }
            return sb.ToString();
        }

        public static bool Matches(this ParameterInfo[] us, Type[] them)
        {
            int count = us.Count();
            for (int i = 0; i < count; i++)
            {
                if (us[i].ParameterType != them[i]) return false;
            }
            return true;
        }
    }
    public class MethodChecker<M, T> where T : class, new()
    {
        private Type mightyType;

        List<PropertyInfo> Properties;
        List<MethodInfo> SyncMethods;
        List<MethodInfo> AsyncMethods;

        public MethodChecker(bool isAbstract, bool isVirtual)
        {
            mightyType = typeof(M);

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
                    out bool async,
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
            }
        }

        public void CheckMethod(out MightyMethodType methodType, out bool async, out bool withParams, out bool fromProcedure, MethodInfo method)
        {
            Type returnType;
            withParams = false;
            fromProcedure = false;

            {
                var words = method.Name.CamelCaseSplit();
                int wordCount = words.Count;
                int lastWord = 0;

                async = (words[words.Count - 1] == "Async");
                if (async)
                {
                    async = true;
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
                    if (!method.IsStatic) throw new InvalidOperationException($"Mighty factory method Open must be static in {mightyType.FriendlyName()}");
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
                    if (async)
                    {
                        if (methodType == MightyMethodType.New ||
                            methodType == MightyMethodType.CreateCommand ||
                            methodType == MightyMethodType.ResultsAsExpando ||
                            methodType == MightyMethodType.GetColumnInfo ||
                            methodType == MightyMethodType.IsValid ||
                            methodType == MightyMethodType.HasPrimaryKey ||
                            methodType == MightyMethodType.GetPrimaryKey ||
                            methodType == MightyMethodType.Factory)
                        {
                            throw new InvalidOperationException($"Async {methodType} method {method.Name} illegal");
                        }
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
                        if (async) returnType = typeof(Task<T>);
                        else returnType = typeof(T);
                        break;

                    case MightyMethodType.Query:
                        if (async) returnType = typeof(Task<IAsyncEnumerable<T>>);
                        else returnType = typeof(IEnumerable<T>);
                        break;

                    case MightyMethodType.QueryMultiple:
                        if (async) returnType = typeof(Task<IAsyncEnumerable<IAsyncEnumerable<T>>>); // throw new NotImplementedException("AsyncMultipleResultSets<T> not implemented yet");
                        else returnType = typeof(MultipleResultSets<T>);
                        break;

                    case MightyMethodType.Execute:
                        if (withParams || fromProcedure)
                        {
                            if (async) returnType = typeof(Task<object>);
                            else returnType = typeof(object);
                        }
                        else
                        {
                            if (async) returnType = typeof(Task<int>);
                            else returnType = typeof(int);
                        }
                        break;

                    case MightyMethodType.Scalar:
                        if (async) returnType = typeof(Task<object>);
                        else returnType = typeof(object);
                        break;

                    case MightyMethodType.Aggregate:
                        if (async) returnType = typeof(Task<object>);
                        else returnType = typeof(object);
                        break;

                    case MightyMethodType.Paged:
                        if (async) returnType = typeof(Task<PagedResults<T>>);
                        else returnType = typeof(PagedResults<T>);
                        break;

                    case MightyMethodType.Insert:
                        var methodParams = method.GetParameters();
                        if (methodParams.Matches(new Type[] { typeof(object), typeof(DbConnection) }) ||
                            methodParams.Matches(new Type[] { typeof(CancellationToken), typeof(object), typeof(DbConnection) }))
                        {
                            if (async) returnType = typeof(Task<T>);
                            else returnType = typeof(T);
                        }
                        else
                        {
                            if (async) returnType = typeof(Task<List<T>>);
                            else returnType = typeof(List<T>);
                        }
                        break;

                    case MightyMethodType.Delete:
                    case MightyMethodType.Save:
                    case MightyMethodType.Update:
                        if (async) returnType = typeof(Task<int>);
                        else returnType = typeof(int);
                        break;

#if KEY_VALUES
                    case MightyMethodType.KeyValues:
                        if (async) returnType = typeof(Task<IDictionary<string, string>>);
                        else returnType = typeof(IDictionary<string, string>);
                        break;
#endif

                    case MightyMethodType.OpenConnection:
                        if (async) returnType = typeof(Task<DbConnection>);
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
