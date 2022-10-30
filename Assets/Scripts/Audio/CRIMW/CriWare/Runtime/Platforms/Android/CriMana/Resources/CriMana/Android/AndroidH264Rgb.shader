Shader "CriMana/AndroidH264Rgb"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		[HideInInspector] _MovieTexture_ST ("MovieTexture_ST", Vector) = (1.0, 1.0, 0, 0)
		[HideInInspector] _AlphaTexture_ST("AlphaTexture_ST", Vector) = (1.0, 1.0, 0, 0)
		[HideInInspector] _TextureRGB ("TextureRGB", 2D) = "white" {}
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

			GLSLPROGRAM
			#version 100 // this will be converted to 300 es when using OpenGLES3
			#pragma multi_compile _ CRI_ALPHA_MOVIE
			#pragma multi_compile _ CRI_APPLY_TARGET_ALPHA
			#pragma multi_compile _ CRI_LINEAR_COLORSPACE
			#ifdef VERTEX
			attribute vec4 _glesVertex;
			attribute vec4 _glesMultiTexCoord0;
			#ifdef CRI_APPLY_TARGET_ALPHA
			attribute vec4 _glesColor;
			varying lowp float alpha;
			#endif
			uniform highp vec4 _MainTex_ST;
			uniform highp vec4 _MovieTexture_ST;
			varying mediump vec2 xlv_TEXCOORD0;
			#ifdef CRI_ALPHA_MOVIE
			uniform highp vec4 _AlphaTexture_ST;
			varying mediump vec2 xlv_TEXCOORD1;
			#endif
			void main ()
			{
				highp vec4 tmpvar;
				tmpvar.w = 1.0;
				tmpvar.xyz = _glesVertex.xyz;
				gl_Position = (unity_MatrixVP * (unity_ObjectToWorld * tmpvar)); // UnityObjectToClipPos(*)
				xlv_TEXCOORD0 = (((_glesMultiTexCoord0.xy * _MainTex_ST.xy) + _MainTex_ST.zw) * _MovieTexture_ST.xy) + _MovieTexture_ST.zw;
			#ifdef 	CRI_ALPHA_MOVIE
				xlv_TEXCOORD1 = (((_glesMultiTexCoord0.xy * _MainTex_ST.xy) + _MainTex_ST.zw) * _AlphaTexture_ST.xy) + _AlphaTexture_ST.zw;
			#endif
			#ifdef CRI_APPLY_TARGET_ALPHA
				alpha = _glesColor.w;
			#endif
			}
			#endif
			#ifdef FRAGMENT
			#extension GL_OES_EGL_image_external : enable
			#extension GL_OES_EGL_image_external_essl3 : enable
			#if (defined(SHADER_API_GLES3) && defined(GL_OES_EGL_image_external_essl3)) || defined(GL_OES_EGL_image_external) && !defined(SHADER_API_GLCORE)
			uniform samplerExternalOES _TextureRGB;
			#else
			uniform sampler2D _TextureRGB;
			#endif
			varying mediump vec2 xlv_TEXCOORD0;
			uniform lowp float _Transparency;
			#ifdef 	CRI_ALPHA_MOVIE
			uniform sampler2D _TextureA;
			varying mediump vec2 xlv_TEXCOORD1;
			#endif
			#ifdef CRI_APPLY_TARGET_ALPHA
			varying lowp float alpha;
			#endif
			void main ()
			{
			#if (defined(SHADER_API_GLES3) && defined(GL_OES_EGL_image_external_essl3)) || defined(GL_OES_EGL_image_external) && !defined(SHADER_API_GLCORE)
				gl_FragData[0] = texture2D(_TextureRGB, xlv_TEXCOORD0);
			#else
				gl_FragData[0].x = gl_FragData[0].y = gl_FragData[0].z = 0.0f;
			#endif
			#ifdef CRI_LINEAR_COLORSPACE
				gl_FragData[0].x = pow(max(gl_FragData[0].x, 0.001), 2.2);
				gl_FragData[0].y = pow(max(gl_FragData[0].y, 0.001), 2.2);
				gl_FragData[0].z = pow(max(gl_FragData[0].z, 0.001), 2.2);
			#endif
			#ifdef 	CRI_ALPHA_MOVIE
				gl_FragData[0].w = texture2D(_TextureA, xlv_TEXCOORD1).x;
			#endif
			#ifdef CRI_APPLY_TARGET_ALPHA
				gl_FragData[0].w *= alpha;
			#endif
				gl_FragData[0].w *= 1.0 - _Transparency;
			}
			#endif
			ENDGLSL
		}
	}
}
