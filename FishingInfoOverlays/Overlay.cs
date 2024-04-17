using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Force.DeepCloner;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.GameData.Locations;
using StardewValley.Internal;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Tools;
using Object = StardewValley.Object;

namespace StardewMods
{
    /// <summary>The mod entry point.</summary>
    public class Overlay
    {
        ITranslationHelper translate;
        private IMonitor Monitor;
        private IModHelper Helper;
        private IManifest ModManifest;

        private bool hideText = false;    //world fish preview data 
        private Farmer who;
        private int screen;
        private int totalPlayersOnThisPC;


        private List<string> fishHere;
        private Dictionary<string, int> fishChances;
        private Dictionary<string, int> fishChancesSlow;
        private int fishChancesModulo;
        private List<string> oldGeneric;
        private Dictionary<string, int> fishFailed;
        private bool isMinigameOther = false;

        private bool isMinigame = false;    //minigame fish preview data, Reflection
        private string miniFish;
        private bool hasSonar;                                  //MAYBE MAKE SONAR A SEPARATE SETTING: ignore (pointless item), minigame only, full mod (required for mod to work)


        public static Dictionary<string, LocationData> locationData;
        public static Dictionary<string, string> fishData;
        public static Texture2D[] background = new Texture2D[2];
        public static Color colorBg;
        public static Color colorText;


        public static int[] miniMode = new int[4];   //config values
        public static bool[] barCrabEnabled = new bool[4];
        public static Vector2[] barPosition = new Vector2[4];
        public static int[] iconMode = new int[4];
        public static float[] barScale = new float[4];
        public static int[] maxIcons = new int[4];
        public static int[] maxIconsPerRow = new int[4];
        public static int[] backgroundMode = new int[4];
        public static int extraCheckFrequency;
        public static int[] scanRadius = new int[4];
        public static bool[] showTackles = new bool[4];
        public static bool[] showPercentages = new bool[4];
        public static int[] sortMode = new int[4];
        public static bool[] uncaughtDark = new bool[4];
        public static bool[] onlyFish = new bool[4];


        public Overlay(ModEntry entry)
        {
            this.Helper = entry.Helper;
            this.Monitor = entry.Monitor;
            this.ModManifest = entry.ModManifest;
            this.translate = entry.Helper.Translation;
        }



        public void Rendered(object sender, RenderedEventArgs e)
        {
            Farmer backup = Game1.player;
            who = Game1.player;
            try
            {
                screen = Context.ScreenId;
                if (Game1.eventUp || who.CurrentItem == null ||
                    !((who.CurrentItem is FishingRod) || (who.CurrentItem.Name.Equals("Crab Pot", StringComparison.Ordinal) && barCrabEnabled[screen]))) return;//code stop conditions

                totalPlayersOnThisPC = 1;
                foreach (IMultiplayerPeer peer in Helper.Multiplayer.GetConnectedPlayers())
                {
                    if (peer.IsSplitScreen) totalPlayersOnThisPC++;
                }

                hasSonar = false;
                FishingRod rod = Game1.player.CurrentItem as FishingRod;
                if (rod != null)  //dummy workaround for preventing player from getting special items
                {
                    who = new Farmer();
                    foreach (var m in Game1.player.mailReceived) who.mailReceived.Add(m);
                    who.currentLocation = Game1.player.currentLocation;
                    who.setTileLocation(Game1.player.Tile);
                    who.setSkillLevel("Fishing", Game1.player.FishingLevel);
                    //if there's ever any downside of referencing player rod directly, use below + add bait/tackle to it
                    //FishingRod rod = (FishingRod)(Game1.player.CurrentTool as FishingRod).getOne();
                    //who.CurrentTool = rod;
                    who.CurrentTool = rod;
                    who.luckLevel.Value = Game1.player.LuckLevel;
                    foreach (var item in Game1.player.fishCaught) who.fishCaught.Add(item);
                    foreach (var m in Game1.player.secretNotesSeen) who.secretNotesSeen.Add(m);
                    hasSonar = rod.GetTackleQualifiedItemIDs().Contains("(O)SonarBobber");
                    Game1.player = who;
                }

                SpriteFont font = Game1.smallFont;
                var miniData = ItemRegistry.GetDataOrErrorItem(miniFish);                                   //UI INIT
                Rectangle source = miniData.GetSourceRect();
                SpriteBatch batch = Game1.spriteBatch;

                batch.End();    //stop current UI drawing and start mode where where layers work from 0f-1f
                batch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise);

                //MINIGAME PREVIEW
                if (isMinigame && miniMode[screen] < 3 && Game1.activeClickableMenu is BobberBar bar && bar.scale == 1f && (hasSonar || miniMode[screen] != 1)) //scale == 1f when moving elements appear
                {
                    if (miniMode[screen] < 2) //Full minigame
                    {
                        //rod+bar textture cut to only cover the minigame bar
                        batch.Draw(Game1.mouseCursors, (new Vector2(bar.xPositionOnScreen + 126, bar.yPositionOnScreen + 292) + bar.everythingShake),
                            new Rectangle(658, 1998, 15, 149), Color.White * bar.scale, 0f, new Vector2(18.5f, 74f) * bar.scale, (4f * bar.scale), SpriteEffects.None, 0.01f);

                        //green moving bar player controls
                        batch.Draw(Game1.mouseCursors, (new Vector2(bar.xPositionOnScreen + 64, bar.yPositionOnScreen + 12 + (int)bar.bobberBarPos) + bar.barShake + bar.everythingShake),
                            new Rectangle(682, 2078, 9, 2), bar.bobberInBar ? Color.White : (Color.White * 0.25f * ((float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 100.0), 2) + 2f)), 0f, Vector2.Zero, (4f), SpriteEffects.None, 0.89f);
                        batch.Draw(Game1.mouseCursors, (new Vector2(bar.xPositionOnScreen + 64, bar.yPositionOnScreen + 12 + (int)bar.bobberBarPos + 8) + bar.barShake + bar.everythingShake),
                            new Rectangle(682, 2081, 9, 1), bar.bobberInBar ? Color.White : (Color.White * 0.25f * ((float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 100.0), 2) + 2f)), 0f, Vector2.Zero, (new Vector2(4f, bar.bobberBarHeight - 16)), SpriteEffects.None, 0.89f);
                        batch.Draw(Game1.mouseCursors, (new Vector2(bar.xPositionOnScreen + 64, bar.yPositionOnScreen + 12 + (int)bar.bobberBarPos + bar.bobberBarHeight - 8) + bar.barShake + bar.everythingShake),
                            new Rectangle(682, 2085, 9, 2), bar.bobberInBar ? Color.White : (Color.White * 0.25f * ((float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 100.0), 2) + 2f)), 0f, Vector2.Zero, (4f), SpriteEffects.None, 0.89f);

