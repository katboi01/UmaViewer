# Shader decompile tools

Offline tooling to recover the game's `Gallop/*` / `Cygames/*` shader logic so it can
be re-authored as Metal-compatible URP shaders (see `Assets/Shaders/GallopMetal/`).
The game ships DirectX-only (DXBC/SM5) compiled shaders in its AssetBundles, which
render magenta on non-DirectX targets; these tools expose the real logic to port from.

This is a **decompile + read + hand-port** workflow, not a one-click DX11->ShaderLab
converter. The DXBC->HLSL step is automated; turning that HLSL into a clean ShaderLab
pass is manual (read the logic, re-author with named uniforms).

## Requirements

- macOS (the decrypt reads the committed `Assets/Plugins/libsqlite3mc_mac.dylib`).
- Python: `pip install UnityPy lz4`
- Build tools: `brew install cmake ninja spirv-cross spirv-headers vulkan-headers`
- `build.sh` clones [redstrate/dxbc](https://codeberg.org/redstrate/dxbc) (DXVK's DXBC
  decoder) into `.build/` and applies two macOS patches (Linux `SCHED_IDLE`, and the
  one-arg `pthread_setname_np`). All build output stays in `.build/` (gitignored).

Operates on the game's proprietary assets; the DB/AB keys used are the same ones in
`Assets/Scripts/Config.cs`. Recovered shader logic is derivative of the game's shaders,
same as the viewer itself.

## Full process

1. **Build the decompiler** (once):
   ```
   ./build.sh          # -> ./dxbc2hlsl
   ```

2. **Decrypt a shader bundle** from a game data folder. Bundle names come from the
   meta DB (`shader` is the character shader bundle):
   ```
   python3 decrypt_bundle.py shader --data-path /path/to/Persistent [--region jp|global]
   # -> shader.decrypted  (a UnityFS AssetBundle)
   ```

3. **Extract the DXBC + reflection** for one shader. The reflection is the name legend
   for the decompiled HLSL (Unity strips it from the DXBC itself):
   ```
   python3 extract_dxbc.py shader.decrypted "Gallop/3D/Chara/Toon/TSER" --out dxbc
   # -> dxbc/TSER_00_vs.dxbc, TSER_04_ps.dxbc, ...   + prints textures + cbuffer layout
   ```
   Each program is vertex (`vs`) or pixel (`ps`); the main lit fragment is usually the
   largest `ps`. `cbN[i]` in the HLSL is the cbuffer param at byte offset `i*16`.

4. **Decompile a program to HLSL** (or MSL):
   ```
   ./dxbc2hlsl dxbc/TSER_08_ps.dxbc      > TSER_08_ps.hlsl
   ./dxbc2hlsl dxbc/TSER_08_ps.dxbc msl  > TSER_08_ps.metal
   ```

5. **Read + port.** The output is register-machine HLSL with encoded constants
   (`asfloat(0x3F800000)` = 1.0, etc.). Cross-reference `cbN[i]`/`tN`/`sN` against the
   reflection from step 3, work out the algorithm, and re-author a clean URP pass in
   `Assets/Shaders/GallopMetal/`. `UmaContainerCharacter` swaps a game shader to the
   same-named project shader at load when the original is `!isSupported`.

## What was already recovered

`Gallop/3D/Chara/Toon/TSER` (see `Assets/Shaders/GallopMetal/GallopMetalToon.hlsl`):
- `_ToonMap` is the shadow-color texture (`shad_c`), sampled at the main UV; the toon
  term blends shadow<-lit diffuse by the half-lambert light angle.
- 6-zone costume recoloring: `_MaskColorTex` RGB channels split low/high around 0.5,
  tinting the lit diffuse by `_MaskColor{R1,R2,G1,G2,B1,B2}` and the shadow by the
  matching `_MaskToonColor*`. Gated in-game by `USE_MASK_COLOR`.
- Alpha test: `_TripleMaskMap.b < _Cutoff`.
- App-fed globals (`UmaViewerGlobalShader`): `_MainLightColor`, `_GlobalToonColor`,
  `_GlobalRimColor`, ...
- Not yet ported: dirt (`_DirtTex`), env matcap (`_EnvMap`), emissive (`_EmissiveTex`),
  vertical gradient (`_TopColor`/`_BottomColor`), and the exact two-layer rim.
