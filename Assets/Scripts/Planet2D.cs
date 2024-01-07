using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class Planet2D : MonoBehaviour
{
    public Transform test;
    public float height = 32f;
    public float sphereHeight = 32f;

    static float[] region_y = new float[]
    {
        81.4873f,
        162.9746f,
        325.9493f,
        651.8986f,
        1303.7972f,
        2607.5945f,
        5215.1891f,
        10430.3784f
    };

    static int[] region_cw = new int[]
    {
        4,
        8,
        16,
        32,
        64,
        128,
        256
    };

    static int[] region_ch = new int[]
    {
        3,
        6,
        11,
        21,
        41,
        82,
        100,
    };

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(Vector3.zero, new Vector3(1f, 1f, 0f).normalized * 6000f);
        Gizmos.DrawRay(Vector3.zero, new Vector3(-1f, 1f, 0f).normalized * 6000f);
        Gizmos.DrawWireSphere(Vector3.zero, 81.4873f); // 32 * 4 * 4 / tau
        Gizmos.DrawWireSphere(Vector3.zero, 162.9747f);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(Vector3.zero, 6000f);
        Gizmos.DrawWireSphere(Vector3.zero, 6250f);

        Gizmos.DrawRay(new Vector3(0f, 6000f, 0f), Vector3.right * 4096f);
        Gizmos.DrawRay(new Vector3(0f, 6250f, 0f), Vector3.right * 4096f);

        {
            Gizmos.color = Color.magenta;

            Gizmos.DrawWireCube(test.position, Vector3.one);
            var point = test.position;
            float altitude = point.magnitude;
            int region = Mathf.FloorToInt(Mathf.Log(altitude / region_y[0], 2));

            if (region >= 0 && region < region_ch.Length)
            {

                float region_bot = region_y[region];
                float region_top = region_y[region + 1];
                int widthChunks = region_cw[region];
                int heightChunks = region_ch[region];
                float region_height_actual = heightChunks * 32;

                var cubized = PlanetMath.CubizePoint2(point / altitude); // [-1, 1]

                var cx = Mathf.FloorToInt(((cubized.x + 1f) / 2f) * widthChunks);
                var cy = Mathf.FloorToInt((altitude - region_bot) / region_height_actual * heightChunks);

                var chunk = PlanetMath.GetPoints(cx, cy, widthChunks / 2, region_bot, region_top, widthChunks);

                var left = chunk.s100 - chunk.s000;
                var up = chunk.s010 - chunk.s000;
                var forward = chunk.s001 - chunk.s000;

                Gizmos.DrawLineList(new Vector3[]
                {
                    point, chunk.s000,
                    point, chunk.s100,
                    point, chunk.s010,
                    point, chunk.s110,
                    point, chunk.s001
                });

                // Gizmos.matrix = new Matrix4x4(left, up, forward, chunk.s000);
                // Gizmos.DrawCube(Vector3.one / 2f, Vector3.one);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        for (int region = 0; region < 7; region++)
        {
            float region_bot = region_y[region];
            float region_top = region_y[region + 1];
            int widthChunks = region_cw[region];
            int heightChunks = region_ch[region];


            // visualize region
            {
                var y = region_bot;
                var y2 = region_top;
                var c00 = new Vector3(-y, y);
                var c10 = new Vector3(+y, y);
                var c01 = new Vector3(-y2, y2);
                var c11 = new Vector3(+y2, y2);

                Gizmos.color = Color.yellow;
                Gizmos.DrawLineList(new Vector3[]
                {
                    c00, c10,

                    c00, PlanetMath.CubeToSphere(c00, y),
                    c10, PlanetMath.CubeToSphere(c10, y),


                    c01, c11,

                    c01, PlanetMath.CubeToSphere(c01, y2),
                    c11, PlanetMath.CubeToSphere(c11, y2),
                });
            }

            for (int i = 0; i < widthChunks; i++)
            {
                for (int j = 0; j < heightChunks; j++)
                {
                    var cp = PlanetMath.GetPoints(i, j, widthChunks / 2, region_bot, region_top, widthChunks);

                    var s00 = cp.s000;
                    var s10 = cp.s100;
                    var s01 = cp.s010;
                    var s11 = cp.s110;

                    if (j == 0 && region > 0)
                    {
                        var prev_bot = region_y[region - 1];
                        var prev_cw = region_cw[region - 1];
      
                        var prev_ch = region_ch[region - 1];
                        var adj_cp = PlanetMath.GetPoints(Mathf.FloorToInt(i / 2), prev_ch - 1, prev_cw / 2, prev_bot, region_bot, prev_cw);

                        var top_midpoint = (adj_cp.s010 + adj_cp.s110) / 2;
                        if (i % 2 == 0) s10 = top_midpoint;
                        else s00 = top_midpoint;
                    }

                    Gizmos.color = Color.blue;
                    Gizmos.DrawLineList(new Vector3[]
                    {
                        s00, s10,
                        s10, s11,
                        s11, s01,
                        s01, s00
                    });
                }
            }
        }
    }
}

public class PlanetMath
{

    public static float[] region_y = new float[]
    {
        81.4873f,
        162.9746f,
        325.9493f,
        651.8986f,
        1303.7972f,
        2607.5945f,
        5215.1891f,
        10430.3784f
    };

    public static int[] region_cw = new int[]
    {
        4,
        8,
        16,
        32,
        64,
        128,
        256
    };

    public static int[] region_ch = new int[]
    {
        3,
        6,
        11,
        21,
        41,
        82,
        100,
    };

    public struct ChunkPoints
    {
        public Vector3 s000, s010, s100, s110, s001;
    }

    public struct ChunkCoords
    {
        public int x;
        public int y;
        public int z;
        public int region;

        public int blockX;
        public int blockY;
        public int blockZ;
    }

    public static ChunkCoords PointToChunkCoords(Vector3 point)
    {
        float altitude = point.magnitude;
        int region = Mathf.FloorToInt(Mathf.Log(altitude / region_y[0], 2));

        if (region >= 0 && region < region_ch.Length)
        {
            float region_bot = region_y[region];
            float region_top = region_y[region + 1];
            int widthChunks = region_cw[region];
            int heightChunks = region_ch[region];
            float region_height_actual = heightChunks * 32;

            var cubized = PlanetMath.CubizePoint2(point / altitude); // [-1, 1]

            var x = ((cubized.x + 1f) / 2f) * widthChunks;
            var y = (altitude - region_bot) / region_height_actual * heightChunks;
            var z = ((cubized.z + 1f) / 2f) * widthChunks;

            var cx = Mathf.FloorToInt(x);
            var cy = Mathf.FloorToInt(y);
            var cz = Mathf.FloorToInt(z);

            return new ChunkCoords()
            {
                x = cx,
                y = cy,
                z = cz,
                region = region,

                blockX = Mathf.FloorToInt(x * 32),
                blockY = Mathf.FloorToInt(y * 32),
                blockZ = Mathf.FloorToInt(z * 32)
            };
        }

        return new ChunkCoords() { x = -1, y = -1, z = -1, region = -1 };
    }

    public static ChunkPoints GetPoints(int i, int j, int k, int region)
    {
        float region_bot = region_y[region];
        float region_top = region_y[region + 1];
        int widthChunks = region_cw[region];

        return GetPoints(i, j, k, region_bot, region_top, widthChunks);
    }

    public static ChunkPoints GetPoints(int i, int j, int k, float region_bot, float region_top, int region_cw)
    {
        var region_height = region_top - region_bot;
        var c_width = (region_bot * 2) / region_cw;
        var c_height = 32f;

        var y0 = region_bot + c_height * j;
        var y1 = Mathf.Min(region_top, region_bot + c_height * (j + 1));

        var xscale0 = 1 + (y0 - region_bot) / region_height;
        var xscale1 = 1 + (y1 - region_bot) / region_height;

        var x0 = (c_width * i);
        var x1 = (c_width * (i + 1));

        var z0 = (c_width * k);
        var z1 = (c_width * (k + 1));

        var c000 = new Vector3(-y0 + x0 * xscale0, y0, -y0 + z0 * xscale0);
        var c100 = new Vector3(-y0 + x1 * xscale0, y0, -y0 + z0 * xscale0);
        var c010 = new Vector3(-y1 + x0 * xscale1, y1, -y0 + z0 * xscale1);
        var c001 = new Vector3(-y0 + x0 * xscale0, y0, -y0 + z1 * xscale0);
        var c110 = new Vector3(-y1 + x1 * xscale1, y1, -y0 + z0 * xscale1);

        var s000 = CubeToSphere(c000, y0);
        var s100 = CubeToSphere(c100, y0);
        var s010 = CubeToSphere(c010, y1);
        var s001 = CubeToSphere(c001, y1);

        var s110 = CubeToSphere(c110, y1);

        return new ChunkPoints()
        {
            s000 = s000,
            s100 = s100,
            s010 = s010,
            s001 = s001,
            s110 = s110
        };
    }

    public static Vector3 RegionToSphere(Vector3 blockPos, int region)
    {
        float region_bot = region_y[region];
        float region_top = region_y[region + 1];

        var region_height = region_top - region_bot;
        var region_width = region_cw[region];

        var y0 = region_bot + blockPos.y;

        var xscale0 = 1 + (y0 - region_bot) / region_height;

        return CubeToSphere(new Vector3(
            -y0 + blockPos.x / (region_width * 32) * (y0 * 2),
            y0,
            -y0 + blockPos.z / (region_width * 32) * (y0 * 2)
            ), y0);

        // return CubeToSphere(new Vector3(-y0 + blockPos.x * xscale0, y0, -y0 + blockPos.z * xscale0), y0);
    }

    public static UnityEngine.Vector3 CubeToSphere(UnityEngine.Vector3 v, float altitude)
    {
        v /= altitude;

        float x2 = v.x * v.x;
        float y2 = v.y * v.y;
        float z2 = v.z * v.z;
        Vector3 s;
        s.x = v.x * Mathf.Sqrt(1f - y2 / 2f - z2 / 2f + y2 * z2 / 3f);
        s.y = v.y * Mathf.Sqrt(1f - x2 / 2f - z2 / 2f + x2 * z2 / 3f);
        s.z = v.z * Mathf.Sqrt(1f - x2 / 2f - y2 / 2f + x2 * y2 / 3f);

        return s * altitude;
        // return v.normalized * altitude;
    }

    public static UnityEngine.Vector3 CubizePoint2(UnityEngine.Vector3 position)
    {
        double x, y, z;
        x = position.x;
        y = position.y;
        z = position.z;

        double fx, fy, fz;
        fx = System.Math.Abs(x);
        fy = System.Math.Abs(y);
        fz = System.Math.Abs(z);

        const double inverseSqrt2 = 0.70710676908493042;

        if (fy >= fx && fy >= fz)
        {
            double a2 = x * x * 2.0;
            double b2 = z * z * 2.0;
            double inner = -a2 + b2 - 3.0;
            double innersqrt = -System.Math.Sqrt((inner * inner) - 12.0 * a2);

            if (x == 0.0)
            {
                position.x = 0.0f;
            }
            else
            {
                position.x = (float)(System.Math.Sqrt(innersqrt + a2 - b2 + 3.0) * inverseSqrt2);
            }

            if (z == 0.0)
            {
                position.z = 0.0f;
            }
            else
            {
                position.z = (float)(System.Math.Sqrt(innersqrt - a2 + b2 + 3.0) * inverseSqrt2);
            }

            if (position.x > 1.0f) position.x = 1.0f;
            if (position.z > 1.0f) position.z = 1.0f;

            if (x < 0) position.x = -position.x;
            if (z < 0) position.z = -position.z;

            position.y = y > 0 ? 1.0f : -1.0f;
        }
        else if (fx >= fy && fx >= fz)
        {
            double a2 = y * y * 2.0;
            double b2 = z * z * 2.0;
            double inner = -a2 + b2 - 3.0;
            double innersqrt = -System.Math.Sqrt((inner * inner) - 12.0 * a2);

            if (y == 0.0)
            {
                position.y = 0.0f;
            }
            else
            {
                position.y = (float)(System.Math.Sqrt(innersqrt + a2 - b2 + 3.0) * inverseSqrt2);
            }

            if (z == 0.0)
            {
                position.z = 0.0f;
            }
            else
            {
                position.z = (float)(System.Math.Sqrt(innersqrt - a2 + b2 + 3.0) * inverseSqrt2);
            }

            if (position.y > 1.0f) position.y = 1.0f;
            if (position.z > 1.0f) position.z = 1.0f;

            if (y < 0) position.y = -position.y;
            if (z < 0) position.z = -position.z;

            position.x = x > 0 ? 1.0f : -1.0f;
        }
        else
        {
            double a2 = x * x * 2.0;
            double b2 = y * y * 2.0;
            double inner = -a2 + b2 - 3.0;
            double innersqrt = -System.Math.Sqrt((inner * inner) - 12.0 * a2);

            if (x == 0.0)
            {
                position.x = 0.0f;
            }
            else
            {
                position.x = (float)(System.Math.Sqrt(innersqrt + a2 - b2 + 3.0) * inverseSqrt2);
            }

            if (y == 0.0)
            {
                position.y = 0.0f;
            }
            else
            {
                position.y = (float)(System.Math.Sqrt(innersqrt - a2 + b2 + 3.0) * inverseSqrt2);
            }

            if (position.x > 1.0f) position.x = 1.0f;
            if (position.y > 1.0f) position.y = 1.0f;

            if (x < 0) position.x = -position.x;
            if (y < 0) position.y = -position.y;

            position.z = z > 0 ? 1.0f : -1.0f;
        }

        return position;
    }
}
