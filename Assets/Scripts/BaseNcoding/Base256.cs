using System.Text;

namespace BaseNcoding
{
	public class Base256 : Base
	{
		public const string DefaultAlphabet = "!#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~ ¡¢£¤¥¦§¨©ª«¬­®¯°±²³´µ¶·¸¹º»¼½¾¿ÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖ×ØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõö÷øùúûüýþÿĀāĂăĄąĆćĈĉĊċČčĎďĐđĒēĔĕĖėĘęĚěĜĝĞğĠġĢģĤĥĦħĨĩĪīĬĭĮįİıĲĳĴĵĶķĸĹĺĻļĽľĿŀŁł";
		public const char DefaultSpecial = (char)0;

		public override bool HasSpecial => false;

		public Base256(string alphabet = DefaultAlphabet, char special = DefaultSpecial,
			Encoding textEncoding = null)
			: base(256, alphabet, special, textEncoding)
		{
		}

		public override string Encode(byte[] data)
		{
			var result = new char[data.Length];

			for (int i = 0; i < data.Length; i++)
				result[i] = Alphabet[data[i]];

			return new string(result);
		}

		public override byte[] Decode(string data)
		{
			unchecked
			{
				byte[] result = new byte[data.Length];

				for (int i = 0; i < data.Length; i++)
					result[i] = (byte)InvAlphabet[data[i]];

				return result;
			}
		}
	}
}
