using System;

namespace DereTore.Common {
    public static class DereToreHelper {

        public static ushort SwapEndian(ushort v) {
            var r = (ushort)(v & 0xff);
            r <<= 8;
            v >>= 8;
            r |= (ushort)(v & 0xff);
            return r;
        }

        public static short SwapEndian(short v) {
            unchecked {
                var s = (ushort)v;
                return (short)SwapEndian(s);
            }
        }

        public static uint SwapEndian(uint v) {
            var r = v & 0xff;
            r <<= 8;
            v >>= 8;
            r |= v & 0xff;
            r <<= 8;
            v >>= 8;
            r |= v & 0xff;
            r <<= 8;
            v >>= 8;
            r |= v & 0xff;
            return r;
        }

        public static int SwapEndian(int v) {
            unchecked {
                var s = (uint)v;
                return (int)SwapEndian(s);
            }
        }

        public static ulong SwapEndian(ulong v) {
            var r = v & 0xff;
            r <<= 8;
            v >>= 8;
            r |= v & 0xff;
            r <<= 8;
            v >>= 8;
            r |= v & 0xff;
            r <<= 8;
            v >>= 8;
            r |= v & 0xff;
            r <<= 8;
            v >>= 8;
            r |= v & 0xff;
            r <<= 8;
            v >>= 8;
            r |= v & 0xff;
            r <<= 8;
            v >>= 8;
            r |= v & 0xff;
            r <<= 8;
            v >>= 8;
            r |= v & 0xff;
            return r;
        }

        public static long SwapEndian(long v) {
            unchecked {
                var s = (ulong)v;
                return (long)SwapEndian(s);
            }
        }

        public static float SwapEndian(float v) {
            var bytes = BitConverter.GetBytes(v);
            var b = bytes[0];
            bytes[0] = bytes[1];
            bytes[1] = b;
            b = bytes[2];
            bytes[2] = bytes[3];
            bytes[3] = b;
            return BitConverter.ToSingle(bytes, 0);
        }

        public static double SwapEndian(double v) {
            var bytes = BitConverter.GetBytes(v);
            Array.Reverse(bytes);
            return BitConverter.ToDouble(bytes, 0);
        }

        public static void Swap<T>(ref T t1, ref T t2) {
            var t = t1;
            t1 = t2;
            t2 = t;
        }

    }
}
