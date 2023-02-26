using System.Runtime.InteropServices;

namespace DereTore.Exchange.Audio.HCA.Native {
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct RvaHeader {

        public uint RVA {
            get { return _rva; }
            set { _rva = value; }
        }

        public float Volume {
            get { return _volume; }
            set { _volume = value; }
        }

        private uint _rva;
        private float _volume;

    }
}
