
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
            { "Default", new Vector2(-1.1f, 0f) }
        };
        public Dictionary<string, List<KeyValuePair<string, Vector2>>> SpouseRoom_Manual_TileOffsets { get; set; } = new Dictionary<string, List<KeyValuePair<string, Vector2>>>()
        {
            { "Default", new List<KeyValuePair<string, Vector2>>() { new KeyValuePair<string, Vector2>("Up", new Vector2(-999f)) } }
        };

        //spouse kitchen
        public Dictionary<string, List<KeyValuePair<string, Vector2>>> Kitchen_Manual_TileOffsets { get; set; } = new Dictionary<string, List<KeyValuePair<string, Vector2>>>()
        {
            { "Default", new List<KeyValuePair<string, Vector2>>() { new KeyValuePair<string, Vector2>("Down", new Vector2(-999f)) } }
        };
        //spouse patio
        public Dictionary<string, List<KeyValuePair<string, Vector2>>> Patio_Manual_TileOffsets { get; set; } = new Dictionary<string, List<KeyValuePair<string, Vector2>>>()
        {
            { "Default", new List<KeyValuePair<string, Vector2>>() { new KeyValuePair<string, Vector2>("Down", new Vector2(-999f)) } }
        };
        //spouse porch
        public Dictionary<string, List<KeyValuePair<string, Vector2>>> Porch_Manual_TileOffsets { get; set; } = new Dictionary<string, List<KeyValuePair<string, Vector2>>>()
        {
            { "Default", new List<KeyValuePair<string, Vector2>>() { new KeyValuePair<string, Vector2>("Down", new Vector2(-999f)) } }
        };
    }
}
