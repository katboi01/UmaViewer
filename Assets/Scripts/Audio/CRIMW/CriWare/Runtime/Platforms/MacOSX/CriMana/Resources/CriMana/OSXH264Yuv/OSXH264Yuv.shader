Shader "CriMana/OSXH264Yuv"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		[HideInInspector] _MovieTexture_ST ("MovieTexture_ST", Vector) = (1.0, 1.0, 0, 0)
		[HideInInspector] _TextureRGB("TextureRGB", 2D) = "white" {}
		[HideInInspector] _TextureA("TextureA", 2D) = "white" {}
		[HideInInspector] _SrcBlendMode("SrcBlendMode", Int) = 0
		[HideInInspector] _DstBlendMode("DstBlendMode", Int) = 0
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
			#pragma fragmentoption ARB_precision_hint_fastest

			#include "UnityCG.cginc"

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
#ifdef CRI_APPLY_TARGET_ALPHA
				float4 color : COLOR;
#endif
			};

			float4 _MainTex_ST;
			float4 _MovieTexture_ST;

			v2f vert(appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv  = (TRANSFORM_TEX(v.texcoord, _MainTex) * _MovieTexture_ST.xy) + _MovieTexture_ST.zw;
#ifdef CRI_APPLY_TARGET_ALPHA
				o.color = v.color;
#endif
				return o;
			}

			sampler2D _TextureRGB;
#ifdef CRI_ALPHA_MOVIE
			sampler2D _TextureA;
#endif
			fixed _Transparency;

			fixed4 frag(v2f i) : COLOR
			{
				fixed4 color = tex2D(_TextureRGB, i.uv);
#ifdef CRI_ALPHA_MOVIE
				fixed4 alpha = tex2D(_TextureA, i.uv);
				color.a = alpha.r;
#endif
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
