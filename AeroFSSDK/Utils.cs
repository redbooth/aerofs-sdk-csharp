using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AeroFSSDK
{
    static class Utils
    {
        public static string FormatWith(this string format, object arg0)
        {
            return string.Format(format, arg0);
        }

        public static string FormatWith(this string format, object arg0, object arg1)
        {
            return string.Format(format, arg0, arg1);
        }

        public static string FormatWith(this string format, object arg0, object arg1, object arg2)
        {
            return string.Format(format, arg0, arg1, arg2);
        }

        public static string FormatWith(this string format, params object[] args)
        {
            return string.Format(format, args);
        }

        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        public static string Format<T>(this T value, string param, IDictionary<T, string> fields) where T : struct, IConvertible
        {
            var selectedFields = fields.Where(field => value.HasFlag(field.Key))
                .Select(field => field.Value)
                .ToArray();
            return selectedFields.Length == 0
                ? ""
                : "?{0}={1}".FormatWith(param, string.Join(",", selectedFields));
        }

        // N.B. technically all values contains the flag 0
        public static bool HasFlag<T>(this T value, T flag) where T : struct, IConvertible
        {
            var v = value.ToInt64(null);
            var f = flag.ToInt64(null);
            return (v & f) == f;
        }
    }
}
