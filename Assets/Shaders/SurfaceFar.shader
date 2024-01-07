Shader "Custom/SurfaceFar"
{
    Properties
    {
        _MainTex("Albedo (RGB)", 2D) = "white" {}
        [KeywordEnum(X, Y, Z)] _Faces("Faces", Float) = 0
    }
    SubShader
    {
        Tags { "LightMode" = "ForwardBase" }
        LOD 200

        Pass
        {
            CGPROGRAM
            #pragma shader_feature _FACES_X _FACES_Y _FACES_Z
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "PlanetMath.hlsl"
            #include "Lighting.cginc"

            // compile shader into multiple variants, with and without shadows
            // (we don't care about any lightmaps yet, so skip these variants)
            #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
            // shadow helper functions and macros
            #include "AutoLight.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord : TEXCOORD0;
                float3 color : COLOR;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 cubeUV : TEXCOORD0;
                float2 atlasUV : TEXCOORD1;

                SHADOW_COORDS(5) // put shadows data into TEXCOORD5
                fixed3 diff : COLOR0;
                fixed3 ambient : COLOR1;
            };

            // Material Data
            sampler2D _MainTex;
            float4 _MainTex_ST;

            UNITY_DECLARE_TEX2DARRAY(_game_Blocks);
            int _game_Blocks_grass;

            // Planet Data
            sampler2D _planet_SurfaceMask;

            v2f vert(appdata v)
            {
                v2f o;

                o.pos = UnityObjectToClipPos(v.vertex);

#if defined(_FACES_X)
                float2 uv = v.color.yz * 255;
                o.atlasUV = uv * float2(2. / (256. * 3.), 2. / (256. * 2.));
#elif defined(_FACES_Y)
                float2 uv = v.color.xz * 255;
                o.atlasUV = uv * float2(2. / (256. * 3.), 2. / (256. * 2.));
#elif defined(_FACES_Z)
                float2 uv = v.color.xy * 255;
                o.atlasUV = uv * float2(2. / (256. * 3.), 2. / (256. * 2.));
#endif
                o.cubeUV = TRANSFORM_TEX(uv, _MainTex);

                // diffuse lighting
                half3 worldNormal = UnityObjectToWorldNormal(v.normal);
                half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
                o.diff = nl * _LightColor0.rgb;
                o.ambient = ShadeSH9(half4(worldNormal, 1));
                // compute shadows data
                TRANSFER_SHADOW(o)

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 mask_surface = tex2D(_planet_SurfaceMask, i.atlasUV);
                clip(.76 - mask_surface.r);

                fixed4 col = UNITY_SAMPLE_TEX2DARRAY(_game_Blocks, float3(i.cubeUV, _game_Blocks_grass));

                // compute shadow attenuation (1.0 = fully lit, 0.0 = fully shadowed)
                fixed shadow = SHADOW_ATTENUATION(i);
                // darken light's illumination with shadow, keep ambient intact
                fixed3 lighting = i.diff * shadow + i.ambient;
                col.rgb *= lighting;

                return col;
            }
            ENDCG
        }
    }
}