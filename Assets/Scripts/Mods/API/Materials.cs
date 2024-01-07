using Jint;
using Jint.Native;
using Jint.Native.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PlanetCraft.Mods.API
{
    public class BakedMaterial
    {
        public int[] textures;

        public void Build(VoxelGenerator gen, int x, int y, int z)
        {
            if (gen.Block(x, y + 1, z) == 0)
                CubeTop(gen, x, y, z);

            if (gen.Block(x, y - 1, z) == 0)
                CubeBot(gen, x, y, z);

            if (gen.Block(x + 1, y, z) == 0)
                CubeEast(gen, x, y, z);

            if (gen.Block(x - 1, y, z) == 0)
                CubeWest(gen, x, y, z);

            if (gen.Block(x, y, z + 1) == 0)
                CubeNorth(gen, x, y, z);

            if (gen.Block(x, y, z - 1) == 0)
                CubeSouth(gen, x, y, z);
        }
        
        void CubeTop(VoxelGenerator gen, int x, int y, int z)
        {
            gen.vertices[gen.vertCount + 0] = (new Vector3(x, y + 1, z + 1));
            gen.vertices[gen.vertCount + 1] = (new Vector3(x + 1, y + 1, z + 1));
            gen.vertices[gen.vertCount + 2] = (new Vector3(x + 1, y + 1, z));
            gen.vertices[gen.vertCount + 3] = (new Vector3(x, y + 1, z));

            gen.normals[gen.vertCount + 0] = (Vector3.up);
            gen.normals[gen.vertCount + 1] = (Vector3.up);
            gen.normals[gen.vertCount + 2] = (Vector3.up);
            gen.normals[gen.vertCount + 3] = (Vector3.up);

            int textureIndex = textures[0];

            float cr = (x + .5f) / (float)gen.chunkSize;
            float cg = (y + .5f) / (float)gen.chunkSize;
            float cb = (z + .5f) / (float)gen.chunkSize;

            gen.color0[gen.vertCount + 0] = (new Color(cr, cg, cb, 0 * VoxelGenerator.tCornerUnit));
            gen.color0[gen.vertCount + 1] = (new Color(cr, cg, cb, 1 * VoxelGenerator.tCornerUnit));
            gen.color0[gen.vertCount + 2] = (new Color(cr, cg, cb, 2 * VoxelGenerator.tCornerUnit));
            gen.color0[gen.vertCount + 3] = (new Color(cr, cg, cb, 3 * VoxelGenerator.tCornerUnit));

            Cube(gen, textureIndex);
        }

        void CubeNorth(VoxelGenerator gen, int x, int y, int z)
        {

            gen.vertices[gen.vertCount + 0] = (new Vector3(x + 1, y, z + 1));
            gen.vertices[gen.vertCount + 1] = (new Vector3(x + 1, y + 1, z + 1));
            gen.vertices[gen.vertCount + 2] = (new Vector3(x, y + 1, z + 1));
            gen.vertices[gen.vertCount + 3] = (new Vector3(x, y, z + 1));

            gen.normals[gen.vertCount + 0] = (Vector3.forward);
            gen.normals[gen.vertCount + 1] = (Vector3.forward);
            gen.normals[gen.vertCount + 2] = (Vector3.forward);
            gen.normals[gen.vertCount + 3] = (Vector3.forward);

            int textureIndex = textures[1];

            float cr = (x + .5f) / (float)gen.chunkSize;
            float cg = (y + .5f) / (float)gen.chunkSize;
            float cb = (z + .5f) / (float)gen.chunkSize;

            gen.color0[gen.vertCount + 0] = (new Color(cr, cg, cb, 4 * VoxelGenerator.tCornerUnit));
            gen.color0[gen.vertCount + 1] = (new Color(cr, cg, cb, 5 * VoxelGenerator.tCornerUnit));
            gen.color0[gen.vertCount + 2] = (new Color(cr, cg, cb, 6 * VoxelGenerator.tCornerUnit));
            gen.color0[gen.vertCount + 3] = (new Color(cr, cg, cb, 7 * VoxelGenerator.tCornerUnit));

            Cube(gen, textureIndex);
        }

        void CubeEast(VoxelGenerator gen, int x, int y, int z)
        {

            gen.vertices[gen.vertCount + 0] = (new Vector3(x + 1, y, z));
            gen.vertices[gen.vertCount + 1] = (new Vector3(x + 1, y + 1, z));
            gen.vertices[gen.vertCount + 2] = (new Vector3(x + 1, y + 1, z + 1));
            gen.vertices[gen.vertCount + 3] = (new Vector3(x + 1, y, z + 1));

            gen.normals[gen.vertCount + 0] = (Vector3.right);
            gen.normals[gen.vertCount + 1] = (Vector3.right);
            gen.normals[gen.vertCount + 2] = (Vector3.right);
            gen.normals[gen.vertCount + 3] = (Vector3.right);

            int textureIndex = textures[2];

            float cr = (x + .5f) / (float)gen.chunkSize;
            float cg = (y + .5f) / (float)gen.chunkSize;
            float cb = (z + .5f) / (float)gen.chunkSize;

            gen.color0[gen.vertCount + 0] = (new Color(cr, cg, cb, 8 * VoxelGenerator.tCornerUnit));
            gen.color0[gen.vertCount + 1] = (new Color(cr, cg, cb, 9 * VoxelGenerator.tCornerUnit));
            gen.color0[gen.vertCount + 2] = (new Color(cr, cg, cb, 10 * VoxelGenerator.tCornerUnit));
            gen.color0[gen.vertCount + 3] = (new Color(cr, cg, cb, 11 * VoxelGenerator.tCornerUnit));

            Cube(gen, textureIndex);
        }

        void CubeBot(VoxelGenerator gen, int x, int y, int z)
        {

            gen.vertices[gen.vertCount + 0] = (new Vector3(x, y, z));
            gen.vertices[gen.vertCount + 1] = (new Vector3(x + 1, y, z));
            gen.vertices[gen.vertCount + 2] = (new Vector3(x + 1, y, z + 1));
            gen.vertices[gen.vertCount + 3] = (new Vector3(x, y, z + 1));

            gen.normals[gen.vertCount + 0] = (Vector3.down);
            gen.normals[gen.vertCount + 1] = (Vector3.down);
            gen.normals[gen.vertCount + 2] = (Vector3.down);
            gen.normals[gen.vertCount + 3] = (Vector3.down);

            int textureIndex = textures[3];

            float cr = (x + .5f) / (float)gen.chunkSize;
            float cg = (y + .5f) / (float)gen.chunkSize;
            float cb = (z + .5f) / (float)gen.chunkSize;

            gen.color0[gen.vertCount + 0] = (new Color(cr, cg, cb, 12 * VoxelGenerator.tCornerUnit));
            gen.color0[gen.vertCount + 1] = (new Color(cr, cg, cb, 13 * VoxelGenerator.tCornerUnit));
            gen.color0[gen.vertCount + 2] = (new Color(cr, cg, cb, 14 * VoxelGenerator.tCornerUnit));
            gen.color0[gen.vertCount + 3] = (new Color(cr, cg, cb, 15 * VoxelGenerator.tCornerUnit));

            Cube(gen, textureIndex);
        }

        void CubeSouth(VoxelGenerator gen, int x, int y, int z)
        {

            gen.vertices[gen.vertCount + 0] = (new Vector3(x, y, z));
            gen.vertices[gen.vertCount + 1] = (new Vector3(x, y + 1, z));
            gen.vertices[gen.vertCount + 2] = (new Vector3(x + 1, y + 1, z));
            gen.vertices[gen.vertCount + 3] = (new Vector3(x + 1, y, z));

            gen.normals[gen.vertCount + 0] = (Vector3.back);
            gen.normals[gen.vertCount + 1] = (Vector3.back);
            gen.normals[gen.vertCount + 2] = (Vector3.back);
            gen.normals[gen.vertCount + 3] = (Vector3.back);

            int textureIndex = textures[4];

            float cr = (x + .5f) / (float)gen.chunkSize;
            float cg = (y + .5f) / (float)gen.chunkSize;
            float cb = (z + .5f) / (float)gen.chunkSize;

            gen.color0[gen.vertCount + 0] = (new Color(cr, cg, cb, 16 * VoxelGenerator.tCornerUnit));
            gen.color0[gen.vertCount + 1] = (new Color(cr, cg, cb, 17 * VoxelGenerator.tCornerUnit));
            gen.color0[gen.vertCount + 2] = (new Color(cr, cg, cb, 18 * VoxelGenerator.tCornerUnit));
            gen.color0[gen.vertCount + 3] = (new Color(cr, cg, cb, 19 * VoxelGenerator.tCornerUnit));

            Cube(gen, textureIndex);
        }

        void CubeWest(VoxelGenerator gen, int x, int y, int z)
        {

            gen.vertices[gen.vertCount + 0] = (new Vector3(x, y, z + 1));
            gen.vertices[gen.vertCount + 1] = (new Vector3(x, y + 1, z + 1));
            gen.vertices[gen.vertCount + 2] = (new Vector3(x, y + 1, z));
            gen.vertices[gen.vertCount + 3] = (new Vector3(x, y, z));

            gen.normals[gen.vertCount + 0] = (Vector3.left);
            gen.normals[gen.vertCount + 1] = (Vector3.left);
            gen.normals[gen.vertCount + 2] = (Vector3.left);
            gen.normals[gen.vertCount + 3] = (Vector3.left);

            int textureIndex = textures[5];

            float cr = (x + .5f) / (float)gen.chunkSize;
            float cg = (y + .5f) / (float)gen.chunkSize;
            float cb = (z + .5f) / (float)gen.chunkSize;

            gen.color0[gen.vertCount + 0] = (new Color(cr, cg, cb, 20 * VoxelGenerator.tCornerUnit));
            gen.color0[gen.vertCount + 1] = (new Color(cr, cg, cb, 21 * VoxelGenerator.tCornerUnit));
            gen.color0[gen.vertCount + 2] = (new Color(cr, cg, cb, 22 * VoxelGenerator.tCornerUnit));
            gen.color0[gen.vertCount + 3] = (new Color(cr, cg, cb, 23 * VoxelGenerator.tCornerUnit));

            Cube(gen, textureIndex);
        }

        void Cube(VoxelGenerator gen, int textureIndex)
        {
            gen.triangles[gen.triCount + 0] = (gen.vertCount); //1
            gen.triangles[gen.triCount + 1] = (gen.vertCount + 1); //2
            gen.triangles[gen.triCount + 2] = (gen.vertCount + 2); //3
            gen.triangles[gen.triCount + 3] = (gen.vertCount); //1
            gen.triangles[gen.triCount + 4] = (gen.vertCount + 2); //3
            gen.triangles[gen.triCount + 5] = (gen.vertCount + 3); //4

            gen.uv0[gen.vertCount + 0] = new Vector2(textureIndex, 0);
            gen.uv0[gen.vertCount + 1] = new Vector2(textureIndex, 0);
            gen.uv0[gen.vertCount + 2] = new Vector2(textureIndex, 0);
            gen.uv0[gen.vertCount + 3] = new Vector2(textureIndex, 0);

            gen.uv1[gen.vertCount + 0] = new Vector2(1, 0);
            gen.uv1[gen.vertCount + 1] = new Vector2(1, 1);
            gen.uv1[gen.vertCount + 2] = new Vector2(0, 1);
            gen.uv1[gen.vertCount + 3] = new Vector2(0, 0);

            // Array.Copy(gen.vertices, gen.vertCount, gen.uv2, gen.vertCount, 4);

            gen.triCount += 6;
            gen.vertCount += 4;
        }
    }

    public class Materials
    {
        public List<BakedMaterial> materials = new List<BakedMaterial>();
        public Dictionary<string, int> materialsIndices = new Dictionary<string, int>();

        AssetRegistry assets;
        Texture textures;

        public Materials(AssetRegistry assets, Texture textures)
        {
            this.assets = assets;
            this.textures = textures;
        }

        public int get(string id)
        {
            if (materialsIndices.ContainsKey(id))
                return materialsIndices[id];
            BakedMaterial bakedMaterial = new BakedMaterial();

            var material = assets.materials[id];
            var variant = material.variants["*"];
            var model = assets.models[variant["model"]];

            Dictionary<string, int> referencedTextures = variant.Where(kvp => kvp.Key.StartsWith('#')).ToDictionary(kvp => kvp.Key, kvp => textures.get(kvp.Value));
            bakedMaterial.textures = new int[]
            {
                referencedTextures[model.elements[0].faces.up.texture],
                referencedTextures[model.elements[0].faces.north.texture],
                referencedTextures[model.elements[0].faces.east.texture],
                referencedTextures[model.elements[0].faces.down.texture],
                referencedTextures[model.elements[0].faces.south.texture],
                referencedTextures[model.elements[0].faces.west.texture],
            };

            var index = materials.Count;
            materials.Add(bakedMaterial);
            materialsIndices.Add(id, index);
            return index;
        }
    }
}
