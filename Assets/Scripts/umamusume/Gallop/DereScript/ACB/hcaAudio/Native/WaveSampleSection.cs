using System.Runtime.InteropServices;

namespace DereTore.Exchange.Audio.HCA.Native {
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct WaveSampleSection {

        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 4)]
        public byte[] SMPL;

        public uint SmplSize;
        public uint Manufacturer;
        public uint Product;
        public uint SamplePeriod;
        public uint MidiUnityNote;
        public uint MidiPitchFraction;
        public uint SmpteFormat;
        public uint SmpteOffset;
        public uint SampleLoops;
        public uint SamplerData;
        public uint LoopIdentifier;
        public uint LoopType;
        public uint LoopStart;
        public uint LoopEnd;
        public uint LoopFraction;
        public uint LoopPlayCount;

        public static WaveSampleSection CreateDefault() {
            var v = default(WaveSampleSection);
            v.SmplSize = 0x3c;
            v.Manufacturer = 0;
            v.Product = 0;
            v.SamplePeriod = 0;
            v.MidiUnityNote = 0x3c;
            v.MidiPitchFraction = 0;
            v.SmpteFormat = 0;
            v.SmpteOffset = 0;
            v.SampleLoops = 1;
            v.SamplerData = 0x18;
            v.LoopIdentifier = 0;
            v.LoopType = 0;
            v.LoopStart = 0;
            v.LoopEnd = 0;
            v.LoopFraction = 0;
            v.LoopPlayCount = 0;
            HcaHelper.SetString(out v.SMPL, "smpl");
            return v;
        }

    }
}
