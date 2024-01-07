using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PlanetCraft.Mods.API
{
    public class Events
    {
        public Events()
        {

        }

        public void emit(string ev)
        {
            Debug.Log(ev);
        }
    }
}
