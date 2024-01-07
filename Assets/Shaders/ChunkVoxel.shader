Shader "Voxel/Chunk (Raymarched)"
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
            #include "PlanetMath.hlsl"

            #define MAX_RAY_STEPS 64
            #define GRID_SCALE .0625

            float sdSphere(float3 p, float d) { return length(p) - d; }
            float sdBox(float3 p, float3 b) {
                float3 d = abs(p) - b;
                return min(max(d.x, max(d.y, d.z)), 0.0) + length(max(d, 0.0));
            }

            bool getVoxel(int3 c) {
                float3 p = c + 0.5;
                float d = min(sdBox(p, float3(7.0, 7, 7)), max(-sdSphere(p, 9.5), sdBox(p, float3(8.0, 8.0, 8.0))));
                return d < 0.0;
            }

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                CHUNK_DATA(0, 1, 0) // chunk data in TEXCOORD0-1, COLOR0
                float3 cubePos : TEXCOORD2;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 normal : NORMAL;
                CHUNK_COORDS(0, 1) // put chunk data into TEXCOORD0-1

                // voxel data
                float4 worldPos : TEXCOORD2;  // used to shoot the rays through the pixels
                float4 screenPos : TEXCOORD3; // Used to sample the back face pass
                float3 cubePos : TEXCOORD4;

                SHADOW_COORDS(5) // put shadows data into TEXCOORD5
                fixed3 diff : COLOR0;
                fixed3 ambient : COLOR1;
            };

            v2f vert(appdata v)
            {
                v2f o;

                float4 vertex = v.vertex;

                o.pos = UnityObjectToClipPos(vertex);
                o.normal = v.normal;

                // compute chunk data
                CHUNK_APPLY(o, v)

                o.worldPos = mul(unity_ObjectToWorld, vertex);
                o.screenPos = ComputeScreenPos(vertex);
                o.screenPos.z = -(mul(UNITY_MATRIX_MV, vertex).z * _ProjectionParams.w);					// old calculation, as I used the depth buffer comparision for min max ray march. I will leave this for future use
                o.cubePos = vertices[int(v.blockPos.w / CORNER_UNIT)]; //v.cubePos;

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
                
                // setup variables
                float3 objPos = mul(unity_WorldToObject, float4(i.worldPos.xyz, 1.0)).xyz;
                float3 camPos_obj = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos.xyz, 1.0)).xyz;

                // ray generation
                float3 rayPos = i.cubePos - sign(i.cubePos) * .5 + -(i.normal * GRID_SCALE * .1);
                float3 rayDir = normalize(objPos - camPos_obj);
                // rayDir = normalize(cubizePoint3(i.worldPos.xyz + rayDir * .1) - cubizePoint3(i.worldPos.xyz));
                // float3 rayDir = normalize(cubizePoint3(i.worldPos.xyz) - cubizePoint3(_WorldSpaceCameraPos.xyz));
                rayPos /= GRID_SCALE;

                // ray marcher setup
                int3 mapPos = int3(floor(rayPos + 0.));
                float3 deltaDist = abs(length(rayDir) / rayDir);
                int3 rayStep = int3(sign(rayDir));
                float3 sideDist = (sign(rayDir) * (float3(mapPos)-rayPos) + (sign(rayDir) * 0.5) + 0.5) * deltaDist;
                bool3 mask;

                // ray marcher loop
                bool hit = false;
                for (int step = 0; step < MAX_RAY_STEPS; step++) {
                    if (getVoxel(mapPos)) {
                        hit = true;
                        continue;
                    }
                    mask = sideDist.xyz <= min(sideDist.yzx, sideDist.zxy);
                    sideDist += float3(mask)*deltaDist;
                    mapPos += int3(mask)*rayStep;
                }

                if (any(abs(mapPos) > 8)) discard;

                if (hit) {
                    // calculate color
                    if (mask.x) {
                        col *= fixed4(.5, .5, .5, 1.);
                    }
                    if (mask.y) {
                        col *= fixed4(1., 1., 1., 1.);
                    }
                    if (mask.z) {
                        col *= fixed4(.75, .75, .75, 1.);
                    }
                }


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