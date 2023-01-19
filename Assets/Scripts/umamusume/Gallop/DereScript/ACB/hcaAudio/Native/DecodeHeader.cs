using System.Runtime.InteropServices;

namespace DereTore.Exchange.Audio.HCA.Native {
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DecodeHeader {

        public uint DEC {
            get { return _dec; }
            set { _dec = value; }
        }

        public ushort BlockSize {
            get { return _blockSize; }
            set { _blockSize = value; }
        }

        public byte R01 {
            get { return _r01; }
            set { _r01 = value; }
        }

        public byte R02 {
            get { return _r02; }
            set { _r02 = value; }
        }

        public byte Count1 {
            get { return _count1; }
            set { _count1 = value; }
        }

        public byte Count2 {
            get { return _count2; }
            set { _count2 = value; }
        }

        public byte R03 {
            get { return (byte)(_tmpField1 & 0x0f); }
            set { _tmpField1 = (byte)((_tmpField1 & 0xf0) | (value & 0x0f)); }
        }

        public byte R04 {
            get { return (byte)((_tmpField1 & 0xf0) >> 4); }
            set { _tmpField1 = (byte)((_tmpField1 & 0x0f) | ((value & 0x0f) << 4)); }
        }

        public bool EnableCount2 {
            get { return _enableCount2 != 0; }
            set { _enableCount2 = (byte)(value ? 1 : 0); }
        }

        private uint _dec;
        private ushort _blockSize;
        private byte _r01;
        private byte _r02;
        private byte _count1;
        private byte _count2;
        private byte _tmpField1;
        private byte _enableCount2;

    }
}
