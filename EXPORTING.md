# Exporting models and animations

Headless CLI (`Assets/Scripts/CliExporter.cs`, enabled by `--export`) that writes an MMD
**PMX** (+ textures) and a **VMD** motion for viewers like [babylon-mmd](https://github.com/noname0310/babylon-mmd), reusing the in-app exporters.

## Prerequisites

- A built player. macOS: `tools/build-mac.sh` → `Build/UmaViewer.app`. Windows: `tools/build-win.sh` → `Build/Windows/UmaViewer.exe` (needs the "Windows Build Support (Mono)" editor module), or the CI IL2CPP artifact from `.github/workflows/build.yml`.
- A game data folder (the `Persistent` directory) with the encrypted `meta` DB and asset bundles.
- Run with `-batchmode` but **not** `-nographics`: a null GPU device loads no meshes/textures.

## Command

```
Build/UmaViewer.app/Contents/MacOS/UmaViewer -batchmode --export \
  --data-path /path/to/Persistent --region jp \
  --chara 1127 --costume 00 --anim anm_eve_chr1127_00_idle01_loop \
  --out ./export/1127 -logFile ./logs/export.log
```

On Windows the only changes are the executable (`Build\Windows\UmaViewer.exe`) and the `--data-path`.

### Arguments

| Arg | Meaning |
|---|---|
| `--data-path DIR` | Game data folder (contains `meta`). Required. |
| `--chara ID` | Character id, e.g. `1127`. Required. |
| `--costume NN` | Costume id (default `00`). |
| `--anim NAME` | Motion asset name, e.g. `anm_eve_chr1127_00_idle01_loop`. Omit to export the model only. |
| `--out DIR` | Output directory. |
| `--region jp\|global` | Which DB key/asset set to decrypt. Defaults to `Config.json` (`jp`). The `meta` DB is region-encrypted, so a wrong region fails to load. |
| `--no-model` | Skip the PMX + textures; export only the VMD (see below). |
| `--blink` | Bake a periodic eye-blink (`まばたき`) over the motion. |
| `--no-mouth` | Strip the mouth vowel morphs (`あいうえお▲□`) so a viewer can drive lip-sync at runtime. |
| `--seconds N` | Record N seconds raw (no loop trim) instead of one period; for `tools/loopify_vmd.py`. |
| `--pmx-name F` | Output PMX filename (default `chr<id>_<costume>.pmx`). Extension optional. |
| `--vmd-name F` | Output VMD filename (defaults to the PMX's stem). Extension optional. |
| `--bake-physics` | Record the cloth simulation into the VMD instead of exporting PMX rigid bodies. See below. |
| `--warmup N` | Settle N animation periods before capturing a loop (default 2). Diagnostic. |
| `--physics-fps 30\|60` | Step rate for `--physics-ref` captures (default 60). Diagnostic. |
| `--physics-ref F` | Write the CySpring-simulated spring-bone rotations per frame to JSON. Diagnostic. |
| `--spring-dump F` | Write the raw per-bone CySpring parameters to JSON. Diagnostic. |

### Output

- `chr<id>_<costume>.pmx` — clean MMD skeleton (game control/IK bones stripped, their weights baked onto standard bones; parents ordered before children).
- `Texture2D/` — the model's textures.
- `chr<id>_<costume>.vmd` — the motion, when `--anim` is given. `_loop` motions are trimmed to one seamless period.

## Model and motion are separate

Export a model once, then batch any number of motions against it:

```
# once: model + textures (no --anim)
UmaViewer --export --chara 1127 --out ./export/1127

# then: VMD only, model untouched (--no-model)
UmaViewer --export --no-model --chara 1127 --anim anm_eve_chr1127_00_idle01_loop --out ./export/1127
UmaViewer --export --no-model --chara 1127 --anim anm_eve_type00_run01_loop      --out ./export/1127
```

VMD recording still loads the character each time (unavoidable headless); `--no-model` just skips re-writing the PMX/textures.

## Seamless loops for fast motions

The CLI trims a `_loop` motion using the clip length as the period. For fast cyclic motions (runs) whose true period differs from the clip length, that leaves a small seam. Use `tools/export-anim.sh`, which records several strides and then finds the real period and crossfades:

```
tools/export-anim.sh --data-path /path/to/Persistent --chara 1127 \
  --anim anm_eve_type00_run01_loop --blink --no-mouth --tiles 4 --out ./export/1127
```

It records `--seconds` of raw motion, runs `tools/loopify_vmd.py` (full-body resample → fundamental-period search → tail crossfade → optional blink/mouth-strip), and leaves the PMX + textures + a seamless VMD. Runs on macOS/Linux and Git Bash/WSL on Windows; set `UMAVIEWER_BIN` to point at a player elsewhere. `--tiles N` repeats the loop (e.g. a less frequent blink).
`--period N` pins the loop length to N frames (30fps) instead of detecting it. Use it when the
detector settles on a short sub-cycle -- a held pose like `salute01_loop` barely moves, and its
60-frame cycle was being cut to 8. The clip length is logged as `CLI_CLIP` during the export. A pinned
period also starts the loop at the capture start (`loopify_vmd.py --start 0`), which the player aligns
to the clip's own frame 0 (`CLI_PHASE` reports where it landed) instead of searching for the best seam.
Without `--period`, the seam search picks the phase.

A `_s` (start) or `_e` (end) clip can be recorded on its own by naming it in `--anim`. The viewer only
plays these inside a loop's chain (previous idle `_e`, then the clip's `_s`, then the loop), so the
exporter plays them directly; the VMD begins at the clip's frame 0, from the rest pose. `_e` clips end,
and `_s` clips begin, in the game's shared neutral stance (hands clasped in front), which is common to
all characters and body types.

The wrapper forwards `--pmx-name`, `--vmd-name` and `--bake-physics` to the player. `loopify_vmd.py` tiles every track it finds, so baked cloth survives the loop build; its `--seam-cloth` option also weighs those tracks when choosing the loop phase, which it otherwise picks from the humanoid bones alone.

## Cloth: simulated or baked

The game simulates cloth with **CySpring**, a Verlet spring-bone solver: each bone keeps an
implicit velocity from its previous position, is pulled back toward its rest direction by a
stiffness term, damped by a drag term, and then projected back onto a fixed distance from its
parent every step. PMX physics is something else entirely — rigid bodies joined by 6DOF spring
constraints, solved iteratively. There is no parameter mapping between the two, only a
resemblance.

**Simulated (default).** The PMX carries rigid bodies and joints built from the CySpring data:
kinematic colliders on the body and legs, a dynamic body per swaying bone, joints along each
chain with the per-bone stiffness mapped onto the joint spring. Enable babylon-mmd's physics
runtime or those bones stay put and the skirt clips. The cloth reacts at runtime, and it is an
approximation — expect it to move differently from the game.

**Baked (`--bake-physics`).** The exporter runs CySpring itself, records the resulting
spring-bone rotations as VMD tracks, and writes the PMX with no rigid bodies or joints at all.
The cloth is then the solver's own output rather than a reconstruction of it. The cost is that
the motion is fixed: it cannot react to anything at runtime, and every animation needs its own
bake. For a fixed looping emote that is usually the right trade.

Notes on the baked path:

- Spring bones are renamed in the PMX so their names fit a VMD track's 15-byte field. The
  originals (up to 20 characters, and colliding under truncation) are kept as each bone's
  English name. `Sp_Hi_MSkirt0_FLL_00` becomes `HiMS0FLL0`.
- Baked tracks keep every frame, ignoring the key reduction applied to body motion, because
  cloth can move ~34 deg between frames and interpolating across that loses the detail.
- The simulation is currently stepped at 30fps while the game runs at 60. CySpring's constants
  are per-step so its output is rate-dependent: the same run captured at 30 instead of 60 moves
  the tail bones ~50 deg on average. `--physics-fps` fixes this for `--physics-ref` captures but
  not for the bake — see the comment in `CliExporter.cs` for why the recording loop cannot
  currently separate the two rates.
- A VMD frame number is the timestamp, at 30 ticks per second, with no frame-rate field. Writing
  60 keys per second would halve the playback speed rather than add detail, so the VMD stays at
  30fps whatever the simulation is stepped at.

### Diagnostics

`--physics-ref F` writes the CySpring-simulated spring-bone rotations per frame to JSON: the
solver's own output, for comparing against whatever a runtime produces from the PMX bodies.
`--spring-dump F` writes the raw per-bone CySpring parameters. Both need the real
`CySpringPlugin.dll`, so run them on Windows.

The solver scales the raw asset values — `StiffnessForce/1000`, `DragForce/100`,
`Gravity/10000` (constants at `0x18008ec8c`, `0x18008ec80`, `0x18008ec90` in `CySpringPlugin.dll`).
Read the raw fields without those divisors and the numbers are meaningless: on chr1127 they run
130..700 and 200..1050.

## Notes for babylon-mmd

- Bone and morph names are MMD-standard Japanese (`センター`, `右足`, `あ`, `まばたき`, …); the PMX also stores the game bone name as each bone's English name.
- Skirt/hair/ear/tail physics ships one of two ways, and they are mutually exclusive — see **Cloth: simulated or baked** below.
- The VMD carries **no camera** data (model motion only).
- Facial morphs are a separate channel: a motion with a paired `_face` asset records real expressions; otherwise the mouth stays neutral. With `--no-mouth`, drive the vowel morphs (`あいうえお`) yourself at runtime.
