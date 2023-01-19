using System.Runtime.InteropServices;

namespace DereTore.Exchange.Audio.HCA.Native {
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct WaveRiffSection {

        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 4)]
        public byte[] RIFF;

        public uint RiffSize;

        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 4)]
        public byte[] WAVE;

        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 4)]
        public byte[] FMT;

        public uint FmtSize;
        public ushort FmtType;
        public ushort FmtChannels;
        public uint FmtSamplingRate;
        public uint FmtSamplesPerSec;
        public ushort FmtSamplingSize;
        public ushort FmtBitCount;

        public static WaveRiffSection CreateDefault() {
            var v = default(WaveRiffSection);
            v.RiffSize = 0;
            v.FmtSize = 0x10;
            v.FmtType = 0;
            v.FmtChannels = 0;
            v.FmtSamplingRate = 0;
            v.FmtSamplesPerSec = 0;
            v.FmtSamplingSize = 0;
            v.FmtBitCount = 0;
            HcaHelper.SetString(out v.RIFF, "RIFF");
            HcaHelper.SetString(out v.WAVE, "WAVE");
            HcaHelper.SetString(out v.FMT, "fmt ");
            return v;
        }

    }
}
