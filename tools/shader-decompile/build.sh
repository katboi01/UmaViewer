#!/usr/bin/env bash
# Build the DXBC->HLSL decompiler (dxbc2hlsl) on macOS.
# Clones redstrate/dxbc (DXVK's DXBC->SPIR-V decoder), applies two macOS portability
# patches, builds it, then compiles dxbc2hlsl.cpp against it + brew's SPIRV-Cross.
set -euo pipefail
cd "$(dirname "$0")"
BUILD=".build"

command -v cmake >/dev/null || { echo "need cmake (brew install cmake ninja)"; exit 1; }
for f in spirv-cross spirv-headers vulkan-headers; do
  brew list "$f" >/dev/null 2>&1 || { echo "brew install $f"; brew install "$f"; }
done

if [ ! -d "$BUILD/redstrate_dxbc" ]; then
  mkdir -p "$BUILD"
  git clone --depth 1 https://codeberg.org/redstrate/dxbc.git "$BUILD/redstrate_dxbc"
fi
DX="$BUILD/redstrate_dxbc"

# macOS patch 1: SCHED_IDLE/SCHED_BATCH are Linux-only
TH="$DX/src/util/thread.h"
grep -q UMAVIEWER_SCHED_FALLBACK "$TH" || python3 - "$TH" <<'PY'
import sys
p=sys.argv[1]; s=open(p).read()
inj="#ifndef UMAVIEWER_SCHED_FALLBACK\n#define UMAVIEWER_SCHED_FALLBACK\n#ifndef SCHED_IDLE\n#define SCHED_IDLE SCHED_OTHER\n#endif\n#ifndef SCHED_BATCH\n#define SCHED_BATCH SCHED_OTHER\n#endif\n#endif\n"
s=s.replace("#pragma once","#pragma once\n"+inj,1) if "#pragma once" in s else inj+s
open(p,"w").write(s)
PY

# macOS patch 2: pthread_setname_np takes one arg on macOS
EN="$DX/src/util/util_env.cpp"
grep -q "__APPLE__" "$EN" || python3 - "$EN" <<'PY'
import sys
p=sys.argv[1]; s=open(p).read()
old='    ::pthread_setname_np(pthread_self(), posixName.data());'
new='#ifdef __APPLE__\n    ::pthread_setname_np(posixName.data());\n#else\n    ::pthread_setname_np(pthread_self(), posixName.data());\n#endif'
open(p,"w").write(s.replace(old,new,1))
PY

FAKE_VULKAN="$(find /opt/homebrew/lib -maxdepth 1 -name '*.dylib' | head -1)"   # DXVK only needs headers
cmake -S "$DX" -B "$DX/build" -G Ninja -DCMAKE_BUILD_TYPE=Release \
  -DCMAKE_PREFIX_PATH=/opt/homebrew -DCMAKE_POLICY_VERSION_MINIMUM=3.5 \
  -DCMAKE_CXX_STANDARD=20 -DCMAKE_CXX_STANDARD_REQUIRED=ON \
  -DVulkan_LIBRARY="$FAKE_VULKAN" -DCMAKE_EXE_LINKER_FLAGS="-L/opt/homebrew/lib"
ninja -C "$DX/build" dxbc

clang++ -std=c++20 -O2 \
  -I "$DX/src/dxbc" -I "$DX/src/util" -I "$DX/src/spirv" -I "$DX/include/windows" -I /opt/homebrew/include \
  dxbc2hlsl.cpp \
  -o dxbc2hlsl -L "$DX/build/src/dxbc" -L "$DX/build/src/spirv" -L "$DX/build/src/util" \
  -L /opt/homebrew/lib -ldxbc -ldxbc-spirv -ldxbc-util \
  -lspirv-cross-core -lspirv-cross-glsl -lspirv-cross-hlsl -lspirv-cross-msl
echo "built ./dxbc2hlsl"
