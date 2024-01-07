const float isqrt2 = 0.70710676908493042;

float3 cubify(float3 s)
{
	float xx2 = s.x * s.x * 2.0;
	float yy2 = s.y * s.y * 2.0;

	float2 v = float2(xx2 - yy2, yy2 - xx2);

	float ii = v.y - 3.0;
	ii *= ii;

	float isqrt = -sqrt(ii - 12.0 * xx2) + 3.0;

	v = sqrt(v + isqrt);
	v *= isqrt2;

	return sign(s) * float3(v, 1.0);
}

float3 sphere2cube(float3 sphere)
{
	float3 f = abs(sphere);

	bool a = f.y >= f.x && f.y >= f.z;
	bool b = f.x >= f.z;

	return a ? cubify(sphere.xzy).xzy : b ? cubify(sphere.yzx).zxy : cubify(sphere);
}

static float3 cube2sphere(float3 p) {
    float alt = length(p);
    p /= alt;
    return p * sqrt(
        1. - ((p * p).yxx + (p * p).zzy) / 2. + (p * p).yxx * (p * p).zzy / 3.
    ) * alt;
};

float3 cubizePoint2(float3 position)
{
    float x, y, z;
    x = position.x;
    y = position.y;
    z = position.z;

    float fx, fy, fz;
    fx = abs(x);
    fy = abs(y);
    fz = abs(z);

    const float inverseSqrt2 = 0.70710676908493042;

    if (fy >= fx && fy >= fz) {
        float a2 = x * x * 2.0;
        float b2 = z * z * 2.0;
        float inner = -a2 + b2 - 3;
        float innersqrt = -sqrt((inner * inner) - 12.0 * a2);

        if (x == 0.0 || x == -0.0) {
            position.x = 0.0;
        }
        else {
            position.x = sqrt(innersqrt + a2 - b2 + 3.0) * inverseSqrt2;
        }

        if (z == 0.0 || z == -0.0) {
            position.z = 0.0;
        }
        else {
            position.z = sqrt(innersqrt - a2 + b2 + 3.0) * inverseSqrt2;
        }

        if (position.x > 1.0) position.x = 1.0;
        if (position.z > 1.0) position.z = 1.0;

        if (x < 0) position.x = -position.x;
        if (z < 0) position.z = -position.z;

        if (y > 0) {
            // top face
            position.y = 1.0;
        }
        else {
            // bottom face
            position.y = -1.0;
        }
    }
    else if (fx >= fy && fx >= fz) {
        float a2 = y * y * 2.0;
        float b2 = z * z * 2.0;
        float inner = -a2 + b2 - 3;
        float innersqrt = -sqrt((inner * inner) - 12.0 * a2);

        if (y == 0.0 || y == -0.0) {
            position.y = 0.0;
        }
        else {
            position.y = sqrt(innersqrt + a2 - b2 + 3.0) * inverseSqrt2;
        }

        if (z == 0.0 || z == -0.0) {
            position.z = 0.0;
        }
        else {
            position.z = sqrt(innersqrt - a2 + b2 + 3.0) * inverseSqrt2;
        }

        if (position.y > 1.0) position.y = 1.0;
        if (position.z > 1.0) position.z = 1.0;

        if (y < 0) position.y = -position.y;
        if (z < 0) position.z = -position.z;

        if (x > 0) {
            // right face
            position.x = 1.0;
        }
        else {
            // left face
            position.x = -1.0;
        }
    }
    else {
        float a2 = x * x * 2.0;
        float b2 = y * y * 2.0;
        float inner = -a2 + b2 - 3;
        float innersqrt = -sqrt((inner * inner) - 12.0 * a2);

        if (x == 0.0 || x == -0.0) {
            position.x = 0.0;
        }
        else {
            position.x = sqrt(innersqrt + a2 - b2 + 3.0) * inverseSqrt2;
        }

        if (y == 0.0 || y == -0.0) {
            position.y = 0.0;
        }
        else {
            position.y = sqrt(innersqrt - a2 + b2 + 3.0) * inverseSqrt2;
        }

        if (position.x > 1.0) position.x = 1.0;
        if (position.y > 1.0) position.y = 1.0;

        if (x < 0) position.x = -position.x;
        if (y < 0) position.y = -position.y;

        if (z > 0) {
            // front face
            position.z = 1.0;
        }
        else {
            // back face
            position.z = -1.0;
        }
    }

    return position;
}

float3 cubizePoint3(float3 position) {
    float alt = length(position);
    return cubizePoint2(position / alt) * alt;
}

float region_y[8] =
{
    81.4873,
    162.9746,
    325.9493,
    651.8986,
    1303.7972,
    2607.5945,
    5215.1891,
    10430.3784
};

int region_cw[7] =
{
    4,
    8,
    16,
    32,
    64,
    128,
    256
};

int region_ch[7] =
{
    3,
    6,
    11,
    21,
    41,
    82,
    100,
};

void PointToChunkCoords(float3 p, out int4 ChunkCoordsXYZ, out float3 ChunkCoordsBlock)
{
    float altitude = length(p);
    int region = 6; //int(log2(altitude / region_y[0]));
    if (region >= 0 && region < 7)
    {
        float region_bot = region_y[region];
        float region_top = region_y[region + 1];
        int widthChunks = region_cw[region];
        int heightChunks = region_ch[region];
        float region_height_actual = heightChunks * 32;

        float3 cubized = cubizePoint2(p / altitude); //[-1, 1]

        float x = ((cubized.x + 1) / 2) * widthChunks;
        float y = (altitude - region_bot) / region_height_actual * heightChunks;
        float z = ((cubized.z + 1) / 2) * widthChunks;

        int cx = int(x);
        int cy = int(y);
        int cz = int(z);

        ChunkCoordsXYZ = int4(cx, cy, cz, region);
        ChunkCoordsBlock = float3(x, y, z) * 32;
    }
    else
    {
        ChunkCoordsXYZ = int4(-1, -1, -1, -1);
        ChunkCoordsBlock = float3(1, 1, 0);
    }
}