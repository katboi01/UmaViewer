// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Nars/UmaMusume/Body"
{
	Properties
	{
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		[NoScaleOffset]_MainTex("Base", 2D) = "white" {}
		[NoScaleOffset]_ToonMap("SSS", 2D) = "white" {}
		[NoScaleOffset]_TripleMaskMap("HardShadows", 2D) = "white" {}
		[NoScaleOffset]_OptionMaskMap("HardShadows2", 2D) = "white" {}
		_Outlinewidth("Outline width", Float) = 0
		_HighlightColor("Highlight Color", Color) = (1,1,1,0)
		_HighlightBrightness("Highlight Brightness", Range( 0 , 1)) = 0.4
		_SpecularSmoothness("Specular Smoothness", Float) = 1.03
		_ViewDirY("ViewDir Y", Range( -200 , 200)) = 0
		_Float2("Float 2", Float) = 0.5
		_SpecularBrightness("Specular Brightness", Range( -30 , 30)) = 30
		_ViewDirX("ViewDir X", Range( -200 , 200)) = 0
		_SpecSize("Spec Size", Range( 0 , 10)) = 5
		_FallBackBrightness("FallBack Brightness", Range( 0 , 1)) = 0.6
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "AlphaTest+0"}
		Cull Back
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
			float4 temp_output_288_0 = ( ase_lightColor * 1 );
			float4 temp_cast_1 = (_FallBackBrightness).xxxx;
			#ifdef UNITY_PASS_FORWARDADD
				float4 staticSwitch292 = max( float4( 0,0,0,0 ) , temp_output_288_0 );
			#else
				float4 staticSwitch292 =  (  ( 0.0 - 0.0 > 1.0 ? 0.0 : 0.0 - 0.0 <= 1.0 && 0.0 + 0.0 >= 1.0 ? 0.0 : Isworldlight90 )  - 0.0 > 0.0 ? temp_output_288_0 :  ( 0.0 - 0.0 > 1.0 ? 0.0 : 0.0 - 0.0 <= 1.0 && 0.0 + 0.0 >= 1.0 ? 0.0 : Isworldlight90 )  - 0.0 <= 0.0 &&  ( 0.0 - 0.0 > 1.0 ? 0.0 : 0.0 - 0.0 <= 1.0 && 0.0 + 0.0 >= 1.0 ? 0.0 : Isworldlight90 )  + 0.0 >= 0.0 ? temp_cast_1 : 0.0 ) ;
			#endif
			float4 Globnallightcolour293 = staticSwitch292;
			float4 color322 = IsGammaSpace() ? float4(0.7075472,0,0,0) : float4(0.4588115,0,0,0);
			float2 uv_TripleMaskMap102 = i.uv_texcoord;
			float4 tex2DNode102 = tex2Dlod( _TripleMaskMap, float4( uv_TripleMaskMap102, 0, 0.0) );
			float4 Transparency315 = tex2DNode102;
			float4 color326 = IsGammaSpace() ? float4(0.8018868,0.8018868,0.8018868,0) : float4(0.6070304,0.6070304,0.6070304,0);
			float Opacity341 = ( saturate( ( 1.0 - ( ( distance( ( 1.0 - color322 ).rgb , Transparency315.rgb ) - 0.63 ) / max( 1.25 , 1E-05 ) ) ) ) * saturate( ( 1.0 - ( ( distance( ( 1.0 - color326 ).rgb , Transparency315.rgb ) - 1.52 ) / max( -2.44 , 1E-05 ) ) ) ) );
			o.Emission = ( BaseTexture158 * ( ShadowTexture147 * _Float2 ) * Globnallightcolour293 ).rgb;
			clip( Opacity341 - _Cutoff );
		}
		ENDCG
		

		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "Transparent+0" }
		Cull Back
		CGINCLUDE
		#include "UnityPBSLighting.cginc"
		#include "UnityCG.cginc"
		#include "UnityShaderVariables.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		struct Input
		{
			float2 uv_texcoord;
			float3 worldPos;
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

		uniform sampler2D _TripleMaskMap;
		uniform sampler2D _ToonMap;
		uniform sampler2D _MainTex;
		uniform float _HighlightBrightness;
		uniform float4 _HighlightColor;
		uniform float _ViewDirX;
		uniform float _ViewDirY;
		uniform sampler2D _OptionMaskMap;
		uniform float _SpecularBrightness;
		uniform float _SpecularSmoothness;
		uniform float _SpecSize;
		uniform float _FallBackBrightness;
		uniform float _Cutoff = 0.5;
		uniform float _Float2;
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


		float3 StereoCameraViewDirection32_g641( float3 worldPos )
		{
			#if UNITY_SINGLE_PASS_STEREO
			float3 cameraPos = float3((unity_StereoWorldSpaceCameraPos[0]+ unity_StereoWorldSpaceCameraPos[1])*.5); 
			#else
			float3 cameraPos = _WorldSpaceCameraPos;
			#endif
			float3 worldViewDir = normalize((cameraPos - worldPos));
			return worldViewDir;
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
			float4 color322 = IsGammaSpace() ? float4(0.7075472,0,0,0) : float4(0.4588115,0,0,0);
			float2 uv_TripleMaskMap102 = i.uv_texcoord;
			float4 tex2DNode102 = tex2Dlod( _TripleMaskMap, float4( uv_TripleMaskMap102, 0, 0.0) );
			float4 Transparency315 = tex2DNode102;
			float4 color326 = IsGammaSpace() ? float4(0.8018868,0.8018868,0.8018868,0) : float4(0.6070304,0.6070304,0.6070304,0);
			float Opacity341 = ( saturate( ( 1.0 - ( ( distance( ( 1.0 - color322 ).rgb , Transparency315.rgb ) - 0.63 ) / max( 1.25 , 1E-05 ) ) ) ) * saturate( ( 1.0 - ( ( distance( ( 1.0 - color326 ).rgb , Transparency315.rgb ) - 1.52 ) / max( -2.44 , 1E-05 ) ) ) ) );
			float2 uv_ToonMap80 = i.uv_texcoord;
			float4 ShadowTexture147 = tex2Dlod( _ToonMap, float4( uv_ToonMap80, 0, 0.0) );
			float HD2303 = tex2DNode102.g;
			float2 uv_MainTex62 = i.uv_texcoord;
			float4 BaseTexture158 = tex2Dlod( _MainTex, float4( uv_MainTex62, 0, 0.0) );
			float3 localViewMatrix0375_g643 = ViewMatrix0375_g643();
			float3 normalizeResult384_g643 = normalize( localViewMatrix0375_g643 );
			float3 ase_worldPos = i.worldPos;
			float3 temp_output_380_0_g643 = ( float3( 0,0,0 ) + ase_worldPos );
			float3 localViewMatrix1373_g643 = ViewMatrix1373_g643();
			float3 normalizeResult376_g643 = normalize( localViewMatrix1373_g643 );
			float3 localStereoCameraViewPosition30_g641 = StereoCameraViewPosition30_g641();
			float3 rotatedValue385_g643 = RotateAroundAxis( temp_output_380_0_g643, localStereoCameraViewPosition30_g641, normalizeResult376_g643, radians( ( _ViewDirY * -1.0 ) ) );
			float3 rotatedValue387_g643 = RotateAroundAxis( temp_output_380_0_g643, rotatedValue385_g643, normalize( normalizeResult384_g643 ), radians( ( _ViewDirX * 1.0 ) ) );
			float3 normalizeResult389_g643 = normalize( ( rotatedValue387_g643 - temp_output_380_0_g643 ) );
			float3 normalizeResult38_g641 = normalize( normalizeResult389_g643 );
			float3 appendResult15_g642 = (float3(( cos( ( ( 122.1598 / 180.0 ) * UNITY_PI ) ) * sin( ( ( 0.0 / 180.0 ) * UNITY_PI ) ) * -1.0 ) , sin( ( ( 122.1598 / 180.0 ) * UNITY_PI ) ) , ( cos( ( ( 122.1598 / 180.0 ) * UNITY_PI ) ) * cos( ( ( 0.0 / 180.0 ) * UNITY_PI ) ) * -1.0 )));
			float3 normalizeResult2_g642 = normalize( appendResult15_g642 );
			float3 normalizeResult26_g641 = normalize( normalizeResult2_g642 );
			float3 ifLocalVar3_g641 = 0;
			if( 1.0 > 0.0 )
				ifLocalVar3_g641 = normalizeResult38_g641;
			else if( 1.0 == 0.0 )
				ifLocalVar3_g641 = normalizeResult26_g641;
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
			float temp_output_3_0_g644 = ( 0.4 - ( NDL91 * tex2DNode102 ).r );
			float4 lerpResult78 = lerp( ( ShadowTexture147 + ( ShadowTexture147 * ShadowTexture147 * 0.0 ) + ( HD2303 * 0.21 ) ) , ( BaseTexture158 + ( BaseTexture158 * HD2303 * _HighlightBrightness * _HighlightColor ) ) , ( 1.0 - saturate( ( temp_output_3_0_g644 / fwidth( temp_output_3_0_g644 ) ) ) ));
			float2 uv_OptionMaskMap131 = i.uv_texcoord;
			float4 tex2DNode131 = tex2Dlod( _OptionMaskMap, float4( uv_OptionMaskMap131, 0, 0.0) );
			float3 worldPos32_g641 = ase_worldPos;
			float3 localStereoCameraViewDirection32_g641 = StereoCameraViewDirection32_g641( worldPos32_g641 );
			float dotResult31_g641 = dot( ase_normWorldNormal , localStereoCameraViewDirection32_g641 );
			float NDV247 = dotResult31_g641;
			float smoothstepResult260 = smoothstep( 0.0 , _SpecularSmoothness , ( ( _SpecSize * 0.1 ) <= ( NDL91 * NDV247 ) ? 1.0 : 0.0 ));
			float4 Shadow64 = ( lerpResult78 + ( ( ShadowTexture147 * tex2DNode131.g * tex2DNode131.b * _SpecularBrightness * BaseTexture158 ) * smoothstepResult260 * ShadowTexture147 * BaseTexture158 ) );
			float Isworldlight90 = temp_output_82_0;
			float4 temp_output_288_0 = ( ase_lightColor * ase_lightAtten );
			float4 temp_cast_6 = (_FallBackBrightness).xxxx;
			#ifdef UNITY_PASS_FORWARDADD
				float4 staticSwitch292 = max( float4( 0,0,0,0 ) , temp_output_288_0 );
			#else
				float4 staticSwitch292 =  (  ( 0.0 - 0.0 > 1.0 ? 0.0 : 0.0 - 0.0 <= 1.0 && 0.0 + 0.0 >= 1.0 ? 0.0 : Isworldlight90 )  - 0.0 > 0.0 ? temp_output_288_0 :  ( 0.0 - 0.0 > 1.0 ? 0.0 : 0.0 - 0.0 <= 1.0 && 0.0 + 0.0 >= 1.0 ? 0.0 : Isworldlight90 )  - 0.0 <= 0.0 &&  ( 0.0 - 0.0 > 1.0 ? 0.0 : 0.0 - 0.0 <= 1.0 && 0.0 + 0.0 >= 1.0 ? 0.0 : Isworldlight90 )  + 0.0 >= 0.0 ? temp_cast_6 : 0.0 ) ;
			#endif
			float4 Globnallightcolour293 = staticSwitch292;
			c.rgb = ( Shadow64 * Globnallightcolour293 ).rgb;
			c.a = 1;
			clip( Opacity341 - _Cutoff );
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
				UnityGI gi;
				UNITY_INITIALIZE_OUTPUT( UnityGI, gi );
				o.Alpha = LightingStandardCustomLighting( o, worldViewDir, gi ).a;
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
789;81;1121;389;2993.236;-319.876;1.3;True;False
Node;AmplifyShaderEditor.CommentaryNode;294;-5680,1744;Inherit;False;1347.069;756.9989;Comment;11;86;84;87;83;82;85;88;89;247;91;90;Light;0,0.6940067,0.7264151,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;83;-5632,2000;Inherit;False;Constant;_FallbackLight;Fallback Light ;3;1;[Enum];Create;True;0;2;Automatic;0;Fake;1;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;84;-5632,1920;Inherit;False;Constant;_LightDirection;Light Direction;2;1;[Enum];Create;True;0;2;Normal;0;ViewDirection;1;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;87;-5632,2384;Inherit;False;Property;_ViewDirY;ViewDir Y;9;0;Create;True;0;0;0;False;0;False;0;0;-200;200;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;85;-5632,2096;Inherit;False;Constant;_FakeDirX;FakeDir X ;7;0;Create;True;0;0;0;False;0;False;122.1598;0;-200;200;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;88;-5632,2272;Inherit;False;Property;_ViewDirX;ViewDir X;13;0;Create;True;0;0;0;False;0;False;0;0;-200;200;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;82;-5632,1792;Inherit;False;Is_There_A_Light;-1;;640;692d888142f187d479d5e6f976e674b0;0;0;2;FLOAT;0;FLOAT;15
Node;AmplifyShaderEditor.RangedFloatNode;86;-5632,2176;Inherit;False;Constant;_FakeDirY;FakeDir Y;6;0;Create;True;0;0;0;False;0;False;0;0;-200;200;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;306;-5728,16;Inherit;False;3116.3;1634.136;Comment;48;64;62;264;267;252;257;263;266;158;255;249;261;243;301;131;242;295;260;296;245;244;78;250;253;302;298;297;233;304;239;305;299;300;232;303;95;231;97;102;227;80;147;309;308;312;311;314;315;Shading;1,0,0,1;0;0
Node;AmplifyShaderEditor.FunctionNode;89;-5264,2048;Inherit;False;Dot_Creation;-1;;641;2e5876d4432839c4c9496c6ced2f9534;0;7;12;FLOAT;0;False;17;FLOAT;0;False;16;FLOAT;0;False;29;FLOAT;0;False;28;FLOAT;0;False;33;FLOAT;0;False;34;FLOAT;0;False;3;FLOAT;0;FLOAT;42;FLOAT;52
Node;AmplifyShaderEditor.SamplerNode;62;-5744,80;Inherit;True;Property;_MainTex;Base;1;1;[NoScaleOffset];Create;True;0;0;0;False;0;False;-1;None;23368b2b15c396e4285427c2d21f4338;True;0;False;white;Auto;False;Object;-1;MipLevel;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;102;-5583,639;Inherit;True;Property;_TripleMaskMap;HardShadows;3;1;[NoScaleOffset];Create;True;0;0;0;False;0;False;-1;None;05845c9e92dcedd4f9a5b7ad5d707ddd;True;0;False;white;Auto;False;Object;-1;MipLevel;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;80;-5728,304;Inherit;True;Property;_ToonMap;SSS;2;1;[NoScaleOffset];Create;True;0;0;0;False;0;False;-1;None;7ca0e4ead13dcff47854babd60c93998;True;0;False;white;Auto;False;Object;-1;MipLevel;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;91;-4576,1936;Inherit;False;NDL;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;247;-4816,2080;Inherit;False;NDV;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;264;-5255.775,1223.871;Inherit;False;Constant;_Float4;Float 4;9;0;Create;True;0;0;0;False;0;False;0.1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;282;-2448,1136;Inherit;False;1617.594;695.9377;Comment;12;293;292;291;290;289;288;287;286;285;284;283;347;Light Autist;0.4860944,1,0,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;158;-5360,80;Inherit;False;BaseTexture;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;252;-5434.129,1403.161;Inherit;False;91;NDL;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;257;-5357.046,1129.836;Inherit;False;Property;_SpecSize;Spec Size;14;0;Create;True;0;0;0;False;0;False;5;5;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;303;-5072,736;Inherit;False;HD2;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;267;-5423.715,1481.274;Inherit;False;247;NDV;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;147;-5344,288;Inherit;False;ShadowTexture;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;97;-5264,528;Inherit;False;91;NDL;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;90;-5344,1792;Inherit;False;Isworldlight;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;285;-2416,1296;Inherit;False;90;Isworldlight;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;266;-5143.542,1333.11;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;345;-2904.398,-1000.279;Inherit;False;1577.253;711.3821;Comment;14;330;323;321;325;324;331;334;328;320;329;333;341;322;326;Opacity;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;311;-4704,560;Inherit;False;Constant;_Float5;Float 5;9;0;Create;True;0;0;0;False;0;False;0.21;0.21;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;314;-4570.162,773.2895;Inherit;False;Property;_HighlightColor;Highlight Color;6;0;Create;True;0;0;0;False;0;False;1,1,1,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;299;-4737.536,295.9054;Inherit;False;147;ShadowTexture;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;283;-2416,1200;Inherit;False;Constant;_LightColour;Light Colour;11;1;[Enum];Create;True;0;2;Automatic;0;Fake;1;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;227;-4984,546;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;95;-5200,832;Inherit;False;Constant;_LightSmoothness;Light Smoothness;3;0;Create;True;0;0;0;False;0;False;0.4;0.4;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;309;-4717.588,491.2482;Inherit;False;303;HD2;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;263;-5086.575,1217.461;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;298;-4304,544;Inherit;False;158;BaseTexture;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;304;-4415.32,622.4467;Inherit;False;303;HD2;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;305;-4736,368;Inherit;False;147;ShadowTexture;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;308;-4544,448;Inherit;False;Constant;_Float3;Float 3;9;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LightAttenuation;286;-2416,1520;Inherit;False;0;1;FLOAT;0
Node;AmplifyShaderEditor.LightColorNode;284;-2416,1392;Inherit;False;0;3;COLOR;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.RangedFloatNode;239;-4704.968,646.7232;Inherit;False;Property;_HighlightBrightness;Highlight Brightness;7;0;Create;True;0;0;0;False;0;False;0.4;0.4;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCIf;289;-2142,1216;Inherit;False;6;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;242;-4304,336;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;312;-4304,448;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;315;-5251.789,658.5535;Inherit;False;Transparency;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;322;-2854.398,-933.0788;Inherit;False;Constant;_Color0;Color 0;11;0;Create;True;0;0;0;False;0;False;0.7075472,0,0,0;0.7075472,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;231;-4800,736;Inherit;False;Step Antialiasing;-1;;644;2a825e80dfb3290468194f83380797bd;0;2;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;261;-4743.385,1417.633;Inherit;False;Property;_SpecularSmoothness;Specular Smoothness;8;0;Create;True;0;0;0;False;0;False;1.03;1.03;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;243;-4048,624;Inherit;False;4;4;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;326;-2850.286,-623.8107;Inherit;False;Constant;_Color1;Color 1;11;0;Create;True;0;0;0;False;0;False;0.8018868,0.8018868,0.8018868,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;301;-3712,880;Inherit;False;147;ShadowTexture;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;288;-2073,1413;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;300;-4297.575,257.7716;Inherit;False;147;ShadowTexture;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;295;-3809.152,1180.614;Inherit;False;158;BaseTexture;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.Compare;255;-4823.385,1241.633;Inherit;False;5;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;131;-4222,929;Inherit;True;Property;_OptionMaskMap;HardShadows2;4;1;[NoScaleOffset];Create;True;0;0;0;False;0;False;-1;None;1d36ccfaa822c2b4c923d52d507f65fe;True;0;False;white;Auto;False;Object;-1;MipLevel;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;347;-2113.057,1561.396;Inherit;False;Property;_FallBackBrightness;FallBack Brightness;15;0;Create;True;0;0;0;False;0;False;0.6;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;249;-3811,1043;Inherit;False;Property;_SpecularBrightness;Specular Brightness;12;0;Create;True;0;0;0;False;0;False;-30;1;-30;30;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;297;-4048,544;Inherit;False;158;BaseTexture;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;245;-3349.949,914.2349;Inherit;False;5;5;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;232;-3744,752;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;302;-3347.84,1143.388;Inherit;False;147;ShadowTexture;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;321;-2579.898,-950.2789;Inherit;False;315;Transparency;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;296;-3327.845,1233.864;Inherit;False;158;BaseTexture;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;330;-2512.101,-403.8968;Inherit;False;Constant;_Float8;Float 8;13;0;Create;True;0;0;0;False;0;False;-2.44;-2.44;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;334;-2570.346,-566.2331;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;325;-2516.213,-713.1649;Inherit;False;Constant;_Float7;Float 7;14;0;Create;True;0;0;0;False;0;False;1.25;1.25;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;323;-2595.615,-848.6649;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;331;-2508.301,-471.7968;Inherit;False;Constant;_Float9;Float 9;15;0;Create;True;0;0;0;False;0;False;1.52;1.52;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;328;-2575.786,-641.0108;Inherit;False;315;Transparency;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SmoothstepOpNode;260;-4322.978,1174.258;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;233;-3968,416;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;244;-3712,640;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TFHCIf;291;-1744,1328;Inherit;False;6;0;FLOAT;0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;324;-2512.413,-781.0649;Inherit;False;Constant;_Float6;Float 6;12;0;Create;True;0;0;0;False;0;False;0.63;0.63;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;290;-1648,1536;Inherit;False;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;281;-2512,16;Inherit;False;1281.189;1021.536;Comment;16;148;0;143;165;137;166;139;145;159;136;119;156;117;280;343;344;Outputs;0.2077243,1,0,1;0;0
Node;AmplifyShaderEditor.LerpOp;78;-3472,688;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;320;-2228.897,-853.2789;Inherit;False;Color Mask;-1;;647;eec747d987850564c95bde0e5a6d1867;0;4;1;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;5;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;292;-1488,1328;Inherit;False;Property;_Keyword0;Keyword 0;10;0;Create;True;0;0;0;False;0;False;0;0;0;False;UNITY_PASS_FORWARDADD;Toggle;2;Key0;Key1;Fetch;False;True;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;250;-3072,1008;Inherit;False;4;4;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;329;-2224.785,-544.0107;Inherit;False;Color Mask;-1;;648;eec747d987850564c95bde0e5a6d1867;0;4;1;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;5;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;293;-1120,1328;Inherit;False;Globnallightcolour;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;165;-2432,608;Inherit;False;Property;_Float2;Float 2;11;0;Create;True;0;0;0;False;0;False;0.5;0.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;148;-2464,528;Inherit;False;147;ShadowTexture;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;253;-3024,848;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;333;-1808.12,-608.8052;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;137;-2448,688;Inherit;False;Property;_Outlinewidth;Outline width;5;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;117;-2496,352;Inherit;False;293;Globnallightcolour;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;341;-1570.145,-546.6539;Inherit;False;Opacity;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;166;-2224,528;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;145;-2432,928;Inherit;False;Constant;_Float0;Float 0;13;0;Create;True;0;0;0;False;0;False;0.001;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;64;-2848,784;Inherit;False;Shadow;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;159;-2448,432;Inherit;False;158;BaseTexture;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.VertexColorNode;139;-2432,768;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;156;-2013,434;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;119;-2045.898,324.6734;Inherit;False;64;Shadow;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;344;-2103.606,619.9034;Inherit;False;341;Opacity;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;143;-2008,705;Inherit;False;3;3;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;56;-2270.823,2019.463;Inherit;False;794.4915;416.6398;Light ;4;55;53;52;58;ViewDIr;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;58;-1726.823,2163.463;Inherit;False;Viewdir_light;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;52;-2206.823,2067.463;Inherit;False;True;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;280;-1760,368;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.DotProductOpNode;53;-1886.823,2147.463;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OutlineNode;136;-1840,480;Inherit;False;0;True;Masked;0;0;Front;3;0;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;55;-2158.823,2243.463;Inherit;False;World;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.GetLocalVarNode;343;-1752.832,239.0012;Inherit;False;341;Opacity;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;287;-2416,1616;Inherit;False;Property;_FakeColourtint;Fake Colour tint;10;0;Create;True;0;0;0;False;0;False;1,1,1,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;-1504,64;Float;False;True;-1;2;ASEMaterialInspector;0;0;CustomLighting;Nars/UmaMusume/Body;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;True;0;True;TransparentCutout;;Transparent;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;0;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;89;12;82;0
WireConnection;89;17;84;0
WireConnection;89;16;83;0
WireConnection;89;29;85;0
WireConnection;89;28;86;0
WireConnection;89;33;88;0
WireConnection;89;34;87;0
WireConnection;91;0;89;0
WireConnection;247;0;89;42
WireConnection;158;0;62;0
WireConnection;303;0;102;2
WireConnection;147;0;80;0
WireConnection;90;0;82;0
WireConnection;266;0;252;0
WireConnection;266;1;267;0
WireConnection;227;0;97;0
WireConnection;227;1;102;0
WireConnection;263;0;257;0
WireConnection;263;1;264;0
WireConnection;289;0;283;0
WireConnection;289;4;285;0
WireConnection;242;0;299;0
WireConnection;242;1;305;0
WireConnection;242;2;308;0
WireConnection;312;0;309;0
WireConnection;312;1;311;0
WireConnection;315;0;102;0
WireConnection;231;1;227;0
WireConnection;231;2;95;0
WireConnection;243;0;298;0
WireConnection;243;1;304;0
WireConnection;243;2;239;0
WireConnection;243;3;314;0
WireConnection;288;0;284;0
WireConnection;288;1;286;0
WireConnection;255;0;263;0
WireConnection;255;1;266;0
WireConnection;245;0;301;0
WireConnection;245;1;131;2
WireConnection;245;2;131;3
WireConnection;245;3;249;0
WireConnection;245;4;295;0
WireConnection;232;0;231;0
WireConnection;334;0;326;0
WireConnection;323;0;322;0
WireConnection;260;0;255;0
WireConnection;260;2;261;0
WireConnection;233;0;300;0
WireConnection;233;1;242;0
WireConnection;233;2;312;0
WireConnection;244;0;297;0
WireConnection;244;1;243;0
WireConnection;291;0;289;0
WireConnection;291;2;288;0
WireConnection;291;3;347;0
WireConnection;290;1;288;0
WireConnection;78;0;233;0
WireConnection;78;1;244;0
WireConnection;78;2;232;0
WireConnection;320;1;321;0
WireConnection;320;3;323;0
WireConnection;320;4;324;0
WireConnection;320;5;325;0
WireConnection;292;1;291;0
WireConnection;292;0;290;0
WireConnection;250;0;245;0
WireConnection;250;1;260;0
WireConnection;250;2;302;0
WireConnection;250;3;296;0
WireConnection;329;1;328;0
WireConnection;329;3;334;0
WireConnection;329;4;331;0
WireConnection;329;5;330;0
WireConnection;293;0;292;0
WireConnection;253;0;78;0
WireConnection;253;1;250;0
WireConnection;333;0;320;0
WireConnection;333;1;329;0
WireConnection;341;0;333;0
WireConnection;166;0;148;0
WireConnection;166;1;165;0
WireConnection;64;0;253;0
WireConnection;156;0;159;0
WireConnection;156;1;166;0
WireConnection;156;2;117;0
WireConnection;143;0;137;0
WireConnection;143;1;139;0
WireConnection;143;2;145;0
WireConnection;58;0;53;0
WireConnection;280;0;119;0
WireConnection;280;1;117;0
WireConnection;53;0;52;0
WireConnection;53;1;55;0
WireConnection;136;0;156;0
WireConnection;136;2;344;0
WireConnection;136;1;143;0
WireConnection;0;10;343;0
WireConnection;0;13;280;0
WireConnection;0;11;136;0
ASEEND*/
//CHKSM=CAC062AD247551866641DDB9D91752BBAB8934D6