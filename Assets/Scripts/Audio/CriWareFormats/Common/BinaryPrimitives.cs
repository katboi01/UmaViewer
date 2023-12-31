using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Audio.CriWareFormats.Common
{
    public class BinaryPrimitives
    {
        internal static ushort ReverseEndianness(ushort le)
        {
            byte[] tmp = BitConverter.GetBytes(le);
            Array.Reverse(tmp);
            return BitConverter.ToUInt16(tmp,0);
        }

        internal static int ReverseEndianness(int le)
        {
            byte[] tmp = BitConverter.GetBytes(le);
            Array.Reverse(tmp);
            return BitConverter.ToInt32(tmp, 0);
        }

        internal static uint ReverseEndianness(uint le)
        {
            byte[] tmp = BitConverter.GetBytes(le);
            Array.Reverse(tmp);
            return BitConverter.ToUInt32(tmp, 0);
        }

        internal static long ReverseEndianness(long le)
        {
            byte[] tmp = BitConverter.GetBytes(le);
            Array.Reverse(tmp);
            return BitConverter.ToInt64(tmp, 0);
        }

        internal static ulong ReverseEndianness(ulong le)
        {
            byte[] tmp = BitConverter.GetBytes(le);
            Array.Reverse(tmp);
            return BitConverter.ToUInt64(tmp, 0);
        }

        public static float Int32BitsToSingle(int m)
        {
            return BitConverter.ToSingle(BitConverter.GetBytes(m), 0);
        }
    }
}
