using System.Runtime.InteropServices;

namespace DereTore.Exchange.Audio.HCA.Native {
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct LoopHeader {

        public uint LOOP {
            get { return _loop; }
            set { _loop = value; }
        }

        public uint LoopStart {
            get { return _loopStart; }
            set { _loopStart = value; }
        }

        public uint LoopEnd {
            get { return _loopEnd; }
            set { _loopEnd = value; }
        }

        public ushort R01 {
            get { return _r01; }
            set { _r01 = value; }
        }

        public ushort R02 {
            get { return _r02; }
            set { _r02 = value; }
        }

        private uint _loop;
        private uint _loopStart;
        private uint _loopEnd;
        private ushort _r01;
        private ushort _r02;

    }
}
