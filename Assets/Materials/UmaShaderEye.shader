
Shader "Uma/Eye" {
    Properties {
        _MainTex ("Eye Diffuse (_eye0)", 2D) = "white" {}
        _Highlight00 ("Highlight #1 (_hi00)", 2D) = "black" {}
        _Highlight01 ("Highlight #2 (_hi01)", 2D) = "black" {}
        _Highlight02 ("Highlight #3 (_hi02)", 2D) = "black" {}
        _EyeSelect ("Eye Select", Int) = 0
        _Show00 ("Show #1", Range(0.01, 1)) = 0.35
        _Show01 ("Show #2", Range(0.01, 1)) = 0.35
    }
    SubShader {
        Tags {
            "RenderType"="Transparent"
            "VRCFallback"="ToonCutout"
        }

        Pass {
            Name "Forward"
            Tags {
                "LightMode"="ForwardBase"
            }
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "Lighting.cginc"

            // Standard diffuse
            uniform sampler2D _MainTex;
            uniform float4 _MainTex_ST;
            uniform sampler2D _Highlight00;
            uniform float4 _Highlight00_ST;
            uniform sampler2D _Highlight01;
            uniform float4 _Highlight01_ST;
            uniform sampler2D _Highlight02;
            uniform float4 _Highlight02_ST;

            int _EyeSelect;

            float _Show00;
            float _Show01;

            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos: SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
            };

            VertexOutput vert (VertexInput v) {
                VertexOutput o;
                o.uv0 = v.texcoord0;
                o.pos = UnityObjectToClipPos(v.vertex.xyz);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }

            float4 frag (VertexOutput i): COLOR {
                float2 eyeUv = float2(i.uv0.x + (_EyeSelect * 0.25), i.uv0.y);
                float4 diff = tex2D(_MainTex, TRANSFORM_TEX(eyeUv, _MainTex));
                float2 altUv = float2(i.uv0.x * 4, i.uv0.y * 2);
                float4 hi00 = tex2D(_Highlight00, TRANSFORM_TEX(altUv, _Highlight00));
                float4 hi01 = tex2D(_Highlight01, TRANSFORM_TEX(altUv, _Highlight01));
                float4 hi02 = tex2D(_Highlight02, TRANSFORM_TEX(altUv, _Highlight02));

                float4 hl00 = step(_Show00, hi00);
                float4 hl01 = step(_Show01, hi01);

                float3 lightColor = saturate(max(_LightColor0, max(ShadeSH9(half4(0, 0, 0, 1)).rgb, ShadeSH9(half4(0, -1, 0, 1)).rgb)));
                return (diff + hl00 + hl01) * fixed4(lightColor, 1);
            }

            ENDCG
        }

        Pass {
            Name "ShadowCaster"
            Tags {
                "LightMode"="ShadowCaster"
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_SHADOWCASTER
            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                V2F_SHADOW_CASTER;
                float2 uv0 : TEXCOORD1;
            };

            VertexOutput vert (VertexInput v) {
                VertexOutput o;
                o.uv0 = v.texcoord0;
                o.pos = UnityObjectToClipPos(v.vertex);
                TRANSFER_SHADOW_CASTER(o);
                return o;
            }

            float4 frag(VertexOutput i): COLOR {
                SHADOW_CASTER_FRAGMENT(i);
            }

            ENDCG
        }
    }
    FallBack "Diffuse"
}