using System;

namespace DereTore.Exchange.Archive.ACB.Serialization {

    internal static class AcbSerializerPrivateUtilities {

        public static int IndexOf(this Type[] types, Type value) {
            for (var i = 0; i < types.Length; ++i) {
                if (types[i] == value) {
                    return i;
                }
            }
            return -1;
        }

    }
}
