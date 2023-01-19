using System.Text;

namespace Cutt
{
    public static class FNVHash
    {
        public const uint FNV_OFFSET_BASIS_32 = 2166136261u;

        public const int FNV_OFFSET_BASIS_I32 = -2128831035;

        private static uint FNV_PRIME_32 = 16777619u;

        public static int Generate(string seed)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(seed);
            uint num = FNV_OFFSET_BASIS_32;
            for (int i = 0; i < bytes.Length; i++)
            {
                num = (FNV_PRIME_32 * num) ^ bytes[i];
            }
            return (int)num;
        }
    }
}
