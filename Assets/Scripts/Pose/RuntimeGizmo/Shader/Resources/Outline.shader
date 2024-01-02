//Taken and modified from github.com/Shrimpey/Outlined-Diffuse-Shader-Fixed/blob/master/CustomOutline.shader

Shader "Custom/Outline" {
	Properties {
		_OutlineColor ("Outline Color", Color) = (1,.5,0,1)
		_Outline ("Outline width", Range (0, 1)) = .1
	}
 
CGINCLUDE
#include "UnityCG.cginc"
 
struct appdata {
	float4 vertex : POSITION;
	float3 normal : NORMAL;
};
 
struct v2f {
	float4 pos : POSITION;
	float4 color : COLOR;
};
 
uniform float _Outline;
uniform float4 _OutlineColor;
 
v2f vert(appdata v) {
	// just make a copy of incoming vertex data but scaled according to normal direction
	v2f o;

	v.vertex *= ( 1 + _Outline);

	o.pos = UnityObjectToClipPos(v.vertex);
 
	o.color = _OutlineColor;
	return o;
}
ENDCG
 
	SubShader {
		Tags { "DisableBatching" = "True" }
		Pass {
			Name "OUTLINE"
			Tags {"LightMode" = "UniversalForward" }
			Cull Front
			ZWrite On
			ColorMask RGB
			Blend SrcAlpha OneMinusSrcAlpha
 
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			half4 frag(v2f i) :COLOR { return i.color; }
			ENDCG
		}
	}
 
	Fallback "Diffuse"
}
