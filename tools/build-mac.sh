#!/usr/bin/env bash
# Build a macOS standalone player (Mono backend) headlessly.
# Uses Assets/Editor/HeadlessMacBuild.cs (HeadlessMacBuild.BuildMono).
# The project's default scripting backend is IL2CPP (not installed on most macOS
# setups); the build method temporarily switches to Mono2x and restores it.
set -euo pipefail
REPO="$(cd "$(dirname "$0")/.." && pwd)"
VER="2022.3.62f3"

UNITY=""
for c in \
  "/Applications/Unity/Hub/Editor/$VER/Unity.app/Contents/MacOS/Unity" \
  "/Applications/Unity/Unity.app/Contents/MacOS/Unity"; do
  [ -x "$c" ] && UNITY="$c" && break
done
[ -n "$UNITY" ] || { echo "Unity $VER not found (install via Unity Hub)"; exit 1; }

if [ -e "$REPO/Temp/UnityLockfile" ]; then
  echo "Project is open in the Unity Editor. Quit it first (the build needs the lock)."; exit 1
fi

mkdir -p "$REPO/logs"
LOG="$REPO/logs/player-build.log"
echo "Building Build/UmaViewer.app  (log: $LOG)"
"$UNITY" -quit -batchmode -nographics -projectPath "$REPO" \
  -executeMethod HeadlessMacBuild.BuildMono -logFile "$LOG" || true

if grep -q "BUILD_OK" "$LOG"; then
  echo "OK -> $REPO/Build/UmaViewer.app"
else
  echo "BUILD FAILED. Tail of log:"; tail -20 "$LOG"; exit 1
fi
