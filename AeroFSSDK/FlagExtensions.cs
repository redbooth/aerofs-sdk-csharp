using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AeroFSSDK
{
    static class FlagExtensions
    {
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
