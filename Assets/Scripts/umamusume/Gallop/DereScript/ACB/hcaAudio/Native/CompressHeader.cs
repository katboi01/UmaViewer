using System.Runtime.InteropServices;

namespace DereTore.Exchange.Audio.HCA.Native {
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CompressHeader {

        public uint COMP {
            get { return _comp; }
            set { _comp = value; }
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

        public byte R03 {
            get { return _r03; }
            set { _r03 = value; }
        }

        public byte R04 {
            get { return _r04; }
            set { _r04 = value; }
        }

        public byte R05 {
            get { return _r05; }
            set { _r05 = value; }
        }

        public byte R06 {
            get { return _r06; }
            set { _r06 = value; }
        }

        public byte R07 {
            get { return _r07; }
            set { _r07 = value; }
        }

        public byte R08 {
            get { return _r08; }
            set { _r08 = value; }
        }

        public byte Reserved1 {
            get { return _rs1; }
            set { _rs1 = value; }
        }

        public byte Reserved2 {
            get { return _rs2; }
            set { _rs2 = value; }
        }

        private uint _comp;
        private ushort _blockSize;
        private byte _r01;
        private byte _r02;
        private byte _r03;
        private byte _r04;
        private byte _r05;
        private byte _r06;
        private byte _r07;
        private byte _r08;
        private byte _rs1;
        private byte _rs2;

    }
}
