#!/usr/bin/env bash
# Build a Windows x64 standalone player (Mono backend) headlessly.
# Uses Assets/Editor/HeadlessWinBuild.cs (HeadlessWinBuild.BuildMono).
# Requires the "Windows Build Support (Mono)" module for this editor (install via
# Unity Hub). IL2CPP for Windows cannot be produced on macOS, so this builds Mono;
# for an IL2CPP release use the GitHub Actions workflow (.github/workflows/build.yml).
set -euo pipefail
REPO="$(cd "$(dirname "$0")/.." && pwd)"
VER="2022.3.62f3"

# Unity.exe is a native Windows binary: hand it Windows paths, not MSYS ones.
winpath() { if command -v cygpath >/dev/null 2>&1; then cygpath -w "$1"; else printf %s "$1"; fi; }

UNITY=""
for c in \
  "/Applications/Unity/Hub/Editor/$VER/Unity.app/Contents/MacOS/Unity" \
  "/Applications/Unity/Unity.app/Contents/MacOS/Unity" \
  "/c/Program Files/Unity/Hub/Editor/$VER/Editor/Unity.exe" \
  "/c/Program Files/Unity/Editor/Unity.exe" \
  "${PROGRAMFILES:-/c/Program Files}/Unity/Hub/Editor/$VER/Editor/Unity.exe"; do
  [ -x "$c" ] && UNITY="$c" && break
done
[ -n "$UNITY" ] || { echo "Unity $VER not found (install via Unity Hub)"; exit 1; }

# A build that dies (compile error, kill, crash) leaves Temp/UnityLockfile behind, and every
# later build then refuses to start while blaming an editor that is not running. Only treat the
# lock as real if a Unity process actually exists.
if [ -e "$REPO/Temp/UnityLockfile" ]; then
  unity_running=""
  if command -v tasklist >/dev/null 2>&1; then
    tasklist //FI "IMAGENAME eq Unity.exe" 2>/dev/null | grep -qi "Unity.exe" && unity_running=1
  elif command -v pgrep >/dev/null 2>&1; then
    pgrep -x Unity >/dev/null 2>&1 && unity_running=1
  else
    unity_running=1   # cannot tell; assume the lock is real
  fi
  if [ -n "$unity_running" ]; then
    echo "Project is open in the Unity Editor. Quit it first (the build needs the lock)."; exit 1
  fi
  echo "Removing stale Temp/UnityLockfile (no Unity process running)."
  rm -f "$REPO/Temp/UnityLockfile"
fi

mkdir -p "$REPO/logs"
LOG="$REPO/logs/player-build-win.log"
echo "Building Build/Windows/UmaViewer.exe  (log: $LOG)"
"$UNITY" -quit -batchmode -nographics -projectPath "$(winpath "$REPO")" -buildTarget Win64 \
  -executeMethod HeadlessWinBuild.BuildMono -logFile "$(winpath "$LOG")" || true

if grep -q "BUILD_OK" "$LOG"; then
  echo "OK -> $REPO/Build/Windows/UmaViewer.exe"
else
  echo "BUILD FAILED."
  # Compile errors first: they are the usual cause and the log tail rarely shows them.
  if grep -q "error CS" "$LOG"; then
    echo "-> Compile errors:"; grep -m 20 "error CS" "$LOG" | sed 's/^/   /'
  else
    echo "Tail of log:"; tail -30 "$LOG"
    # Only a genuinely missing module, not any line that happens to name the build target --
    # "StandaloneWindows64" appears throughout a normal log, so matching it alone sent a
    # compile failure to the wrong conclusion.
    if grep -qiE "module is not installed|BuildTarget is not supported|no .* module (is )?(installed|found)" "$LOG"; then
      echo "-> Install 'Windows Build Support (Mono)' for $VER in Unity Hub, then retry."
    fi
  fi
  exit 1
fi
