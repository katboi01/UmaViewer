using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using K4os.Compression.LZ4;
using UnityEngine;

// WebGL rejects AssetBundles stamped for another target. The game ships Standalone
// (StandaloneWindows64 = 19) bundles, so on WebGL we rewrite each SerializedFile's
// m_TargetPlatform to WebGL (20) and re-emit an uncompressed UnityFS for LoadFromMemory.
// Meshes and textures (DXT/BPTC on WebGL2) then load and bind to the in-project shaders.
public static class WebGLBundlePatch
{
    const int SrcTarget = 19;
    const int DstTarget = 20;
    static int _logged;

    public static byte[] Patch(byte[] data)
    {
        try { return PatchInternal(data); }
        catch (Exception e)
        {
            Debug.LogWarning("[WebGLBundlePatch] left bundle unpatched: " + e.Message);
            return data;
        }
    }

    static byte[] PatchInternal(byte[] data)
    {
        var r = new Cursor(data);
        string sig = r.CString();
        if (sig != "UnityFS") return data;

        int formatVersion = r.I32BE();
        string unityVersion = r.CString();
        string unityRevision = r.CString();
        r.I64BE();                       // total size (recomputed on emit)
        int compBlocksInfoSize = r.I32BE();
        int uncompBlocksInfoSize = r.I32BE();
        int archiveFlags = r.I32BE();

        if (formatVersion >= 7) r.Align(16);

        byte[] blocksInfoComp;
        if ((archiveFlags & 0x80) != 0) // BlocksInfoAtTheEnd
        {
            blocksInfoComp = new byte[compBlocksInfoSize];
            Array.Copy(data, data.Length - compBlocksInfoSize, blocksInfoComp, 0, compBlocksInfoSize);
        }
        else
        {
            blocksInfoComp = r.Bytes(compBlocksInfoSize);
        }

        byte[] blocksInfo = Decompress(blocksInfoComp, 0, compBlocksInfoSize, uncompBlocksInfoSize, archiveFlags & 0x3F);

        var bi = new Cursor(blocksInfo);
        byte[] dataHash = bi.Bytes(16);
        int blockCount = bi.I32BE();
        var blocks = new List<(int u, int c, int f)>(blockCount);
        for (int i = 0; i < blockCount; i++)
            blocks.Add((bi.I32BE(), bi.I32BE(), bi.U16BE()));

        int nodeCount = bi.I32BE();
        var nodes = new List<(long off, long size, int flags, string path)>(nodeCount);
        for (int i = 0; i < nodeCount; i++)
            nodes.Add((bi.I64BE(), bi.I64BE(), bi.I32BE(), bi.CString()));

        if ((archiveFlags & 0x200) != 0) r.Align(16); // BlockInfoNeedPaddingAtStart
        int blocksStart = r.Pos;

        // Decompress every block into one contiguous blob (directory offsets index into it).
        var blob = new MemoryStream();
        int bp = blocksStart;
        foreach (var b in blocks)
        {
            blob.Write(Decompress(data, bp, b.c, b.u, b.f & 0x3F), 0, b.u);
            bp += b.c;
        }
        byte[] payload = blob.ToArray();

        int patched = 0;
        foreach (var n in nodes)
        {
            if (n.path.IndexOf(".res", StringComparison.OrdinalIgnoreCase) >= 0) continue;
            if (PatchSerializedFileTarget(payload, (int)n.off, (int)n.size)) patched++;
        }
        if (patched == 0) return data;

        byte[] outBytes = Emit(formatVersion, unityVersion, unityRevision, dataHash, blocks, nodes, payload);
        if (_logged < 3)
        {
            _logged++;
            Debug.Log($"[WebGLBundlePatch] fmt={formatVersion} uv='{unityVersion}' blocks={blockCount} nodes={nodeCount} payload={payload.Length} patched={patched} in={data.Length} out={outBytes.Length}");
        }
        return outBytes;
    }

    // Flip m_TargetPlatform (int32, right after the null-terminated unityVersion string in the
    // SerializedFile metadata). Header field layout follows Unity's SerializedFile format.
    static bool PatchSerializedFileTarget(byte[] buf, int start, int size)
    {
        if (size < 24) return false;
        int p = start;
        uint Be32(ref int q) { uint v = (uint)(buf[q] << 24 | buf[q + 1] << 16 | buf[q + 2] << 8 | buf[q + 3]); q += 4; return v; }

        Be32(ref p);                 // metadataSize
        Be32(ref p);                 // fileSize
        uint version = Be32(ref p);  // SerializedFile version
        Be32(ref p);                 // dataOffset
        if (version >= 9) p += 4;    // endianess byte + 3 reserved
        if (version >= 22) p += 4 + 8 + 8 + 8; // large header: metadataSize, fileSize, dataOffset, unknown

        if (version >= 7)            // skip unityVersion string
        {
            while (p < start + size && buf[p] != 0) p++;
            p++;
        }
        if (version < 8) return false;
        if (p + 4 > start + size) return false;

        int t = buf[p] | buf[p + 1] << 8 | buf[p + 2] << 16 | buf[p + 3] << 24; // little-endian metadata
        if (t != SrcTarget) return false;
        buf[p] = DstTarget & 0xFF; buf[p + 1] = 0; buf[p + 2] = 0; buf[p + 3] = 0;
        return true;
    }

