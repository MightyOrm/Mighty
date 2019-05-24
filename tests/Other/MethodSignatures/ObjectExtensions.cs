using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
}
