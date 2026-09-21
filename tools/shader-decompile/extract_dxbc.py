#!/usr/bin/env python3
"""Extract per-program DXBC and the reflection for one shader in a decrypted bundle.

Unity strips the RDEF (reflection) chunk from bundle DXBC, so generic DX tools
can't recover resource names. The names/offsets live in the SerializedShader,
which this dumps so you can read the decompiled HLSL (cb0=$Globals, cb1=UnityPerMaterial;
cbN[i] maps to the param at byte offset i*16).

Usage:
  python3 extract_dxbc.py shader.decrypted "Gallop/3D/Chara/Toon/TSER" [--out dxbc]

Requires: pip install UnityPy lz4
"""
import argparse, os, struct, sys
import UnityPy
import lz4.block

PTYPE = {0: "ps", 1: "vs", 2: "gs", 3: "hs", 4: "ds", 5: "cs"}


def find_shader(env, name):
    for o in env.objects:
        if o.type.name != "Shader":
            continue
        tt = o.read_typetree()
        if tt.get("m_ParsedForm", {}).get("m_Name") == name:
            return tt
    return None


def dump_dxbc(tt, outdir, short):
    blob = bytes(tt["compressedBlob"])
    comp = tt["compressedLengths"]; dec = tt["decompressedLengths"]
    cl = comp[0][0] if isinstance(comp[0], list) else comp[0]
    dl = dec[0][0] if isinstance(dec[0], list) else dec[0]
    raw = lz4.block.decompress(blob[:cl], uncompressed_size=dl)
    os.makedirs(outdir, exist_ok=True)
    i = idx = 0
    while True:
        i = raw.find(b'DXBC', i)
        if i < 0:
            break
        total = struct.unpack('<I', raw[i + 24:i + 28])[0]
        n = struct.unpack('<I', raw[i + 28:i + 32])[0]
        ptype = "?"
        for k in range(n):
            off = struct.unpack('<I', raw[i + 32 + 4 * k:i + 36 + 4 * k])[0]
            if raw[i + off:i + off + 4] in (b'SHEX', b'SHDR'):
                tok = struct.unpack('<I', raw[i + off + 8:i + off + 12])[0]
                ptype = PTYPE.get((tok >> 16) & 0xFFFF, str(tok >> 16))
                break
        open(os.path.join(outdir, "%s_%02d_%s.dxbc" % (short, idx, ptype)), "wb").write(raw[i:i + total])
        print("  #%02d %s %dB" % (idx, ptype, total))
        idx += 1; i += total


def dump_reflection(tt):
    p0 = tt["m_ParsedForm"]["m_SubShaders"][0]["m_Passes"][0]
    idx2name = {e[1]: e[0] for e in p0["m_NameIndices"] if isinstance(e, (list, tuple)) and len(e) == 2}
    for stage in ("progVertex", "progFragment"):
        cp = p0.get(stage, {}).get("m_CommonParameters")
        if not cp:
            continue
        print("\n[%s] textures:" % stage)
        for t in cp.get("m_TextureParams", []):
            print("  t%d: %s" % (t["m_Index"], idx2name.get(t["m_NameIndex"], t["m_NameIndex"])))
        for cb in cp.get("m_ConstantBuffers", []):
            print("  cbuffer %s (size %s):" % (idx2name.get(cb["m_NameIndex"]), cb.get("m_Size")))
            for vp in cb.get("m_VectorParams", []):
                print("     +%-4s %s (dim %s%s)" % (vp.get("m_Index"), idx2name.get(vp.get("m_NameIndex"), vp.get("m_NameIndex")),
                                                    vp.get("m_Dim"), " int" if vp.get("m_Type") == 1 else ""))
            for mp in cb.get("m_MatrixParams", []):
                print("     +%-4s %s (%dx%d)" % (mp.get("m_Index"), idx2name.get(mp.get("m_NameIndex")), mp.get("m_RowCount"), mp.get("m_ColumnCount")))


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("bundle")
    ap.add_argument("shader")
    ap.add_argument("--out", default="dxbc")
    a = ap.parse_args()
    env = UnityPy.load(a.bundle)
    tt = find_shader(env, a.shader)
    if not tt:
        sys.exit("shader not found: " + a.shader)
    print("=== %s: DXBC programs -> %s/ ===" % (a.shader, a.out))
    dump_dxbc(tt, a.out, a.shader.split("/")[-1])
    print("\n=== reflection (name legend for the decompiled HLSL) ===")
    dump_reflection(tt)


if __name__ == "__main__":
    main()
