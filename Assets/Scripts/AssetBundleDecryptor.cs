using System;
using System.IO;

public static class AssetBundleDecryptor
{
    // 默认值（你之前提供的）
    private static readonly byte[] DefaultBaseKeys = new byte[]
    {
        0x53, 0x2B, 0x46, 0x31, 0xE4, 0xA7, 0xB9, 0x47, 0x3E, 0x7C, 0xFB
    };
    private const long DefaultKey = -7673907454518172050L;

    /// <summary>
    /// 使用默认 baseKeys + key 解密文件并返回解密后的 byte[]（一次性读取全部到内存）。
    /// 前 256 字节保持原样（不做 XOR），从偏移 256 开始对每个字节做 data[i] ^= keys[i % keys.Length]。
    /// </summary>
    /// <param name="inputFilePath">要解密的输入文件路径</param>
    /// <returns>解密后的字节数组（可直接传给 AssetBundle.LoadFromMemory）</returns>
    public static byte[] DecryptFileToBytes(string inputFilePath, long key)
    {
        return DecryptFileToBytes(inputFilePath, DefaultBaseKeys, key);
    }

    /// <summary>
    /// 通用接口：使用指定的 baseKeys 与 key 解密文件并返回解密后的 byte[]。
    /// </summary>
    /// <param name="inputFilePath">输入文件路径（必须存在）</param>
    /// <param name="baseKeys">baseKeys 数组（每个元素是一个 byte，函数会为每个 baseKeys 元素生成 8 字节的一段）</param>
    /// <param name="key">int64 key（支持负数）；转成 8 字节小端 two's-complement 后与 baseKeys 异或以构造 keys 平坦数组）</param>
    /// <returns>解密后的字节数组</returns>
    public static byte[] DecryptFileToBytes(string inputFilePath, byte[] baseKeys, long key)
    {
        if (string.IsNullOrEmpty(inputFilePath))
            throw new ArgumentNullException(nameof(inputFilePath));
        if (!File.Exists(inputFilePath))
            throw new FileNotFoundException("Input file not found", inputFilePath);
        if (baseKeys == null || baseKeys.Length == 0)
            throw new ArgumentException("baseKeys must not be null or empty", nameof(baseKeys));

        // 读取整个文件到内存（用户要求不分块）
        byte[] data = File.ReadAllBytes(inputFilePath);

        // 构造 keyBytes（8 字节小端）。确保是小端序。
        byte[] keyBytes = BitConverter.GetBytes(key);
        if (!BitConverter.IsLittleEndian)
            Array.Reverse(keyBytes);

        // 构造平坦 keys: 对 baseKeys 中的每个字节生成 8 个字节: base ^ keyBytes[j]
        int baseLen = baseKeys.Length;
        int keysLen = baseLen * 8;
        byte[] keys = new byte[keysLen];
        for (int i = 0; i < baseLen; ++i)
        {
            byte b = baseKeys[i];
            int baseOffset = i << 3; // i * 8
            for (int j = 0; j < 8; ++j)
            {
                keys[baseOffset + j] = (byte)(b ^ keyBytes[j]);
            }
        }

        // 如果文件长度 <= 256，则没有任何字节被 XOR，直接返回原数据
        if (data.Length <= 256)
            return data;

        // 从偏移 256 开始，对每个字节按 keys 循环做异或
        for (int i = 256; i < data.Length; ++i)
        {
            data[i] ^= keys[i % keysLen];
        }

        return data;
    }
}
