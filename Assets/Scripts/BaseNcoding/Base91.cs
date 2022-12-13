using System;
using System.Collections.Generic;
using System.Text;

namespace BaseNcoding
{
	public class Base91 : Base
	{
		public const string DefaultAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!#$%&()*+,./:;<=>?@[]^_`{|}~\"";
		public const char DefaultSpecial = (char)0;

		public override bool HasSpecial => false;

		public Base91(string alphabet = DefaultAlphabet, char special = DefaultSpecial, Encoding textEncoding = null)
			: base(91, alphabet, special, textEncoding)
		{
			BlockBitsCount = 13;
			BlockCharsCount = 2;
		}

		public override string Encode(byte[] data)
		{
			StringBuilder result = new StringBuilder(data.Length);

			int ebq = 0, en = 0;
			for (int i = 0; i < data.Length; ++i)
			{
				ebq |= (data[i] & 255) << en;
				en += 8;
				if (en > 13)
				{
					int ev = ebq & 8191;

					if (ev > 88)
					{
						ebq >>= 13;
						en -= 13;
					}
					else
					{
						ev = ebq & 16383;
						ebq >>= 14;
						en -= 14;
					}

					int quotient = Math.DivRem(ev, 91, out int remainder);
					result.Append(Alphabet[remainder]);
					result.Append(Alphabet[quotient]);
				}
			}

			if (en > 0)
			{
				int quotient = Math.DivRem(ebq, 91, out int remainder);
				result.Append(Alphabet[remainder]);
				if (en > 7 || ebq > 90)
					result.Append(Alphabet[quotient]);
			}

			return result.ToString();
		}

		public override byte[] Decode(string data)
		{
			unchecked
			{
				int dbq = 0, dn = 0, dv = -1;

				List<byte> result = new List<byte>(data.Length);
				for (int i = 0; i < data.Length; ++i)
				{
					if (InvAlphabet[data[i]] == -1)
						continue;
					if (dv == -1)
						dv = InvAlphabet[data[i]];
					else
					{
						dv += InvAlphabet[data[i]] * 91;
						dbq |= dv << dn;
						dn += (dv & 8191) > 88 ? 13 : 14;
						do
						{
							result.Add((byte)dbq);
							dbq >>= 8;
							dn -= 8;
						}
						while (dn > 7);
						dv = -1;
					}
				}

				if (dv != -1)
					result.Add((byte)(dbq | dv << dn));

				return result.ToArray();
			}
		}
	}
}
