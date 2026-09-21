#ifndef GALLOP_METAL_TOON_INCLUDED
#define GALLOP_METAL_TOON_INCLUDED

// Shared toon core for the Metal re-authoring of the Gallop character shaders.
// Faithful port of the dominant terms of Gallop/3D/Chara/Toon/TSER, recovered by
// decompiling the game's DXBC (DXVK -> SPIR-V -> HLSL): 6-zone costume coloring via
// _TripleMaskMap (each RGB channel split low/high) x _MaskColor* (lit) / _MaskToonColor*
// (shadow), main-light modulation, toon-ramp lerp between lit and shadow, plus rim.
// Secondary terms (dirt / env / emissive / gradient) are omitted for now.
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

CBUFFER_START(UnityPerMaterial)
    float4 _MainTex_ST;
    float4 _SpecularColor, _RimColor, _RimColor2, _ToonBrightColor, _ToonDarkColor, _OutlineColor;
    float4 _MaskColorR1, _MaskColorR2, _MaskColorG1, _MaskColorG2, _MaskColorB1, _MaskColorB2;
    float4 _MaskToonColorR1, _MaskToonColorR2, _MaskToonColorG1, _MaskToonColorG2, _MaskToonColorB1, _MaskToonColorB2;
    float _SpecularPower, _ToonStep, _ToonFeather, _EnvRate, _EnvBias;
    float _RimStep, _RimFeather, _RimSpecRate, _OutlineWidth, _Cutoff;
CBUFFER_END

TEXTURE2D(_MainTex);       SAMPLER(sampler_MainTex);
TEXTURE2D(_ToonMap);       SAMPLER(sampler_ToonMap);
TEXTURE2D(_TripleMaskMap);
TEXTURE2D(_MaskColorTex);

struct AttrToon { float4 positionOS : POSITION; float3 normalOS : NORMAL; float2 uv : TEXCOORD0; };
struct VaryToon { float4 positionHCS : SV_POSITION; float2 uv : TEXCOORD0; float3 nWS : TEXCOORD1; float3 vWS : TEXCOORD2; };

VaryToon ToonVert(AttrToon IN)
{
    VaryToon o;
    VertexPositionInputs p = GetVertexPositionInputs(IN.positionOS.xyz);
    o.positionHCS = p.positionCS;
    o.uv = TRANSFORM_TEX(IN.uv, _MainTex);
    o.nWS = TransformObjectToWorldNormal(IN.normalOS);
    o.vWS = GetWorldSpaceViewDir(p.positionWS);
    return o;
}

// Apply the 6-zone mask coloring: each mask channel c<0.5 selects color1, c>0.5 selects color2.
half3 ApplyZones(half3 col, half3 m, half4 R1, half4 R2, half4 G1, half4 G2, half4 B1, half4 B2)
{
    half3 lo = saturate((0.5 - min(m, 0.49)) * 2.0408);
    half3 hi = saturate((max(m, 0.51) - 0.51) * 2.0408);
    col *= lerp(half3(1,1,1), R1.rgb, lo.r);
    col *= lerp(half3(1,1,1), R2.rgb, hi.r);
    col *= lerp(half3(1,1,1), G1.rgb, lo.g);
    col *= lerp(half3(1,1,1), G2.rgb, hi.g);
    col *= lerp(half3(1,1,1), B1.rgb, lo.b);
    col *= lerp(half3(1,1,1), B2.rgb, hi.b);
    return col;
}

half4 ToonShade(VaryToon IN, bool doCutout)
{
    half4 baseCol = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
    half4 mask = SAMPLE_TEXTURE2D(_TripleMaskMap, sampler_MainTex, IN.uv);
    if (doCutout) clip(mask.b - _Cutoff);   // alpha test uses TripleMask.b vs _Cutoff (decompiled)

    float3 N = normalize(IN.nWS);
    float3 V = normalize(IN.vWS);
    Light L = GetMainLight();

    // Real Cygames toon model (from decompile): _ToonMap is the shadow-color texture
    // (UmaViewer binds *shad_c), sampled at the main UV. 6-zone recoloring tints the
    // lit diffuse by _MaskColor* and the shadow by _MaskToonColor* through _MaskColorTex
    // (RGB channels split low/high). All default white, so this is a no-op on baked
    // materials and applies on mask-colored costumes. Blend shadow<-lit by light angle.
    half3 shadowTex = SAMPLE_TEXTURE2D(_ToonMap, sampler_ToonMap, IN.uv).rgb;
    half3 area = SAMPLE_TEXTURE2D(_MaskColorTex, sampler_MainTex, IN.uv).rgb;
    half3 lit    = ApplyZones(baseCol.rgb, area, _MaskColorR1,_MaskColorR2,_MaskColorG1,_MaskColorG2,_MaskColorB1,_MaskColorB2);
    half3 shadow = ApplyZones(shadowTex,   area, _MaskToonColorR1,_MaskToonColorR2,_MaskToonColorG1,_MaskToonColorG2,_MaskToonColorB1,_MaskToonColorB2);
    float hl = dot(N, L.direction) * 0.5 + 0.5;
    float litAmt = saturate((hl - (_ToonStep - _ToonFeather)) / max(_ToonFeather * 2.0, 1e-4));
    // Cap the lit blend below 1 so fully-lit, front-facing surfaces (e.g. the pants)
    // still show the baked fold-shadows carried in the shadow texture.
    litAmt = min(litAmt, 0.82);
    half3 col = lerp(shadow, lit, litAmt) * L.color;

    // Rim omitted for now: the real rim is view/light-dependent and shadow-masked;
    // a naive fresnel drew white edges along every limb silhouette.
    return half4(col, baseCol.a);
}

// Unlit textured (eyes)
half4 UnlitShade(VaryToon IN)
{
    return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
}

// Screen-space-constant normal-extrude outline
struct AttrOL { float4 positionOS : POSITION; float3 normalOS : NORMAL; };
struct VaryOL { float4 positionHCS : SV_POSITION; };
VaryOL OutlineVert(AttrOL IN)
{
    VaryOL o;
    float3 posWS = TransformObjectToWorld(IN.positionOS.xyz);
    float3 normWS = TransformObjectToWorldNormal(IN.normalOS);
    float4 clip = TransformWorldToHClip(posWS);
    float3 normVS = mul((float3x3)UNITY_MATRIX_V, normWS);
    float2 offset = normalize(normVS.xy + 1e-5);
    clip.xy += offset * (_OutlineWidth * 0.004) * clip.w;
    o.positionHCS = clip;
    return o;
}
half4 OutlineFrag(VaryOL IN) : SV_Target { return half4(_OutlineColor.rgb, 1); }

#endif
