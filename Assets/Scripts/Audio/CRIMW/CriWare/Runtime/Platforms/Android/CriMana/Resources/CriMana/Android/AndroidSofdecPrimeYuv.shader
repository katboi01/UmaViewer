Shader "CriMana/AndroidSofdecPrimeYuv"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		[HideInInspector] _MovieTexture_ST ("MovieTexture_ST", Vector) = (1.0, 1.0, 0, 0)
		[HideInInspector] _MovieChromaTexture_ST("MovieChromaTexture_ST", Vector) = (1.0, 1.0, 0, 0)
		[HideInInspector] _MovieAlphaTexture_ST("MovieAlphaTexture_ST", Vector) = (1.0, 1.0, 0, 0)
		[HideInInspector] _TextureY ("TextureY", 2D) = "white" {}
		[HideInInspector] _TextureU ("TextureU", 2D) = "white" {}
		[HideInInspector] _TextureV ("TextureV", 2D) = "white" {}
		[HideInInspector] _TextureUV ("TextureUV", 2D) = "white" {}
		[HideInInspector] _TextureA ("TextureA", 2D) = "white" {}
		[HideInInspector] _SrcBlendMode ("SrcBlendMode", Int) = 0
		[HideInInspector] _DstBlendMode ("DstBlendMode", Int) = 0
		[HideInInspector] _CullMode("CullMode", Int) = 2
		[HideInInspector] _ZWriteMode("ZWriteMode", Int) = 1
	}

	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"PreviewType"="Plane"
		}

		Pass
		{
			Blend [_SrcBlendMode] [_DstBlendMode]
			Cull [_CullMode]
			ZWrite [_ZWriteMode]
			ZTest [unity_GUIZTestMode]

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#if defined(SHADER_API_PSP2) || defined(SHADER_API_PS3)
			// seems that ARB_precision_hint_fastest is not supported on these platforms.
			#else
			#pragma fragmentoption ARB_precision_hint_fastest
			#endif

			#include "UnityCG.cginc"

			#pragma multi_compile _ CRI_UV_FORMAT
			#pragma multi_compile _ CRI_ALPHA_MOVIE
			#pragma multi_compile _ CRI_APPLY_TARGET_ALPHA
			#pragma multi_compile _ CRI_LINEAR_COLORSPACE

			struct appdata
			{
				float4 vertex   : POSITION;
				half2  texcoord : TEXCOORD0;
#ifdef CRI_APPLY_TARGET_ALPHA
				float4 color    : COLOR;
#endif
            };

			struct v2f
			{
				float4   pos : SV_POSITION;
				half2     uv : TEXCOORD0;
				half2    uv2 : TEXCOORD1;
#ifdef CRI_ALPHA_MOVIE
				half2    uv3 : TEXCOORD2;
#endif
#ifdef CRI_APPLY_TARGET_ALPHA
				float4 color : COLOR;
#endif
			};

			float4 _MainTex_ST;
			float4 _MovieTexture_ST;
			float4 _MovieChromaTexture_ST;
#ifdef CRI_ALPHA_MOVIE
			float4 _MovieAlphaTexture_ST;
#endif

			v2f vert(appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv  = (TRANSFORM_TEX(v.texcoord, _MainTex) * _MovieTexture_ST.xy) + _MovieTexture_ST.zw;
				o.uv2  = (TRANSFORM_TEX(v.texcoord, _MainTex) * _MovieChromaTexture_ST.xy) + _MovieChromaTexture_ST.zw;
#ifdef CRI_ALPHA_MOVIE
				o.uv3  = (TRANSFORM_TEX(v.texcoord, _MainTex) * _MovieAlphaTexture_ST.xy) + _MovieAlphaTexture_ST.zw;
#endif
#ifdef CRI_APPLY_TARGET_ALPHA
				o.color = v.color;
#endif
				return o;
			}

			static const fixed3x3 yuv_to_rgb = {
				{1.16438,      0.0,  1.59603},
				{1.16438, -0.39176, -0.81297},
				{1.16438,  2.01723,      0.0}
				};

			sampler2D _TextureY;
#ifdef CRI_UV_FORMAT
			sampler2D _TextureUV;
#else
			sampler2D _TextureU;
			sampler2D _TextureV;
#endif
#ifdef CRI_ALPHA_MOVIE
			sampler2D _TextureA;
#endif
			fixed _Transparency;

			fixed4 frag(v2f i) : COLOR
			{
				fixed4 color;
				color.r = tex2D(_TextureY, i.uv).r;
#ifdef CRI_UV_FORMAT
				color.g = tex2D(_TextureUV, i.uv2).r;
				color.b = tex2D(_TextureUV, i.uv2).a;
#else
				color.g = tex2D(_TextureU, i.uv2).r;
				color.b = tex2D(_TextureV, i.uv2).r;
#endif
#ifdef CRI_ALPHA_MOVIE
				color.a = tex2D(_TextureA, i.uv3).r;
#else
				color.a = 1.0;
#endif
				color.rgb = color.rgb + fixed3(-0.06275, -0.50196, -0.50196);
				color.rgb = mul(yuv_to_rgb, color.rgb);
#ifdef CRI_LINEAR_COLORSPACE
				color.rgb = pow(max(color.rgb, 0), 2.2);
#endif
#ifdef CRI_APPLY_TARGET_ALPHA
				color.a = color.a * i.color.a;
#endif
				color.a *= 1 - _Transparency;

				return color;
			}
			ENDCG
		}
	}
}
