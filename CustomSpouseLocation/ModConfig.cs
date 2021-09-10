
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace StardewMods
{
    internal class ModConfig//Defaults with 0fs are ignored
    {
        public bool SpritePreviewMode { get; set; } = false;
        //spouse room
        public int SpouseRoom_RandomTileChance { get; set; } = 100;
        public bool SpouseRoom_RandomCanUse_MapChairs_DownOnly { get; set; } = true;
        public bool SpouseRoom_RandomCanUse_FurnitureChairs_UpOnly { get; set; } = true;
        public Dictionary<string, Vector2> SpouseRoomRandomFaceTileOffset { get; set; } = new Dictionary<string, Vector2>()
        {
            { "Default", new Vector2(-1f, 0f) },
            { "sebastianFrog", new Vector2(0f, 1f) }
        };
        public Dictionary<string, List<KeyValuePair<string, Vector2>>> SpouseRoom_ManualTileOffsets { get; set; } = new Dictionary<string, List<KeyValuePair<string, Vector2>>>()
        {
            { "Default", new List<KeyValuePair<string, Vector2>>() { new KeyValuePair<string, Vector2>("Up", Vector2.Zero) } },
            { "sebastianFrog", new List<KeyValuePair<string, Vector2>>() { new KeyValuePair<string, Vector2>("Down", Vector2.Zero) } }
        };

        //spouse kitchen
        public Dictionary<string, List<KeyValuePair<string, Vector2>>> Kitchen_TileOffsets { get; set; } = new Dictionary<string, List<KeyValuePair<string, Vector2>>>()
        {
            { "Default", new List<KeyValuePair<string, Vector2>>() { new KeyValuePair<string, Vector2>("Down", Vector2.Zero) } }
        };
        //spouse patio
        public Dictionary<string, List<KeyValuePair<string, Vector2>>> Patio_TileOffsets { get; set; } = new Dictionary<string, List<KeyValuePair<string, Vector2>>>()
        {
            { "Default", new List<KeyValuePair<string, Vector2>>() { new KeyValuePair<string, Vector2>("Down", Vector2.Zero) } }
        };
        //spouse porch
        public Dictionary<string, List<KeyValuePair<string, Vector2>>> Porch_TileOffsets { get; set; } = new Dictionary<string, List<KeyValuePair<string, Vector2>>>()
        {
            { "Default", new List<KeyValuePair<string, Vector2>>() { new KeyValuePair<string, Vector2>("Down", Vector2.Zero) } }
        };
    }
}
