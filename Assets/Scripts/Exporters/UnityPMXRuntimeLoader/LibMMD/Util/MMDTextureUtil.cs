using LibMMD.Material;

namespace LibMMD.Util
{
    public static class MMDTextureUtil
    {
        private static readonly string[] GlobalToonNames =
        {
            "toon0.bmp",
            "toon01.bmp",
            "toon02.bmp",
            "toon03.bmp",
            "toon04.bmp",
            "toon05.bmp",
            "toon06.bmp",
            "toon07.bmp",
            "toon08.bmp",
            "toon09.bmp",
            "toon10.bmp"
        };

        public static MMDTexture GetGlobalToon(int index, string rootPath)
        {
            if (index >= 0 && index < GlobalToonNames.Length - 1)
            {
                return new MMDTexture(GlobalToonNames[index + 1]);
            }
            return null;
        }
    }
}