using Jint;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;

public struct VoxelMaterial
{
    public Dictionary<string, Dictionary<string, string>> variants;
}

public struct ModelElementFace
{
    public string texture;
}

public struct ModelFaces
{
    public ModelElementFace down;
    public ModelElementFace up;
    public ModelElementFace north;
    public ModelElementFace south;
    public ModelElementFace west;
    public ModelElementFace east;
}

public struct ModelElement
{
    public int[] from;
    public int[] to;
    public ModelFaces faces;
}

public struct Model
{
    public string parent;
    public Dictionary<string, string> textures;
    public ModelElement[] elements;
}

public class AssetRegistry
{
    public Dictionary<string, VoxelMaterial> materials;
    public Dictionary<string, Model> models;
    public Dictionary<string, UnityEngine.Texture2D> textures;
    public Dictionary<string, string> scripts;

    private string modsPath;

    public AssetRegistry(string modsPath)
    {
        this.modsPath = modsPath;
        materials = new Dictionary<string, VoxelMaterial>();
        models = new Dictionary<string, Model>();
        textures = new Dictionary<string, UnityEngine.Texture2D>();
        scripts = new Dictionary<string, string>();
    }

    public void Mount(string mod)
    {
        var rootNamespace = Path.GetFileName(mod);
        foreach(var type in new[] {"scripts", "models", "materials", "textures"})
        {
            switch(type)
            {
                case "scripts":
                    var scriptsPath = Path.Join(mod, "scripts");
                    if (!Directory.Exists(scriptsPath)) break;
                    foreach (var script in Directory.EnumerateFiles(scriptsPath, "*.js", SearchOption.AllDirectories))
                    {
                        var path = Path.ChangeExtension(Path.GetRelativePath(scriptsPath, script).Replace(@"\", "/"), null);
                        var source = File.ReadAllText(script);
                        var id = $"{rootNamespace}:{path}";

                        if (path.StartsWith("block/"))
                        {
                            source = $@"export const block = {{}};
Object.defineProperty(block, ""id"", {{
    value: ""{id}"",
    writable: false,
    enumerable: true,
    configurable: false
}});
{source}";
                        }

                        RegisterScript(rootNamespace, path, source);
                    }
                    break;
                case "models":
                    var modelsPath = Path.Join(mod, "models");
                    if (!Directory.Exists(modelsPath)) break;
                    foreach (var models in Directory.EnumerateFiles(modelsPath, "*.json", SearchOption.AllDirectories))
                    {
                        var path = Path.ChangeExtension(Path.GetRelativePath(modelsPath, models).Replace(@"\", "/"), null);
                        var source = File.ReadAllText(models);
                        var model = JsonConvert.DeserializeObject<Model>(source);
                        RegisterModel(rootNamespace, path, model);
                    }
                    break;
                case "materials":
                    var materialsPath = Path.Join(mod, "materials");
                    if (!Directory.Exists(materialsPath)) break;
                    foreach (var materials in Directory.EnumerateFiles(materialsPath, "*.json", SearchOption.AllDirectories))
                    {
                        var path = Path.ChangeExtension(Path.GetRelativePath(materialsPath, materials).Replace(@"\", "/"), null);
                        var source = File.ReadAllText(materials);
                        var material = JsonConvert.DeserializeObject<VoxelMaterial>(source);
                        RegisterMaterial(rootNamespace, path, material);
                    }
                    break;
                case "textures":
                    var texturesPath = Path.Join(mod, "textures");
                    if (!Directory.Exists(texturesPath)) break;
                    foreach (var texture in Directory.EnumerateFiles(texturesPath, "*.png", SearchOption.AllDirectories))
                    {
                        var path = Path.ChangeExtension(Path.GetRelativePath(texturesPath, texture).Replace(@"\", "/"), null);
                        var source = File.ReadAllBytes(texture);
                        var tex = new Texture2D(2, 2);
                        tex.LoadImage(source, false);
                        RegisterTexture(rootNamespace, path, tex);
                    }
                    break;
            }
        }
    }


    public void RegisterMaterial(string ns, string path, VoxelMaterial bs)
    {
        if (bs.variants != null)
            bs.variants = bs.variants.ToDictionary(kvp => kvp.Key, kvp => 
                kvp.Value.ToDictionary(kvp => kvp.Key, kvp =>
                    ResolveAssetPath(kvp.Value, ns)
                )
            );
        materials.Add(ResolveAssetPath(path, ns), bs);
    }

    public void RegisterModel(string ns, string path, Model model)
    {
        if (model.textures != null)
            model.textures = model.textures.ToDictionary(kvp => kvp.Key, kvp => ResolveAssetPath(kvp.Value, ns));
        models.Add(ResolveAssetPath(path, ns), model);
    }

    public void RegisterTexture(string ns, string path, UnityEngine.Texture2D texture)
    {
        textures.Add(ResolveAssetPath(path, ns), texture);
    }
    public void RegisterScript(string ns, string path, string source)
    {
        scripts.Add(ResolveAssetPath(path, ns), source);
    }

    public string ResolveAssetPath(string path, string defaultNamespace)
    {
        return path.Contains(':') ? path : $"{defaultNamespace}:{path}";
    }
}
