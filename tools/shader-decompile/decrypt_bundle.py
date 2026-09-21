#!/usr/bin/env python3
"""Resolve a game AssetBundle by name from the encrypted meta DB and decrypt it.

Reproduces UmaViewer's on-disk decryption offline:
  1. open <MainPath>/meta (SQLite Multiple Ciphers, ChaCha20) with the region key
  2. SELECT n,h,e FROM a WHERE n=<bundle>  -> url (hash) + per-file key
  3. read <MainPath>/dat/<hash[:2]>/<hash> and XOR the payload after byte 256
     with the per-file key derived from the entry key + ABKey

Usage:
  python3 decrypt_bundle.py shader --data-path /path/to/Persistent [--region jp|global]
  # -> writes ./shader.decrypted (a UnityFS bundle)

Requires the committed libsqlite3mc_mac.dylib (Assets/Plugins) for the meta read.
Keys are the same ones in Assets/Scripts/Config.cs.
"""
import argparse, ctypes, os, struct, sys

# Keys from Assets/Scripts/Config.cs
DB_BASE_KEY = bytes([0xF1,0x70,0xCE,0xA4,0xDF,0xCE,0xA3,0xE1,0xA5,0xD8,0xC7,0x0B,0xD1,0x00,0x00,0x00])
DB_KEY_JP = bytes([0x6D,0x5B,0x65,0x33,0x63,0x36,0x63,0x25,0x54,0x71,0x2D,0x73,0x50,0x53,0x63,0x38,
                   0x6D,0x34,0x37,0x7B,0x35,0x63,0x70,0x23,0x37,0x34,0x53,0x29,0x73,0x43,0x36,0x33])
DB_KEY_GLOBAL = bytes([0x36,0x23,0x6b,0x4c,0x2a,0x39,0x21,0x75,0x52,0x26,0x32,0x76,0x25,0x50,0x3f,0x35,
                       0x5d,0x77,0x58,0x6d,0x40,0x71,0x38,0x5e,0x4c,0x31,0x28,0x74,0x29,0x59,0x37,0x24,0x53])
AB_KEY = bytes([0x53,0x2B,0x46,0x31,0xE4,0xA7,0xB9,0x47,0x3E,0x7C,0xFB])

HERE = os.path.dirname(os.path.abspath(__file__))
DEFAULT_DYLIB = os.path.normpath(os.path.join(HERE, "..", "..", "Assets", "Plugins", "libsqlite3mc_mac.dylib"))


def gen_final_key(region_key):
    return bytes(region_key[i] ^ DB_BASE_KEY[i % 13] for i in range(len(region_key)))


def open_meta(lib, meta_path, region_key):
    lib.sqlite3_open_v2.argtypes = [ctypes.c_char_p, ctypes.POINTER(ctypes.c_void_p), ctypes.c_int, ctypes.c_char_p]
    lib.sqlite3mc_config.argtypes = [ctypes.c_void_p, ctypes.c_char_p, ctypes.c_int]
    lib.sqlite3_key.argtypes = [ctypes.c_void_p, ctypes.c_char_p, ctypes.c_int]
    db = ctypes.c_void_p()
    if lib.sqlite3_open_v2(meta_path.encode(), ctypes.byref(db), 1, None) != 0:  # READONLY
        raise RuntimeError("sqlite3_open_v2 failed for " + meta_path)
    lib.sqlite3mc_config(db, b"cipher", 3)  # 3 = ChaCha20
    fk = gen_final_key(region_key)
    lib.sqlite3_key(db, fk, len(fk))
    return db


def query_row(lib, db, name):
    lib.sqlite3_prepare_v2.argtypes = [ctypes.c_void_p, ctypes.c_char_p, ctypes.c_int, ctypes.POINTER(ctypes.c_void_p), ctypes.c_void_p]
    lib.sqlite3_step.argtypes = [ctypes.c_void_p]
    lib.sqlite3_column_text.argtypes = [ctypes.c_void_p, ctypes.c_int]; lib.sqlite3_column_text.restype = ctypes.c_char_p
    lib.sqlite3_finalize.argtypes = [ctypes.c_void_p]
    stmt = ctypes.c_void_p()
    sql = "SELECT h,e FROM a WHERE n='%s'" % name.replace("'", "''")
    if lib.sqlite3_prepare_v2(db, sql.encode(), -1, ctypes.byref(stmt), None) != 0:
        raise RuntimeError("prepare failed (wrong key/region?)")
    row = None
    if lib.sqlite3_step(stmt) == 100:  # SQLITE_ROW
        h = lib.sqlite3_column_text(stmt, 0)
        e = lib.sqlite3_column_text(stmt, 1)
        row = (h.decode(), int(e.decode()))
    lib.sqlite3_finalize(stmt)
    return row


def fkey(entry_key):
    kb = struct.pack('<q', entry_key)
    keys = bytearray(len(AB_KEY) * 8)
    for i, b in enumerate(AB_KEY):
        for j in range(8):
            keys[i * 8 + j] = b ^ kb[j]
    return bytes(keys)


def decrypt_ab(path, entry_key):
    data = bytearray(open(path, 'rb').read())
    if entry_key != 0 and len(data) > 256:
        fk = fkey(entry_key)
        for i in range(256, len(data)):
            data[i] ^= fk[i % len(fk)]
    return bytes(data)


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("bundle", help="meta entry name, e.g. 'shader'")
    ap.add_argument("--data-path", required=True, help="game data folder (contains meta, dat/)")
    ap.add_argument("--region", choices=["jp", "global"], default="jp")
    ap.add_argument("--dylib", default=DEFAULT_DYLIB)
    ap.add_argument("-o", "--out", default=None)
    a = ap.parse_args()

    lib = ctypes.CDLL(a.dylib)
    region_key = DB_KEY_JP if a.region == "jp" else DB_KEY_GLOBAL
    db = open_meta(lib, os.path.join(a.data_path, "meta"), region_key)
    row = query_row(lib, db, a.bundle)
    if not row:
        sys.exit("bundle '%s' not found in meta" % a.bundle)
    h, e = row
    dat = os.path.join(a.data_path, "dat", h[:2], h)
    if not os.path.exists(dat):
        sys.exit("dat file missing: " + dat)
    out = a.out or (a.bundle.replace("/", "_") + ".decrypted")
    open(out, "wb").write(decrypt_ab(dat, e))
    magic = open(out, "rb").read(8)
    print("wrote %s  (hash=%s key=%d)  magic=%r" % (out, h, e, magic))


if __name__ == "__main__":
    main()
