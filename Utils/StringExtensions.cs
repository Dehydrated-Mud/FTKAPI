using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FTKAPI.Utils
{
    public static class StringExtensions
    {

        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source?.IndexOf(toCheck, comp) >= 0;
        }
        public static bool ContainsAny(this string source, List<string> toCheck)
        {
            bool flag = false;
            foreach (string str in toCheck)
            {
                flag |= source?.IndexOf(str, StringComparison.OrdinalIgnoreCase) >= 0;
                if (flag)
                {
                    break;
                }
            }
            return flag;
        }

        public static string TrimStr(this string str, string trimStr,
                      bool trimEnd = true, bool repeatTrim = true,
                      StringComparison comparisonType = StringComparison.OrdinalIgnoreCase)
        {
            int strLen;
            do
            {
                strLen = str.Length;
                {
                    if (trimEnd)
                    {
                        if (!(str ?? "").EndsWith(trimStr)) return str;
                        var pos = str.LastIndexOf(trimStr, comparisonType);
                        if ((!(pos >= 0)) || (!(str.Length - trimStr.Length == pos))) break;
                        str = str.Substring(0, pos);
                    }
                    else
                    {
                        if (!(str ?? "").StartsWith(trimStr)) return str;
                        var pos = str.IndexOf(trimStr, comparisonType);
                        if (!(pos == 0)) break;
                        str = str.Substring(trimStr.Length, str.Length - trimStr.Length);
                    }
                }
            } while (repeatTrim && strLen > str.Length);
            return str;
        }

        // the following is C#6 syntax, if you're not using C#6 yet
        // replace "=> ..." by { return ... }

        public static string TrimEnd(this string str, string trimStr,
                bool repeatTrim = true,
                StringComparison comparisonType = StringComparison.OrdinalIgnoreCase)
                => TrimStr(str, trimStr, true, repeatTrim, comparisonType);

        public static string TrimStart(this string str, string trimStr,
                bool repeatTrim = true,
                StringComparison comparisonType = StringComparison.OrdinalIgnoreCase)
                => TrimStr(str, trimStr, false, repeatTrim, comparisonType);

        public static string Trim(this string str, string trimStr, bool repeatTrim = true,
            StringComparison comparisonType = StringComparison.OrdinalIgnoreCase)
            => str.TrimStart(trimStr, repeatTrim, comparisonType)
                  .TrimEnd(trimStr, repeatTrim, comparisonType);



    }
}
