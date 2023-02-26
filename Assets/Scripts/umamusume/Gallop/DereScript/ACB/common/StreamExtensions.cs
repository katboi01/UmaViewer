using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace DereTore.Common {
    // TODO: Merge StreamExtensions to EndianBinary[Reader/Writer].
    public static class StreamExtensions {

        public static void CopyTo(this Stream source, Stream destination) {
            var buffer = new byte[1024];
            var read = source.Read(buffer, 0, buffer.Length);
            while (read > 0) {
                destination.Write(buffer, 0, read);
                if (read < buffer.Length) {
                    break;
                }
                read = source.Read(buffer, 0, buffer.Length);
            }
        }

        public static int Write(this Stream stream, sbyte value) {
            unchecked {
                stream.WriteByte((byte)value);
            }
            return 1;
        }

        public static int Write(this Stream stream, int value) {
            if (!BitConverter.IsLittleEndian) {
                value = DereToreHelper.SwapEndian(value);
            }
            stream.Write(BitConverter.GetBytes(value), 0, 4);
            return 4;
        }

        public static int Write(this Stream stream, short value) {
            if (!BitConverter.IsLittleEndian) {
                value = DereToreHelper.SwapEndian(value);
            }
            stream.Write(BitConverter.GetBytes(value), 0, 2);
            return 2;
        }

        public static int Write(this Stream stream, float value) {
            if (!BitConverter.IsLittleEndian) {
                value = DereToreHelper.SwapEndian(value);
            }
            stream.Write(BitConverter.GetBytes(value), 0, 4);
            return 4;
        }

        public static int Write<T>(this Stream stream, T value) where T : struct {
            var size = Marshal.SizeOf(typeof(T));
            var bytes = new byte[size];
            var ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(value, ptr, true);
            Marshal.Copy(ptr, bytes, 0, size);
            Marshal.FreeHGlobal(ptr);
            stream.Write(bytes, 0, size);
            return size;
        }

        public static void WriteRange<T>(this Stream stream, T[] value) where T : struct {
            var size = Marshal.SizeOf(typeof(T));
            var count = value.Length;
            var bytes = new byte[size * count];
            var ptr = Marshal.AllocHGlobal(size);
            var baseIndex = 0;
            for (var i = 0; i < count; ++i) {
                Marshal.StructureToPtr(value[i], ptr, true);
                stream.Write(bytes, 0, size);
                Marshal.Copy(ptr, bytes, baseIndex, size);
                baseIndex += size;
            }
            Marshal.FreeHGlobal(ptr);
        }

        public static int Read<T>(this Stream stream, out T value) where T : struct {
            var size = Marshal.SizeOf(typeof(T));
            var bytes = new byte[size];
            var bytesRead = stream.Read(bytes, 0, size);
            var ptr = Marshal.AllocHGlobal(size);
            Marshal.Copy(bytes, 0, ptr, size);
            value = (T)Marshal.PtrToStructure(ptr, typeof(T));
            Marshal.FreeHGlobal(ptr);
            return bytesRead;
        }

        public static T Read<T>(this Stream stream) where T : struct {
            T value;
            Read(stream, out value);
            return value;
        }

        public static int Skip(this Stream stream, int length) {
            var buffer = new byte[length];
            return stream.Read(buffer, 0, buffer.Length);
        }

        public static sbyte PeekSByte(this Stream stream) {
            return PeekSByte(stream, stream.Position);
        }

        public static byte PeekByte(this Stream stream) {
            return PeekByte(stream, stream.Position);
        }

        public static short PeekInt16LE(this Stream stream) {
            return PeekInt16LE(stream, stream.Position);
        }

        public static short PeekInt16BE(this Stream stream) {
            return PeekInt16BE(stream, stream.Position);
        }

        public static ushort PeekUInt16LE(this Stream stream) {
            return PeekUInt16LE(stream, stream.Position);
        }

        public static ushort PeekUInt16BE(this Stream stream) {
            return PeekUInt16BE(stream, stream.Position);
        }

        public static int PeekInt32LE(this Stream stream) {
            return PeekInt32LE(stream, stream.Position);
        }

        public static int PeekInt32BE(this Stream stream) {
            return PeekInt32BE(stream, stream.Position);
        }

        public static uint PeekUInt32LE(this Stream stream) {
            return PeekUInt32LE(stream, stream.Position);
        }

        public static uint PeekUInt32BE(this Stream stream) {
            return PeekUInt32BE(stream, stream.Position);
        }

        public static long PeekInt64LE(this Stream stream) {
            return PeekInt64LE(stream, stream.Position);
        }

        public static long PeekInt64BE(this Stream stream) {
            return PeekInt64BE(stream, stream.Position);
        }

        public static ulong PeekUInt64LE(this Stream stream) {
            return PeekUInt64LE(stream, stream.Position);
        }

        public static ulong PeekUInt64BE(this Stream stream) {
            return PeekUInt64BE(stream, stream.Position);
        }

        public static float PeekSingleLE(this Stream stream) {
            return PeekSingleBE(stream, stream.Position);
        }

        public static float PeekSingleBE(this Stream stream) {
            return PeekSingleBE(stream, stream.Position);
        }

        public static double PeekDoubleLE(this Stream stream) {
            return PeekDoubleLE(stream, stream.Position);
        }

        public static double PeekDoubleBE(this Stream stream) {
            return PeekDoubleBE(stream, stream.Position);
        }

        public static sbyte PeekSByte(this Stream stream, long offset) {
            unchecked {
                return (sbyte)PeekByte(stream, offset);
            }
        }

        public static byte PeekByte(this Stream stream, long offset) {
            var position = stream.Position;
            stream.Seek(offset, SeekOrigin.Begin);
            var value = stream.ReadByte();
            stream.Position = position;
            return (byte)value;
        }

        public static short PeekInt16BE(this Stream stream, long offset) {
            var bytes = GetNumberBytes(stream, offset, sizeof(short), false);
            return BitConverter.ToInt16(bytes, 0);
        }

        public static short PeekInt16LE(this Stream stream, long offset) {
            var bytes = GetNumberBytes(stream, offset, sizeof(short), true);
            return BitConverter.ToInt16(bytes, 0);
        }

        public static ushort PeekUInt16BE(this Stream stream, long offset) {
            var bytes = GetNumberBytes(stream, offset, sizeof(ushort), false);
            return BitConverter.ToUInt16(bytes, 0);
        }

        public static ushort PeekUInt16LE(this Stream stream, long offset) {
            var bytes = GetNumberBytes(stream, offset, sizeof(ushort), true);
            return BitConverter.ToUInt16(bytes, 0);
        }

        public static int PeekInt32BE(this Stream stream, long offset) {
            var bytes = GetNumberBytes(stream, offset, sizeof(int), false);
            return BitConverter.ToInt32(bytes, 0);
        }

        public static int PeekInt32LE(this Stream stream, long offset) {
            var bytes = GetNumberBytes(stream, offset, sizeof(int), true);
            return BitConverter.ToInt32(bytes, 0);
        }

        public static uint PeekUInt32BE(this Stream stream, long offset) {
            var bytes = GetNumberBytes(stream, offset, sizeof(uint), false);
            return BitConverter.ToUInt32(bytes, 0);
        }

        public static uint PeekUInt32LE(this Stream stream, long offset) {
            var bytes = GetNumberBytes(stream, offset, sizeof(uint), true);
            return BitConverter.ToUInt32(bytes, 0);
        }

        public static long PeekInt64BE(this Stream stream, long offset) {
            var bytes = GetNumberBytes(stream, offset, sizeof(long), false);
            return BitConverter.ToInt64(bytes, 0);
        }

        public static long PeekInt64LE(this Stream stream, long offset) {
            var bytes = GetNumberBytes(stream, offset, sizeof(long), true);
            return BitConverter.ToInt64(bytes, 0);
        }

        public static ulong PeekUInt64BE(this Stream stream, long offset) {
            var bytes = GetNumberBytes(stream, offset, sizeof(ulong), false);
            return BitConverter.ToUInt64(bytes, 0);
        }

        public static ulong PeekUInt64LE(this Stream stream, long offset) {
            var bytes = GetNumberBytes(stream, offset, sizeof(ulong), true);
            return BitConverter.ToUInt64(bytes, 0);
        }

        public static float PeekSingleBE(this Stream stream, long offset) {
            var bytes = GetNumberBytes(stream, offset, sizeof(float), false);
            return BitConverter.ToSingle(bytes, 0);
        }

        public static float PeekSingleLE(this Stream stream, long offset) {
            var bytes = GetNumberBytes(stream, offset, sizeof(float), true);
            return BitConverter.ToSingle(bytes, 0);
        }

        public static double PeekDoubleBE(this Stream stream, long offset) {
            var bytes = GetNumberBytes(stream, offset, sizeof(double), false);
            return BitConverter.ToDouble(bytes, 0);
        }

        public static double PeekDoubleLE(this Stream stream, long offset) {
            var bytes = GetNumberBytes(stream, offset, sizeof(double), true);
            return BitConverter.ToDouble(bytes, 0);
        }

        public static string PeekZeroEndedString(this Stream stream, long offset, Encoding encoding) {
            var streamLength = stream.Length;
            var stringLength = 0;
            var originalPosition = stream.Position;
            stream.Seek(offset, SeekOrigin.Begin);
            for (var i = offset; i <= streamLength; i++) {
                var dummy = stream.ReadByte();
                if (dummy > 0) {
                    stringLength++;
                } else {
                    break;
                }
            }
            var stringBytes = PeekBytes(stream, offset, stringLength);
            var ret = encoding.GetString(stringBytes);
            stream.Position = originalPosition;
            return ret;
        }

        public static string PeekZeroEndedStringAsAscii(this Stream stream, long offset) {
            return PeekZeroEndedString(stream, offset, Encoding.ASCII);
        }

        public static string PeekZeroEndedStringAsUtf8(this Stream stream, long offset) {
            return PeekZeroEndedString(stream, offset, Encoding.UTF8);
        }

        public static byte[] ReadBytes(byte[] array, int offset, int length) {
            var ret = new byte[length];
            Array.Copy(array, offset, ret, 0, length);
            return ret;
        }

        public static byte[] PeekBytes(this Stream stream, int offset, int length) {
            return PeekBytes(stream, (long)offset, length);
        }

        public static byte[] PeekBytes(this Stream stream, long offset, int length) {
            var originalPosition = stream.Position;
            stream.Seek(offset, SeekOrigin.Begin);
            var finalBuffer = new byte[length];
            var secondBuffer = new byte[length];
            var bytesLeft = length;
            var currentIndex = 0;
            int read;
            do {
                read = stream.Read(secondBuffer, 0, bytesLeft);
                Array.Copy(secondBuffer, 0, finalBuffer, currentIndex, read);
                bytesLeft -= read;
                currentIndex += read;
            } while (bytesLeft > 0 && read > 0);
            stream.Position = originalPosition;
            return finalBuffer;
        }

        public static void SeekAndWriteBytes(this Stream stream, byte[] data, long offset) {
            stream.Seek(offset, SeekOrigin.Begin);
            WriteBytes(stream, data);
        }

        public static void SeekAndWriteByte(this Stream stream, byte value, long offset) {
            stream.Seek(offset, SeekOrigin.Begin);
            WriteByte(stream, value);
        }

        public static void SeekAndWriteSByte(this Stream stream, sbyte value, long offset) {
            stream.Seek(offset, SeekOrigin.Begin);
            WriteSByte(stream, value);
        }

        public static void SeekAndWriteUInt16LE(this Stream stream, ushort value, long offset) {
            stream.Seek(offset, SeekOrigin.Begin);
            WriteUInt16LE(stream, value);
        }

        public static void SeekAndWriteUInt16BE(this Stream stream, ushort value, long offset) {
            stream.Seek(offset, SeekOrigin.Begin);
            WriteUInt16BE(stream, value);
        }

        public static void SeekAndWriteInt16LE(this Stream stream, short value, long offset) {
            stream.Seek(offset, SeekOrigin.Begin);
            WriteInt16LE(stream, value);
        }

        public static void SeekAndWriteInt16BE(this Stream stream, short value, long offset) {
            stream.Seek(offset, SeekOrigin.Begin);
            WriteInt16BE(stream, value);
        }

        public static void SeekAndWriteUInt32LE(this Stream stream, uint value, long offset) {
            stream.Seek(offset, SeekOrigin.Begin);
            WriteUInt32LE(stream, value);
        }

        public static void SeekAndWriteUInt32BE(this Stream stream, uint value, long offset) {
            stream.Seek(offset, SeekOrigin.Begin);
            WriteUInt32BE(stream, value);
        }

        public static void SeekAndWriteInt32LE(this Stream stream, int value, long offset) {
            stream.Seek(offset, SeekOrigin.Begin);
            WriteInt32LE(stream, value);
        }

        public static void SeekAndWriteInt32BE(this Stream stream, int value, long offset) {
            stream.Seek(offset, SeekOrigin.Begin);
            WriteInt32BE(stream, value);
        }

        public static void SeekAndWriteUInt64LE(this Stream stream, ulong value, long offset) {
            stream.Seek(offset, SeekOrigin.Begin);
            WriteUInt64LE(stream, value);
        }

        public static void SeekAndWriteUInt64BE(this Stream stream, ulong value, long offset) {
            stream.Seek(offset, SeekOrigin.Begin);
            WriteUInt64BE(stream, value);
        }

        public static void SeekAndWriteInt64LE(this Stream stream, long value, long offset) {
            stream.Seek(offset, SeekOrigin.Begin);
            WriteInt64LE(stream, value);
        }

        public static void SeekAndWriteInt64BE(this Stream stream, long value, long offset) {
            stream.Seek(offset, SeekOrigin.Begin);
            WriteInt64BE(stream, value);
        }

        public static void SeekAndWriteSingleLE(this Stream stream, float value, long offset) {
            stream.Seek(offset, SeekOrigin.Begin);
            WriteSingleLE(stream, value);
        }

        public static void SeekAndWriteSingleBE(this Stream stream, float value, long offset) {
            stream.Seek(offset, SeekOrigin.Begin);
            WriteSingleBE(stream, value);
        }

        public static void SeekAndWriteDoubleLE(this Stream stream, double value, long offset) {
            stream.Seek(offset, SeekOrigin.Begin);
            WriteDoubleLE(stream, value);
        }

        public static void SeekAndWriteDoubleBE(this Stream stream, double value, long offset) {
            stream.Seek(offset, SeekOrigin.Begin);
            WriteDoubleBE(stream, value);
        }

        public static void SeekAndWriteZeroEndedStringAsAscii(this Stream stream, string value, long offset) {
            stream.Seek(offset, SeekOrigin.Begin);
            WriteZeroEndedStringAsAscii(stream, value);
        }

        public static void SeekAndWriteZeroEndedStringAsUtf8(this Stream stream, string value, long offset) {
            stream.Seek(offset, SeekOrigin.Begin);
            WriteZeroEndedStringAsUtf8(stream, value);
        }

        public static void WriteBytes(this Stream stream, byte[] data) {
            stream.Write(data, 0, data.Length);
        }

        public static void WriteByte(this Stream stream, byte value) {
            stream.WriteByte(value);
        }

        public static void WriteSByte(this Stream stream, sbyte value) {
            unchecked {
                stream.WriteByte((byte)value);
            }
        }

        public static void WriteUInt16LE(this Stream stream, ushort value) {
            var bytes = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian) {
                Array.Reverse(bytes);
            }
            WriteBytes(stream, bytes);
        }

        public static void WriteUInt16BE(this Stream stream, ushort value) {
            var bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian) {
                Array.Reverse(bytes);
            }
            WriteBytes(stream, bytes);
        }

        public static void WriteInt16LE(this Stream stream, short value) {
            var bytes = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian) {
                Array.Reverse(bytes);
            }
            WriteBytes(stream, bytes);
        }

        public static void WriteInt16BE(this Stream stream, short value) {
            var bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian) {
                Array.Reverse(bytes);
            }
            WriteBytes(stream, bytes);
        }

        public static void WriteUInt32LE(this Stream stream, uint value) {
            var bytes = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian) {
                Array.Reverse(bytes);
            }
            WriteBytes(stream, bytes);
        }

        public static void WriteUInt32BE(this Stream stream, uint value) {
            var bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian) {
                Array.Reverse(bytes);
            }
            WriteBytes(stream, bytes);
        }

        public static void WriteInt32LE(this Stream stream, int value) {
            var bytes = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian) {
                Array.Reverse(bytes);
            }
            WriteBytes(stream, bytes);
        }

        public static void WriteInt32BE(this Stream stream, int value) {
            var bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian) {
                Array.Reverse(bytes);
            }
            WriteBytes(stream, bytes);
        }

        public static void WriteUInt64LE(this Stream stream, ulong value) {
            var bytes = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian) {
                Array.Reverse(bytes);
            }
            WriteBytes(stream, bytes);
        }

        public static void WriteUInt64BE(this Stream stream, ulong value) {
            var bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian) {
                Array.Reverse(bytes);
            }
            WriteBytes(stream, bytes);
        }

        public static void WriteInt64LE(this Stream stream, long value) {
            var bytes = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian) {
                Array.Reverse(bytes);
            }
            WriteBytes(stream, bytes);
        }

        public static void WriteInt64BE(this Stream stream, long value) {
            var bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian) {
                Array.Reverse(bytes);
            }
            WriteBytes(stream, bytes);
        }

        public static void WriteSingleLE(this Stream stream, float value) {
            var bytes = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian) {
                Array.Reverse(bytes);
            }
            WriteBytes(stream, bytes);
        }

        public static void WriteSingleBE(this Stream stream, float value) {
            var bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian) {
                Array.Reverse(bytes);
            }
            WriteBytes(stream, bytes);
        }

        public static void WriteDoubleLE(this Stream stream, double value) {
            var bytes = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian) {
                Array.Reverse(bytes);
            }
            WriteBytes(stream, bytes);
        }

        public static void WriteDoubleBE(this Stream stream, double value) {
            var bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian) {
                Array.Reverse(bytes);
            }
            WriteBytes(stream, bytes);
        }

        public static void WriteZeroEndedStringAsAscii(this Stream stream, string value) {
            WriteZeroEndedString(stream, value, Encoding.ASCII);
        }

        public static void WriteZeroEndedStringAsUtf8(this Stream stream, string value) {
            WriteZeroEndedString(stream, value, Encoding.UTF8);
        }

        private static void WriteZeroEndedString(this Stream stream, string value, Encoding encoding) {
            if (value != null) {
                var bytes = encoding.GetBytes(value);
                WriteBytes(stream, bytes);
            }
            stream.WriteByte(0);
        }

        private static byte[] GetNumberBytes(Stream stream, long offset, int byteCount, bool isLittleEndian) {
            var data = PeekBytes(stream, offset, byteCount);
            if (BitConverter.IsLittleEndian != isLittleEndian) {
                Array.Reverse(data);
            }
            return data;
        }

    }
}
