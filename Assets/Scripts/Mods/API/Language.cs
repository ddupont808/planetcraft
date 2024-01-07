using System.Collections.Generic;
using UnityEngine;

namespace PlanetCraft.Mods.API
{
    public class Language
    {
        public Dictionary<string, Dictionary<string, string>> localization = new Dictionary<string, Dictionary<string, string>>();

        public void add(string id, string language, string text) {
            if (!localization.ContainsKey(language))
                localization[language] = new Dictionary<string, string>();
            localization[language].Add(id, text);
        }
    }
}
