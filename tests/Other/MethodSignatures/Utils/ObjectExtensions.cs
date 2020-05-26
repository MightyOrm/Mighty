using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Mighty.MethodSignatures
{
    public static partial class ObjectExtensions
    {
        /// <summary>
        /// Assert that none in a list of methods has a param of a given type
        /// </summary>
        /// <param name="methods"></param>
        /// <param name="t"></param>
        public static void DoNotContainParamType(this IEnumerable<MethodInfo> methods, Type t)
        {
            methods.ForEach(m => {
                if (m.ContainsParamType(t))
                {
                    throw new InvalidOperationException($"Found {t.Name} parameter in sync-only method {m.Name}");
                }
            });
        }

        /// <summary>
        /// Returns <c>true</c> iff this method has at least one param of the specified type
        /// </summary>
        /// <param name="method"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static bool ContainsParamType(this MethodInfo method, Type t)
        {
            var param = method.GetParameters().Where(p => t == p.ParameterType).FirstOrDefault();
            return param != null;
        }

        /// <summary>
        /// enumerable for each
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <param name="action"></param>
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var item in enumerable) action(item);
        }

        /// <summary>
        /// Split a camel case name into its separate words
        /// </summary>
        /// <param name="str"></param>
        /// <param name="enforceFirstUpper"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Produce a more quickly readble, friendly type name
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string FriendlyName(this Type type)
        {
#if NET40
            return type.Name;
#else
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
#endif
        }

        /// <summary>
        /// Confirm that the parameter types in a <see cref="ParameterInfo"/> array match the specified array of types
        /// </summary>
        /// <param name="us"></param>
        /// <param name="them"></param>
        /// <returns></returns>
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
}
