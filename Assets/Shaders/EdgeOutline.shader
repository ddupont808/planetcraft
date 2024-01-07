Shader "EdgeOutline" {
	Properties {
		_Color("Color", Color) = (1,1,1)
	}

	SubShader {
		ZTest Less
		Cull Off
		Color [_Color]
		Pass { }
	}
}