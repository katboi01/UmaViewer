using System.Runtime.InteropServices;

namespace DereTore.Exchange.Audio.HCA.Native {
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct VbrHeader {

        public uint VBR {
            get { return _vbr; }
            set { _vbr = value; }
        }

        public ushort R01 {
            get { return _r01; }
            set { _r01 = value; }
        }

        public ushort R02 {
            get { return _r02; }
            set { _r02 = value; }
        }

        private uint _vbr;
        private ushort _r01;
        private ushort _r02;

    }
}
