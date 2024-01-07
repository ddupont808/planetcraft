using PlanetCraft.Mods.API;
using System;
using System.Collections.Generic;
using UnityEngine;

public class VoxelGenerator
{
    private Blocks blockRegistry;
    private Dictionary<byte, string> blockIds;

    public const float tCornerUnit = 1f / 24f;

    public Vector3[] vertices;
    public Vector3[] normals;
    public int[] triangles;
    public Vector2[] uv0;
    public Vector2[] uv1;
    public Vector3[] uv2;
    public Color[] color0;

    public int vertCount = 0;
    public int triCount = 0;

    World world;
    public int chunkSize = 32;
    int chunkX;
    int chunkY;
    int chunkZ;

    public void Initialize(int chunkSize, Blocks blocks)
    {
        this.chunkSize = chunkSize;
        blockRegistry = blocks;
        blockIds = new Dictionary<byte, string>()
        {
            { 1, "base:block/stone" },
            { 2, "base:block/grass" },
            { 3, "base:block/dirt" },
            { 4, "base:block/cobblestone" },
            { 5, "base:block/bedrock" },
            { 6, "base:block/bricks" },
            { 7, "base:block/clay" },
            { 8, "base:block/coal_ore" },
            { 9, "base:block/crafting_table" },
            { 10, "base:block/diamond_ore" },
            { 11, "base:block/gravel" },
            { 12, "base:block/iron_ore" },
            { 13, "base:block/oak_leaves" },
            { 14, "base:block/oak_log" },
            { 15, "base:block/oak_planks" },
            { 16, "base:block/sand" },
            { 17, "base:block/snow" },
            { 18, "base:block/tnt" }
        };

        vertices = new Vector3[chunkSize * chunkSize * chunkSize * 4];
        normals = new Vector3[chunkSize * chunkSize * chunkSize * 4];
        triangles = new int[chunkSize * chunkSize * chunkSize * 6];
        uv0 = new Vector2[chunkSize * chunkSize * chunkSize * 4];
        uv1 = new Vector2[chunkSize * chunkSize * chunkSize * 4];
        uv2 = new Vector3[chunkSize * chunkSize * chunkSize * 4];
        color0 = new Color[chunkSize * chunkSize * chunkSize * 4];

        Debug.Log("Initialized VoxelGenerator");
        int size = sizeof(float) * (
                        vertices.Length * 3 + 
                        normals.Length * 3 + 
                        uv0.Length * 2 + 
                        uv1.Length * 2 + 
                        uv2.Length * 3 + 
                        color0.Length * 4
                    ) + sizeof(int) * triangles.Length;
        Debug.Log("Memory Usage: " + (size / 1024.0 / 1024.0).ToString("0.00") + "MB");
    }


    public void GenerateMesh(World world, int chunkX, int chunkY, int chunkZ)
    {
        this.world = world;
        this.chunkX = chunkX;
        this.chunkY = chunkY;
        this.chunkZ = chunkZ;
        triCount = 0;
        vertCount = 0;

        for (int x = 0; x < chunkSize; x++)
        {
            for (int z = 0; z < chunkSize; z++)
            {
                for (int y = chunkSize - 1; y >= 0; y--)
                {
                    //This code will run for every block in the chunk
                    if (Block(x, y, z) != 0)
                    {
                        var block = blockRegistry.blocks[blockIds[Block(x, y, z)]];
                        block.material.Build(this, x, y, z);
                    }
                }
            }
        }
    }
    
    public void SpherizeMesh(int cx, int cy, int cz, int chunkRegion)
    {
        var distortion = PlanetMath.GetPoints(cx + chunkX / chunkSize, cy + chunkY / chunkSize, cz + chunkZ / chunkSize, chunkRegion);
        /*
        Quaternion rotation = Quaternion.LookRotation(
            (distortion.s001 - distortion.s000).normalized, 
            (distortion.s010 - distortion.s000).normalized
            );
        */

        for (int i = 0; i < vertCount; i++)
        {
            var vertex = vertices[i];
            var blockPos = vertex + new Vector3(cx * chunkSize + chunkX, cy * chunkSize + chunkY, cz * chunkSize + chunkZ);

            var sphere_vert = PlanetMath.RegionToSphere(blockPos, chunkRegion);

            vertices[i] = sphere_vert - distortion.s000;
            // normals[i] = rotation * normals[i];
        }
    }

    public byte Block(int x, int y, int z)
    {
        return world.Block(x + chunkX, y + chunkY, z + chunkZ);
    }
    
    public void ApplyMesh(Mesh mesh)
    {
        Vector3[] new_vertices = new Vector3[vertCount];
        Vector3[] new_normals = new Vector3[vertCount];
        int[] new_triangles = new int[triCount];
        Vector2[] new_uv0 = new Vector2[vertCount];
        Vector2[] new_uv1 = new Vector2[vertCount];
        Vector3[] new_uv2 = new Vector3[vertCount];
        Color[] new_color0 = new Color[vertCount];

        Array.Copy(vertices, new_vertices, vertCount);
        Array.Copy(normals, new_normals, vertCount);
        Array.Copy(triangles, new_triangles, triCount);
        Array.Copy(uv0, new_uv0, vertCount);
        Array.Copy(uv1, new_uv1, vertCount);
        Array.Copy(uv2, new_uv2, vertCount);
        Array.Copy(color0, new_color0, vertCount);

        mesh.Clear();
        // mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = new_vertices;
        mesh.SetUVs(0, new_uv0);
        mesh.SetUVs(1, new_uv1);
        mesh.SetUVs(2, new_uv2);
        mesh.triangles = new_triangles;
        mesh.colors = new_color0;
        mesh.normals = new_normals;
        // mesh.Optimize();
        mesh.RecalculateNormals();
    }
}
