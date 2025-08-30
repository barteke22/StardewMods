using Microsoft.Xna.Framework;
using StardewValley;
using System;
using Object = StardewValley.Object;

namespace FishingMinigames
{
    public class MinigameMessage
    {
        public long multiplayerID;
        public string stage;
        public string voiceType;
        public float voicePitch;
        public bool drawAttachments;
        public string whichFish;
        public int fishQuality;
        public int maxFishSize;
        public float fishSize;
        public float itemSpriteSize;
        public int stack;
        public bool recordSize;
        public bool furniture;
        public Rectangle sourceRect;
        public int x;
        public int y;
        public int oldFacingDirection;


        public MinigameMessage()
        {
            this.multiplayerID = -1L;
            this.stage = null;
            this.sourceRect = new Rectangle();
        }

        public MinigameMessage(Farmer whichPlayer, string stage, string voiceType, float voicePitch, bool drawAttachments, string whichFish, int fishQuality, int maxFishSize, float fishSize, float itemSpriteSize, int stack, bool recordSize, bool furniture, Rectangle sourceRect, int x, int y, int oldFacingDirection)
        {
            this.multiplayerID = whichPlayer.UniqueMultiplayerID;
            this.stage = stage;
            this.voiceType = voiceType;
            this.voicePitch = voicePitch;
            this.drawAttachments = drawAttachments;
            this.whichFish = whichFish;
            this.fishQuality = fishQuality;
            this.maxFishSize = maxFishSize;
            this.fishSize = fishSize;
            this.itemSpriteSize = itemSpriteSize;
            this.stack = stack;
            this.recordSize = recordSize;
            this.furniture = furniture;
            this.sourceRect = sourceRect;
            this.x = x;
            this.y = y;
            this.oldFacingDirection = oldFacingDirection;
        }
    }


    class MinigameColor
    {
        public Color color;
        public Vector2 pos = new Vector2(0f);
        public int whichSlider = 0;
    }
    class DummyMenu : StardewValley.Menus.IClickableMenu
    {
        public DummyMenu()
        {
            //this is just to prevent other mods from interfering with minigames
        }
    }


    //[HarmonyPatch(typeof(Tool), "getDescription")]
    class HarmonyPatches
    {
        public static void getDescription_Nets(ref string __result, ref Tool __instance)
        {
            try
            {
                if (__instance is StardewValley.Tools.FishingRod rod && __instance.UpgradeLevel != 1)//bamboo+ (except training)
                {
                    string desc = FishingMinigames.ModEntry.AddEffectDescriptions(__instance.Name, __instance.Description);

                    if (__instance.UpgradeLevel > 1)//fiber/iridium
                    {
                        Object bait = rod.GetBait();
                        if (bait != null)
                        {
                            desc += "\n\n" + bait.DisplayName + ((bait.Quality == 0) ? "" : " (" + FishingMinigames.ModEntry.translate.Get("Mods.Infinite") + ")")
                                   + ":\n" + FixDescriptionNewlines(bait.getDescription());
                        }
                        foreach (var tackle in rod.GetTackle())
                        {
                            if (tackle != null)
                            {
                                desc += "\n\n" + tackle.DisplayName + ((tackle.Quality == 0) ? "" : " (" + FishingMinigames.ModEntry.translate.Get("Mods.Infinite") + ")")
                                       + ":\n" + FixDescriptionNewlines(tackle.getDescription());
                            }
                        }
                    }
                    if (desc.EndsWith("\n")) desc = desc.Substring(0, desc.Length - 1);
                    __result = Game1.parseText(desc, Game1.smallFont, desc.Length * 10);
                }
            }
            catch (System.Exception e)
            {
                Log.Error("Error in harmony patch: " + e.Message);
            }
        }
        internal static string FixDescriptionNewlines(string desc)
        {
            var lines = desc.Split(Environment.NewLine, StringSplitOptions.None);
            desc = null;
            foreach (var s in lines)
            {
                if (desc != null && !string.IsNullOrEmpty(s))
                {
                    if (s.StartsWith(" * ")) desc += '\n';
                    else if (char.IsUpper(s[0])) desc += ' ';
                }
                desc += s;
            }
            return desc;
        }
    }
}
