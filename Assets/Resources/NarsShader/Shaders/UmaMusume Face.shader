// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Nars/UmaMusume/Face"
{
	Properties
	{
		[NoScaleOffset]_MainTex("Base", 2D) = "white" {}
		[NoScaleOffset]_ToonMap("SSS", 2D) = "white" {}
		[NoScaleOffset]_TripleMaskMap("HardShadows", 2D) = "white" {}
		_Outlinewidth("Outline width", Float) = 0
		_ShadowStrength("Shadow Strength", Range( 0 , 3)) = 3
		_ViewDirY("ViewDir Y", Range( -200 , 200)) = 0
		_ViewDirX("ViewDir X", Range( -200 , 200)) = 0
		_FallBackBrightness1("FallBack Brightness", Range( 0 , 1)) = 0.6
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ }
		Cull Front
		CGPROGRAM
		#pragma target 3.0
		#pragma surface outlineSurf Outline nofog  keepalpha noshadow noambient novertexlights nolightmap nodynlightmap nodirlightmap nometa noforwardadd vertex:outlineVertexDataFunc 
		
		void outlineVertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float outlineVar = ( _Outlinewidth * v.color * 0.001 ).r;
			v.vertex.xyz += ( v.normal * outlineVar );
		}
		inline half4 LightingOutline( SurfaceOutput s, half3 lightDir, half atten ) { return half4 ( 0,0,0, s.Alpha); }
		void outlineSurf( Input i, inout SurfaceOutput o )
		{
			float2 uv_MainTex62 = i.uv_texcoord;
			float4 BaseTexture158 = tex2Dlod( _MainTex, float4( uv_MainTex62, 0, 0.0) );
			float2 uv_ToonMap80 = i.uv_texcoord;
			float4 ShadowTexture147 = tex2Dlod( _ToonMap, float4( uv_ToonMap80, 0, 0.0) );
			float3 ase_worldPos = i.worldPos;
			#if defined(LIGHTMAP_ON) && UNITY_VERSION < 560 //aseld
			float3 ase_worldlightDir = 0;
			#else //aseld
			float3 ase_worldlightDir = normalize( UnityWorldSpaceLightDir( ase_worldPos ) );
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
			float4 temp_output_333_0 = ( ase_lightColor * 1 );
			float4 temp_cast_1 = (_FallBackBrightness1).xxxx;
			#ifdef UNITY_PASS_FORWARDADD
				float4 staticSwitch337 = max( float4( 0,0,0,0 ) , temp_output_333_0 );
			#else
				float4 staticSwitch337 =  (  ( 0.0 - 0.0 > 1.0 ? 0.0 : 0.0 - 0.0 <= 1.0 && 0.0 + 0.0 >= 1.0 ? 0.0 : Isworldlight90 )  - 0.0 > 0.0 ? temp_output_333_0 :  ( 0.0 - 0.0 > 1.0 ? 0.0 : 0.0 - 0.0 <= 1.0 && 0.0 + 0.0 >= 1.0 ? 0.0 : Isworldlight90 )  - 0.0 <= 0.0 &&  ( 0.0 - 0.0 > 1.0 ? 0.0 : 0.0 - 0.0 <= 1.0 && 0.0 + 0.0 >= 1.0 ? 0.0 : Isworldlight90 )  + 0.0 >= 0.0 ? temp_cast_1 : 0.0 ) ;
			#endif
			float4 Globnallightcolour338 = staticSwitch337;
			o.Emission = ( BaseTexture158 * ( ShadowTexture147 * 0.5 ) * Globnallightcolour338 ).rgb;
		}
		ENDCG
		

		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGINCLUDE
		#include "UnityPBSLighting.cginc"
		#include "UnityCG.cginc"
		#include "UnityShaderVariables.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		struct Input
		{
			float3 worldPos;
			float2 uv_texcoord;
			float3 worldNormal;
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

		uniform float _FallBackBrightness1;
		uniform sampler2D _ToonMap;
		uniform sampler2D _MainTex;
		uniform sampler2D _TripleMaskMap;
		uniform float _ViewDirX;
		uniform float _ViewDirY;
		uniform float _ShadowStrength;
		uniform float _Outlinewidth;


		float3 ViewMatrix0375_g643(  )
		{
			return UNITY_MATRIX_V[0];
		}


		float3 ViewMatrix1373_g643(  )
		{
			return UNITY_MATRIX_V[1];
		}


		float3 StereoCameraViewPosition30_g641(  )
		{
			#if UNITY_SINGLE_PASS_STEREO
			float3 cameraPos = float3((unity_StereoWorldSpaceCameraPos[0]+ unity_StereoWorldSpaceCameraPos[1])*.5); 
			#else
			float3 cameraPos = _WorldSpaceCameraPos;
			#endif
			return cameraPos;
		}


		float3 RotateAroundAxis( float3 center, float3 original, float3 u, float angle )
		{
			original -= center;
			float C = cos( angle );
			float S = sin( angle );
			float t = 1 - C;
			float m00 = t * u.x * u.x + C;
			float m01 = t * u.x * u.y - S * u.z;
			float m02 = t * u.x * u.z + S * u.y;
			float m10 = t * u.x * u.y + S * u.z;
			float m11 = t * u.y * u.y + C;
			float m12 = t * u.y * u.z - S * u.x;
			float m20 = t * u.x * u.z - S * u.y;
			float m21 = t * u.y * u.z + S * u.x;
			float m22 = t * u.z * u.z + C;
			float3x3 finalMatrix = float3x3( m00, m01, m02, m10, m11, m12, m20, m21, m22 );
			return mul( finalMatrix, original ) + center;
		}


		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			v.vertex.xyz += 0;
			v.vertex.w = 1;
		}

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
			float4 temp_cast_1 = (_FallBackBrightness1).xxxx;
			#ifdef UNITY_PASS_FORWARDADD
				float4 staticSwitch337 = max( float4( 0,0,0,0 ) , temp_output_333_0 );
			#else
				float4 staticSwitch337 =  (  ( 0.0 - 0.0 > 1.0 ? 0.0 : 0.0 - 0.0 <= 1.0 && 0.0 + 0.0 >= 1.0 ? 0.0 : Isworldlight90 )  - 0.0 > 0.0 ? temp_output_333_0 :  ( 0.0 - 0.0 > 1.0 ? 0.0 : 0.0 - 0.0 <= 1.0 && 0.0 + 0.0 >= 1.0 ? 0.0 : Isworldlight90 )  - 0.0 <= 0.0 &&  ( 0.0 - 0.0 > 1.0 ? 0.0 : 0.0 - 0.0 <= 1.0 && 0.0 + 0.0 >= 1.0 ? 0.0 : Isworldlight90 )  + 0.0 >= 0.0 ? temp_cast_1 : 0.0 ) ;
			#endif
			float4 Globnallightcolour338 = staticSwitch337;
			float2 uv_ToonMap80 = i.uv_texcoord;
			float4 ShadowTexture147 = tex2Dlod( _ToonMap, float4( uv_ToonMap80, 0, 0.0) );
			float2 uv_MainTex62 = i.uv_texcoord;
			float4 BaseTexture158 = tex2Dlod( _MainTex, float4( uv_MainTex62, 0, 0.0) );
			float2 uv_TripleMaskMap310 = i.uv_texcoord;
			float4 tex2DNode310 = tex2D( _TripleMaskMap, uv_TripleMaskMap310 );
			float3 localViewMatrix0375_g643 = ViewMatrix0375_g643();
			float3 normalizeResult384_g643 = normalize( localViewMatrix0375_g643 );
			float3 temp_output_380_0_g643 = ( float3( 0,0,0 ) + ase_worldPos );
			float3 localViewMatrix1373_g643 = ViewMatrix1373_g643();
			float3 normalizeResult376_g643 = normalize( localViewMatrix1373_g643 );
			float3 localStereoCameraViewPosition30_g641 = StereoCameraViewPosition30_g641();
			float3 rotatedValue385_g643 = RotateAroundAxis( temp_output_380_0_g643, localStereoCameraViewPosition30_g641, normalizeResult376_g643, radians( ( _ViewDirY * -1.0 ) ) );
			float3 rotatedValue387_g643 = RotateAroundAxis( temp_output_380_0_g643, rotatedValue385_g643, normalize( normalizeResult384_g643 ), radians( ( _ViewDirX * 1.0 ) ) );
			float3 normalizeResult389_g643 = normalize( ( rotatedValue387_g643 - temp_output_380_0_g643 ) );
			float3 normalizeResult38_g641 = normalize( normalizeResult389_g643 );
			float3 appendResult15_g642 = (float3(( cos( ( ( 0.0 / 180.0 ) * UNITY_PI ) ) * sin( ( ( 0.0 / 180.0 ) * UNITY_PI ) ) * -1.0 ) , sin( ( ( 0.0 / 180.0 ) * UNITY_PI ) ) , ( cos( ( ( 0.0 / 180.0 ) * UNITY_PI ) ) * cos( ( ( 0.0 / 180.0 ) * UNITY_PI ) ) * -1.0 )));
			float3 normalizeResult2_g642 = normalize( appendResult15_g642 );
			float3 normalizeResult26_g641 = normalize( normalizeResult2_g642 );
			float3 ifLocalVar3_g641 = 0;
			if( 1.0 > 0.0 )
				ifLocalVar3_g641 = normalizeResult38_g641;
			else if( 1.0 == 0.0 )
				ifLocalVar3_g641 = normalizeResult26_g641;
			float3 ifLocalVar8_g641 = 0;
			if( temp_output_82_0 > 0.0 )
				ifLocalVar8_g641 = ase_worldlightDir;
			else if( temp_output_82_0 == 0.0 )
				ifLocalVar8_g641 = ifLocalVar3_g641;
			float3 ifLocalVar9_g641 = 0;
			if( 1.0 > 0.0 )
				ifLocalVar9_g641 = ifLocalVar3_g641;
			else if( 1.0 == 0.0 )
				ifLocalVar9_g641 = ifLocalVar8_g641;
			float3 ase_worldNormal = i.worldNormal;
			float3 ase_normWorldNormal = normalize( ase_worldNormal );
			float dotResult46_g641 = dot( ifLocalVar9_g641 , ase_normWorldNormal );
			float NDL91 = dotResult46_g641;
			float smoothstepResult311 = smoothstep( 0.0 , ( tex2DNode310.g * ( 1.0 - NDL91 ) ) , ( ( NDL91 * _ShadowStrength ) * tex2DNode310.g ));
			float temp_output_3_0_g644 = ( smoothstepResult311 - 0.26 );
			float4 lerpResult78 = lerp( ShadowTexture147 , BaseTexture158 , saturate( ( temp_output_3_0_g644 / fwidth( temp_output_3_0_g644 ) ) ));
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
		CGPROGRAM
		#pragma surface surf StandardCustomLighting keepalpha fullforwardshadows noshadow vertex:vertexDataFunc 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float3 worldPos : TEXCOORD2;
				float3 worldNormal : TEXCOORD3;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				vertexDataFunc( v, customInputData );
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				o.worldNormal = worldNormal;
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				o.worldPos = worldPos;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xy;
				float3 worldPos = IN.worldPos;
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = IN.worldNormal;
				SurfaceOutputCustomLightingCustom o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputCustomLightingCustom, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18901