                        //treasure
                        batch.Draw(Game1.mouseCursors, (new Vector2(bar.xPositionOnScreen + 64 + 18, (float)(bar.yPositionOnScreen + 12 + 24) + bar.treasurePosition) + bar.treasureShake + bar.everythingShake),
                            new Rectangle(638, 1865, 20, 24), Color.White, 0f, new Vector2(10f, 10f), (2f * bar.treasureScale), SpriteEffects.None, 0.9f);
                        if (bar.treasureCatchLevel > 0f && !bar.treasureCaught)//treasure progress
                        {
                            batch.Draw(Game1.staminaRect, new Rectangle((int)(bar.xPositionOnScreen + 64), (int)(bar.yPositionOnScreen + 12 + (int)bar.treasurePosition), (int)(40), (int)(8)), null, Color.DimGray * 0.5f, 0f, Vector2.Zero, SpriteEffects.None, 0.9f);
                            batch.Draw(Game1.staminaRect, new Rectangle((int)(bar.xPositionOnScreen + 64), (int)(bar.yPositionOnScreen + 12 + (int)bar.treasurePosition), (int)((bar.treasureCatchLevel * 40f)), (int)(8)), null, Color.Orange, 0f, Vector2.Zero, SpriteEffects.None, 0.9f);
                        }
                    }
                    else batch.Draw(Game1.mouseCursors, (new Vector2(bar.xPositionOnScreen + 82, (bar.yPositionOnScreen + 36) + bar.bobberPosition) + bar.fishShake + bar.everythingShake),
                        new Rectangle(614 + (isBossFish ? 20 : 0), 1840, 20, 20), Color.Black, 0f, new Vector2(10f, 10f),
                        (2.05f), SpriteEffects.None, 0.9f);//Simple bar.game shadow fish

