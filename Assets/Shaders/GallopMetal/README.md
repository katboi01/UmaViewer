# GallopMetal shaders

Metal-compatible re-authorings of the game's `Gallop/3D/Chara/*` shaders, used on
macOS (and any non-DirectX target) where the game's own AssetBundle shaders are
DX11-only and render magenta. Each file re-declares a game shader by its exact name
(e.g. `Gallop/3D/Chara/Toon/TSER`); `UmaContainerCharacter` swaps a material's shader
to the same-named project shader at load when the original reports `!isSupported`.

`GallopMetalToon.hlsl` is the shared URP toon core. The current shading is an
approximation (base texture + toon ramp + rim + normal-extrude outline); it renders
correctly but is not pixel-identical to the DirectX original.

## Fidelity notes (from decompiling the real shaders)

The real `Toon/TSER` was decompiled (DXVK `DxbcModule` -> SPIR-V -> SPIRV-Cross HLSL;
tooling lives in the session scratchpad). Key facts for anyone improving fidelity:

- Costume recoloring is a 6-zone system: sample `_MaskColorTex` (the costume "area"
  map, bound by UmaViewer), split each RGB channel low/high around 0.5, and tint by
  `_MaskColorR1/R2/G1/G2/B1/B2` (lit) and `_MaskToonColorR1..B2` (shadow), lerped by
  the toon ramp. It is gated by the `USE_MASK_COLOR` keyword and only applies to
  mob/generic costumes with DB color sets, not characters with baked costume textures.
  (`ApplyZones()` in the core implements this but is currently disabled pending
  correct `_MaskColorTex` wiring + testing.)
- Alpha test compares `_TripleMaskMap.b < _Cutoff`.
- Globals `_MainLightColor`, `_GlobalToonColor`, `_GlobalRimColor` are fed by
  `UmaViewerGlobalShader`.
- Secondary terms not yet ported: dirt (`_DirtTex`), env matcap (`_EnvMap`),
  emissive (`_EmissiveTex`), vertical gradient (`_TopColor`/`_BottomColor`).
