using System.Runtime.InteropServices;

namespace DereTore.Exchange.Audio.HCA.Native {
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct WaveDataSection {

        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 4)]
        public byte[] DATA;

        public uint DataSize;

        public static WaveDataSection CreateDefault() {
            var v = default(WaveDataSection);
            v.DataSize = 0;
            HcaHelper.SetString(out v.DATA, "data");
            return v;
        }

    }
}
