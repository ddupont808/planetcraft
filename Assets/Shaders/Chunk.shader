Shader "Custom/Chunk" {
	Properties{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_ColorMap("Color Map", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0

		_Lighting("Lighting", 3D) = "white" {}
	}
	SubShader{
		Tags { "RenderType" = "Opaque" }
		LOD 200

		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows vertex:vert
		#pragma target 3.0

		#include "UnityCG.cginc"

		sampler2D _MainTex;
		float4 _MainTex_ST;

		sampler2D _ColorMap;

		struct Input {
			float2 texcoord;
			float2 texcoord_far;
			float distance;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

		void vert(inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input, o);
			// float2 uv = v.color.xz * 255;
			o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
			o.texcoord_far = TRANSFORM_TEX(v.texcoord1, _MainTex);
			o.distance = length(ObjSpaceViewDir(v.vertex));
		}

		void surf(Input IN, inout SurfaceOutputStandard o) {
			fixed4 color_close = tex2D(_MainTex, IN.texcoord) * _Color;
			fixed4 color_far = tex2D(_ColorMap, IN.texcoord_far) * _Color;

			fixed4 c = lerp(color_close, color_far, saturate(IN.distance / 100.0));

			o.Albedo = c.rgb;
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}