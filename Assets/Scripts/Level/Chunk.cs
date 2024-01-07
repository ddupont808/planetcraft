using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Experimental.GlobalIllumination;
using System.IO;

public class Chunk : MonoBehaviour
{
    public World world;

    // Chunk Data
    private byte[] heights;
    private byte[] lighting;
    private byte[] data;

    // GameObject Data
    private Texture3D lightData;
    private Mesh mesh;
    private Material material;
    private MeshCollider col;

    public int chunkSize = 32;

    public int chunkX;
    public int chunkY;
    public int chunkZ;
    public int cx, cy, cz, chunkRegion;
    public bool update;
    bool modifiedSinceLoad;

    public Matrix4x4 matrix;

    void LateUpdate()
    {
        if (update)
        {
            GenerateMesh();
            CalculateLighting();
            update = false;
        }
    }

    // Use this for initialization
    void Start()
    {
        heights = new byte[chunkSize * chunkSize];
        mesh = GetComponent<MeshFilter>().mesh;
        col = GetComponent<MeshCollider>();

        material = new Material(GetComponent<MeshRenderer>().sharedMaterial);
        GetComponent<MeshRenderer>().sharedMaterial = material;

        GenerateMesh();
    }

    public void GenerateMesh()
    {
        var mesher = world.voxelGenerator;
        mesher.GenerateMesh(world, chunkX, chunkY, chunkZ);
        mesher.SpherizeMesh(cx, cy, cz, chunkRegion);
        mesher.ApplyMesh(mesh);

        col.sharedMesh = null;
        col.sharedMesh = mesh;
    }

    private void OnDrawGizmosSelected()
    {
        var distortion = PlanetMath.GetPoints(cx + chunkX / chunkSize, cy + chunkY / chunkSize, cz + chunkZ / chunkSize, chunkRegion);

        Gizmos.DrawLineList(new Vector3[]
        {
            distortion.s000, distortion.s100,
            distortion.s000, distortion.s010,
            distortion.s000, distortion.s001,

            distortion.s000, PlanetMath.RegionToSphere(new Vector3(chunkX + cx * chunkSize, chunkY + cy * chunkSize, chunkZ + cz * chunkSize), chunkRegion)
        });
    }

    void PopulateData()
    {
        data = new byte[chunkSize * chunkSize * chunkSize];
        for (int y = 0; y < chunkSize; y++)
        {
            for (int x = 0; x < chunkSize; x++)
            {
                for (int z = 0; z < chunkSize; z++)
                {
                    data[y * chunkSize * chunkSize + x * chunkSize + z] = Block(x, y, z);
                }
            }
        }
    }

    void CalculateLighting()
    {
        if (lightData == null)
        {
            lightData = new Texture3D(chunkSize, chunkSize, chunkSize, TextureFormat.R8, false);
            lightData.wrapMode = TextureWrapMode.Clamp;
            lightData.filterMode = FilterMode.Point;
            material.SetTexture("_Lighting", lightData);
        }

        byte[] lighting = new byte[chunkSize * chunkSize * chunkSize];

        // skylight
        for (int x = 0; x < chunkSize; x++)
        {
            for (int z = 0; z < chunkSize; z++)
            {
                var isSunlit = true;
                for (int y = chunkSize - 1; y >= 0; y--)
                {
                    if(isSunlit && Block(x, y, z) != 0)
                    {
                        heights[x + z * chunkSize] = (byte)y;
                        isSunlit = false;
                    }
                    lighting[x + y * chunkSize + z * chunkSize * chunkSize] = isSunlit ? (byte)15 : (byte)0;
                }
            }
        }

        // bad propogation
        int[][] masks = new int[][]
        {
            new int[] { -1, 0, 0 },
            new int[] { 1, 0, 0 },
            new int[] { 0, -1, 0 },
            new int[] { 0, 1, 0 },
            new int[] { 0, 0, -1 },
            new int[] { 0, 0, 1 }
        };
        for (int it = 0; it < 15; it++)
        {
            for (int x = 1; x < chunkSize - 1; x++)
            {
                for (int y = 1; y < chunkSize - 1; y++)
                {
                    for (int z = 1; z < chunkSize - 1; z++)
                    {
                        if (Block(x, y, z) != 0 || y > heights[x + z * chunkSize]) continue;

                        var lightLevel = lighting[x + y * chunkSize + z * chunkSize * chunkSize];
                        for (int m = 0; m < 6; m++)
                        {
                            var mask = masks[m];
                            var adjLevel = lighting[(x + mask[0]) + (y + mask[1]) * chunkSize + (z + mask[2]) * chunkSize * chunkSize];
                            if (adjLevel > lightLevel)
                                lightLevel = (byte)(adjLevel - 1);
                        }
                        lighting[x + y * chunkSize + z * chunkSize * chunkSize] = lightLevel;
                    }
                }
            }
        }

        Color32[] lightData_raw = new Color32[chunkSize * chunkSize * chunkSize];
        for (int i = 0; i < lighting.Length; i++)
            lightData_raw[i] = new Color32((byte)((lighting[i] << 4) + lighting[i]), 0, 0, 0);
        lightData.SetPixels32(lightData_raw);
        lightData.Apply();
    }

    public void Serialize(Stream output)
    {
        PopulateData();
        using (var writer = new BinaryWriter(output))
        {
            writer.Write(heights);
            writer.Write(lighting);
            writer.Write(data);
        }
    }

    public byte Block(int x, int y, int z)
    {
        if (modifiedSinceLoad && x >= 0 && y >= 0 && z >= 0 && x < chunkSize && y < chunkSize && z < chunkSize)
            return data[y * chunkSize * chunkSize + x * chunkSize + z];
        return world.GenerateBlock(x + chunkX, y + chunkY, z + chunkZ);
    }

    public void SetBlock(int x, int y, int z, byte block, bool queueUpdate)
    {
        if(!modifiedSinceLoad)
        {
            PopulateData();
            modifiedSinceLoad = true;
        }

        if (x >= 0 && y >= 0 && z >= 0 && x < chunkSize && y < chunkSize && z < chunkSize)
        {
            Debug.Log($"set {x}, {y}, {z} to {block}");
            data[y * chunkSize * chunkSize + x * chunkSize + z] = block;
        }
        else
            world.Block(x + chunkX, y + chunkY, z + chunkZ, block);

        if (queueUpdate)
            update = true;
    }
}