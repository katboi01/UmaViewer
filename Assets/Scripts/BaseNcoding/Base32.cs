using System.Text;

namespace BaseNcoding
{
	public class Base32 : Base
	{
		public const string DefaultAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
		public const char DefaultSpecial = '>';

		public override bool HasSpecial => true;

		public Base32(string alphabet = DefaultAlphabet, char special = DefaultSpecial, Encoding textEncoding = null)
		: base(32, alphabet, special, textEncoding)
		{
		}

		public override string Encode(byte[] data)
		{
			int dataLength = data.Length;
			StringBuilder result = new StringBuilder((dataLength + 4) / 5 * 8);

			byte x1, x2;
			int i;

			int length5 = (dataLength / 5) * 5;
			for (i = 0; i < length5; i += 5)
			{
				x1 = data[i];
				result.Append(Alphabet[x1 >> 3]);

				x2 = data[i + 1];
				result.Append(Alphabet[((x1 << 2) & 0x1C) | (x2 >> 6)]);
				result.Append(Alphabet[(x2 >> 1) & 0x1F]);

				x1 = data[i + 2];
				result.Append(Alphabet[((x2 << 4) & 0x10) | (x1 >> 4)]);

				x2 = data[i + 3];
				result.Append(Alphabet[((x1 << 1) & 0x1E) | (x2 >> 7)]);
				result.Append(Alphabet[(x2 >> 2) & 0x1F]);

				x1 = data[i + 4];
				result.Append(Alphabet[((x2 << 3) & 0x18) | (x1 >> 5)]);
				result.Append(Alphabet[x1 & 0x1F]);
			}

			switch (dataLength - length5)
			{
				case 1:
					x1 = data[i];
					result.Append(Alphabet[x1 >> 3]);
					result.Append(Alphabet[(x1 << 2) & 0x1C]);

					result.Append(Special, 6);
					break;
				case 2:
					x1 = data[i];
					result.Append(Alphabet[x1 >> 3]);
					x2 = data[i + 1];
					result.Append(Alphabet[((x1 << 2) & 0x1C) | (x2 >> 6)]);
					result.Append(Alphabet[(x2 >> 1) & 0x1F]);
					result.Append(Alphabet[(x2 << 4) & 0x10]);

					result.Append(Special, 4);
					break;
				case 3:
					x1 = data[i];
					result.Append(Alphabet[x1 >> 3]);
					x2 = data[i + 1];
					result.Append(Alphabet[((x1 << 2) & 0x1C) | (x2 >> 6)]);
					result.Append(Alphabet[(x2 >> 1) & 0x1F]);
					x1 = data[i + 2];
					result.Append(Alphabet[((x2 << 4) & 0x10) | (x1 >> 4)]);
					result.Append(Alphabet[(x1 << 1) & 0x1E]);

					result.Append(Special, 3);
					break;
				case 4:
					x1 = data[i];
					result.Append(Alphabet[x1 >> 3]);
					x2 = data[i + 1];
					result.Append(Alphabet[((x1 << 2) & 0x1C) | (x2 >> 6)]);
					result.Append(Alphabet[(x2 >> 1) & 0x1F]);
					x1 = data[i + 2];
					result.Append(Alphabet[((x2 << 4) & 0x10) | (x1 >> 4)]);
					x2 = data[i + 3];
					result.Append(Alphabet[((x1 << 1) & 0x1E) | (x2 >> 7)]);
					result.Append(Alphabet[(x2 >> 2) & 0x1F]);
					result.Append(Alphabet[(x2 << 3) & 0x18]);

					result.Append(Special);
					break;
			}

			return result.ToString();
		}

