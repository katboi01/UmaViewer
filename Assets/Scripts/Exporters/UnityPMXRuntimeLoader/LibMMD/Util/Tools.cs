using System.Text;

namespace LibMMD.Util
{
    public static class Tools
    {
        public static readonly Encoding JapaneseEncoding = Encoding.GetEncoding(932); 
        public const float MmdMathConstEps = (float) 1.0e-7;
    }
}