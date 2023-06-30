using System;

namespace LibMMD.Unity3D
{
	public class ByteArrayReader
	{
		public static int ReadInt(byte[] data, ref int offset) {
			var ret = BitConverter.ToInt32(data, offset);
			offset += 4;
			return ret;
		}

		public static uint ReadUInt(byte[] data, ref int offset) {
			var ret = BitConverter.ToUInt32(data, offset);
			offset += 4;
			return ret;
		}

		public static short ReadShort(byte[] data, ref int offset) {
			var ret = BitConverter.ToInt16(data, offset);
			offset += 2;
			return ret;
		}

		public static ushort ReadUShort(byte[] data, ref int offset) {
			var ret = BitConverter.ToUInt16(data, offset);
			offset += 2;
			return ret;
		}
	}
}