789;81;1121;389;2070.441;-1022.954;2.72116;True;False
Node;AmplifyShaderEditor.CommentaryNode;325;-1872,-992;Inherit;False;1347.069;756.9989;Comment;11;85;84;87;88;83;82;86;89;91;247;90;Lights;0.579394,1,0,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;88;-1824,-464;Inherit;False;Property;_ViewDirX;ViewDir X;6;0;Create;True;0;0;0;False;0;False;0;0;-200;200;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;84;-1824,-816;Inherit;False;Constant;_LightDirection;Light Direction;2;1;[Enum];Create;True;0;2;Normal;0;ViewDirection;1;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;83;-1824,-736;Inherit;False;Constant;_FallbackLight;Fallback Light ;2;1;[Enum];Create;True;0;2;Automatic;0;Fake;1;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;87;-1824,-352;Inherit;False;Property;_ViewDirY;ViewDir Y;5;0;Create;True;0;0;0;False;0;False;0;0;-200;200;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;82;-1824,-944;Inherit;False;Is_There_A_Light;-1;;640;692d888142f187d479d5e6f976e674b0;0;0;2;FLOAT;0;FLOAT;15
Node;AmplifyShaderEditor.RangedFloatNode;85;-1824,-640;Inherit;False;Constant;_FakeDirX;FakeDir X ;7;0;Create;True;0;0;0;False;0;False;0;0;-200;200;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;86;-1824,-560;Inherit;False;Constant;_FakeDirY;FakeDir Y;6;0;Create;True;0;0;0;False;0;False;0;0;-200;200;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;89;-1472,-688;Inherit;False;Dot_Creation;-1;;641;2e5876d4432839c4c9496c6ced2f9534;0;7;12;FLOAT;0;False;17;FLOAT;0;False;16;FLOAT;0;False;29;FLOAT;0;False;28;FLOAT;0;False;33;FLOAT;0;False;34;FLOAT;0;False;3;FLOAT;0;FLOAT;42;FLOAT;52
Node;AmplifyShaderEditor.RegisterLocalVarNode;91;-768,-800;Inherit;False;NDL;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;324;-464,-976;Inherit;False;1663.408;915.915;Comment;19;147;80;62;158;64;270;78;271;231;95;311;97;313;321;317;310;320;318;322;Shading;1,0,0,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;321;-352,-544;Inherit;False;91;NDL;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;313;-416,-816;Inherit;False;Property;_ShadowStrength;Shadow Strength;4;0;Create;True;0;0;0;False;0;False;3;0.182;0;3;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;90;-1536,-944;Inherit;False;Isworldlight;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;327;-896,1168;Inherit;False;1617.594;695.9377;Comment;12;338;337;336;335;334;333;332;331;330;329;328;341;Light Autist;0.4860944,1,0,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;97;-352,-896;Inherit;False;91;NDL;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;330;-864,1328;Inherit;False;90;Isworldlight;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;328;-864,1232;Inherit;False;Constant;_LightColour;Light Colour;6;1;[Enum];Create;True;0;2;Automatic;0;Fake;1;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;320;-128,-864;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;310;-288,-736;Inherit;True;Property;_TripleMaskMap;HardShadows;2;1;[NoScaleOffset];Create;True;0;0;0;False;0;False;-1;None;438fdeaa34f2adf4d9bda8aefeb312b7;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LightAttenuation;331;-864,1552;Inherit;False;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;317;-160,-544;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LightColorNode;329;-864,1424;Inherit;False;0;3;COLOR;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;322;48,-640;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;62;96,-528;Inherit;True;Property;_MainTex;Base;0;1;[NoScaleOffset];Create;True;0;0;0;False;0;False;-1;None;bf857c4f87097504aa2c360fa3bc2469;True;0;False;white;Auto;False;Object;-1;MipLevel;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;80;96,-320;Inherit;True;Property;_ToonMap;SSS;1;1;[NoScaleOffset];Create;True;0;0;0;False;0;False;-1;None;64cc5ff020b4ec843a1a5c5e1e239a4a;True;0;False;white;Auto;False;Object;-1;MipLevel;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;341;-566.3372,1674.242;Inherit;False;Property;_FallBackBrightness1;FallBack Brightness;7;0;Create;True;0;0;0;False;0;False;0.6;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCIf;334;-592,1248;Inherit;False;6;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;333;-592,1440;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;318;48,-752;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;158;416,-528;Inherit;False;BaseTexture;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;95;224,-800;Inherit;False;Constant;_LightSmoothness;Light Smoothness;3;0;Create;True;0;0;0;False;0;False;0.26;0.4;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;147;432,-320;Inherit;False;ShadowTexture;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SmoothstepOpNode;311;224,-720;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCIf;336;-192,1360;Inherit;False;6;0;FLOAT;0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;335;-96,1552;Inherit;False;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;270;448,-928;Inherit;False;147;ShadowTexture;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;337;64,1360;Inherit;False;Property;_Keyword0;Keyword 0;10;0;Create;True;0;0;0;False;0;False;0;0;0;False;UNITY_PASS_FORWARDADD;Toggle;2;Key0;Key1;Fetch;False;True;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;231;464,-752;Inherit;False;Step Antialiasing;-1;;644;2a825e80dfb3290468194f83380797bd;0;2;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;326;-482,21.46355;Inherit;False;1237.712;1021.536;Comment;14;136;0;143;166;148;156;159;165;137;139;145;119;339;340;Outputs;0.981343,1,0,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;271;464,-832;Inherit;False;158;BaseTexture;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;148;-432,496;Inherit;False;147;ShadowTexture;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;78;752,-816;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;165;-368,608;Inherit;False;Constant;_Float2;Float 2;10;0;Create;True;0;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;338;432,1360;Inherit;False;Globnallightcolour;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.VertexColorNode;139;-384,768;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;64;944,-816;Inherit;False;Shadow;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;145;-400,928;Inherit;False;Constant;_Float0;Float 0;13;0;Create;True;0;0;0;False;0;False;0.001;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;339;16,208;Inherit;False;338;Globnallightcolour;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;166;-144,496;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;159;-208,400;Inherit;False;158;BaseTexture;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;137;-384,688;Inherit;False;Property;_Outlinewidth;Outline width;3;0;Create;True;0;0;0;False;0;False;0;1.36;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;143;16,544;Inherit;False;3;3;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;56;-1344,592;Inherit;False;794.4915;416.6398;Light ;4;55;53;52;58;ViewDIr;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;119;80,304;Inherit;False;64;Shadow;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;156;16,416;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;332;-864,1648;Inherit;False;Constant;_FakeColourtint;Fake Colour tint;5;0;Create;True;0;0;0;False;0;False;1,1,1,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DotProductOpNode;53;-976,720;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;58;-800,736;Inherit;False;Viewdir_light;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;55;-1232,816;Inherit;False;World;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;340;288,224;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;52;-1296,640;Inherit;False;True;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RegisterLocalVarNode;247;-1008,-656;Inherit;False;NDV;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OutlineNode;136;175.8069,431.8843;Inherit;False;0;True;None;0;0;Front;3;0;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;492.7116,71.46355;Float;False;True;-1;2;ASEMaterialInspector;0;0;CustomLighting;Nars/UmaMusume/Face;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;89;12;82;0
WireConnection;89;17;84;0
WireConnection;89;16;83;0
WireConnection;89;29;85;0
WireConnection;89;28;86;0
WireConnection;89;33;88;0
WireConnection;89;34;87;0
WireConnection;91;0;89;0
WireConnection;90;0;82;0
WireConnection;320;0;97;0
WireConnection;320;1;313;0
WireConnection;317;0;321;0
WireConnection;322;0;310;2
WireConnection;322;1;317;0
WireConnection;334;0;328;0
WireConnection;334;4;330;0
WireConnection;333;0;329;0
WireConnection;333;1;331;0
WireConnection;318;0;320;0
WireConnection;318;1;310;2
WireConnection;158;0;62;0
WireConnection;147;0;80;0
WireConnection;311;0;318;0
WireConnection;311;2;322;0
WireConnection;336;0;334;0
WireConnection;336;2;333;0
WireConnection;336;3;341;0
WireConnection;335;1;333;0
WireConnection;337;1;336;0
WireConnection;337;0;335;0
WireConnection;231;1;95;0
WireConnection;231;2;311;0
WireConnection;78;0;270;0
WireConnection;78;1;271;0
WireConnection;78;2;231;0
WireConnection;338;0;337;0
WireConnection;64;0;78;0
WireConnection;166;0;148;0
WireConnection;166;1;165;0
WireConnection;143;0;137;0
WireConnection;143;1;139;0
WireConnection;143;2;145;0
WireConnection;156;0;159;0
WireConnection;156;1;166;0
WireConnection;156;2;339;0
WireConnection;53;0;52;0
WireConnection;53;1;55;0
WireConnection;58;0;53;0
WireConnection;340;0;339;0
WireConnection;340;1;119;0
WireConnection;247;0;89;42
WireConnection;136;0;156;0
WireConnection;136;1;143;0
WireConnection;0;13;340;0
WireConnection;0;11;136;0
ASEEND*/
//CHKSM=D4A0518E1D98A1249A7478DAD2593C6140E8E53A