using System;
using System.IO;

public static class AssetBundleDecryptor
{
    public static byte[] DecryptFileToBytes(string inputFilePath, byte[] Keys)
    {
        if (string.IsNullOrEmpty(inputFilePath))
            throw new ArgumentNullException(nameof(inputFilePath));
        if (!File.Exists(inputFilePath))
            throw new FileNotFoundException("Input file not found", inputFilePath);
        if (Keys == null)
            throw new ArgumentException("Keys must not be null or empty", nameof(Keys));

        byte[] data = File.ReadAllBytes(inputFilePath);

        if (data.Length <= 256)
            return data;

        for (int i = 256; i < data.Length; ++i)
        {
            data[i] ^= Keys[i % Keys.Length];
        }

        return data;
    }
}
