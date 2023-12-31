using System;
using System.Collections.Generic;
using System.Text;

namespace CriWareFormats.Common
{
    public static class Endian
    {
        public static ushort Reverse(ushort value)
        {
            return (ushort)(value << 8 | value >> 8);
        }

        public static uint Reverse(uint value)
        {
            value = (value & 0x00FF00FFu) << 8 | (value & 0xFF00FF00u) >> 8;
            return (value & 0x0000FFFFu) << 16 | (value & 0xFFFF0000u) >> 16;
        }

        public static ulong Reverse(ulong value)
        {
            value = (value & 0x00FF00FF00FF00FFu) << 8 | (value & 0xFF00FF00FF00FF00u) >> 8;
            value = (value & 0x0000FFFF0000FFFFu) << 16 | (value & 0xFFFF0000FFFF0000u) >> 16;
            return value << 32 | value >> 32;
        }
    }
}
