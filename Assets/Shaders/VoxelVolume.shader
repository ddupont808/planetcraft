Shader "Voxel/Volume"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 worldPos : TEXCOORD1;			// used to shoot the rays through the pixels
                float4 screenPos : TEXCOORD2;			// Used to sample the back face pass
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;


            #define MAX_RAY_STEPS 64
            #define GRID_SCALE .0625
            
            float sdSphere(float3 p, float d) { return length(p) - d; }
            float sdBox(float3 p, float3 b) {
                float3 d = abs(p) - b;
                return min(max(d.x, max(d.y, d.z)), 0.0) + length(max(d, 0.0));
            }
            
            bool getVoxel(int3 c) {
                float3 p = c + 0.5;
                float d = max(-sdSphere(p, 7.5), sdBox(p, float3(6.0, 6.0, 6.0)));
                return d < 0.0;
            }


            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.screenPos = ComputeScreenPos(o.vertex);
                o.screenPos.z = -(mul(UNITY_MATRIX_MV, v.vertex).z * _ProjectionParams.w);					// old calculation, as I used the depth buffer comparision for min max ray march. I will leave this for future use
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col;
                
                

                // setup variables
                float3 objPos = mul(unity_WorldToObject, float4(i.worldPos.xyz, 1.0)).xyz;
                float3 camPos_obj = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos.xyz, 1.0)).xyz;

                // ray generation
                float3 rayPos = objPos;
                float3 rayDir = normalize(objPos - camPos_obj);
                rayPos /= GRID_SCALE;

                // ray marcher setup
                int3 mapPos = int3(floor(rayPos + 0.));
                float3 deltaDist = abs(float3(1., 1., 1.) / rayDir);
                int3 rayStep = int3(sign(rayDir));
                float3 sideDist = (sign(rayDir) * (float3(mapPos)-rayPos) + (sign(rayDir) * 0.5) + 0.5) * deltaDist;
                bool3 mask;

                // ray marcher loop
                bool hit = false;
                for (int i = 0; i < MAX_RAY_STEPS; i++) {
                    if (getVoxel(mapPos)) {
                        hit = true;
                        continue;
                    }
                    mask = sideDist.xyz <= min(sideDist.yzx, sideDist.zxy);
                    sideDist += float3(mask)*deltaDist;
                    mapPos += int3(mask)*rayStep;
                }
                if (!hit) discard;

                // calculate color
                if (mask.x) {
                    col = fixed4(.5, .5, .5, 1.);
                }
                if (mask.y) {
                    col = fixed4(1., 1., 1., 1.);
                }
                if (mask.z) {
                    col = fixed4(.75, .75, .75, 1.);
                }

                return col;
            }
            ENDCG
        }
    }
}
