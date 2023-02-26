namespace DereTore.Exchange.Audio.HCA {
    internal sealed class DataBits {

        public DataBits(byte[] data, uint size) {
            _data = data;
            _size = size * 8 - 16;
            _bit = 0;
        }

        public int CheckBit(int bitSize)
        {
            var v = 0;
            if (_bit + bitSize <= _size)
            {
                var i = _bit >> 3;
                byte data;

                if (i + 2 >= _data.Length)
                {
                    data = 0; //なぜかインデックス外になることがあるため
                }
                else
                {
                    data = _data[i + 2];
                }

                v = _data[i];
                v = (v << 8) | _data[i + 1];
                v = (v << 8) | data;
                v &= Mask[_bit & 7];
                v >>= 24 - (_bit & 7) - bitSize;
            }
            return v;
        }

        public int GetBit(int bitSize) {
            var v = CheckBit(bitSize);
            _bit += bitSize;
            return v;
        }

        public void AddBit(int bitSize) {
            _bit += bitSize;
        }

        private static readonly int[] Mask = { 0xffffff, 0x7fffff, 0x3fffff, 0x1fffff, 0x0fffff, 0x07ffff, 0x03ffff, 0x01ffff };

        private readonly byte[] _data;
        private readonly uint _size;
        private int _bit;

    }
}
