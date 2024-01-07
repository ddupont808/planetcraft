using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PlanetCraft.Mods.API
{
    public class Texture
    {
        public Dictionary<string, int> textureIndices = new Dictionary<string, int>();
        int defaultIndex;

        public Texture(AssetRegistry assetRegistry)
        {
            var blocks = assetRegistry.textures.Where(kvp => kvp.Key.Split(":")[1].StartsWith("block/") || kvp.Key == "base:internal/error").ToArray();
            Texture2DArray blocks2d = new Texture2DArray(16, 16, blocks.Length, TextureFormat.RGBA32, true);
            blocks2d.filterMode = FilterMode.Point;
            blocks2d.wrapMode = TextureWrapMode.Repeat;
            for (int i = 0; i < blocks.Length; i++)
            {
                if (blocks[i].Key == "base:internal/error")
                {
                    Debug.Log("Error index: " + i);
                    defaultIndex = i;
                }
                blocks2d.SetPixels(blocks[i].Value.GetPixels(0), i, 0);
                textureIndices.Add(blocks[i].Key, i);
            }
            blocks2d.Apply();
            Shader.SetGlobalTexture("_game_Blocks", blocks2d);
            Shader.SetGlobalInt("_game_Blocks_grass", get("base:block/grass"));
        }

        public int get(string id)
        {
            return textureIndices.GetValueOrDefault(id, defaultIndex);
        }
    }
}