		public override byte[] Decode(string data)
		{
			unchecked
			{
				if (string.IsNullOrEmpty(data))
					return new byte[0];

				int additionalBytes = 0, diff = 0, tempLen = 0;


				int lastSpecialInd = data.Length;
				while (data[lastSpecialInd - 1] == Special)
					lastSpecialInd--;
				int tailLength = data.Length - lastSpecialInd;

				switch (tailLength)
				{
					case 6:
						additionalBytes = 4;
						break;
					case 4:
						additionalBytes = 3;
						break;
					case 3:
						additionalBytes = 2;
						break;
					case 1:
						additionalBytes = 1;
						break;
				}

				diff = tailLength - additionalBytes;
				tailLength = additionalBytes;
				tempLen = data.Length - diff;

				byte[] result = new byte[(tempLen + 7) / 8 * 5 - tailLength];
				int length5 = result.Length / 5 * 5;
				int x1, x2, x3, x4, x5, x6, x7, x8;

				int i, srcInd = 0;
				for (i = 0; i < length5; i += 5)
				{
					x1 = InvAlphabet[data[srcInd++]];
					x2 = InvAlphabet[data[srcInd++]];
					x3 = InvAlphabet[data[srcInd++]];
					x4 = InvAlphabet[data[srcInd++]];
					x5 = InvAlphabet[data[srcInd++]];
					x6 = InvAlphabet[data[srcInd++]];
					x7 = InvAlphabet[data[srcInd++]];
					x8 = InvAlphabet[data[srcInd++]];

					result[i] = (byte)((x1 << 3) | ((x2 >> 2) & 0x07));
					result[i + 1] = (byte)((x2 << 6) | ((x3 << 1) & 0x3E) | ((x4 >> 4) & 0x01));
					result[i + 2] = (byte)((x4 << 4) | ((x5 >> 1) & 0xF));
					result[i + 3] = (byte)((x5 << 7) | ((x6 << 2) & 0x7C) | ((x7 >> 3) & 0x03));
					result[i + 4] = (byte)((x7 << 5) | (x8 & 0x1F));
				}

				switch (tailLength)
				{
					case 4:
						x1 = InvAlphabet[data[srcInd++]];
						x2 = InvAlphabet[data[srcInd++]];
						result[i] = (byte)((x1 << 3) | ((x2 >> 2) & 0x07));
						break;
					case 3:
						x1 = InvAlphabet[data[srcInd++]];
						x2 = InvAlphabet[data[srcInd++]];
						x3 = InvAlphabet[data[srcInd++]];
						x4 = InvAlphabet[data[srcInd++]];

						result[i] = (byte)((x1 << 3) | ((x2 >> 2) & 0x07));
						result[i + 1] = (byte)((x2 << 6) | ((x3 << 1) & 0x3E) | ((x4 >> 4) & 0x01));
						break;
					case 2:
						x1 = InvAlphabet[data[srcInd++]];
						x2 = InvAlphabet[data[srcInd++]];
						x3 = InvAlphabet[data[srcInd++]];
						x4 = InvAlphabet[data[srcInd++]];
						x5 = InvAlphabet[data[srcInd++]];

						result[i] = (byte)((x1 << 3) | ((x2 >> 2) & 0x07));
						result[i + 1] = (byte)((x2 << 6) | ((x3 << 1) & 0x3E) | ((x4 >> 4) & 0x01));
						result[i + 2] = (byte)((x4 << 4) | ((x5 >> 1) & 0xF));
						break;
					case 1:
						x1 = InvAlphabet[data[srcInd++]];
						x2 = InvAlphabet[data[srcInd++]];
						x3 = InvAlphabet[data[srcInd++]];
						x4 = InvAlphabet[data[srcInd++]];
						x5 = InvAlphabet[data[srcInd++]];
						x6 = InvAlphabet[data[srcInd++]];
						x7 = InvAlphabet[data[srcInd++]];

						result[i] = (byte)((x1 << 3) | ((x2 >> 2) & 0x07));
						result[i + 1] = (byte)((x2 << 6) | ((x3 << 1) & 0x3E) | ((x4 >> 4) & 0x01));
						result[i + 2] = (byte)((x4 << 4) | ((x5 >> 1) & 0xF));
						result[i + 3] = (byte)((x5 << 7) | ((x6 << 2) & 0x7C) | ((x7 >> 3) & 0x03));
						break;
				}

				return result;
			}
		}
	}
}
