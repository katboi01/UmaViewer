// Metal re-authoring of Gallop/3D/Chara/MultiplyCheek (URP). The cheek blush is a multiply decal
// drawn over the face, not a toon-lit surface, so this is a small standalone pass: multiply the
// framebuffer by the cheek texture, gated by its alpha so untouched areas stay unchanged.
Shader "Gallop/3D/Chara/MultiplyCheek"
{
    Properties
    {
        _MainTex ("Cheek", 2D) = "white" { }
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Transparent" "Queue" = "Transparent" }

        Pass
        {
            Name "MultiplyCheek"
            Tags { "LightMode" = "UniversalForward" }
            Cull Back
            Blend DstColor Zero
            ZWrite Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
            CBUFFER_END

            struct Attr { float4 positionOS : POSITION; float2 uv : TEXCOORD0; };
            struct Vary { float4 positionHCS : SV_POSITION; float2 uv : TEXCOORD0; };

            Vary vert(Attr IN)
            {
                Vary o;
                o.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                o.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                return o;
            }

            half4 frag(Vary IN) : SV_Target
            {
                half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                // Blend DstColor Zero multiplies the framebuffer by this; alpha 0 -> white (no-op).
                return half4(lerp(half3(1,1,1), tex.rgb, tex.a), 1);
            }
            ENDHLSL
        }
    }
    Fallback Off
}