    static byte[] Emit(int formatVersion, string unityVersion, string unityRevision, byte[] dataHash,
        List<(int u, int c, int f)> blocks, List<(long off, long size, int flags, string path)> nodes, byte[] payload)
    {
        var info = new MemoryStream();
        var iw = new Writer(info);
        iw.Bytes(dataHash);
        iw.I32BE(blocks.Count);
        foreach (var b in blocks) { iw.I32BE(b.u); iw.I32BE(b.u); iw.U16BE(0); } // uncompressed
        iw.I32BE(nodes.Count);
        foreach (var n in nodes) { iw.I64BE(n.off); iw.I64BE(n.size); iw.I32BE(n.flags); iw.CString(n.path); }
        byte[] blocksInfo = info.ToArray();

        var outMs = new MemoryStream();
        var w = new Writer(outMs);
        w.CString("UnityFS");
        w.I32BE(formatVersion);
        w.CString(unityVersion);
        w.CString(unityRevision);
        long sizePos = outMs.Position;
        w.I64BE(0);                        // total size, backfilled
        w.I32BE(blocksInfo.Length);        // compressed == uncompressed (uncompressed emit)
        w.I32BE(blocksInfo.Length);
        w.I32BE(0x40);                     // uncompressed, blocksInfo at front, dir+blocks combined
        if (formatVersion >= 7) w.Align(16);
        w.Bytes(blocksInfo);
        w.Bytes(payload);

        long total = outMs.Position;
        outMs.Position = sizePos;
        new Writer(outMs).I64BE(total);
        return outMs.ToArray();
    }

    static byte[] Decompress(byte[] src, int offset, int compSize, int uncompSize, int type)
    {
        if (type == 0)
        {
            var outb = new byte[uncompSize];
            Array.Copy(src, offset, outb, 0, uncompSize);
            return outb;
        }
        if (type == 2 || type == 3) // LZ4 / LZ4HC share the block decoder
        {
            var outb = new byte[uncompSize];
            int n = LZ4Codec.Decode(src, offset, compSize, outb, 0, uncompSize);
            if (n != uncompSize) throw new InvalidDataException($"LZ4 decode {n} != {uncompSize}");
            return outb;
        }
        throw new NotSupportedException($"compression type {type}");
    }

    sealed class Cursor
    {
        readonly byte[] b; public int Pos;
        public Cursor(byte[] data) { b = data; }
        public int I32BE() { int v = b[Pos] << 24 | b[Pos + 1] << 16 | b[Pos + 2] << 8 | b[Pos + 3]; Pos += 4; return v; }
        public int U16BE() { int v = b[Pos] << 8 | b[Pos + 1]; Pos += 2; return v; }
        public long I64BE() { long v = 0; for (int i = 0; i < 8; i++) v = v << 8 | (uint)b[Pos + i]; Pos += 8; return v; }
        public byte[] Bytes(int n) { var o = new byte[n]; Array.Copy(b, Pos, o, 0, n); Pos += n; return o; }
        public string CString() { int s = Pos; while (b[Pos] != 0) Pos++; string r = Encoding.UTF8.GetString(b, s, Pos - s); Pos++; return r; }
        public void Align(int a) { int m = Pos % a; if (m != 0) Pos += a - m; }
    }

    sealed class Writer
    {
        readonly MemoryStream s;
        public Writer(MemoryStream ms) { s = ms; }
        public void I32BE(int v) { s.WriteByte((byte)(v >> 24)); s.WriteByte((byte)(v >> 16)); s.WriteByte((byte)(v >> 8)); s.WriteByte((byte)v); }
        public void U16BE(int v) { s.WriteByte((byte)(v >> 8)); s.WriteByte((byte)v); }
        public void I64BE(long v) { for (int i = 7; i >= 0; i--) s.WriteByte((byte)(v >> (i * 8))); }
        public void Bytes(byte[] v) { s.Write(v, 0, v.Length); }
        public void CString(string v) { var d = Encoding.UTF8.GetBytes(v); s.Write(d, 0, d.Length); s.WriteByte(0); }
        public void Align(int a) { int m = (int)(s.Position % a); if (m != 0) for (int i = 0; i < a - m; i++) s.WriteByte(0); }
    }
}