                    //fish
                    batch.Draw(miniData.GetTexture(), (new Vector2(bar.xPositionOnScreen + 82, (bar.yPositionOnScreen + 36) + bar.bobberPosition) + bar.fishShake + bar.everythingShake),
                        source, (!uncaughtDark[screen] || who.fishCaught.ContainsKey(miniData.QualifiedItemId)) ? Color.White : Color.DarkSlateGray, 0f, new Vector2(9.5f, 9f),
                        (3f), SpriteEffects.FlipHorizontally, 1f);
                }



                if (iconMode[screen] != 3)
                {
                    float iconScale = Game1.pixelZoom / 2f * barScale[screen];
                    int iconCount = 0;
                    float boxWidth = 0;
                    float boxHeight = 0;
                    Vector2 boxTopLeft = barPosition[screen];
                    Vector2 boxBottomLeft = barPosition[screen];


                    //this.Monitor.Log("\n", LogLevel.Debug);
                    if (who.currentLocation is MineShaft && who.CurrentItem.Name.Equals("Crab Pot", StringComparison.Ordinal))//crab pot
                    {
                        string warning = translate.Get("Bar.CrabMineWarning");
                        DrawStringWithBorder(batch, font, warning, boxBottomLeft + new Vector2(source.Width * iconScale, 0), Color.Red, 0f, Vector2.Zero, 1f * barScale[screen], SpriteEffects.None, 1f, colorBg); //text
                        batch.End();
                        batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise);
                        return;
                    }


                    if (rod != null && showTackles[screen])    //BAIT AND TACKLE (BOBBERS) PREVIEW
                    {
                        Object bait = rod.GetBait();
                        if (bait != null)
                        {
                            var data = ItemRegistry.GetData(bait.QualifiedItemId);
                            source = data.GetSourceRect();
                            if (backgroundMode[screen] == 0) AddBackground(batch, boxTopLeft, boxBottomLeft, iconCount, source, iconScale, boxWidth, boxHeight);

                            int baitCount = bait.Stack;
                            batch.Draw(data.GetTexture(), boxBottomLeft, source, Color.White, 0f, Vector2.Zero, 1.9f * barScale[screen], SpriteEffects.None, 0.9f);

                            if (bait.Quality == 4) batch.Draw(Game1.mouseCursors, boxBottomLeft + (new Vector2(13f, (showPercentages[screen] ? 24 : 16)) * barScale[screen]),
                                new Rectangle(346, 392, 8, 8), Color.White, 0f, Vector2.Zero, 1.9f * barScale[screen], SpriteEffects.None, 1f);
                            else Utility.drawTinyDigits(baitCount, batch, boxBottomLeft + new Vector2((source.Width * iconScale) - Utility.getWidthOfTinyDigitString(baitCount, 2f * barScale[screen]),
                                (showPercentages[screen] ? 26 : 19) * barScale[screen]), 2f * barScale[screen], 1f, colorText);

                            if (iconMode[screen] == 1) boxBottomLeft += new Vector2(0, (source.Width * iconScale) + (showPercentages[screen] ? 10 * barScale[screen] : 0));
                            else boxBottomLeft += new Vector2(source.Width * iconScale, 0);
                            iconCount++;
                        }
                        var tackles = rod.GetTackle();
                        var anyTackle = false;
                        foreach (var tackle in tackles)
                        {
                            if (tackle != null)
                            {
                                var data = ItemRegistry.GetDataOrErrorItem(tackle?.QualifiedItemId);
                                source = data.GetSourceRect();
                                if (backgroundMode[screen] == 0) AddBackground(batch, boxTopLeft, boxBottomLeft, iconCount, source, iconScale, boxWidth, boxHeight);

                                int tackleCount = FishingRod.maxTackleUses - tackle.uses.Value;
                                batch.Draw(data.GetTexture(), boxBottomLeft, source, Color.White, 0f, Vector2.Zero, 1.9f * barScale[screen], SpriteEffects.None, 0.9f);

                                if (tackle.Quality == 4) batch.Draw(Game1.mouseCursors, boxBottomLeft + (new Vector2(13f, (showPercentages[screen] ? 24 : 16)) * barScale[screen]),
                                    new Rectangle(346, 392, 8, 8), Color.White, 0f, Vector2.Zero, 1.9f * barScale[screen], SpriteEffects.None, 1f);
                                else Utility.drawTinyDigits(tackleCount, batch, boxBottomLeft + new Vector2((source.Width * iconScale) - Utility.getWidthOfTinyDigitString(tackleCount, 2f * barScale[screen]),
                                    (showPercentages[screen] ? 26 : 19) * barScale[screen]), 2f * barScale[screen], 1f, colorText);

                                if (iconMode[screen] == 1) boxBottomLeft += new Vector2(0, (source.Width * iconScale) + (showPercentages[screen] ? 10 * barScale[screen] : 0));
                                else boxBottomLeft += new Vector2(source.Width * iconScale, 0);
                                anyTackle = true;
                            }
                            if (anyTackle) iconCount++;
                        }
                        if (iconMode[screen] == 2 && (bait != null || anyTackle))
                        {
                            boxBottomLeft = boxTopLeft + new Vector2(0, (source.Width * iconScale) + (showPercentages[screen] ? 10 * barScale[screen] : 0));
                            boxWidth = (iconCount * source.Width * iconScale) + boxTopLeft.X;
                            boxHeight += (source.Width * iconScale) + (showPercentages[screen] ? 10 * barScale[screen] : 0);
                            if (bait != null && anyTackle) iconCount--;
                        }
                    }



                    bool foundWater = false;
                    Vector2 nearestWaterTile = new Vector2(99999f, 99999f);      //any water nearby + nearest water tile check
                    if (who.currentLocation.canFishHere())
                    {
                        Vector2 scanTopLeft = who.Tile - new Vector2(scanRadius[screen] + 1);
                        Vector2 scanBottomRight = who.Tile + new Vector2(scanRadius[screen] + 2);
                        for (int x = (int)scanTopLeft.X; x < (int)scanBottomRight.X; x++)
                        {
                            for (int y = (int)scanTopLeft.Y; y < (int)scanBottomRight.Y; y++)
                            {
                                if (who.currentLocation.isTileFishable(x, y) && !who.currentLocation.isTileBuildingFishable(x, y))
                                {
                                    Vector2 tile = new Vector2(x, y);
                                    float distance = Vector2.DistanceSquared(who.Tile, tile);
                                    float distanceNearest = Vector2.DistanceSquared(who.Tile, nearestWaterTile);
                                    if (distance < distanceNearest || (distance == distanceNearest && Game1.player.GetGrabTile() == tile)) nearestWaterTile = tile;
                                    foundWater = true;
                                }
                            }
                        }
                    }

                    var defaultSource = new Rectangle(0, 0, 16, 16);
                    if (foundWater)
                    {
                        if (who.CurrentItem is FishingRod)   //LOCATION FISH PREVIEW
                        {
                            who.setTileLocation(nearestWaterTile);
                            if (!isMinigame)
                            {
                                if (oldGeneric == null)
                                {
                                    oldGeneric = new();
                                    fishFailed = new();
                                    fishHere = new() { "(O)168" };
                                    fishChances = new() { { "-1", 0 }, { "(O)168", 0 } };
                                    fishChancesSlow = new();
                                    fishChancesModulo = 1;
                                }
                                AddGenericFishToList(who.currentLocation.Name);
                            }
                        }
                        else AddCrabPotFish();


                        foreach (var fish in fishHere)
                        {
                            if (onlyFish[screen] && fish != "(O)168" && !fishData.ContainsKey(fish)) continue;//skip if not fish, except trash

                            int percent = fishChancesSlow.ContainsKey(fish) ? (int)Math.Round((float)fishChancesSlow[fish] / fishChancesSlow["-1"] * 100f) : 0; //chance of this fish

                            if (iconCount < maxIcons[screen] && percent > 0)
                            {
                                var data = ItemRegistry.GetDataOrErrorItem(fish);
                                bool caught = (!uncaughtDark[screen] || who.fishCaught.ContainsKey(data.QualifiedItemId));
                                if (fish == "(O)168") caught = true;

                                iconCount++;
                                string fishNameLocalized = caught ? data.DisplayName : "???";
                                Texture2D txt2d = data.GetTexture();
                                source = data.GetSourceRect();

                                if (fishNameLocalized.StartsWith("Error Item")) continue;

                                if (fish == "(O)168") batch.Draw(txt2d, boxBottomLeft + new Vector2(2 * barScale[screen], -5 * barScale[screen]), source, (caught ? Color.White : Color.DarkSlateGray),
                                    0f, Vector2.Zero, FixScale(source, 1.9f) * barScale[screen], SpriteEffects.None, 0.98f);//icon trash
                                else batch.Draw(txt2d, FixSourceOffset(source, boxBottomLeft), source, (caught ? Color.White : Color.DarkSlateGray), 0f, Vector2.Zero, FixScale(source, 1.9f) * barScale[screen], SpriteEffects.None, 0.98f);//icon

                                if (showPercentages[screen])
                                {
                                    DrawStringWithBorder(batch, font, percent + "%", boxBottomLeft + new Vector2((8 * iconScale), 27f * barScale[screen]),
                                        (caught) ? colorText : colorText * 0.8f, 0f, new Vector2(font.MeasureString(percent + "%").X / 2f, 0f), 0.58f * barScale[screen], SpriteEffects.None, 1f, colorBg);//%
                                }

                                if (fish == miniFish && miniMode[screen] < 4) batch.Draw(background[screen], new Rectangle((int)boxBottomLeft.X - 1, (int)boxBottomLeft.Y - 1, (int)(16 * iconScale) + 1, (int)((16 * iconScale) + (showPercentages[screen] ? 10 * barScale[screen] : 0) + 1)),
                                    null, Color.GreenYellow, 0f, Vector2.Zero, SpriteEffects.None, 0.9f);//minigame outline

                                if (backgroundMode[screen] == 0) AddBackground(batch, boxTopLeft, boxBottomLeft, iconCount, defaultSource, iconScale, boxWidth, boxHeight);


                                if (iconMode[screen] == 0)      //Horizontal Preview
                                {
                                    if (iconCount % maxIconsPerRow[screen] == 0) boxBottomLeft = new Vector2(boxTopLeft.X, boxBottomLeft.Y + (16 * iconScale) + (showPercentages[screen] ? 10 * barScale[screen] : 0)); //row switch
                                    else boxBottomLeft += new Vector2(16 * iconScale, 0);
                                }
                                else                    //Vertical Preview
                                {
                                    if (iconMode[screen] == 2 && !hideText)  // + text
                                    {
                                        DrawStringWithBorder(batch, font, fishNameLocalized, boxBottomLeft + new Vector2(16 * iconScale, 0), (caught) ? colorText : colorText * 0.8f, 0f, new Vector2(0, -3), 1f * barScale[screen], SpriteEffects.None, 0.98f, colorBg); //text
                                        boxWidth = Math.Max(boxWidth, boxBottomLeft.X + (font.MeasureString(fishNameLocalized).X * barScale[screen]) + (16 * iconScale));
                                    }

                                    if (iconCount % maxIconsPerRow[screen] == 0) //row switch
                                    {
                                        if (iconMode[screen] == 2) boxBottomLeft = new Vector2(boxWidth + (20 * barScale[screen]), boxTopLeft.Y);
                                        else boxBottomLeft = new Vector2(boxBottomLeft.X + (16 * iconScale), boxTopLeft.Y);
                                    }
                                    else boxBottomLeft += new Vector2(0, (16 * iconScale) + (showPercentages[screen] ? 10 * barScale[screen] : 0));
                                    if (iconMode[screen] == 2 && iconCount <= maxIconsPerRow[screen]) boxHeight += (16 * iconScale) + (showPercentages[screen] ? 10 * barScale[screen] : 0);
                                }
                            }
                        }
                        if (backgroundMode[screen] == 1) AddBackground(batch, boxTopLeft, boxBottomLeft, iconCount, defaultSource, iconScale, boxWidth, boxHeight);
                    }
                    else if (backgroundMode[screen] == 1) AddBackground(batch, boxTopLeft, boxBottomLeft, iconCount, defaultSource, iconScale, boxWidth, boxHeight);
                }

                batch.End();
                batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise);
            }
            finally
            {
                Game1.player = backup;
            }
        }

        private bool isBossFish = false;
        public void OnMenuChanged(object sender, MenuChangedEventArgs e)   //Minigame data
        {
            if (e.NewMenu is BobberBar)
            {
                isMinigame = true;
            }
            else
            {
                isMinigame = false;
                if (e.OldMenu is BobberBar) miniFish = null;
            }
        }
        public void OnRenderMenu(object sender, RenderedActiveMenuEventArgs e)
        {
            if (Game1.activeClickableMenu is BobberBar bar && isMinigame)
            {
                miniFish = bar.whichFish;
                isBossFish = bar.bossFish;
            }
        }
        public void OnModMessageReceived(object sender, ModMessageReceivedEventArgs e)
        {
            if (e.FromModID == "barteke22.FishingMinigames")
            {
                if (e.Type == "whichFish")
                {
                    miniFish = e.ReadAs<string>();
                    if (miniFish == "-1") isMinigameOther = false;
                    else isMinigameOther = true;
                }
                if (e.Type == "hideText") hideText = e.ReadAs<bool>();
            }
        }

        private void AddGenericFishToList(string locationName)         //From GameLocation.cs getFish() - might be better to just go full dynamic??
        {
            //List<string> tempFish = new();
            //bool magicBait = who.CurrentTool is FishingRod rod && rod.HasMagicBait();
            //if (locationName.Equals("BeachNightMarket", StringComparison.Ordinal)) locationName = "Beach";

            //if (locationData.TryGetValue(locationName, out var data))
            //{
            //    foreach (var item in data.Fish)
            //    {
            //        bool passed = true;
            //        if (!magicBait)
            //        {
            //            if (item.Season != null && item.Season != Game1.getLocationFromName(locationName).GetSeason()) passed = false;
            //            if (item.BobberPosition != null && !item.BobberPosition.Value.Contains(who.Tile)) passed = false;
            //            if (item.CatchLimit != -1)
            //            {
            //                int count = 0;
            //                if (item.ItemId != null && who.fishCaught.TryGetValue(item.ItemId, out var c)) count = c[0];
            //                else if (item.RandomItemId != null)
            //                {
            //                    foreach (var i in item.RandomItemId)
            //                    {
            //                        if (who.fishCaught.TryGetValue(i, out c)) count += c[0];
            //                    }
            //                }
            //                if (count >= item.CatchLimit) passed = false;
            //            }
            //            if (item.Chance == 0f) passed = false;
            //            if (!string.IsNullOrEmpty(item.FishAreaId))
            //            {
            //                if (!who.currentLocation.TryGetFishAreaForTile(who.Tile, out var id, out _) && id != item.FishAreaId) passed = false;
            //            }
            //            if (item.MinFishingLevel > who.FishingLevel) passed = false;
            //            if (item.PlayerPosition != null && !item.PlayerPosition.Value.Contains(Game1.player.Tile)) passed = false;
            //            if (item.RequireMagicBait && !magicBait) passed = false;
            //            if (item.ToolUpgradeLevel > who.CurrentTool?.UpgradeLevel) passed = false;
            //            if (!item.IgnoreFishDataRequirements)
            //            {
            //                //try get & check
            //            }
            //        }
            //        if (passed)
            //        {
            //            List<string> tmp = null;
            //            if (item.ItemId != null)
            //            {
            //                tmp = new() { item.ItemId };
            //            }
            //            else
            //            {
            //                tmp = item.RandomItemId;
            //            }
            //            foreach (var n in tmp)
            //            {
            //                if (!tempFish.Contains(n)) tempFish.Add(n);
            //            }
            //        }
            //    }
            //    if ((tempFish.Count == 0 && oldGeneric.Count != 0) || tempFish.Count > 0 && (!(new HashSet<string>(oldGeneric).SetEquals(tempFish))))//reset lists if generic list changed
            //    {
            //        oldGeneric = tempFish.ToList();
            //        fishFailed = new();
            //        fishHere = new() { "(O)168" };
            //        fishChances = new() { { "-1", 0 }, { "(O)168", 0 } };
            //        fishChancesSlow = new();
            //        fishChancesModulo = 1;

            //        foreach (var key in oldGeneric)
            //        {
            //            if (sortMode[screen] == 0) SortItemIntoListByDisplayName(key);
            //            else fishHere.Add(key);

            //            if (!fishChances.ContainsKey(key)) fishChances.Add(key, 1);
            //        }
            //    }
            //}
            if (extraCheckFrequency == 0) AddFishGenericNew(locationName);
            else AddFishToListDynamic();
        }

        private void AddFishGenericNew(string locationName)
        {
            oldGeneric = null;
            fishChancesSlow = new() { { "-1", 100 }, { "(O)168", 0 } };//-1 represents the total

            var data = GetFishFromLocationData(locationName, who.Tile, 5, Game1.player, !who.fishCaught.Any(), false, Game1.player.currentLocation, null);

            float total = data.Sum(f => f.Value);
            var j = data.FirstOrDefault(f => junk.Contains(f.Key));
            float notJunk = j.Key != null ? j.Value : 1f;
            total -= notJunk;
            foreach (var fish in data)
            {
                if (!junk.Contains(fish.Key))
                {
                    notJunk *= 1f - fish.Value;
                }
            }
            total += notJunk;
            foreach (var fish in data)
            {
                if (!junk.Contains(fish.Key))
                {
                    if (sortMode[screen] == 0) SortItemIntoListByDisplayName(fish.Key); //sort by name
                    else fishHere.Add(fish.Key);
                    fishChancesSlow[fish.Key] = (int)Math.Round(fish.Value / total * 100);
                }
            }
            fishChancesSlow["(O)168"] = (int)Math.Round(notJunk / total * 100);

            if (sortMode[screen] == 1) SortListByPercentages(); //sort by %
        }

        /// <summary>
        /// MODIFIED COPY OF GameLocation.GetFishFromLocationData();
        /// </summary>
        internal static Dictionary<string, float> GetFishFromLocationData(string locationName, Vector2 bobberTile, int waterDepth, Farmer player, bool isTutorialCatch, bool isInherited, GameLocation location, ItemQueryContext itemQueryContext)
        {
            Dictionary<string, float> passed = [];


            if (location == null)
            {
                location = Game1.getLocationFromName(locationName);
            }
            Dictionary<string, LocationData> dictionary = DataLoader.Locations(Game1.content);
            LocationData locationData = ((location != null) ? location.GetData() : GameLocation.GetData(locationName));
            Dictionary<string, string> allFishData = DataLoader.Fish(Game1.content);
            Season season = Game1.GetSeasonForLocation(location);
            if (location == null || !location.TryGetFishAreaForTile(bobberTile, out var fishAreaId, out var _))
            {
                fishAreaId = null;
            }
            bool usingMagicBait = false;
            bool hasCuriosityLure = false;
            string baitTargetFish = null;
            bool usingGoodBait = false;
            FishingRod rod = player?.CurrentTool as FishingRod;
            if (rod != null)
            {
                usingMagicBait = rod.HasMagicBait();
                hasCuriosityLure = rod.HasCuriosityLure();
                Object bait = rod.GetBait();
                if (bait?.QualifiedItemId == "(O)SpecificBait" && bait.preservedParentSheetIndex.Value != null)
                {
                    baitTargetFish = "(O)" + bait.preservedParentSheetIndex.Value;
                }
                if (bait?.QualifiedItemId != "(O)685")
                {
                    usingGoodBait = true;
                }
            }
            Point playerTile = player.TilePoint;
            if (itemQueryContext == null)
            {
                itemQueryContext = new ItemQueryContext(location, null, Game1.random);
            }
            IEnumerable<SpawnFishData> possibleFish = dictionary["Default"].Fish;
            if (locationData != null && locationData.Fish?.Count > 0)
            {
                possibleFish = possibleFish.Concat(locationData.Fish);
            }
            possibleFish = from p in possibleFish
                           orderby p.Precedence, Game1.random.Next()
                           select p;
            int targetedBaitTries = 0;
            HashSet<string> ignoreQueryKeys = (usingMagicBait ? GameStateQuery.MagicBaitIgnoreQueryKeys : null);
            Item firstNonTargetFish = null;
            foreach (SpawnFishData spawn in possibleFish)
            {
                if ((isInherited && !spawn.CanBeInherited) || (spawn.FishAreaId != null && fishAreaId != spawn.FishAreaId) || (spawn.Season.HasValue && !usingMagicBait && spawn.Season != season))
                {
                    continue;
                }
                Microsoft.Xna.Framework.Rectangle? playerPosition = spawn.PlayerPosition;
                if (playerPosition.HasValue && !playerPosition.GetValueOrDefault().Contains(playerTile.X, playerTile.Y))
                {
                    continue;
                }
                playerPosition = spawn.BobberPosition;
                if ((playerPosition.HasValue && !playerPosition.GetValueOrDefault().Contains((int)bobberTile.X, (int)bobberTile.Y)) || player.FishingLevel < spawn.MinFishingLevel || waterDepth < spawn.MinDistanceFromShore || (spawn.MaxDistanceFromShore > -1 && waterDepth > spawn.MaxDistanceFromShore) || (spawn.RequireMagicBait && !usingMagicBait))
                {
                    continue;
                }
                float chance = spawn.GetChance(hasCuriosityLure, player.DailyLuck, player.LuckLevel, (float value, IList<QuantityModifier> modifiers, QuantityModifier.QuantityModifierMode mode) => Utility.ApplyQuantityModifiers(value, modifiers, mode, location), spawn.ItemId == baitTargetFish);
                if (spawn.UseFishCaughtSeededRandom)
                {
                    if (!Utility.CreateRandom(Game1.uniqueIDForThisGame, player.stats.Get("PreciseFishCaught") * 859).NextBool(chance))
                    {
                        continue;
                    }
                }
                if (spawn.Condition != null && !GameStateQuery.CheckConditions(spawn.Condition, location, null, null, null, null, ignoreQueryKeys))
                {
                    continue;
                }
                var items = ItemQueryResolver.TryResolve(spawn, itemQueryContext, ItemQuerySearchMode.All, avoidRepeat: false, null, (string query) => query.Replace("BOBBER_X", ((int)bobberTile.X).ToString()).Replace("BOBBER_Y", ((int)bobberTile.Y).ToString()).Replace("WATER_DEPTH", waterDepth.ToString()), delegate (string query, string error)
                {
                    DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(54, 4);
                    defaultInterpolatedStringHandler.AppendLiteral("Location '");
                    defaultInterpolatedStringHandler.AppendFormatted(location.NameOrUniqueName);
                    defaultInterpolatedStringHandler.AppendLiteral("' failed parsing item query '");
                    defaultInterpolatedStringHandler.AppendFormatted(query);
                    defaultInterpolatedStringHandler.AppendLiteral("' for fish '");
                    defaultInterpolatedStringHandler.AppendFormatted(spawn.Id);
                    defaultInterpolatedStringHandler.AppendLiteral("': ");
                    defaultInterpolatedStringHandler.AppendFormatted(error);
                    //log.Error(defaultInterpolatedStringHandler.ToStringAndClear());
                }, null);
                foreach (var i in items)
                {
                    Item fish = i.Item as Item;
                    if (fish == null)
                    {
                        continue;
                    }
                    if (!string.IsNullOrWhiteSpace(spawn.SetFlagOnCatch))
                    {
                        fish.SetFlagOnPickup = spawn.SetFlagOnCatch;
                    }
                    if (spawn.IsBossFish)
                    {
                        fish.SetTempData("IsBossFish", value: true);
                    }
                    if ((spawn.CatchLimit <= -1 || !player.fishCaught.TryGetValue(fish.QualifiedItemId, out var values) || values[0] < spawn.CatchLimit))
                    {
                        float c = CheckGenericFishRequirements(fish, allFishData, location, player, spawn, waterDepth, usingMagicBait, hasCuriosityLure, spawn.ItemId == baitTargetFish, isTutorialCatch);
                        if (c > 0)
                        {
                            if (baitTargetFish == null || !(fish.QualifiedItemId != baitTargetFish) || targetedBaitTries >= 2)
                            {
                                passed[fish.QualifiedItemId] = chance * c;
                            }
                            if (firstNonTargetFish == null)
                            {
                                firstNonTargetFish = fish;
                            }
                            targetedBaitTries++;
                        }
                    }
                }
            }
            if (passed.Count == 0)
            {
                if (isTutorialCatch && firstNonTargetFish == null)
                {
                    passed["(O)145"] = 1;
                }
            }
            return passed;
        }

        /// <summary>
        /// MODIFIED COPY OF GameLocation.CheckGenericFishRequirements();
        /// </summary>
        internal static float CheckGenericFishRequirements(Item fish, Dictionary<string, string> allFishData, GameLocation location, Farmer player, SpawnFishData spawn, int waterDepth, bool usingMagicBait, bool hasCuriosityLure, bool usingTargetBait, bool isTutorialCatch)
        {
            if (!fish.HasTypeObject() || !allFishData.TryGetValue(fish.ItemId, out var rawSpecificFishData))
            {
                return isTutorialCatch ? 0 : 1;
            }
            string[] specificFishData = rawSpecificFishData.Split('/');
            if (ArgUtility.Get(specificFishData, 1) == "trap")
            {
                return isTutorialCatch ? 0 : 1;
            }
            bool isTrainingRod = player?.CurrentTool?.QualifiedItemId == "(T)TrainingRod";
            if (isTrainingRod)
            {
                if (!ArgUtility.TryGetInt(specificFishData, 1, out var difficulty, out var error7))
                {
                    return 0;
                }
                if (difficulty >= 50)
                {
                    return 0;
                }
            }
            if (isTutorialCatch)
            {
                if (!ArgUtility.TryGetOptionalBool(specificFishData, 13, out var isTutorialFish, out var error6))
                {
                    return 0;
                }
                if (!isTutorialFish)
                {
                    return 0;
                }
            }
            if (!spawn.IgnoreFishDataRequirements)
            {
                if (!usingMagicBait)
                {
                    if (!ArgUtility.TryGet(specificFishData, 5, out var rawTimeSpans, out var error5))
                    {
                        return 0;
                    }
                    string[] timeSpans = ArgUtility.SplitBySpace(rawTimeSpans);
                    bool found = false;
                    for (int i = 0; i < timeSpans.Length; i += 2)
                    {
                        if (!ArgUtility.TryGetInt(timeSpans, i, out var startTime, out error5) || !ArgUtility.TryGetInt(timeSpans, i + 1, out var endTime, out error5))
                        {
                            return 0;
                        }
                        if (Game1.timeOfDay >= startTime && Game1.timeOfDay < endTime)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        return 0;
                    }
                }
                if (!usingMagicBait)
                {
                    if (!ArgUtility.TryGet(specificFishData, 7, out var weather, out var error4))
                    {
                        return 0;
                    }
                    if (!(weather == "rainy"))
                    {
                        if (weather == "sunny" && location.IsRainingHere())
                        {
                            return 0;
                        }
                    }
                    else if (!location.IsRainingHere())
                    {
                        return 0;
                    }
                }
                if (!ArgUtility.TryGetInt(specificFishData, 12, out var minFishingLevel, out var error3))
                {
                    return 0;
                }
                if (player.FishingLevel < minFishingLevel)
                {
                    return 0;
                }
                if (!ArgUtility.TryGetInt(specificFishData, 9, out var maxDepth, out var error2) || !ArgUtility.TryGetFloat(specificFishData, 10, out var chance, out error2) || !ArgUtility.TryGetFloat(specificFishData, 11, out var depthMultiplier, out error2))
                {
                    return 0;
                }
                float dropOffAmount = depthMultiplier * chance;
                chance -= (float)Math.Max(0, maxDepth - waterDepth) * dropOffAmount;
                chance += (float)player.FishingLevel / 50f;
                if (isTrainingRod)
                {
                    chance *= 1.1f;
                }
                chance = Math.Min(chance, 0.9f);
                if ((double)chance < 0.25 && hasCuriosityLure)
                {
                    if (spawn.CuriosityLureBuff > -1f)
                    {
                        chance += spawn.CuriosityLureBuff;
                    }
                    else
                    {
                        float max = 0.25f;
                        float min = 0.08f;
                        chance = (max - min) / max * chance + (max - min) / 2f;
                    }
                }
                if (usingTargetBait)
                {
                    chance *= 1.66f;
                }
                if (spawn.ApplyDailyLuck)
                {
                    chance += (float)player.DailyLuck;
                }
                List<QuantityModifier> chanceModifiers = spawn.ChanceModifiers;
                if (chanceModifiers != null && chanceModifiers.Count > 0)
                {
                    chance = Utility.ApplyQuantityModifiers(chance, spawn.ChanceModifiers, spawn.ChanceModifierMode, location);
                }
                return chance;
            }
            return 1;
        }


        private void AddFishToListDynamic()                            //very performance intensive check for fish fish available in this area - simulates fishing
        {
            int freq = (isMinigame || isMinigameOther) ? 6 / totalPlayersOnThisPC : extraCheckFrequency * 10 / totalPlayersOnThisPC; //minigame lowers frequency
            for (int i = 0; i < freq; i++)
            {
                Game1.stats.TimesFished++;
                string fish = AddHardcoded();
                Game1.stats.TimesFished--;
                if (fish != "-2")//not fully hardcoded
                {
                    if (fish == "-1")//dynamic
                    {
                        //int nuts = 5;                                                                           //"fix" for preventing player from not getting specials       ----start1
                        //bool mail1 = false;
                        //bool mail2 = false;
                        //bool caughtIridiumKrobus = Game1.player.mailReceived.Contains("caughtIridiumKrobus");
                        //if (who.currentLocation is IslandLocation)
                        //{
                        //    nuts = (Game1.player.team.limitedNutDrops.ContainsKey("IslandFishing")) ? Game1.player.team.limitedNutDrops["IslandFishing"] : 0;
                        //    if (nuts < 5) Game1.player.team.limitedNutDrops["IslandFishing"] = 5;
                        //    mail1 = Game1.player.mailReceived.Contains("islandNorthCaveOpened");
                        //    mail2 = Game1.player.mailForTomorrow.Contains("islandNorthCaveOpened");
                        //    if (mail1) Game1.player.mailReceived.Remove("islandNorthCaveOpened");
                        //    if (mail2) Game1.player.mailForTomorrow.Remove("islandNorthCaveOpened");                                                                         //-----end1
                        //}

                        Game1.stats.TimesFished++;
                        item = who.currentLocation.getFish(0, (who.CurrentTool as FishingRod).GetBait()?.ItemId, 5, who, 100, who.Tile, who.currentLocation.Name) as Object;
                        Game1.stats.TimesFished--;
                        try
                        {
                            if (item.DisplayName.StartsWith("Error Item"))
                            {
                                Monitor.LogOnce("Skipped Object of type" + item.GetType() + ", ID: " + item.QualifiedItemId + ", CodeName: " + item.Name + ", Category: " + item.Category + ". DisplayName is \"Error Item\".", LogLevel.Error);
                                continue;
                            }
                            fish = item.QualifiedItemId;
                        }
                        catch (Exception)
                        {
                            Monitor.LogOnce("Skipped Object of type" + item.GetType() + ", ID: " + item.QualifiedItemId + ", CodeName: " + item.Name + ", Category: " + item.Category + ". Missing DisplayName.", LogLevel.Error);
                            continue;
                        }

                        //if (who.currentLocation is IslandLocation)
                        //{
                        //    if (!caughtIridiumKrobus && Game1.player.mailReceived.Contains("caughtIridiumKrobus")) Game1.player.mailReceived.Remove("caughtIridiumKrobus");//"fix"----start2
                        //    if (nuts < 5) Game1.player.team.limitedNutDrops["IslandFishing"] = nuts;
                        //    if (mail1) Game1.player.mailReceived.Add("islandNorthCaveOpened");
                        //    if (mail2) Game1.player.mailForTomorrow.Add("islandNorthCaveOpened");                                                                                //-----end2
                        //}
                    }
                    int val;
                    if (fishChances["-1"] < int.MaxValue) //percentages, slow version (the one shown) is updated less over time
                    {
                        if (junk.Contains(fish))
                        {
                            fishChances.TryGetValue("(O)168", out val);
                            fishChances["(O)168"] = val + 1;
                        }
                        else if (!fishHere.Contains(fish))
                        {
                            fishChances = new() { { "-1", 0 } };//reset % on new fish added
                            foreach (var f in fishHere) fishChances.Add(f, 1);
                            fishChancesSlow = new();
                            fishChancesModulo = 1;

                            if (sortMode[screen] == 0) SortItemIntoListByDisplayName(fish); //sort by name
                            else fishHere.Add(fish);
                            fishChances.Add(fish, 1);
                        }
                        else
                        {
                            fishChances.TryGetValue(fish, out val);
                            fishChances[fish] = val + 1;
                        }
                    }
                    fishChances.TryGetValue("-1", out val);
                    fishChances["-1"] = val + 1;
                    if (fishChances["-1"] % fishChancesModulo == 0)
                    {
                        if (fishChancesModulo < 10000) fishChancesModulo *= 10;
                        fishChancesSlow = fishChances.ToDictionary(entry => entry.Key, entry => entry.Value);
                    }
                    if (sortMode[screen] == 1) SortListByPercentages(); //sort by %



                    //if fish not in last X attempts, redo lists
                    if (!junk.Contains(fish))
                    {
                        fishChances.TryGetValue(fish, out val);
                        float chance = (float)val / fishChances["-1"] * 100f;
                        if (chance < 0.5f) fishFailed[fish] = 5000;
                        else if (chance < 1f) fishFailed[fish] = 3500;
                        else if (chance < 2f) fishFailed[fish] = 3000;
                        else if (chance < 3f) fishFailed[fish] = 2500;
                        else if (chance < 4f) fishFailed[fish] = 1500;
                        else fishFailed[fish] = 1000;
                    }
                }
                foreach (var key in fishFailed.Keys.ToList())
                {
                    fishFailed[key]--;
                    if (fishFailed[key] < 1) oldGeneric = null;
                }
            }
        }

        private static string[] junk = { "(O)167", "(O)168", "(O)169", "(O)170", "(O)171", "(O)172" };

        private string AddHardcoded()//-2 skip dynamic, -1 dynamic, above -1 = item to add to dynamic
        {
            if (who.currentLocation is Caldera)
            {
                if (Game1.random.NextDouble() < 0.05 && !Game1.player.mailReceived.Contains("CalderaPainting")) return "(F)2732";//physics 101
                return "-1";
            }
            if (who.currentLocation is Forest)
            {
                if (who.Tile.Y > 108f && !Game1.player.mailReceived.Contains("caughtIridiumKrobus")) return "(F)2396";//iridium krobus
                return "-1";
            }
            if (who.currentLocation is IslandLocation)
            {
                if (new Random((int)(Game1.stats.DaysPlayed + Game1.stats.TimesFished + Game1.uniqueIDForThisGame)).NextDouble() < 0.15 && (!Game1.player.team.limitedNutDrops.ContainsKey("IslandFishing") || Game1.player.team.limitedNutDrops["IslandFishing"] < 5)) return "(O)73";//nut

                if (who.currentLocation is IslandFarmCave)
                {
                    if (Game1.random.NextDouble() < 0.1) return "(H)78";//frog hat 78 + 900000
                    else if (who.currentLocation.HasUnlockedAreaSecretNotes(Game1.player) && Game1.random.NextDouble() < (0.08) && who.currentLocation.tryToCreateUnseenSecretNote(Game1.player) != null) return "(O)842";//journal
                    else return "(O)168";
                }

                if (who.currentLocation is IslandNorth)
                {
                    if ((bool)(Game1.getLocationFromName("IslandNorth") as IslandNorth).bridgeFixed.Value &&
                        (new Random((int)who.Tile.X * 2000 + (int)who.Tile.Y * 777 + (int)Game1.uniqueIDForThisGame / 2 + (int)Game1.stats.DaysPlayed + (int)Game1.stats.TimesFished)).NextDouble() < 0.1) return "(O)821";//spine
                    return "-1";
                }

                if (who.currentLocation is IslandSouthEast && who.Tile.X >= 17 && who.Tile.X <= 21 && who.Tile.Y >= 19 && who.Tile.Y <= 23)
                {
                    if (!(Game1.player.currentLocation as IslandSouthEast).fishedWalnut.Value)
                    {
                        fishHere = new() { "(O)73" };
                        fishChancesSlow = new() { { "-1", 1 }, { "(O)73", 1 }, { "(O)168", 0 } };
                    }
                    else
                    {
                        fishHere = new() { "(O)168" };
                        fishChancesSlow = new() { { "-1", 1 }, { "(O)168", 1 } };
                    }
                    oldGeneric = null;
                    return "-2";
                }

                if (who.currentLocation is IslandWest)
                {
                    if (Game1.player.hasOrWillReceiveMail("islandNorthCaveOpened") &&
                        (new Random((int)who.Tile.X * 2000 + (int)who.Tile.Y * 777 + (int)Game1.uniqueIDForThisGame / 2 + (int)Game1.stats.DaysPlayed + (int)Game1.stats.TimesFished)).NextDouble() < 0.1) return "(O)825";//skull
                    return "-1";
                }
            }
            if (who.currentLocation is Railroad)
            {
                if (Game1.currentSeason.Equals("winter")) return "-2";
                else if (Game1.player.secretNotesSeen.Contains(GameLocation.NECKLACE_SECRET_NOTE_INDEX) && !Game1.player.hasOrWillReceiveMail(GameLocation.CAROLINES_NECKLACE_MAIL)) return GameLocation.CAROLINES_NECKLACE_ITEM_QID;
                else if (!who.mailReceived.Contains("gotSpaFishing")) return "(F)2423";
                else if (Game1.random.NextDouble() < 0.08) return "(F)2423";//vista
                else return "(O)168";
            }
            return "-1";
        }

        private void AddCrabPotFish()
        {
            fishHere = new();

            bool isMariner = who.professions.Contains(10);
            if (!isMariner) fishHere.Add("(O)168");//trash
            fishChancesSlow = new();

            double fishChance = 1f;
            if (!isMariner && who.currentLocation.TryGetFishAreaForTile(who.Tile, out var _, out var data))
            {
                fishChance = 1f - ((double?)data?.CrabPotJunkChance) ?? 0.2;
            }

            IList<string> crabPotFishForTile = who.currentLocation.GetCrabPotFishForTile(who.Tile);
            foreach (KeyValuePair<string, string> item in fishData)
            {
                if (!item.Value.Contains("trap"))
                {
                    continue;
                }
                string[] array = item.Value.Split('/');
                string[] array2 = ArgUtility.SplitBySpace(array[4]);
                foreach (string text in array2)
                {
                    foreach (string item2 in crabPotFishForTile)
                    {
                        if (text == item2)//match area
                        {
                            string fish = "(O)" + item.Key;
                            if (!fishHere.Contains(fish))
                            {
                                if (sortMode[screen] == 0) SortItemIntoListByDisplayName(fish);
                                else fishHere.Add(fish);

                                if (showPercentages[screen] || sortMode[screen] == 1)
                                {
                                    float rawChance = float.Parse(array[2]);//chance
                                    fishChancesSlow.Add(fish, (int)Math.Round(rawChance * fishChance * 100f));
                                    fishChance *= (1f - rawChance);
                                }
                            }
                            break;
                        }
                    }
                }
            }
            if (isMariner) fishChancesSlow.Add("-1", fishChancesSlow.Sum(x => x.Value));
            else
            {
                fishChancesSlow.Add("(O)168", 100 - fishChancesSlow.Sum(x => x.Value));
                fishChancesSlow.Add("-1", 100);
            }

            if (sortMode[screen] == 1) SortListByPercentages();
        }

        private Object item;
        private void SortItemIntoListByDisplayName(string itemId)
        {
            var data = ItemRegistry.GetDataOrErrorItem(itemId);
            for (int j = 0; j < fishHere.Count; j++)
            {
                if (string.Compare(data.DisplayName, ItemRegistry.GetDataOrErrorItem(fishHere[j]).DisplayName, StringComparison.CurrentCulture) <= 0)
                {
                    fishHere.Insert(j, itemId);
                    return;
                }
            }
            fishHere.Add(itemId);
        }

        private void SortListByPercentages()
        {
            int index = 0;
            foreach (var item in fishChancesSlow.OrderByDescending(d => d.Value).ToList())
            {
                if (fishHere.Contains(item.Key))
                {
                    fishHere.Remove(item.Key);
                    fishHere.Insert(index, item.Key);
                    index++;
                }
            }
        }


        public void OnWarped(object sender, WarpedEventArgs e)//for less janky cleanup on loc change
        {
            if (e.IsLocalPlayer) oldGeneric = null;
        }


        /// <summary>Makes text a tiny bit bolder and adds a border behind it. The border uses text colour's alpha for its aplha value. 6 DrawString operations, so 6x less efficient.</summary>
        private void DrawStringWithBorder(SpriteBatch batch, SpriteFont font, string text, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth, Color? borderColor = null)
        {
            Color border = borderColor.HasValue ? borderColor.Value : Color.Black;
            border.A = color.A;
            batch.DrawString(font, text, position + new Vector2(-1.2f * scale, -1.2f * scale), border, rotation, origin, scale, effects, layerDepth - 0.00001f);
            batch.DrawString(font, text, position + new Vector2(1.2f * scale, -1.2f * scale), border, rotation, origin, scale, effects, layerDepth - 0.00001f);
            batch.DrawString(font, text, position + new Vector2(-1.2f * scale, 1.2f * scale), border, rotation, origin, scale, effects, layerDepth - 0.00001f);
            batch.DrawString(font, text, position + new Vector2(1.2f * scale, 1.2f * scale), border, rotation, origin, scale, effects, layerDepth - 0.00001f);

            batch.DrawString(font, text, position + new Vector2(-0.2f * scale, -0.2f * scale), color, rotation, origin, scale, effects, layerDepth);
            batch.DrawString(font, text, position + new Vector2(0.2f * scale, 0.2f * scale), color, rotation, origin, scale, effects, layerDepth);
        }
        private void AddBackground(SpriteBatch batch, Vector2 boxTopLeft, Vector2 boxBottomLeft, int iconCount, Rectangle source, float iconScale, float boxWidth, float boxHeight)
        {
            if (backgroundMode[screen] == 0)
            {
                batch.Draw(background[backgroundMode[screen]], new Rectangle((int)boxBottomLeft.X - 1, (int)boxBottomLeft.Y - 1, (int)(source.Width * iconScale) + 1, (int)((source.Width * iconScale) + 1 + (showPercentages[screen] ? 10 * barScale[screen] : 0))),
                    null, colorBg, 0f, Vector2.Zero, SpriteEffects.None, 0.5f);
            }
            else if (backgroundMode[screen] == 1)
            {
                if (iconMode[screen] == 0) batch.Draw(background[backgroundMode[screen]], new Rectangle((int)boxTopLeft.X - 2, (int)boxTopLeft.Y - 2, (int)(source.Width * iconScale * Math.Min(iconCount, maxIconsPerRow[screen])) + 5,
               (int)(((source.Width * iconScale) + (showPercentages[screen] ? 10 * barScale[screen] : 0)) * Math.Ceiling(iconCount / (maxIconsPerRow[screen] * 1.0))) + 5), null, colorBg, 0f, Vector2.Zero, SpriteEffects.None, 0.5f);
                else if (iconMode[screen] == 1) batch.Draw(background[backgroundMode[screen]], new Rectangle((int)boxTopLeft.X - 2, (int)boxTopLeft.Y - 2, (int)(source.Width * iconScale * Math.Ceiling(iconCount / (maxIconsPerRow[screen] * 1.0))) + 5,
                    (int)(((source.Width * iconScale) + (showPercentages[screen] ? 10 * barScale[screen] : 0)) * Math.Min(iconCount, maxIconsPerRow[screen])) + 5), null, colorBg, 0f, Vector2.Zero, SpriteEffects.None, 0.5f);
                else if (iconMode[screen] == 2) batch.Draw(background[backgroundMode[screen]], new Rectangle((int)boxTopLeft.X - 2, (int)boxTopLeft.Y - 2, (int)(boxWidth - boxTopLeft.X + 6), (int)boxHeight + 4),
                    null, colorBg, 0f, Vector2.Zero, SpriteEffects.None, 0.5f);
            }
        }

        public static float FixScale(Rectangle source, float scale)
        {
            float bigger = source.Width > source.Height ? source.Width : source.Height;

            if (bigger > 0)
            {
                return scale * 16 / bigger;
            }
            return scale;
        }
        public static Vector2 FixSourceOffset(Rectangle source, Vector2 offset)
        {
            if (source.Height != 16 || source.Width != 16)//this is probably wrong (might just be offset by half original size), but non-standard sizes are hard to test
            {
                float scale = FixScale(source, 1f);

                float w = scale * source.Width;
                float h = scale * source.Height;
                if (w < 16f)
                {
                    offset.X += (int)(16f - (w / 2f));
                }
                if (h < 16f)
                {
                    offset.Y += (int)(16f - (h / 2f));
                }
            }
            return offset;
        }
    }
}
