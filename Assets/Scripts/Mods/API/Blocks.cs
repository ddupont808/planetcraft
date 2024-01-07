using Jint;
using Jint.Native;
using Jint.Native.Object;
using System.Collections.Generic;
using UnityEngine;

namespace PlanetCraft.Mods.API
{
    public struct Block
    {
        public BakedMaterial material;
    }

    public class Blocks
    {
        public Dictionary<string, Block> blocks = new Dictionary<string, Block>();

        AssetRegistry assets;
        Materials materials;

        public Blocks(AssetRegistry assets, Materials materials)
        {
            this.assets = assets;
            this.materials = materials;
        }

        public void add(string id, ObjectInstance block)
        {
            Debug.Log($"Registering block {id}");
            blocks.Add(id, new Block()
            {
                material = materials.materials[(int)block.Get("material").AsNumber()]
            });
        }
    }
}
