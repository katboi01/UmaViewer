using System.Runtime.InteropServices;

namespace DereTore.Exchange.Audio.HCA.Native {
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct WaveNoteSection {

        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 4)]
        public byte[] NOTE;

        public uint NoteSize;
        public uint Name;

        public static WaveNoteSection CreateDefault() {
            var v = default(WaveNoteSection);
            v.Name = 0;
            v.NoteSize = 0;
            HcaHelper.SetString(out v.NOTE, "note");
            return v;
        }

    }
}
