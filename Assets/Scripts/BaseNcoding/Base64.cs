using System;
using System.Text;

namespace BaseNcoding
{
	public class Base64 : Base
	{
		public const string DefaultAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
		public const char DefaultSpecial = '=';

		public override bool HasSpecial => true;

		public Base64(string alphabet = DefaultAlphabet, char special = DefaultSpecial,
			Encoding textEncoding = null, bool parallel = false)
			: base(64, alphabet, special, textEncoding, parallel)
		{
		}

		public override string Encode(byte[] data)
		{
			int resultLength = (data.Length + 2) / 3 * 4;
			char[] result = new char[resultLength];

			int length3 = data.Length / 3;
			if (!Parallel)
			{
				EncodeBlock(data, result, 0, length3);
			}
			else
			{
				int processorCount = Math.Min(length3, Environment.ProcessorCount);
				System.Threading.Tasks.Parallel.For(0, processorCount, i =>
				{
					int beginInd = i * length3 / processorCount;
					int endInd = (i + 1) * length3 / processorCount;
					EncodeBlock(data, result, beginInd, endInd);
				});
			}

			int ind;
			int x1, x2;
			int srcInd, dstInd;
			switch (data.Length - length3 * 3)
			{
				case 1:
					ind = length3;
					srcInd = ind * 3;
					dstInd = ind * 4;
					x1 = data[srcInd];
					result[dstInd] = Alphabet[x1 >> 2];
					result[dstInd + 1] = Alphabet[(x1 << 4) & 0x30];
					result[dstInd + 2] = Special;
					result[dstInd + 3] = Special;
					break;
				case 2:
					ind = length3;
					srcInd = ind * 3;
					dstInd = ind * 4;
					x1 = data[srcInd];
					x2 = data[srcInd + 1];
					result[dstInd] = Alphabet[x1 >> 2];
					result[dstInd + 1] = Alphabet[((x1 << 4) & 0x30) | (x2 >> 4)];
					result[dstInd + 2] = Alphabet[(x2 << 2) & 0x3C];
					result[dstInd + 3] = Special;
					break;
			}

			return new string(result);
		}

		public override byte[] Decode(string data)
		{
			unchecked
			{
				if (string.IsNullOrEmpty(data))
					return new byte[0];

				int lastSpecialInd = data.Length;
				while (data[lastSpecialInd - 1] == Special)
					lastSpecialInd--;
				int tailLength = data.Length - lastSpecialInd;

				int resultLength = (data.Length + 3) / 4 * 3 - tailLength;
				byte[] result = new byte[resultLength];

				int length4 = (data.Length - tailLength) / 4;
				if (!Parallel)
				{
					DecodeBlock(data, result, 0, length4);
				}
				else
				{
					int processorCount = Math.Min(length4, Environment.ProcessorCount);
					System.Threading.Tasks.Parallel.For(0, processorCount, i =>
					{
						int beginInd = i * length4 / processorCount;
						int endInd = (i + 1) * length4 / processorCount;
						DecodeBlock(data, result, beginInd, endInd);
					});
				}

				int ind;
				int x1, x2, x3;
				int srcInd, dstInd;
				switch (tailLength)
				{
					case 2:
						ind = length4;
						srcInd = ind * 4;
						dstInd = ind * 3;
						x1 = InvAlphabet[data[srcInd]];
						x2 = InvAlphabet[data[srcInd + 1]];
						result[dstInd] = (byte)((x1 << 2) | ((x2 >> 4) & 0x3));
						break;
					case 1:
						ind = length4;
						srcInd = ind * 4;
						dstInd = ind * 3;
						x1 = InvAlphabet[data[srcInd]];
						x2 = InvAlphabet[data[srcInd + 1]];
						x3 = InvAlphabet[data[srcInd + 2]];
						result[dstInd] = (byte)((x1 << 2) | ((x2 >> 4) & 0x3));
						result[dstInd + 1] = (byte)((x2 << 4) | ((x3 >> 2) & 0xF));
						break;
				}

				return result;
			}
		}

		private void EncodeBlock(byte[] src, char[] dst, int beginInd, int endInd)
		{
			for (int ind = beginInd; ind < endInd; ind++)
			{
				int srcInd = ind * 3;
				int dstInd = ind * 4;

				byte x1 = src[srcInd];
				byte x2 = src[srcInd + 1];
				byte x3 = src[srcInd + 2];

				dst[dstInd] = Alphabet[x1 >> 2];
				dst[dstInd + 1] = Alphabet[((x1 << 4) & 0x30) | (x2 >> 4)];
				dst[dstInd + 2] = Alphabet[((x2 << 2) & 0x3C) | (x3 >> 6)];
				dst[dstInd + 3] = Alphabet[x3 & 0x3F];
			}
		}

		private void DecodeBlock(string src, byte[] dst, int beginInd, int endInd)
		{
			unchecked
			{
				for (int ind = beginInd; ind < endInd; ind++)
				{
					int srcInd = ind * 4;
					int dstInd = ind * 3;

					int x1 = InvAlphabet[src[srcInd]];
					int x2 = InvAlphabet[src[srcInd + 1]];
					int x3 = InvAlphabet[src[srcInd + 2]];
					int x4 = InvAlphabet[src[srcInd + 3]];

					dst[dstInd] = (byte)((x1 << 2) | ((x2 >> 4) & 0x3));
					dst[dstInd + 1] = (byte)((x2 << 4) | ((x3 >> 2) & 0xF));
					dst[dstInd + 2] = (byte)((x3 << 6) | (x4 & 0x3F));
				}
			}
		}
	}
}
