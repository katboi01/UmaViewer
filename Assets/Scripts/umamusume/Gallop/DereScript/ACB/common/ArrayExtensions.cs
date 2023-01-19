using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace DereTore.Common {
    public static class ArrayExtensions {

        public static void ZeroMem(this int[] array) {
            if (array == null) {
                return;
            }
            for (var i = 0; i < array.Length; ++i) {
                array[i] = 0;
            }
        }

        public static void ZeroMem(this uint[] array) {
            if (array == null) {
                return;
            }
            for (var i = 0; i < array.Length; ++i) {
                array[i] = 0;
            }
        }

        public static void ZeroMem(this byte[] array) {
            if (array == null) {
                return;
            }
            for (var i = 0; i < array.Length; ++i) {
                array[i] = 0;
            }
        }

        public static void ZeroMem(this float[] array) {
            if (array == null) {
                return;
            }
            for (var i = 0; i < array.Length; ++i) {
                array[i] = 0f;
            }
        }

        public static int Write(this byte[] array, uint value, int index) {
            if (!BitConverter.IsLittleEndian) {
                value = DereToreHelper.SwapEndian(value);
            }
            var bytes = BitConverter.GetBytes(value);
            for (var i = 0; i < 4; ++i) {
                array[index + i] = bytes[i];
            }
            return 4;
        }

        public static int Write<T>(this byte[] array, T value, int index) where T : struct {
            var size = Marshal.SizeOf(typeof(T));
            var bytes = new byte[size];
            var ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(value, ptr, true);
            Marshal.Copy(ptr, bytes, 0, size);
            Marshal.FreeHGlobal(ptr);
            Array.Copy(bytes, 0, array, index, size);
            return size;
        }

        public static int Write(this byte[] array, byte[] value, int index) {
            var length = value.Length;
            Array.Copy(value, 0, array, index, length);
            return length;
        }

        public static byte[] Append(this byte[] data, byte[] toAppend) {
            var buffer = new byte[data.Length + toAppend.Length];
            data.CopyTo(buffer, 0);
            toAppend.CopyTo(buffer, data.Length);
            return buffer;
        }

        public static byte[] Append(this byte[] data, byte toAppend) {
            var buffer = new byte[data.Length + 1];
            data.CopyTo(buffer, 0);
            buffer[buffer.Length - 1] = toAppend;
            return buffer;
        }

        public static int IndexOf<T>(this T[] array, T item) {
            for (var i = 0; i < array.Length; ++i) {
                if (EqualityComparer<T>.Default.Equals(array[i], item)) {
                    return i;
                }
            }
            return -1;
        }

    }
}
