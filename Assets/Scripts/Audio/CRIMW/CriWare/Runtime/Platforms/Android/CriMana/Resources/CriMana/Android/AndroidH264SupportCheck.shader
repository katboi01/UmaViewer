Shader "CriMana/AndroidH264DummySupportCheck"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
	}

	SubShader
	{
		Pass
		{
			ZTest Always
			Cull Off
			Zwrite Off
			Blend One Zero

			GLSLPROGRAM
			#version 100 // this will be converted to 300 es when using OpenGLES3
			#ifdef VERTEX
			attribute vec4 _glesVertex;
			void main()
			{
				highp vec4 tmpvar;
				tmpvar.w = 1.0;
				tmpvar.xyz = _glesVertex.xyz;
				gl_Position = (unity_MatrixVP * (unity_ObjectToWorld * tmpvar)); // UnityObjectToClipPos(*)
			}

			#endif
			#ifdef FRAGMENT
			#extension GL_OES_EGL_image_external : enable
			#extension GL_OES_EGL_image_external_essl3 : enable
			void main()
			{
#if (defined(SHADER_API_GLES3) && defined(GL_OES_EGL_image_external_essl3)) || defined(GL_OES_EGL_image_external)
				gl_FragData[0].x = gl_FragData[0].y = gl_FragData[0].z = 1.0f;
#else
				gl_FragData[0].x = gl_FragData[0].y = gl_FragData[0].z = 0.0f;
				Dummy code for shader compilation failure.
#endif
			}
			#endif
			ENDGLSL
		}
	}
}
