using System.Runtime.InteropServices;

namespace DereTore.Exchange.Archive.ACB {
    [StructLayout(LayoutKind.Explicit)]
    public struct NumericUnion {

        [FieldOffset(0)]
        public byte U8;
        [FieldOffset(0)]
        public sbyte S8;
        [FieldOffset(0)]
        public short S16;
        [FieldOffset(0)]
        public ushort U16;
        [FieldOffset(0)]
        public int S32;
        [FieldOffset(0)]
        public uint U32;
        [FieldOffset(0)]
        public long S64;
        [FieldOffset(0)]
        public ulong U64;
        [FieldOffset(0)]
        public float R32;
        [FieldOffset(0)]
        public double R64;

    }
}
