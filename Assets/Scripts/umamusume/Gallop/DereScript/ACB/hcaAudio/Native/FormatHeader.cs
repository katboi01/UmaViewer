using System.Runtime.InteropServices;

namespace DereTore.Exchange.Audio.HCA.Native {
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct FormatHeader {

        public uint FMT {
            get { return _fmt; }
            set { _fmt = value; }
        }

        public uint Channels {
            get { return _tempField1 & 0x000000ff; }
            set { _tempField1 = (_tempField1 & 0xffffff00) | (value & 0x000000ff); }
        }

        public uint SamplingRate {
            get { return (_tempField1 & 0xffffff00) >> 8; }
            set { _tempField1 = (_tempField1 & 0x000000ff) | ((value & 0x00ffffff) << 8); }
        }

        public uint Blocks {
            get { return _blocks; }
            set { _blocks = value; }
        }

        public ushort R01 {
            get { return _r01; }
            set { _r01 = value; }
        }

        public ushort R02 {
            get { return _r02; }
            set { _r02 = value; }
        }

        private uint _fmt;
        private uint _tempField1;
        private uint _blocks;
        private ushort _r01;
        private ushort _r02;

    }
}
