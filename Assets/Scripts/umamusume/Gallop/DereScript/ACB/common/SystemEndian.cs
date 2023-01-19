using System;

namespace DereTore.Common {
    public static class SystemEndian {

        public static readonly Endian Type = BitConverter.IsLittleEndian ? Endian.LittleEndian : Endian.BigEndian;

    }
}
