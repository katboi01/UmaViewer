// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

/*
 * MMD Shader for Unity
 *
 * Copyright 2012 Masataka SUMI, Takahiro INOUE
 *
 *    Licensed under the Apache License, Version 2.0 (the "License");
 *    you may not use this file except in compliance with the License.
 *    You may obtain a copy of the License at
 *
 *        http://www.apache.org/licenses/LICENSE-2.0
 *
 *    Unless required by applicable law or agreed to in writing, software
 *    distributed under the License is distributed on an "AS IS" BASIS,
 *    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *    See the License for the specific language governing permissions and
 *    limitations under the License.
 */
float4 _Color;
float _Opacity;
float4 _OutlineColor;
float _OutlineWidth;

struct v2f
{
	float4 pos : SV_POSITION;
	float2 uv  : TEXCOORD0;
};

v2f vert( appdata_base v )
{
	v2f o;
	float4 pos = UnityObjectToClipPos(v.vertex);
	float4 normal = UnityObjectToClipPos(float4(v.normal, 0.0));
	float width = _OutlineWidth / 1024.0; //目コピ調整値(算術根拠無し)
	float depth_offset = pos.z / 4194304.0; //僅かに奥に移動(floatの仮数部は23bitなので(1<<21)程度で割った値は丸めに入らないが非常に小さな値の筈)
	o.pos = pos + normal * float4(width, width, 0.0, 0.0) + float4(0.0, 0.0, depth_offset, 0.0);

	return o;
}
half4 frag( v2f i ) : COLOR
{
	return half4( _OutlineColor.rgb, _OutlineColor.a * _Opacity );
}
