namespace DereTore.Exchange.Audio.HCA {
    public static class MagicValues {

        public static readonly uint HCA = 0x00414348;

        public static readonly uint FMT = 0x00746d66;

        public static readonly uint COMP = 0x706d6f63;

        public static readonly uint DEC = 0x00636564;

        public static readonly uint VBR = 0x00726276;

        public static readonly uint ATH = 0x00687461;

        public static readonly uint LOOP = 0x706f6f6c;

        public static readonly uint CIPH = 0x68706963;

        public static readonly uint RVA = 0x00617672;

        public static readonly uint COMM = 0x6d6d6f63;

        public static readonly uint PAD = 0x00646170;

        public static bool IsMagicMatch(uint valueRead, uint valueToCheck) {
            return (valueRead & 0x7f7f7f7f) == valueToCheck;
        }

    }
}
