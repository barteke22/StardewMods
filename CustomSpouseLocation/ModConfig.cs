
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace StardewMods
{
    internal class ModConfig//Defaults with 0fs are ignored
    {
        public string SpritePreviewName { get; set; } = "";
        //spouse room
        public string SpouseRoom_Auto_Blacklist { get; set; } = "";
        public int SpouseRoom_Auto_Chance { get; set; } = 100;
        public bool SpouseRoom_Auto_PerformanceMode { get; set; } = false;
        public string SpouseRoom_Auto_MapChairs_DownOnly_Blacklist { get; set; } = "All";
        public string SpouseRoom_Auto_FurnitureChairs_UpOnly_Blacklist { get; set; } = "";
        public Dictionary<string, Vector2> SpouseRoom_Auto_Facing_TileOffset { get; set; } = new Dictionary<string, Vector2>()
        {
            { "Default", new Vector2(-1f, 0f) },
            { "sebastianFrog", new Vector2(0f, 1f) }
        };
        public Dictionary<string, List<KeyValuePair<string, Vector2>>> SpouseRoom_Manual_TileOffsets { get; set; } = new Dictionary<string, List<KeyValuePair<string, Vector2>>>()
        {
            { "Default", new List<KeyValuePair<string, Vector2>>() { new KeyValuePair<string, Vector2>("Up", Vector2.Zero) } },
            { "sebastianFrog", new List<KeyValuePair<string, Vector2>>() { new KeyValuePair<string, Vector2>("Down", Vector2.Zero) } }
        };

        //spouse kitchen
        public Dictionary<string, List<KeyValuePair<string, Vector2>>> Kitchen_Manual_TileOffsets { get; set; } = new Dictionary<string, List<KeyValuePair<string, Vector2>>>()
        {
            { "Default", new List<KeyValuePair<string, Vector2>>() { new KeyValuePair<string, Vector2>("Down", Vector2.Zero) } }
        };
        //spouse patio
        public Dictionary<string, List<KeyValuePair<string, Vector2>>> Patio_Manual_TileOffsets { get; set; } = new Dictionary<string, List<KeyValuePair<string, Vector2>>>()
        {
            { "Default", new List<KeyValuePair<string, Vector2>>() { new KeyValuePair<string, Vector2>("Down", Vector2.Zero) } }
        };
        //spouse porch
        public Dictionary<string, List<KeyValuePair<string, Vector2>>> Porch_Manual_TileOffsets { get; set; } = new Dictionary<string, List<KeyValuePair<string, Vector2>>>()
        {
            { "Default", new List<KeyValuePair<string, Vector2>>() { new KeyValuePair<string, Vector2>("Down", Vector2.Zero) } }
        };
    }
}
