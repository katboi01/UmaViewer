using LibMMD.Material;
using System;
using System.IO;

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

        public static MMDTexture GetGlobalToon(int index)
        {
            if (index >= 0 && index < GlobalToonNames.Length - 1)
            {
                return new MMDTexture(GlobalToonNames[index + 1]);
            }
            return null;
        }

        public static int GetGlobalToonIndex(string texturePath, string globalToonPath)
        {
            string globalToonName = Path.GetFileName(texturePath);
            if (!string.IsNullOrEmpty(globalToonName))
            {
                for (int i = 1; i < GlobalToonNames.Length; i++)
                {
                    string globalToon = Path.Combine(globalToonPath, GlobalToonNames[i]);
                    if (string.Equals(globalToonName, Path.GetFileName(globalToon), StringComparison.OrdinalIgnoreCase))
                    {
                        return i - 1;
                    }
                }
            }
            return -1;
        }
    }
}