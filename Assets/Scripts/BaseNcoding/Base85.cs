using System;
using System.IO;
using System.Text;

namespace BaseNcoding
{
	public class Base85 : Base
	{
		public const string DefaultAlphabet = "!\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstu";
		public const char DefaultSpecial = (char)0;
		private static readonly uint[] Pow85 = { 85 * 85 * 85 * 85, 85 * 85 * 85, 85 * 85, 85, 1 };

		public const string Prefix = "<~";
		public const string Postfix = "~>";

		public override bool HasSpecial => false;

		public bool PrefixPostfix { get; set; }

		public Base85(string alphabet = DefaultAlphabet, char special = DefaultSpecial, bool prefixPostfix = false, Encoding textEncoding = null)
			: base(85, alphabet, special, textEncoding)
		{
			PrefixPostfix = prefixPostfix;
			BlockBitsCount = 32;
			BlockCharsCount = 5;
		}

		public override string Encode(byte[] data)
		{
			unchecked
			{
				byte[] encodedBlock = new byte[5];
				int decodedBlockLength = 4;
				int resultLength = data.Length * (encodedBlock.Length / decodedBlockLength);
				if (PrefixPostfix)
					resultLength += Prefix.Length + Postfix.Length;
				char[] result = new char[resultLength];
				int resultInd = 0;

				if (PrefixPostfix)
					CopyString(Prefix, result, ref resultInd);

				int count = 0;
				uint tuple = 0;
				foreach (byte b in data)
				{
					if (count >= decodedBlockLength - 1)
					{
						tuple |= b;
						if (tuple == 0)
							result[resultInd++] = 'z';
						else
							EncodeBlock(encodedBlock.Length, result, ref resultInd, encodedBlock, tuple);
						tuple = 0;
						count = 0;
					}
					else
					{
						tuple |= (uint)(b << (24 - count * 8));
						count++;
					}
				}

				if (count > 0)
					EncodeBlock(count + 1, result, ref resultInd, encodedBlock, tuple);

				if (PrefixPostfix)
					CopyString(Postfix, result, ref resultInd);

				return new string(result);
			}
		}

		public override byte[] Decode(string data)
		{
			unchecked
			{
				string dataWithoutPrefixPostfix = data;
				if (PrefixPostfix)
				{
					if (!dataWithoutPrefixPostfix.StartsWith(Prefix) || !dataWithoutPrefixPostfix.EndsWith(Postfix))
					{
						throw new Exception("ASCII85 encoded data should begin with '" + Prefix + "' and end with '" + Postfix + "'");
					}
					dataWithoutPrefixPostfix = dataWithoutPrefixPostfix.Substring(Prefix.Length, dataWithoutPrefixPostfix.Length - Prefix.Length - Postfix.Length);
				}

				MemoryStream ms = new MemoryStream();
				int count = 0;

				uint tuple = 0;
				int encodedBlockLength = 5;
				byte[] decodedBlock = new byte[4];
				foreach (char c in dataWithoutPrefixPostfix)
				{
					bool processChar;
					switch (c)
					{
						case 'z':
							if (count != 0)
							{
								throw new Exception("The character 'z' is invalid inside an ASCII85 block.");
							}
							decodedBlock[0] = 0;
							decodedBlock[1] = 0;
							decodedBlock[2] = 0;
							decodedBlock[3] = 0;
							ms.Write(decodedBlock, 0, decodedBlock.Length);
							processChar = false;
							break;
						default:
							processChar = true;
							break;
					}

					if (processChar)
					{
						tuple += (uint)InvAlphabet[c] * Pow85[count];
						count++;
						if (count == encodedBlockLength)
						{
							DecodeBlock(decodedBlock.Length, decodedBlock, tuple);
							ms.Write(decodedBlock, 0, decodedBlock.Length);
							tuple = 0;
							count = 0;
						}
					}
				}

				if (count != 0)
				{
					if (count == 1)
					{
						throw new Exception("The last block of ASCII85 data cannot be a single byte.");
					}
					count--;
					tuple += Pow85[count];
					DecodeBlock(count, decodedBlock, tuple);
					for (int i = 0; i < count; i++)
					{
						ms.WriteByte(decodedBlock[i]);
					}
				}

				return ms.ToArray();
			}
		}

		private static void CopyString(string source, char[] dest, ref int destInd)
		{
			for (int i = 0; i < source.Length; i++)
				dest[destInd++] = source[i];
		}

		private void EncodeBlock(int count, char[] result, ref int resultInd, byte[] encodedBlock, uint tuple)
		{
			unchecked
			{
				for (int i = encodedBlock.Length - 1; i >= 0; i--)
				{
					uint quotient = tuple % 85;
					uint remainder = tuple - quotient * 85;
					tuple = quotient;
					encodedBlock[i] = (byte)remainder;
				}

				for (int i = 0; i < count; i++)
					result[resultInd++] = Alphabet[encodedBlock[i]];
			}
		}

		private void DecodeBlock(int bytes, byte[] decodedBlock, uint tuple)
		{
			unchecked
			{
				for (int i = 0; i < bytes; i++)
					decodedBlock[i] = (byte)(tuple >> 24 - i * 8);
			}
		}
	}
}
