#!/usr/bin/env bash
# Build a WebGL player (hostable static site) headlessly.
# Uses Assets/Editor/HeadlessWebBuild.cs (HeadlessWebBuild.Build).
# Requires the "WebGL Build Support" editor module (install via Unity Hub). WebGL is IL2CPP
# only. See WEB.md for the native-plugin work needed before the link succeeds.
set -euo pipefail
REPO="$(cd "$(dirname "$0")/.." && pwd)"
VER="2022.3.62f3"

# --release: lean prod settings (data caching on, lightweight exceptions). Default is a
# debug-friendly build (full stack traces, no data caching).
UMA_ARGS=""
for arg in "$@"; do
  case "$arg" in
    --release) UMA_ARGS="-umaRelease" ;;
    *) echo "Unknown option: $arg" >&2; exit 2 ;;
  esac
done

winpath() { if command -v cygpath >/dev/null 2>&1; then cygpath -w "$1"; else printf %s "$1"; fi; }

UNITY=""
for c in \
  "/Applications/Unity/Hub/Editor/$VER/Unity.app/Contents/MacOS/Unity" \
  "/Applications/Unity/Unity.app/Contents/MacOS/Unity" \
  "/c/Program Files/Unity/Hub/Editor/$VER/Editor/Unity.exe"; do
  [ -x "$c" ] && UNITY="$c" && break
done
[ -n "$UNITY" ] || { echo "Unity $VER not found (install via Unity Hub)"; exit 1; }

if [ -e "$REPO/Temp/UnityLockfile" ]; then
  echo "Project is open in the Unity Editor. Quit it first (the build needs the lock)."; exit 1
fi

mkdir -p "$REPO/logs"
LOG="$REPO/logs/player-build-web.log"
echo "Building Build/Web${UMA_ARGS:+ (release)}  (log: $LOG)"
"$UNITY" -quit -batchmode -nographics -projectPath "$(winpath "$REPO")" -buildTarget WebGL \
  -executeMethod HeadlessWebBuild.Build $UMA_ARGS -logFile "$(winpath "$LOG")" || true

if grep -q "BUILD_OK" "$LOG"; then
  echo "OK -> $REPO/Build/Web  (serve it: python3 tools/serve-web.py)"
else
  echo "BUILD FAILED."
  if grep -q "error CS" "$LOG"; then
    echo "-> Compile errors:"; grep -m 20 "error CS" "$LOG" | sed 's/^/   /'
  else
    tail -30 "$LOG"
    if grep -qiE "module is not installed|BuildTarget is not supported|WebGL.*not"  "$LOG"; then
      echo "-> Install 'WebGL Build Support' for $VER in Unity Hub, then retry."
    fi
    if grep -qiE "undefined symbol|wasm-ld|emcc|linker" "$LOG"; then
      echo "-> Link error: a native plugin has no WebGL build. See WEB.md (sqlite3mc WASM etc.)."
    fi
  fi
  exit 1
fi
