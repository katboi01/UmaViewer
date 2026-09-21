#!/usr/bin/env bash
# Build Assets/Plugins/WebGL/libsqlite3mc.a: the SQLite3MultipleCiphers amalgamation
# compiled to a WebGL static lib with Unity's bundled Emscripten. Run once (or to bump the
# version); the .a is committed so a plain WebGL build doesn't need Emscripten set up.
set -euo pipefail
REPO="$(cd "$(dirname "$0")/.." && pwd)"
VER="2.5.1"; SQLITE="3.53.4"
BT="/Applications/Unity/PlaybackEngines/WebGLSupport/BuildTools/Emscripten"
[ -x "$BT/emscripten/emcc" ] || { echo "Unity Emscripten not found (install WebGL Build Support)"; exit 1; }
export EM_CONFIG="$BT/.emscripten"; export PATH="$BT/node:$BT/python/bin:$PATH"

WORK="$REPO/Build/websqlite"; mkdir -p "$WORK"; cd "$WORK"
ZIP="sqlite3mc-$VER-sqlite-$SQLITE-amalgamation.zip"
[ -f sqlite3mc_amalgamation.c ] || {
  curl -sL -o amalg.zip "https://github.com/utelle/SQLite3MultipleCiphers/releases/download/v$VER/$ZIP"
  unzip -o -q amalg.zip
}
# Single-threaded (WebGL has no pthreads by default); column metadata for Mono.Data.Sqlite.
"$BT/emscripten/emcc" -O2 -DSQLITE_THREADSAFE=0 -DSQLITE_ENABLE_COLUMN_METADATA \
  -c sqlite3mc_amalgamation.c -o sqlite3mc.o
"$BT/emscripten/emar" rcs libsqlite3mc.a sqlite3mc.o
cp -f libsqlite3mc.a "$REPO/Assets/Plugins/WebGL/libsqlite3mc.a"
echo "OK -> Assets/Plugins/WebGL/libsqlite3mc.a"
