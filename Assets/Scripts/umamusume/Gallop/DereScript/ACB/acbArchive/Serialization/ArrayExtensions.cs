namespace DereTore.Exchange.Archive.ACB.Serialization {
    internal static class ArrayExtensions {

        public static byte[] RoundUpTo(this byte[] data, int alignment) {
            var newLength = AcbHelper.RoundUpToAlignment(data.Length, alignment);
            var buffer = new byte[newLength];
            data.CopyTo(buffer, 0);
            return buffer;
        }

        public static byte[] RoundUpTo(this byte[] data, uint alignment) {
            var newLength = AcbHelper.RoundUpToAlignment(data.Length, alignment);
            var buffer = new byte[newLength];
            data.CopyTo(buffer, 0);
            return buffer;
        }

    }
}
