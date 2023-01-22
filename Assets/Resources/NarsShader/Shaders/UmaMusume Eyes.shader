// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Nars/UmaMusume/Eyes"
{
	Properties
	{
		[KeywordEnum(Eye1,Eye2,Eye3,Eye4)] _Styles("Styles", Float) = 0
		[NoScaleOffset]_MainTex("Base", 2D) = "white" {}
		[Toggle]_Switch1and2("Switch 1 and 2 ", Float) = 0
		_HighLightBrightness("HighLight Brightness", Float) = 1
		[KeywordEnum(Yes,No)] _HasHighlight("Has Highlight", Float) = 0
		[KeywordEnum(One,Two)] _NumberofHighLights("Number of HighLights", Float) = 1
		[NoScaleOffset]_High0Tex("HighLight One", 2D) = "white" {}
		[NoScaleOffset]_High1Tex("HighLight Two", 2D) = "white" {}
		_FallBackBrightness("FallBack Brightness", Range( 0 , 1)) = 0.6
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGPROGRAM
		#include "UnityPBSLighting.cginc"
		#include "UnityCG.cginc"
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma shader_feature _HASHIGHLIGHT_YES _HASHIGHLIGHT_NO
		#pragma shader_feature _NUMBEROFHIGHLIGHTS_ONE _NUMBEROFHIGHLIGHTS_TWO
		#pragma shader_feature _STYLES_EYE1 _STYLES_EYE2 _STYLES_EYE3 _STYLES_EYE4
		#pragma surface surf StandardCustomLighting keepalpha addshadow fullforwardshadows noshadow 
		struct Input
		{
			float3 worldPos;
			float2 uv_texcoord;
		};

		struct SurfaceOutputCustomLightingCustom
		{
			half3 Albedo;
			half3 Normal;
			half3 Emission;
			half Metallic;
			half Smoothness;
			half Occlusion;
			half Alpha;
			Input SurfInput;
			UnityGIInput GIData;
		};

		uniform float _FallBackBrightness;
		uniform float _HighLightBrightness;
		uniform sampler2D _MainTex;
		uniform sampler2D _High0Tex;
		uniform sampler2D _High1Tex;
		uniform float _Switch1and2;

		inline half4 LightingStandardCustomLighting( inout SurfaceOutputCustomLightingCustom s, half3 viewDir, UnityGI gi )
		{
			UnityGIInput data = s.GIData;
			Input i = s.SurfInput;
			half4 c = 0;
			#ifdef UNITY_PASS_FORWARDBASE
			float ase_lightAtten = data.atten;
			if( _LightColor0.a == 0)
			ase_lightAtten = 0;
			#else
			float3 ase_lightAttenRGB = gi.light.color / ( ( _LightColor0.rgb ) + 0.000001 );
			float ase_lightAtten = max( max( ase_lightAttenRGB.r, ase_lightAttenRGB.g ), ase_lightAttenRGB.b );
			#endif
			#if defined(HANDLE_SHADOWS_BLENDING_IN_GI)
			half bakedAtten = UnitySampleBakedOcclusion(data.lightmapUV.xy, data.worldPos);
			float zDist = dot(_WorldSpaceCameraPos - data.worldPos, UNITY_MATRIX_V[2].xyz);
			float fadeDist = UnityComputeShadowFadeDistance(data.worldPos, zDist);
			ase_lightAtten = UnityMixRealtimeAndBakedShadows(data.atten, bakedAtten, UnityComputeShadowFade(fadeDist));
			#endif
			float3 ase_worldPos = i.worldPos;
			#if defined(LIGHTMAP_ON) && UNITY_VERSION < 560 //aseld
			float3 ase_worldlightDir = 0;
			#else //aseld
			float3 ase_worldlightDir = Unity_SafeNormalize( UnityWorldSpaceLightDir( ase_worldPos ) );
			#endif //aseld
			#if defined(LIGHTMAP_ON) && ( UNITY_VERSION < 560 || ( defined(LIGHTMAP_SHADOW_MIXING) && !defined(SHADOWS_SHADOWMASK) && defined(SHADOWS_SCREEN) ) )//aselc
			float4 ase_lightColor = 0;
			#else //aselc
			float4 ase_lightColor = _LightColor0;
			#endif //aselc
			float clampResult7_g640 = clamp( ( ( abs( ase_worldlightDir ).x > float3( 0,0,0 ) ? 1.0 : 0.0 ) + ( ase_lightColor.a > 0.0 ? 1.0 : 0.0 ) ) , 0.0 , 1.0 );
			float4 color14_g640 = IsGammaSpace() ? float4(1,0,0,0) : float4(1,0,0,0);
			float4 color8_g640 = IsGammaSpace() ? float4(0,1,0,0) : float4(0,1,0,0);
			#ifdef UNITY_PASS_FORWARDADD
				float4 staticSwitch9_g640 = color8_g640;
			#else
				float4 staticSwitch9_g640 = ( clampResult7_g640 * color14_g640 );
			#endif
			float4 break10_g640 = staticSwitch9_g640;
			float temp_output_82_0 = break10_g640.r;
			float Isworldlight90 = temp_output_82_0;
			float4 temp_output_333_0 = ( ase_lightColor * ase_lightAtten );
			float4 temp_cast_1 = (_FallBackBrightness).xxxx;
			#ifdef UNITY_PASS_FORWARDADD
				float4 staticSwitch337 = max( float4( 0,0,0,0 ) , temp_output_333_0 );
			#else
				float4 staticSwitch337 =  (  ( 0.0 - 0.0 > 1.0 ? 0.0 : 0.0 - 0.0 <= 1.0 && 0.0 + 0.0 >= 1.0 ? 0.0 : Isworldlight90 )  - 0.0 > 0.0 ? temp_output_333_0 :  ( 0.0 - 0.0 > 1.0 ? 0.0 : 0.0 - 0.0 <= 1.0 && 0.0 + 0.0 >= 1.0 ? 0.0 : Isworldlight90 )  - 0.0 <= 0.0 &&  ( 0.0 - 0.0 > 1.0 ? 0.0 : 0.0 - 0.0 <= 1.0 && 0.0 + 0.0 >= 1.0 ? 0.0 : Isworldlight90 )  + 0.0 >= 0.0 ? temp_cast_1 : 0.0 ) ;
			#endif
			float4 Globnallightcolour338 = staticSwitch337;
			float4 temp_cast_2 = (_HighLightBrightness).xxxx;
			float2 uv_TexCoord353 = i.uv_texcoord * float2( 0.25,1 ) + float2( 0,0 );
			float2 uv_TexCoord367 = i.uv_texcoord * float2( 0.25,1 ) + float2( 0.25,0 );
			float2 uv_TexCoord370 = i.uv_texcoord * float2( 0.25,1 ) + float2( 0.5,0 );
			float2 uv_TexCoord373 = i.uv_texcoord * float2( 0.25,1 ) + float2( 0.75,0 );
			#if defined(_STYLES_EYE1)
				float2 staticSwitch360 = uv_TexCoord353;
			#elif defined(_STYLES_EYE2)
				float2 staticSwitch360 = uv_TexCoord367;
			#elif defined(_STYLES_EYE3)
				float2 staticSwitch360 = uv_TexCoord370;
			#elif defined(_STYLES_EYE4)
				float2 staticSwitch360 = uv_TexCoord373;
			#else
				float2 staticSwitch360 = uv_TexCoord353;
			#endif
			float4 tex2DNode62 = tex2Dlod( _MainTex, float4( staticSwitch360, 0, 0.0) );
			float2 uv_TexCoord388 = i.uv_texcoord * float2( 1,2 );
			float4 tex2DNode380 = tex2D( _High0Tex, uv_TexCoord388 );
			float4 tex2DNode385 = tex2D( _High1Tex, uv_TexCoord388 );
			float4 lerpResult381 = lerp( temp_cast_2 , tex2DNode62 , ( 1.0 - ( tex2DNode380 + tex2DNode385 ) ));
			float4 temp_cast_3 = (_HighLightBrightness).xxxx;
			float4 lerpResult392 = lerp( temp_cast_3 , tex2DNode62 , ( 1.0 - tex2DNode380 ));
			float4 temp_cast_4 = (_HighLightBrightness).xxxx;
			float4 lerpResult397 = lerp( temp_cast_4 , tex2DNode62 , ( 1.0 - tex2DNode385 ));
			#if defined(_NUMBEROFHIGHLIGHTS_ONE)
				float4 staticSwitch391 = (( _Switch1and2 )?( lerpResult397 ):( lerpResult392 ));
			#elif defined(_NUMBEROFHIGHLIGHTS_TWO)
				float4 staticSwitch391 = lerpResult381;
			#else
				float4 staticSwitch391 = lerpResult381;
			#endif
			#if defined(_HASHIGHLIGHT_YES)
				float4 staticSwitch390 = staticSwitch391;
			#elif defined(_HASHIGHLIGHT_NO)
				float4 staticSwitch390 = tex2DNode62;
			#else
				float4 staticSwitch390 = staticSwitch391;
			#endif
			float4 BaseTexture158 = staticSwitch390;
			float4 temp_output_351_0 = ( BaseTexture158 * BaseTexture158 );
			float4 lerpResult78 = lerp( temp_output_351_0 , BaseTexture158 , 0.26);
			float4 Shadow64 = lerpResult78;
			c.rgb = ( Globnallightcolour338 * Shadow64 ).rgb;
			c.a = 1;
			return c;
		}

		inline void LightingStandardCustomLighting_GI( inout SurfaceOutputCustomLightingCustom s, UnityGIInput data, inout UnityGI gi )
		{
			s.GIData = data;
		}

		void surf( Input i , inout SurfaceOutputCustomLightingCustom o )
		{
			o.SurfInput = i;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18900
185;132;1416;839;2777.321;-335.403;1;True;True
Node;AmplifyShaderEditor.CommentaryNode;362;-3584.533,-123.3915;Inherit;False;2063.248;1879.443;Comment;29;158;360;62;353;359;358;367;368;369;373;374;375;372;371;377;380;381;383;385;386;387;388;390;391;392;393;395;396;397;;1,1,1,1;0;0
Node;AmplifyShaderEditor.Vector2Node;387;-3525.779,-15.3995;Inherit;False;Constant;_Vector8;Vector 8;10;0;Create;True;0;0;0;False;0;False;1,2;0.75,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.Vector2Node;374;-3522.389,1173.064;Inherit;False;Constant;_Vector6;Vector 6;6;0;Create;True;0;0;0;False;0;False;0.25,1;0.25,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.Vector2Node;369;-3531.473,663.8401;Inherit;False;Constant;_Vector3;Vector 3;8;0;Create;True;0;0;0;False;0;False;0.25,0;0.25,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.Vector2Node;371;-3534.856,848.8988;Inherit;False;Constant;_Vector4;Vector 4;5;0;Create;True;0;0;0;False;0;False;0.25,1;0.25,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.Vector2Node;358;-3536,144;Inherit;False;Constant;_Vector0;Vector 0;3;0;Create;True;0;0;0;False;0;False;0.25,1;0.25,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.Vector2Node;375;-3527.076,1321.751;Inherit;False;Constant;_Vector7;Vector 7;10;0;Create;True;0;0;0;False;0;False;0.75,0;0.75,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.Vector2Node;368;-3543.7,474.7205;Inherit;False;Constant;_Vector2;Vector 2;4;0;Create;True;0;0;0;False;0;False;0.25,1;0.25,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.Vector2Node;359;-3540,272;Inherit;False;Constant;_Vector1;Vector 1;7;0;Create;True;0;0;0;False;0;False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.Vector2Node;372;-3518.856,976.8988;Inherit;False;Constant;_Vector5;Vector 5;9;0;Create;True;0;0;0;False;0;False;0.5,0;0.5,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.TextureCoordinatesNode;388;-3206.317,99.32188;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;353;-3301.188,254.1345;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;370;-3300.044,959.0333;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;373;-3287.577,1283.199;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;367;-3295.888,607.8551;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StaticSwitch;360;-2794.664,771.7491;Inherit;False;Property;_Styles;Styles;0;0;Create;True;0;0;0;False;0;False;0;0;0;True;;KeywordEnum;4;Eye1;Eye2;Eye3;Eye4;Create;False;True;9;1;FLOAT2;0,0;False;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT2;0,0;False;6;FLOAT2;0,0;False;7;FLOAT2;0,0;False;8;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;380;-2889.155,-40.23049;Inherit;True;Property;_High0Tex;HighLight One;6;1;[NoScaleOffset];Create;False;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;385;-2895.152,190.8158;Inherit;True;Property;_High1Tex;HighLight Two;7;1;[NoScaleOffset];Create;False;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;393;-2397.197,94.33471;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;62;-2481.591,651.8398;Inherit;True;Property;_MainTex;Base;1;1;[NoScaleOffset];Create;False;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;MipLevel;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;396;-2397.834,257.4658;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;386;-2540.903,149.5288;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;377;-2817.374,409.0841;Inherit;False;Property;_HighLightBrightness;HighLight Brightness;3;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;397;-2124.084,400.1891;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;392;-2138.381,-59.6656;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;383;-2396.313,175.7564;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;325;-1872,-992;Inherit;False;1347.069;756.9989;Comment;11;85;84;87;88;83;82;86;89;91;247;90;Lights;0.579394,1,0,1;0;0
Node;AmplifyShaderEditor.LerpOp;381;-2133.926,162.9593;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ToggleSwitchNode;395;-1809.888,71.73196;Inherit;False;Property;_Switch1and2;Switch 1 and 2 ;2;0;Create;True;0;0;0;False;0;False;0;True;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;82;-1824,-944;Inherit;False;Is_There_A_Light;-1;;640;98bba35e0adeb7a4db4bb32317e70d55;0;0;2;FLOAT;0;FLOAT;15
Node;AmplifyShaderEditor.StaticSwitch;391;-1812.175,271.3702;Inherit;False;Property;_NumberofHighLights;Number of HighLights;5;0;Create;True;0;0;0;False;0;False;0;1;1;True;;KeywordEnum;2;One;Two;Create;False;True;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;390;-1969.297,748.0529;Inherit;False;Property;_HasHighlight;Has Highlight;4;0;Create;True;0;0;0;False;0;False;0;0;0;True;;KeywordEnum;2;Yes;No;Create;False;True;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;327;-896,1168;Inherit;False;1617.594;695.9377;Comment;12;338;337;336;335;334;333;332;331;330;329;328;398;Light Autist;0.4860944,1,0,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;90;-1536,-944;Inherit;False;Isworldlight;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;158;-1744.723,799.8381;Inherit;False;BaseTexture;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;324;323.5219,-1788.889;Inherit;False;983.0802;394.0726;Comment;6;352;64;78;95;351;271;Shading;1,0,0,1;0;0
Node;AmplifyShaderEditor.LightColorNode;329;-864,1424;Inherit;False;0;3;COLOR;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.LightAttenuation;331;-864,1552;Inherit;False;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;328;-864,1232;Inherit;False;Constant;_LightColour;Light Colour;6;1;[Enum];Create;True;0;2;Automatic;0;Fake;1;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;330;-864,1328;Inherit;False;90;Isworldlight;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCIf;334;-592,1248;Inherit;False;6;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;333;-592,1440;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;398;-614.5943,1604.579;Inherit;False;Property;_FallBackBrightness;FallBack Brightness;8;0;Create;True;0;0;0;False;0;False;0.6;0.6;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;271;443.1941,-1666.888;Inherit;False;158;BaseTexture;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;95;432.1845,-1570.234;Inherit;False;Constant;_LightSmoothness;Light Smoothness;3;0;Create;True;0;0;0;False;0;False;0.26;0.4;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;335;-96,1552;Inherit;False;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;351;681.7751,-1703.547;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TFHCIf;336;-192,1360;Inherit;False;6;0;FLOAT;0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;337;64,1360;Inherit;False;Property;_Keyword0;Keyword 0;10;0;Create;True;0;0;0;False;0;False;0;0;0;False;UNITY_PASS_FORWARDADD;Toggle;2;Key0;Key1;Fetch;False;True;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;78;859.1939,-1628.888;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;326;-482,21.46355;Inherit;False;1237.712;1021.536;Comment;4;0;119;339;340;Outputs;0.981343,1,0,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;338;432,1360;Inherit;False;Globnallightcolour;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;64;1051.194,-1628.888;Inherit;False;Shadow;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;119;40.02758,390.2436;Inherit;False;64;Shadow;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;56;-1344,592;Inherit;False;794.4915;416.6398;Light ;4;55;53;52;58;ViewDIr;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;339;-23.97242,294.2436;Inherit;False;338;Globnallightcolour;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;87;-1824,-352;Inherit;False;Constant;_ViewDirY;ViewDir Y;8;0;Create;True;0;0;0;False;0;False;0;0;-200;200;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;352;929.0795,-1719.013;Inherit;False;EyeSSS;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;58;-800,736;Inherit;False;Viewdir_light;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;340;248.0276,310.2436;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;83;-1824,-736;Inherit;False;Constant;_FallbackLight;Fallback Light ;2;1;[Enum];Create;True;0;2;Automatic;0;Fake;1;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;53;-976,720;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;85;-1824,-640;Inherit;False;Constant;_FakeDirX;FakeDir X ;7;0;Create;True;0;0;0;False;0;False;0;0;-200;200;0;1;FLOAT;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;55;-1232,816;Inherit;False;World;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;88;-1824,-464;Inherit;False;Constant;_ViewDirX;ViewDir X;9;0;Create;True;0;0;0;False;0;False;0;0;-200;200;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;247;-1008,-656;Inherit;False;NDV;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;84;-1824,-816;Inherit;False;Constant;_LightDirection;Light Direction;2;1;[Enum];Create;True;0;2;Normal;0;ViewDirection;1;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;89;-1472,-688;Inherit;False;Dot_Creation;-1;;641;5dc6ab91272cf5948b2879e6e6e1078b;0;7;12;FLOAT;0;False;17;FLOAT;0;False;16;FLOAT;0;False;29;FLOAT;0;False;28;FLOAT;0;False;33;FLOAT;0;False;34;FLOAT;0;False;3;FLOAT;0;FLOAT;42;FLOAT;52
Node;AmplifyShaderEditor.ColorNode;332;-864,1648;Inherit;False;Constant;_FakeColourtint;Fake Colour tint;5;0;Create;True;0;0;0;False;0;False;1,1,1,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;91;-768,-800;Inherit;False;NDL;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;52;-1296,640;Inherit;False;True;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;86;-1824,-560;Inherit;False;Constant;_FakeDirY;FakeDir Y;6;0;Create;True;0;0;0;False;0;False;0;0;-200;200;0;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;492.7116,71.46355;Float;False;True;-1;2;ASEMaterialInspector;0;0;CustomLighting;Nars/UmaMusume/Eyes;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;388;0;387;0
WireConnection;353;0;358;0
WireConnection;353;1;359;0
WireConnection;370;0;371;0
WireConnection;370;1;372;0
WireConnection;373;0;374;0
WireConnection;373;1;375;0
WireConnection;367;0;368;0
WireConnection;367;1;369;0
WireConnection;360;1;353;0
WireConnection;360;0;367;0
WireConnection;360;2;370;0
WireConnection;360;3;373;0
WireConnection;380;1;388;0
WireConnection;385;1;388;0
WireConnection;393;0;380;0
WireConnection;62;1;360;0
WireConnection;396;0;385;0
WireConnection;386;0;380;0
WireConnection;386;1;385;0
WireConnection;397;0;377;0
WireConnection;397;1;62;0
WireConnection;397;2;396;0
WireConnection;392;0;377;0
WireConnection;392;1;62;0
WireConnection;392;2;393;0
WireConnection;383;0;386;0
WireConnection;381;0;377;0
WireConnection;381;1;62;0
WireConnection;381;2;383;0
WireConnection;395;0;392;0
WireConnection;395;1;397;0
WireConnection;391;1;395;0
WireConnection;391;0;381;0
WireConnection;390;1;391;0
WireConnection;390;0;62;0
WireConnection;90;0;82;0
WireConnection;158;0;390;0
WireConnection;334;0;328;0
WireConnection;334;4;330;0
WireConnection;333;0;329;0
WireConnection;333;1;331;0
WireConnection;335;1;333;0
WireConnection;351;0;271;0
WireConnection;351;1;271;0
WireConnection;336;0;334;0
WireConnection;336;2;333;0
WireConnection;336;3;398;0
WireConnection;337;1;336;0
WireConnection;337;0;335;0
WireConnection;78;0;351;0
WireConnection;78;1;271;0
WireConnection;78;2;95;0
WireConnection;338;0;337;0
WireConnection;64;0;78;0
WireConnection;352;0;351;0
WireConnection;58;0;53;0
WireConnection;340;0;339;0
WireConnection;340;1;119;0
WireConnection;53;0;52;0
WireConnection;53;1;55;0
WireConnection;247;0;89;42
WireConnection;89;12;82;0
WireConnection;89;17;84;0
WireConnection;89;16;83;0
WireConnection;89;29;85;0
WireConnection;89;28;86;0
WireConnection;91;0;89;0
WireConnection;0;13;340;0
ASEEND*/
//CHKSM=3963268F096DE90B785301DE9C805620CE6CB8D1