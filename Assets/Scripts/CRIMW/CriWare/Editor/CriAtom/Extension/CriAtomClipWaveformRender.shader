Shader "CRIWARE/CriAtomClipWaveformRender" {
Properties{
	_MainTex("Main Texture", 2D) = "white" { }
}

SubShader{
Pass {
	Tags { "RenderType" = "Opaque" }
	Cull Off
	ZWrite Off
	ZTest Always

	CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"

		uniform sampler2D _MainTex;
		uniform float4 _MainTex_ST;
		uniform fixed4 _BacCol;
		uniform fixed4 _ForCol;
		uniform float _Offset;
		uniform float _Scale;
		uniform int _IsLoop;
		uniform int _IsMute;
		uniform float _Channel;
		uniform int _ForceMono;

		struct appdata {
			float4 vertex   : POSITION;
			float4 texcoord : TEXCOORD0;
		};

		struct v2f {
			float4 pos : SV_POSITION;
			float2 uv  : TEXCOORD0;
		};

		inline float zero_amplitude(v2f v, float volume) {
			return floor(volume * 100.0) == 0.0 ? abs(ddy(v.uv.y)) : volume;
		}

		v2f vert(appdata v) {
			v2f o;
			o.pos = UnityObjectToClipPos(v.vertex);
			o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
			o.uv.x *= _Scale;
			o.uv.x += _Offset;
			return o;
		}

		fixed4 frag(v2f v) : SV_Target {
			float channel_height = (1.0 / (_Channel));
			int ch = floor(v.uv.y / (1.0 / _Channel)) + 1;

			/* uv.y reads the values in reverse order of texture creation. */
			/* but _ForceMono is true, read the first channel at the end. */
			float2 ch_vec2 = {frac(v.uv.x), _Channel > 1 ? (ch - 1) * (1.0 / (_Channel - 1)) : 0.0 };
			ch_vec2.y = _ForceMono < 1 ? 1.0 - ch_vec2.y : ch_vec2.y;

			float volume_max = tex2D(_MainTex, ch_vec2).r * channel_height / 2.0;
			volume_max = zero_amplitude(v, volume_max);
			volume_max = v.uv.x > 1 && _IsLoop < 1 ? abs(ddy(v.uv.y)) : volume_max;
			volume_max = _IsMute >= 1 ? abs(ddy(v.uv.y)) : volume_max;

			float volume_min = tex2D(_MainTex, ch_vec2).g * channel_height / 2.0;
			volume_min = zero_amplitude(v, volume_min);
			volume_min = v.uv.x > 1 && _IsLoop < 1 ? abs(ddy(v.uv.y)) : volume_min;
			volume_min = _IsMute >= 1 ? abs(ddy(v.uv.y)) : volume_min;

			float uvY = v.uv.y - ((channel_height / 2.0) * ch * 2.0) + channel_height / 2.0;
			fixed4 col = lerp(
				_BacCol,
				_ForCol,
				-volume_min <= uvY && uvY <= volume_max
			);
			clip(col.a - 0.5);
			return col;
		}

		ENDCG
	}
}
}
