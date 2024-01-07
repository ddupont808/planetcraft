using Esprima.Ast;
using Jint;
using Jint.Runtime;
using Newtonsoft.Json;
using PlanetCraft.Mods.API;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class ModLoader : MonoBehaviour
{
    public string modDirectory = "mods";

    public PlanetCraft.Mods.API.Console console;
    public PlanetCraft.Mods.API.Language language;
    public PlanetCraft.Mods.API.Blocks blocksRegistry;
    public PlanetCraft.Mods.API.Texture textures;
    public PlanetCraft.Mods.API.Events events;
    public PlanetCraft.Mods.API.Materials materials;

    void Awake()
    {
        var modsPath = Path.Join(Application.persistentDataPath, modDirectory);
        if(!Directory.Exists(modsPath))
        {
            Debug.Log("Creating " + modsPath);
            Directory.CreateDirectory(modsPath);
        }

        var mods = Directory.GetDirectories(modsPath);

        Debug.Log($"Mounting {mods.Length} mods from {modsPath}");
        AssetRegistry assetRegistry = new AssetRegistry(modsPath);
        foreach (var mod in mods)
            assetRegistry.Mount(mod);

        Debug.Log("Initializing API");
        console = new PlanetCraft.Mods.API.Console();
        language = new PlanetCraft.Mods.API.Language();
        textures = new PlanetCraft.Mods.API.Texture(assetRegistry);
        events = new PlanetCraft.Mods.API.Events();
        materials = new PlanetCraft.Mods.API.Materials(assetRegistry, textures);
        blocksRegistry = new PlanetCraft.Mods.API.Blocks(assetRegistry, materials);

        Debug.Log("Starting JS Engine");
        Engine engine;
        {
            var blocks = assetRegistry.scripts.Where(kvp => kvp.Key.Split(":")[1].StartsWith("block/")).Select(kvp => kvp.Key).ToArray();
            engine = new Engine()
                .SetValue("console", console)
                .SetValue("language", language)
                .SetValue("blocks", blocksRegistry)
                .SetValue("textures", textures)
                .SetValue("events", events)
                .SetValue("materials", materials);
            engine.AddModule("internal/assetRegistry", module =>
                module.ExportObject("blocks", blocks));
            foreach (var script in assetRegistry.scripts)
                engine.AddModule(script.Key, script.Value);
        }
        Debug.Log("Executing autorun");
        {
            var autorun = assetRegistry.scripts.Where(kvp => kvp.Key.Split(":")[1].StartsWith("autorun/"));
            foreach (var script in autorun)
                engine.ImportModule(script.Key);
        }

        /*
        Debug.Log($"Loading {mods.Length} mods from {modsPath}");

        List<string> blockModules = new List<string>();
        Dictionary<string, string> moduleSources = new Dictionary<string, string>();
        foreach (var mod in mods)
        {
            var ns = Path.GetFileName(mod);

            var assets = Path.Join(mod, "assets");
            foreach (var blockstates in Directory.EnumerateFiles(Path.Join(assets, "blockstates"), "*.json", SearchOption.AllDirectories))
            {
                var path = Path.ChangeExtension(Path.GetRelativePath(Path.Join(assets, "blockstates"), blockstates).Replace(@"\", "/"), null);
                var source = File.ReadAllText(blockstates);
                var blockstate = JsonConvert.DeserializeObject<Blockstate>(source);
                assetRegistry.RegisterBlockstate(ns, path, blockstate);
            }

            foreach (var models in Directory.EnumerateFiles(Path.Join(assets, "models"), "*.json", SearchOption.AllDirectories))
            {
                var path = Path.ChangeExtension(Path.GetRelativePath(Path.Join(assets, "models"), models).Replace(@"\", "/"), null);
                var source = File.ReadAllText(models);
                var model = JsonConvert.DeserializeObject<Model>(source);
                assetRegistry.RegisterModel(ns, path, model);
            }

            foreach (var textures in Directory.EnumerateFiles(Path.Join(assets, "textures"), "*.png", SearchOption.AllDirectories))
            {
                var path = Path.ChangeExtension(Path.GetRelativePath(Path.Join(assets, "textures"), textures).Replace(@"\", "/"), null);
                var source = File.ReadAllBytes(textures);
                var tex = new Texture2D(2, 2);
                tex.LoadImage(source, true);
                assetRegistry.RegisterTexture(ns, path, tex);
            }

            var scripts = Path.Join(mod, "scripts");
            foreach(var script in Directory.EnumerateFiles(scripts, "*.js", SearchOption.AllDirectories))
            {
                var path = Path.ChangeExtension(Path.GetRelativePath(scripts, script).Replace(@"\", "/"), null);
                var id = $"{ns}:{path}";
                var source = File.ReadAllText(script);

                if(path.StartsWith("block/"))
                {
                    source = $@"export const block = {{}};
Object.defineProperty(block, ""id"", {{
    value: ""{id}"",
    writable: false,
    enumerable: true,
    configurable: false
}});
{source}";
                    blockModules.Add(id);
                }

                engine.AddModule($"{id}", source);
                moduleSources.Add(id, source);
            }
        }

        Debug.Log("Registering " + blockModules.Count + " blocks");
        foreach(var moduleId in blockModules)
        {
            try
            {
                var module = engine.ImportModule(moduleId);
                var block = module.Get("block");
            } catch(JavaScriptException ex)
            {
                throw new Exception($"{ex.Error} \n\n{string.Join("\n", moduleSources[moduleId].Split('\n').Select((line, idx) => $"{idx + 1}: {line}"))}");
            }
        }
        */
    }
}
