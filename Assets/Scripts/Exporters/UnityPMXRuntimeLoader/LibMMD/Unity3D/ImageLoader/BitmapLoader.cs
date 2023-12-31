using System;
using System.IO;
using UnityEngine;

namespace LibMMD.Unity3D.ImageLoader
{
	public class BitmapLoader
	{
		/* BMPPallete: pallete for BMP */
		private struct BmpPallete {
			public byte B;
			public byte G;
			public byte R;
			public byte A;
		};

		/* BMPHeader: header for BMP */
		private struct BmpHeader {
			public ushort BfType;
			public uint BfSize;
			public ushort BfReserved1;
			public ushort BfReserved2;
			public uint BfOffBits;
		}

		/* BMPInfo: info for BMP */
		struct BmpInfo {
			public uint BiSize;
			public int BiWidth;
			public int BiHeight;
			public ushort BiPlanes;
			public ushort BiBitCount;
			public uint BiCompression;
			public uint BiSizeImage;
			public uint BiXPelsPerMeter;
			public uint BiYPelsPerMeter;
			public uint BiClrUsed;
			public uint BiClrImportant;
		};

		public static TextureImage LoadFromFile(string path)
		{
			var data = File.ReadAllBytes (path);
			if (data [0] != 'B' || data [1] != 'M') {
				return null;
			}
			var offset = 0;
			var bmpHeader = ReadBmpHeader (data, ref offset);
			var bodyOffset = bmpHeader.BfOffBits;
			var len = BitConverter.ToInt64 (data, offset);
			if (len == 12) {
				return null;
			}
			var bmpinfo = ReadBmpInfo (data, ref offset);
			var width = bmpinfo.BiWidth;
			var reversed = false;
			int height;
			if (bmpinfo.BiHeight < 0) {
				height = -bmpinfo.BiHeight;	
			} else {
				height = bmpinfo.BiHeight;
				reversed = true;
			}
			var bit = bmpinfo.BiBitCount;
			if (bmpinfo.BiCompression != 0) {
				Debug.LogWarningFormat ("not support compressed bitmap {0}", bmpinfo.BiCompression);
				return null;
			}
			Color[] pallette = null;
			if (bit <= 8) {
				pallette = ReadBmpPallete (data, ref offset, GetPalletteColorCount (bmpinfo.BiClrUsed, bit));
			}
			var pixels = ReadBmpBody (data, (int) bodyOffset, width, height, bit, reversed, pallette);
			return pixels == null ? null : new TextureImage (width, height, pixels);
		}

		private static BmpHeader ReadBmpHeader(byte[] data, ref int offset) {
			var ret = new BmpHeader
			{
				BfType = ByteArrayReader.ReadUShort(data, ref offset),
				BfSize = ByteArrayReader.ReadUInt(data, ref offset),
				BfReserved1 = ByteArrayReader.ReadUShort(data, ref offset),
				BfReserved2 = ByteArrayReader.ReadUShort(data, ref offset),
				BfOffBits = ByteArrayReader.ReadUInt(data, ref offset)
			};
			return ret;
		}

		private static BmpInfo ReadBmpInfo(byte[] data, ref int offset) {
			var ret = new BmpInfo
			{
				BiSize = ByteArrayReader.ReadUInt(data, ref offset),
				BiWidth = ByteArrayReader.ReadInt(data, ref offset),
				BiHeight = ByteArrayReader.ReadInt(data, ref offset),
				BiPlanes = ByteArrayReader.ReadUShort(data, ref offset),
				BiBitCount = ByteArrayReader.ReadUShort(data, ref offset),
				BiCompression = ByteArrayReader.ReadUInt(data, ref offset),
				BiSizeImage = ByteArrayReader.ReadUInt(data, ref offset),
				BiXPelsPerMeter = ByteArrayReader.ReadUInt(data, ref offset),
				BiYPelsPerMeter = ByteArrayReader.ReadUInt(data, ref offset),
				BiClrUsed = ByteArrayReader.ReadUInt(data, ref offset),
				BiClrImportant = ByteArrayReader.ReadUInt(data, ref offset)
			};
			return ret;
		}

		private static Color[] ReadBmpPallete(byte[] data, ref int offset, uint count) {
			var ret = new Color[count];
			const float maxColorVal = 255.0f;
			for (uint i = 0; i < count; i++) {
				ret [i].b = data [offset] / maxColorVal;
				ret [i].g = data [offset + 1] / maxColorVal;
				ret [i].r = data [offset + 2] / maxColorVal;
				ret [i].a = data [offset + 3] / maxColorVal;
				offset += 4;
			}
			return ret;
		}

		private static uint GetPalletteColorCount(uint nClr, ushort nBit) {
			if (nClr != 0) {
				return nClr;
			}
			return ((uint)1) >> nBit;
		}

		private static Color[] ReadBmpBody(byte[] data, int offset, int width, int height,int bit, bool reversed, Color[] pallete) {
			const float maxByte = 255.0f;
			var ret = new Color[width * height];
			var lineByte = (width * bit) / 8;
			if ((lineByte % 4) != 0) {
				lineByte = ((lineByte / 4) + 1) * 4; //force 4-byte alignment
			}
			var t = 0;
			for (var h = 0; h < height; h++) {
				int tl;
				if (reversed) {
					tl = offset + h * lineByte;
				} else {
					tl = offset + (height - h - 1) * lineByte;
				}
				for (var w = 0; w < width; w++)
				{
					byte ci;
					switch (bit) {
					case 1: {
							ci = data[tl + w / 8];
							var mod = w % 8;
							var bitmask = (mod == 0) ? 0x80 : (0x80 >> mod);
							ci = (ci & bitmask) != 0 ? (byte)1 : (byte)0;
							ret[t] = pallete[ci];
							t++;
						}
						break;
					case 4: {
							ci = data [tl + w / 2];
							if (w % 2 == 0)
								ci = (byte)((ci >> 4) & 0x0f);
							else
								ci = (byte)(ci & 0x0f);
							ret[t] = pallete[ci];
							t++;
						}
						break;
					case 8: {
							ci = data [tl + w];
							ret[t] = pallete[ci];
							t++;
						}
						break;
					case 24:
						/* BGR -> RGB */
						ret [t].r = data [tl + w * 3 + 2] / maxByte;
						ret [t].g = data [tl + w * 3 + 1] / maxByte;
						ret [t].b = data [tl + w * 3] / maxByte;
						ret [t].a = 1.0f;
						t++;
						break;
					case 32:
						ret [t].r = data [tl + w * 4 + 2] / maxByte;
						ret [t].g = data [tl + w * 4 + 1] / maxByte;
						ret [t].b = data [tl + w * 4] / maxByte;
						//ret [t].a = data [tl + w * 4 + 3] / maxByte; //有些bmp会变成全透明的orz
						ret [t].a = 1.0f;
						t++;
						break;
					default:
						return null;	
					}
				}
			}
			return ret;
		}

	}
}

