namespace DereTore.Exchange.Audio.HCA {
    internal static class ErrorMessages {

        public static string GetBufferTooSmall(int minimum, int actual) {
            return string.Format("Buffer too small. Required minimum: {0}, actual: {1}",minimum,actual);
        }

        public static string GetInvalidParameter(string paramName) {
            return string.Format("Parameter '{0}' is invalid.",paramName);
        }

        public static string GetChecksumNotMatch(int expected, int actual) {
            return string.Format("Checksum does not match. Expected: {0}({0:x8}), actual: {1}({1:x8}).",expected,actual);
        }

        public static string GetMagicNotMatch(int expected, int actual) {
            return string.Format("Magic does not match. Expected: {0}({0:x8}), actual: {1}({1:x8}).",expected,actual);
        }

        public static string GetAthInitializationFailed() {
            return "ATH table initialization failed.";
        }

        public static string GetCiphInitializationFailed() {
            return "CIPH table initialization failed.";
        }

    }
}
