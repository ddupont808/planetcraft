Shader "Voxel/Chunk (AO)"
{
    Properties
    {

    }
    SubShader
    {
        Tags { "LightMode" = "ForwardBase" }
        LOD 200

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // texture arrays are not available everywhere,
            // only compile shader on platforms where they are
            #pragma require 2darray

            #include "UnityCG.cginc"

            // compile shader into multiple variants, with and without shadows
            // (we don't care about any lightmaps yet, so skip these variants)
            #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
            #include "Lighting.cginc"
            #include "AutoLight.cginc"

            #include "./Include/Chunk.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                CHUNK_DATA(0, 1, 0) // chunk data in TEXCOORD0-1, COLOR0
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                CHUNK_COORDS(0, 1) // put chunk data into TEXCOORD0-1

                SHADOW_COORDS(5) // put shadows data into TEXCOORD5
                fixed3 diff : COLOR0;
                fixed3 ambient : COLOR1;
            };

            v2f vert(appdata v)
            {
                v2f o;

                o.pos = UnityObjectToClipPos(v.vertex);

                // compute chunk data
                CHUNK_APPLY(o, v)

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
                fixed4 col = CHUNK_SAMPLE(i);

                // compute shadow attenuation (1.0 = fully lit, 0.0 = fully shadowed)
                fixed shadow = SHADOW_ATTENUATION(i);
                fixed sky_light = saturate(i.sky_lighting.r + shadow);
                // darken light's illumination with shadow, keep ambient intact
                fixed3 lighting = i.diff * shadow + i.ambient * sky_light;
                col.rgb *= lighting;

                return col;
            }
            ENDCG
        }

        // shadow casting support
        UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
    }
}