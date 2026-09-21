using K4os.Compression.LZ4;
using K4os.Compression.LZ4.Streams;
using System.IO;

public static class LZ4Util
{
    public static byte[] DecompressFromFile(string path)
    {
        var decoder = LZ4Stream.Decode(File.OpenRead(path));
        var target = new MemoryStream();
        decoder.CopyTo(target);
        return target.ToArray();
    }

    public static byte[] CompressFromBytes(byte[] bytes)
    {
        var source = new MemoryStream(bytes);
        var decoder = LZ4Stream.Encode(source);
        var target = new MemoryStream();
        decoder.CopyTo(target);
        return target.ToArray();
    }
}
