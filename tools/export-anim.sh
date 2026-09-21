#!/usr/bin/env bash
# Export a character PMX (+textures) and a seamless looping VMD in one step.
# Records several strides raw via the player's --export --seconds, then loopify_vmd.py
# finds the true loop period and builds a clean loop (optionally blink / mouth-free).
# Runs anywhere bash + python exist (macOS, Linux, Git Bash/WSL on Windows).
set -euo pipefail
HERE="$(cd "$(dirname "$0")" && pwd)"; REPO="$(cd "$HERE/.." && pwd)"

DATA=""; CHARA=""; ANIM=""; OUT=""; REGION="jp"; COSTUME="00"; SECS=5; TILES=1; BLINK=""; NOMOUTH=""
PMXNAME=""; VMDNAME=""; BAKE=""; PERIOD=""
while [ $# -gt 0 ]; do case "$1" in
  --data-path) DATA="$2"; shift 2;;
  --chara)     CHARA="$2"; shift 2;;
  --anim)      ANIM="$2"; shift 2;;
  --out)       OUT="$2"; shift 2;;
  --region)    REGION="$2"; shift 2;;
  --costume)   COSTUME="$2"; shift 2;;
  --seconds)   SECS="$2"; shift 2;;
  --tiles)     TILES="$2"; shift 2;;
  --blink)     BLINK="--blink"; shift;;
  --no-mouth)  NOMOUTH="--no-mouth"; shift;;
  --pmx-name)  PMXNAME="$2"; shift 2;;
  --vmd-name)  VMDNAME="$2"; shift 2;;
  --bake-physics) BAKE="--bake-physics"; shift;;
  --period)    PERIOD="$2"; shift 2;;
  *) echo "unknown arg: $1"; exit 1;;
esac; done

if [ -z "$DATA" ] || [ -z "$CHARA" ] || [ -z "$ANIM" ] || [ -z "$OUT" ]; then
  echo "usage: $0 --data-path DIR --chara ID --anim NAME --out DIR \\"
  echo "          [--region jp|global] [--costume 00] [--seconds 5] [--tiles N] [--blink] [--no-mouth] \\"
  echo "          [--pmx-name model.pmx] [--vmd-name running.vmd] [--bake-physics] [--period FRAMES]"
  exit 1
fi

BIN="${UMAVIEWER_BIN:-}"
if [ -z "$BIN" ]; then
  for c in \
    "$REPO/Build/UmaViewer.app/Contents/MacOS/UmaViewer" \
    "$REPO/Build/Windows/UmaViewer.exe" \
    "$REPO/Build/UmaViewer.exe" \
    "$REPO/Build/UmaViewer.x86_64"; do
    if [ -e "$c" ]; then BIN="$c"; break; fi
  done
fi
[ -n "$BIN" ] || { echo "UmaViewer player not found; build it (tools/build-mac.sh or build-win.sh) or set UMAVIEWER_BIN"; exit 1; }
# Probe rather than trust the first hit: on Windows, python3 usually resolves to the
# Microsoft Store alias stub, which exists on PATH but exits with "Python was not found"
# instead of running anything.
PY=""
for c in python3 python py; do
  p="$(command -v "$c" 2>/dev/null)" || continue
  [ -n "$p" ] || continue
  if "$p" -c "import sys" >/dev/null 2>&1; then PY="$p"; break; fi
done
[ -n "$PY" ] || { echo "python not found"; exit 1; }

mkdir -p "$OUT" "$REPO/logs"; RAW="$OUT/.raw"; mkdir -p "$RAW"
STEM="chr${CHARA}_${COSTUME}"
# Output filenames. Defaults keep the historical chr<id>_<costume> naming; an explicit
# name may omit the extension. These are passed to the player so it writes them directly.
PMXFILE="${PMXNAME:-$STEM.pmx}"; case "$PMXFILE" in *.pmx) ;; *) PMXFILE="$PMXFILE.pmx";; esac
VMDFILE="${VMDNAME:-${PMXFILE%.pmx}.vmd}"; case "$VMDFILE" in *.vmd) ;; *) VMDFILE="$VMDFILE.vmd";; esac
echo "Recording ${SECS}s of $ANIM (chara $CHARA) ..."
"$BIN" -batchmode --export --data-path "$DATA" --region "$REGION" \
  --chara "$CHARA" --costume "$COSTUME" --anim "$ANIM" --seconds "$SECS" \
  --pmx-name "$PMXFILE" --vmd-name "$VMDFILE" $BAKE \
  --out "$RAW" -logFile "$REPO/logs/export-anim.log"

[ -f "$RAW/$VMDFILE" ] || { echo "raw export failed; see logs/export-anim.log"; exit 1; }
cp -f "$RAW/$PMXFILE" "$OUT/$PMXFILE"
[ -d "$RAW/Texture2D" ] && cp -Rf "$RAW/Texture2D" "$OUT/"

echo "Building seamless loop ..."
# --period pins the loop length (in 30fps frames) instead of detecting it; for clips that
# barely move, the detector can settle on a short sub-cycle. It also starts the loop at the
# capture start, which the player aligns to the clip's own phase 0.
PERIODARGS=""; [ -n "$PERIOD" ] && PERIODARGS="--pmin $PERIOD --pmax $((PERIOD+1)) --start 0"
"$PY" "$HERE/loopify_vmd.py" "$RAW/$VMDFILE" "$OUT/$VMDFILE" --tiles "$TILES" $BLINK $NOMOUTH $PERIODARGS
rm -rf "$RAW"
echo "Done -> $OUT/$PMXFILE + $OUT/$VMDFILE"
