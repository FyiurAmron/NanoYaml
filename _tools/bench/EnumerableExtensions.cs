using System;
using System.Collections.Generic;
using System.Text;

namespace Vaxquis.NanoYaml.Bench;

public static class EnumerableExtensions {
    extension<T>(IEnumerable<T> enumerable) {
        public string JoinToString(string separator = ", ") {
            StringBuilder sb = new();
            bool first = true;
            foreach (T item in enumerable) {
                if (!first) {
                    sb.Append(separator);
                }

                sb.Append(item);
                first = false;
            }

            return sb.ToString();
        }
    }
}
