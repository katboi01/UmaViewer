using K4os.Compression.LZ4;
using K4os.Compression.LZ4.Streams;
using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

public static class LZ4Util
{
    [DllImport("libnative.dll")]
    public static extern int LZ4_decompress_safe_ext(IntPtr src, IntPtr dst, int compressedSize, int dstCapacity);

    public static byte[] DecompressFromFile(string path)
    {
        var decoder = LZ4Stream.Decode(File.OpenRead(path));
        var target = new MemoryStream();
        decoder.CopyTo(target);
        return target.ToArray();
    }

    public static byte[] DecompressFromBytesSafe(byte[] bytes)//WIP
    {
        IntPtr data = Marshal.AllocHGlobal(bytes.Length);
        Marshal.Copy(bytes, 0, data, bytes.Length);

        int dataLength = 0;
        dataLength = ((dataLength| bytes[3])<<8);
        dataLength = ((dataLength| bytes[2])<<8);
        dataLength = ((dataLength| bytes[1])<<8);
        dataLength = (dataLength| bytes[0]);

        IntPtr result = Marshal.AllocHGlobal(bytes.Length+ dataLength);

        var output = new byte[bytes.Length + dataLength];
        var outlength = LZ4_decompress_safe_ext(data, result, bytes.Length, output.Length);

        Marshal.Copy(result, output, 0,outlength);

        Array.Resize(ref output, outlength);
        Marshal.FreeHGlobal(data);
        Marshal.FreeHGlobal(result);
        return output;
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

