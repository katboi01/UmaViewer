using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DereTore.Common {
    public static class EnumerableExtensions {

        public static int FirstIndexOf<T>(this IEnumerable<T> enumerable, Predicate<T> predicate, int startIndex) {
            var i = startIndex;
            var newEnumerable = startIndex == 0 ? enumerable : enumerable.Skip(startIndex);
            foreach (var obj in newEnumerable) {
                if (predicate(obj)) {
                    return i;
                }
                ++i;
            }
            return -1;
        }

        public static int FirstIndexOf<T>(this IEnumerable<T> enumerable, Predicate<T> predicate) {
            return FirstIndexOf(enumerable, predicate, 0);
        }

        public static string BuildString<T>(this IEnumerable<T> enumerable) {
            return BuildString(enumerable, DefaultEnumerableStringSeparator);
        }

        public static string BuildString<T>(this IEnumerable<T> enumerable, string separator) {
            var stringBuilder = new StringBuilder();
            var processedItemCount = 0;
            using (var enumerator = enumerable.GetEnumerator()) {
                while (enumerator.MoveNext()) {
                    if (processedItemCount > 0) {
                        stringBuilder.Append(separator);
                    }
                    stringBuilder.Append(enumerator.Current);
                    ++processedItemCount;
                }
            }
            return stringBuilder.ToString();
        }

        public static bool CountMoreThan<T>(this IEnumerable<T> enumerable, int n) {
            var counter = 0;
            using (var iter = enumerable.GetEnumerator()) {
                while (iter.MoveNext()) {
                    ++counter;
                    if (counter > n) {
                        return true;
                    }
                }
            }
            return false;
        }

        private static readonly string DefaultEnumerableStringSeparator = ", ";

    }
}
