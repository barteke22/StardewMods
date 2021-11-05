using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace DiagonalAim
{
    public class ModConfig
    {
        public Keybind ModToggleKey { get; set; } = new Keybind(SButton.None);
        public bool AllowAnyToolOnStandingTile { get; set; } = true;
        public int ExtraReachRadius { get; set; } = 0;
    }
}