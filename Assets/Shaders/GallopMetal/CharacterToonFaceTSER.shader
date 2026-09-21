// Metal re-authoring of Gallop/3D/Chara/ToonFace/TSER (URP). Approximation via shared GallopMetalToon core.
Shader "Gallop/3D/Chara/ToonFace/TSER"
{
    Properties
    {
        _MainTex ("Diffuse Map", 2D) = "white" { }
        _TripleMaskMap ("_TripleMaskMap", 2D) = "white" { }
        _MaskColorTex ("_MaskColorTex", 2D) = "white" { }
        _ToonMap ("_ToonMap", 2D) = "white" { }
        _EnvMap ("_EnvMap", 2D) = "black" { }
        _SpecularColor ("_SpecularColor", Color) = (1,1,1,1)
        _SpecularPower ("_SpecularPower", Range(0,1)) = 0
        _ToonStep ("_ToonStep", Range(0,1)) = 0.5
        _ToonFeather ("_ToonFeather", Range(0.0001,1)) = 0.0001
        _EnvRate ("_EnvRate", Range(0,1)) = 0.5
        _EnvBias ("_EnvBias", Range(0,8)) = 1
        _RimStep ("_RimStep", Range(0,1)) = 0.5
        _RimFeather ("_RimFeather", Range(0.0001,1)) = 0.3
        _RimColor ("_RimColor", Color) = (1,1,1,0.3922)
        _RimColor2 ("_RimColor2", Color) = (1,1,1,0)
        _MaskColorR1 ("_MaskColorR1", Color) = (1,1,1,1)
        _MaskColorR2 ("_MaskColorR2", Color) = (1,1,1,1)
        _MaskColorG1 ("_MaskColorG1", Color) = (1,1,1,1)
        _MaskColorG2 ("_MaskColorG2", Color) = (1,1,1,1)
        _MaskColorB1 ("_MaskColorB1", Color) = (1,1,1,1)
        _MaskColorB2 ("_MaskColorB2", Color) = (1,1,1,1)
        _MaskToonColorR1 ("_MaskToonColorR1", Color) = (1,1,1,1)
        _MaskToonColorR2 ("_MaskToonColorR2", Color) = (1,1,1,1)
        _MaskToonColorG1 ("_MaskToonColorG1", Color) = (1,1,1,1)
        _MaskToonColorG2 ("_MaskToonColorG2", Color) = (1,1,1,1)
        _MaskToonColorB1 ("_MaskToonColorB1", Color) = (1,1,1,1)
        _MaskToonColorB2 ("_MaskToonColorB2", Color) = (1,1,1,1)
        _RimSpecRate ("_RimSpecRate", Range(0,1)) = 0
        _ToonBrightColor ("_ToonBrightColor", Color) = (1,1,1,0)
        _ToonDarkColor ("_ToonDarkColor", Color) = (1,1,1,0)
        _OutlineWidth ("_OutlineWidth", Range(0.01,5)) = 1
        _OutlineColor ("_OutlineColor", Color) = (0.125,0.047,0,0.196)
        _Cutoff ("_Cutoff", Range(0,1)) = 0.5
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Opaque" "Queue" = "Geometry" }

        Pass
        {
            Name "Toon"
            Tags { "LightMode" = "UniversalForward" }
            HLSLPROGRAM
            #pragma vertex ToonVert
            #pragma fragment frag
            #include "GallopMetalToon.hlsl"
            half4 frag(VaryToon IN) : SV_Target { return ToonShade(IN, false); }
            ENDHLSL
        }
        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "SRPDefaultUnlit" }
            Cull Front
            HLSLPROGRAM
            #pragma vertex OutlineVert
            #pragma fragment OutlineFrag
            #include "GallopMetalToon.hlsl"
            ENDHLSL
        }
    }
    Fallback Off
}
