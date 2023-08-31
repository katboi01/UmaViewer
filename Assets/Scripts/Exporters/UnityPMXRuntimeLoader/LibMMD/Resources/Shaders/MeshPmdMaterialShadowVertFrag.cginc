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
float _Opacity;
sampler2D _MainTex;
float4 _MainTex_ST;

struct v2f
{
	float4 pos : SV_POSITION;
	float2 uv  : TEXCOORD0;
};

v2f shadow_vert( appdata_img v )
{
	v2f o;
	o.pos = UnityObjectToClipPos(v.vertex);
	o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
	return o;
}

half4 shadow_frag( v2f i ) : COLOR
{
	float4 tex_color = tex2D(_MainTex, i.uv);
	return half4(0, 0, 0, tex_color.a * _Opacity);
}
