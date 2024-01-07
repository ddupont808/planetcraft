#ifndef __CHUNK_H__
#define __CHUNK_H__

// Material Data
UNITY_DECLARE_TEX2DARRAY(_game_Blocks);

// Chunk Data
sampler3D _Lighting;

#define VOXEL_UNIT (1. / 32.)
#define CORNER_UNIT (1. / 24.)

static const float3 vertices[24] = {
	// top
	float3(0, 1, 1),
	float3(1, 1, 1),
	float3(1, 1, 0),
	float3(0, 1, 0),
	// north
	float3(1, 0, 1),
	float3(1, 1, 1),
	float3(0, 1, 1),
	float3(0, 0, 1),
	// east
	float3(1, 0, 0),
	float3(1, 1, 0),
	float3(1, 1, 1),
	float3(1, 0, 1),
	// bot
	float3(0, -1, 0),
	float3(1, -1, 0),
	float3(1, -1, 1),
	float3(0, -1, 1),
	// south
	float3(0, 0, -1),
	float3(0, 1, -1),
	float3(1, 1, -1),
	float3(1, 0, -1),
	// west
	float3(-1, 0, 1),
	float3(-1, 1, 1),
	float3(-1, 1, 0),
	float3(-1, 0, 0),
};
static const float3 offsets[24] = {
	// top
	float3(0, 0, 0),
	float3(-1, 0, 0),
	float3(-1, 0, -1),
	float3(0, 0, -1),
	// north
	float3(0, 0, 0),
	float3(-1, 0, 0),
	float3(-1, -1, 0),
	float3(0, -1, 0),
	// east
	float3(0, 0, 0),
	float3(0, -1, 0),
	float3(0, -1, -1),
	float3(0, 0, -1),
	// bot
	float3(0, 0, 0),
	float3(-1, 0, 0),
	float3(-1, 0, -1),
	float3(0, 0, -1),
	// south
	float3(0, 0, 0),
	float3(-1, 0, 0),
	float3(-1, -1, 0),
	float3(0, -1, 0),
	// west
	float3(0, 0, 0),
	float3(0, -1, 0),
	float3(0, -1, -1),
	float3(0, 0, -1),
};

fixed4 smoothLighting(float3 blockPos, int vertex)
{
	int offset = vertex & ~3;
	fixed4 a = tex3Dlod(_Lighting, float4(blockPos + (vertices[vertex] + offsets[offset + 0]) * VOXEL_UNIT, 0));
	fixed4 b = tex3Dlod(_Lighting, float4(blockPos + (vertices[vertex] + offsets[offset + 1]) * VOXEL_UNIT, 0));
	fixed4 c = tex3Dlod(_Lighting, float4(blockPos + (vertices[vertex] + offsets[offset + 2]) * VOXEL_UNIT, 0));
	fixed4 d = tex3Dlod(_Lighting, float4(blockPos + (vertices[vertex] + offsets[offset + 3]) * VOXEL_UNIT, 0));

	return (a + b + c + d) / 4.;
}

#define CHUNK_DATA(tc0, tc1, col0) \
	float2 uv0 : TEXCOORD##tc0; \
	float2 uv1 : TEXCOORD##tc1; \
	float4 blockPos : COLOR##col0;

#define CHUNK_COORDS(tex, sky) \
    float4 texcoord : TEXCOORD##tex; \
    fixed4 sky_lighting : TEXCOORD##sky;

#define CHUNK_APPLY(v2f, v) \
    v2f.texcoord = float4(v.uv1, v.uv0.x, v.blockPos.w / CORNER_UNIT); \
    v2f.sky_lighting = smoothLighting(v.blockPos.xyz, int(v.blockPos.w / CORNER_UNIT));

#define CHUNK_SAMPLE(i) UNITY_SAMPLE_TEX2DARRAY(_game_Blocks, i.texcoord.xyz);

#endif