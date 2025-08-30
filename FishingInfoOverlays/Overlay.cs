using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.GameData.Locations;
using StardewValley.Internal;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Tools;
using Object = StardewValley.Object;

namespace FishingInfoOverlays
{
    /// <summary>The mod entry point.</summary>
    public class Overlay(ModEntry entry)
    {
        private readonly ITranslationHelper translate = entry.Helper.Translation;
        private readonly IMonitor Monitor = entry.Monitor;
        private readonly IModHelper Helper = entry.Helper;


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
        private float fixedZooom = 1f;

        private bool isMinigame = false;    //minigame fish preview data, Reflection
        private string miniFish;
        private bool hasSonar;


        public static Dictionary<string, LocationData> locationData;
        public static Dictionary<string, string> fishData;
        public static Texture2D[] background = new Texture2D[2];
        public static Color colorBg;
        public static Color colorText;
        public static bool modAquarium = false;


        public static int[] miniMode = new int[4];   //config values
        public static bool[] barCrabEnabled = new bool[4];
        public static Vector2[] barPosition = new Vector2[4];
        public static int sonarMode;
        public static int[] iconMode = new int[4];
        public static float[] barScale = new float[4];
        public static int[] maxIcons = new int[4];
        public static int[] maxIconsPerRow = new int[4];
        public static int[] backgroundMode = new int[4];
        public static int extraCheckFrequency;
        public static int[] scanRadius = new int[4];
        public static bool[] showTackles = new bool[4];
        public static bool[] showPercentages = new bool[4];
        public static int[] showExtraIcons = new int[4];
        public static int[] sortMode = new int[4];
        public static bool[] uncaughtDark = new bool[4];
        public static bool[] onlyFish = new bool[4];
        public static KeybindList scanKey = new(SButton.LeftShift);

        public void Rendered(object sender, RenderedEventArgs e)
        {
            who = Game1.player;
            screen = Context.ScreenId;
            if (Game1.eventUp || who.CurrentItem == null ||
                !(who.CurrentItem is FishingRod || "Crab Pot".Equals(who.CurrentItem.Name, StringComparison.Ordinal) && barCrabEnabled[screen])) return;//code stop conditions

            totalPlayersOnThisPC = 1;
            foreach (IMultiplayerPeer peer in Helper.Multiplayer.GetConnectedPlayers())
            {
                if (peer.IsSplitScreen) totalPlayersOnThisPC++;
            }

            hasSonar = false;
            int maxDist = 0;
            FishingRod rod = Game1.player.CurrentItem as FishingRod;
            if (rod != null)
            {
                hasSonar = rod.GetTackleQualifiedItemIDs().Contains("(O)SonarBobber");
                if (sonarMode == 0 && !hasSonar) return;

                var m = typeof(FishingRod).GetMethod("getAddedDistance", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                maxDist = (int)m?.Invoke(rod, [who]) + 4;
            }

            SpriteFont font = Game1.smallFont;
            var miniData = ItemRegistry.GetDataOrErrorItem(miniFish);                                   //UI INIT
            Rectangle source = miniData.GetSourceRect();
            SpriteBatch batch = Game1.spriteBatch;

            batch.End();    //stop current UI drawing and start mode where where layers work from 0f-1f
            batch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise);

            //MINIGAME PREVIEW
            if (isMinigame && miniMode[screen] < 2 && Game1.activeClickableMenu is BobberBar bar && bar.scale == 1f && (hasSonar || sonarMode > 1)) //scale == 1f when moving elements appear
            {
                if (miniMode[screen] == 0) //Full minigame
                {
                    //rod+bar textture cut to only cover the minigame bar
                    batch.Draw(Game1.mouseCursors, new Vector2(bar.xPositionOnScreen + 126, bar.yPositionOnScreen + 292) + bar.everythingShake,
                        new Rectangle(658, 1998, 15, 149), Color.White * bar.scale, 0f, new Vector2(18.5f, 74f) * bar.scale, 4f * bar.scale, SpriteEffects.None, 0.01f);

                    //green moving bar player controls
                    batch.Draw(Game1.mouseCursors, new Vector2(bar.xPositionOnScreen + 64, bar.yPositionOnScreen + 12 + (int)bar.bobberBarPos) + bar.barShake + bar.everythingShake,
                        new Rectangle(682, 2078, 9, 2), bar.bobberInBar ? Color.White : Color.White * 0.25f * ((float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 100.0), 2) + 2f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.89f);
                    batch.Draw(Game1.mouseCursors, new Vector2(bar.xPositionOnScreen + 64, bar.yPositionOnScreen + 12 + (int)bar.bobberBarPos + 8) + bar.barShake + bar.everythingShake,
                        new Rectangle(682, 2081, 9, 1), bar.bobberInBar ? Color.White : Color.White * 0.25f * ((float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 100.0), 2) + 2f), 0f, Vector2.Zero, new Vector2(4f, bar.bobberBarHeight - 16), SpriteEffects.None, 0.89f);
                    batch.Draw(Game1.mouseCursors, new Vector2(bar.xPositionOnScreen + 64, bar.yPositionOnScreen + 12 + (int)bar.bobberBarPos + bar.bobberBarHeight - 8) + bar.barShake + bar.everythingShake,
                        new Rectangle(682, 2085, 9, 2), bar.bobberInBar ? Color.White : Color.White * 0.25f * ((float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 100.0), 2) + 2f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.89f);

                    //treasure
                    batch.Draw(Game1.mouseCursors, new Vector2(bar.xPositionOnScreen + 64 + 18, bar.yPositionOnScreen + 12 + 24 + bar.treasurePosition) + bar.treasureShake + bar.everythingShake,
                        new Rectangle(638, 1865, 20, 24), Color.White, 0f, new Vector2(10f, 10f), 2f * bar.treasureScale, SpriteEffects.None, 0.9f);
                    if (bar.treasureCatchLevel > 0f && !bar.treasureCaught)//treasure progress
                    {
                        batch.Draw(Game1.staminaRect, new Rectangle(bar.xPositionOnScreen + 64, bar.yPositionOnScreen + 12 + (int)bar.treasurePosition, 40, 8), null, Color.DimGray * 0.5f, 0f, Vector2.Zero, SpriteEffects.None, 0.9f);
                        batch.Draw(Game1.staminaRect, new Rectangle(bar.xPositionOnScreen + 64, bar.yPositionOnScreen + 12 + (int)bar.treasurePosition, (int)(bar.treasureCatchLevel * 40f), 8), null, Color.Orange, 0f, Vector2.Zero, SpriteEffects.None, 0.9f);
                    }
                }
                else batch.Draw(Game1.mouseCursors, new Vector2(bar.xPositionOnScreen + 82, bar.yPositionOnScreen + 36 + bar.bobberPosition) + bar.fishShake + bar.everythingShake,
                    new Rectangle(614 + (bar.bossFish ? 20 : 0), 1840, 20, 20), Color.Black, 0f, new Vector2(10f, 10f),
                    2.05f, SpriteEffects.None, 0.9f);//Simple bar.game shadow fish

                //fish
                batch.Draw(miniData.GetTexture(), new Vector2(bar.xPositionOnScreen + 82, bar.yPositionOnScreen + 36 + bar.bobberPosition) + bar.fishShake + bar.everythingShake,
                    source, !uncaughtDark[screen] || who.fishCaught.ContainsKey(miniData.QualifiedItemId) ? Color.White : Color.DarkSlateGray, 0f, new Vector2(9.5f, 9f),
                    3f, SpriteEffects.FlipHorizontally, 1f);
            }



            if (iconMode[screen] != 3)
            {
                fixedZooom = (Game1.options.zoomLevel == 1f ? 0.75f : Game1.options.zoomLevel) * 0.8f;
                float iconScale = Game1.pixelZoom / 2f * barScale[screen] / fixedZooom;
                int iconScale16 = (int)(16f * iconScale);
                float fixScale10 = FixIconScale(10f);
                int iconCount = 0;
                float boxWidth = 0;
                float boxHeight = 0;
                Vector2 boxTopLeft = new(FixIconScale(barPosition[screen].X), FixIconScale(barPosition[screen].Y));
                Vector2 boxBottomLeft = new(FixIconScale(barPosition[screen].X), FixIconScale(barPosition[screen].Y));


                //this.Monitor.Log("\n", LogLevel.Debug);
                if (who.currentLocation is MineShaft && "Crab Pot".Equals(who.CurrentItem.Name, StringComparison.Ordinal))//crab pot
                {
                    string warning = translate.Get("Bar.CrabMineWarning");
                    DrawStringWithBorder(batch, font, warning, boxBottomLeft + new Vector2(source.Width * iconScale, 0), Color.Red, 0f, Vector2.Zero, FixIconScale(1f), SpriteEffects.None, 1f, colorBg); //text
                    batch.End();
                    batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise);
                    return;
                }


                bool showPercent = showPercentages[screen];
                bool showExtras = showExtraIcons[screen] != 0;
                if (rod != null && showTackles[screen])    //BAIT AND TACKLE (BOBBERS) PREVIEW
                {
                    Object bait = rod.GetBait();
                    if (bait != null)
                    {
                        var data = ItemRegistry.GetData(bait.QualifiedItemId);
                        source = data.GetSourceRect();
                        if (backgroundMode[screen] == 0) AddBackground(batch, boxTopLeft, boxBottomLeft, iconCount, source, iconScale, boxWidth, boxHeight, showPercent, showExtras);

                        int baitCount = bait.Stack;
                        batch.Draw(data.GetTexture(), boxBottomLeft, source, Color.White, 0f, Vector2.Zero, FixIconScale(1.9f), SpriteEffects.None, 0.9f);

                        if (bait.Quality == 4) batch.Draw(Game1.mouseCursors, boxBottomLeft + new Vector2(FixIconScale(13f), FixIconScale(showPercent ? 24 : 16)),
                            new Rectangle(346, 392, 8, 8), Color.White, 0f, Vector2.Zero, FixIconScale(1.9f), SpriteEffects.None, 1f);
                        else Utility.drawTinyDigits(baitCount, batch, boxBottomLeft + new Vector2(source.Width * iconScale - Utility.getWidthOfTinyDigitString(baitCount, FixIconScale(2f)),
                            FixIconScale(showPercent ? 26 : 19)), FixIconScale(2f), 1f, colorText);

                        if (iconMode[screen] == 1) boxBottomLeft += new Vector2(0, source.Width * iconScale + (showPercent ? fixScale10 : 0));
                        else boxBottomLeft += new Vector2(source.Width * iconScale + (showExtras ? fixScale10 : 0), 0);
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
                            if (backgroundMode[screen] == 0) AddBackground(batch, boxTopLeft, boxBottomLeft, iconCount, source, iconScale, boxWidth, boxHeight, showPercent, showExtras);

                            int tackleCount = FishingRod.maxTackleUses - tackle.uses.Value;
                            batch.Draw(data.GetTexture(), boxBottomLeft, source, Color.White, 0f, Vector2.Zero, FixIconScale(1.9f), SpriteEffects.None, 0.9f);

                            if (tackle.Quality == 4) batch.Draw(Game1.mouseCursors, boxBottomLeft + new Vector2(FixIconScale(13f), FixIconScale(showPercent ? 24 : 16)),
                                new Rectangle(346, 392, 8, 8), Color.White, 0f, Vector2.Zero, FixIconScale(1.9f), SpriteEffects.None, 1f);
                            else Utility.drawTinyDigits(tackleCount, batch, boxBottomLeft + new Vector2(source.Width * iconScale - Utility.getWidthOfTinyDigitString(tackleCount, FixIconScale(2f)),
                                FixIconScale(showPercent ? 26 : 19)), FixIconScale(2f), 1f, colorText);

                            if (iconMode[screen] == 1) boxBottomLeft += new Vector2(0, source.Width * iconScale + (showPercent ? fixScale10 : 0));
                            else boxBottomLeft += new Vector2(source.Width * iconScale + (showExtras ? fixScale10 : 0), 0);
                            anyTackle = true;
                        }
                        if (anyTackle) iconCount++;
                    }
                    if (iconMode[screen] == 2 && (bait != null || anyTackle))
                    {
                        boxBottomLeft = boxTopLeft + new Vector2(0, source.Width * iconScale + (showPercent ? fixScale10 : 0));
                        boxWidth = iconCount * source.Width * iconScale + boxTopLeft.X;
                        boxHeight += source.Width * iconScale + (showPercent ? fixScale10 : 0);
                        iconCount = 1;
                    }
                }



                bool foundWater = false;
                bool showTile = false;
                Vector2 nearestWaterTile = new(99999f, 99999f);      //any water nearby + nearest water tile check
                if (who.currentLocation.canFishHere())
                {
                    if (rod != null)
                    {
                        if (screen == 0 && (hasSonar || sonarMode == 3) && scanKey.IsDown())
                        {
                            showTile = true;
                            int x = (int)Game1.currentCursorTile.X;
                            int y = (int)Game1.currentCursorTile.Y;
                            if (who.currentLocation.isTileFishable(x, y) && !who.currentLocation.isTileBuildingFishable(x, y))
                            {
                                nearestWaterTile = new(x, y);
                                foundWater = true;
                            }
                        }
                        if (!foundWater)
                        {
                            int dir = who.FacingDirection;//0=up,1=r,2=d,3=l
                            bool isX = dir is Game1.left or Game1.right;
                            bool positive = dir > 1;
                            if (!isX) maxDist--;
                            for (int i = maxDist; i >= 0; i--)
                            {
                                int x = (int)who.Tile.X;
                                int y = (int)who.Tile.Y;
                                if (isX)
                                {
                                    if (positive) x -= i;
                                    else x += i;
                                }
                                else
                                {
                                    if (positive) y += i;
                                    else y -= i;
                                }
                                if (who.currentLocation.isTileFishable(x, y) && !who.currentLocation.isTileBuildingFishable(x, y))
                                {
                                    nearestWaterTile = new(x, y);
                                    foundWater = true;
                                    break;
                                }
                            }
                        }
                    }
                    if (!foundWater)
                    {
                        Vector2 scanTopLeft = who.Tile - new Vector2(scanRadius[screen] + 1);
                        Vector2 scanBottomRight = who.Tile + new Vector2(scanRadius[screen] + 2);
                        for (int x = (int)scanTopLeft.X; x < (int)scanBottomRight.X; x++)
                        {
                            for (int y = (int)scanTopLeft.Y; y < (int)scanBottomRight.Y; y++)
                            {
                                if (who.currentLocation.isTileFishable(x, y) && !who.currentLocation.isTileBuildingFishable(x, y))
                                {
                                    Vector2 tile = new(x, y);
                                    float distance = Vector2.DistanceSquared(who.Tile, tile);
                                    float distanceNearest = Vector2.DistanceSquared(who.Tile, nearestWaterTile);
                                    if (distance < distanceNearest || distance == distanceNearest && Game1.player.GetGrabTile() == tile) nearestWaterTile = tile;
                                    foundWater = true;
                                }
                            }
                        }
                    }
                }

                var defaultSource = new Rectangle(0, 0, 16, 16);
                if (foundWater)
                {
                    if (rod != null)   //LOCATION FISH PREVIEW
                    {
                        if (!isMinigame)
                        {
                            if (extraCheckFrequency == 0) AddGenericFishToList(nearestWaterTile);
                            else AddFishToListDynamic(nearestWaterTile);
                        }
                    }
                    else AddCrabPotFish();

                    if (showTile)
                    {
                        batch.Draw(Game1.mouseCursors, new Vector2((int)nearestWaterTile.X * 64 - Game1.viewport.X, (int)nearestWaterTile.Y * 64 - Game1.viewport.Y), new Rectangle(652, 204, 44, 44),
                            new Color(0, 255, 0, 0.5f), 0f, Vector2.Zero, 1.45f, SpriteEffects.None, 1f);
                    }

                    foreach (var fish in fishHere)
                    {
                        string unqualifiedId = fish.Replace("(O)", "", StringComparison.Ordinal);
                        if (onlyFish[screen] && unqualifiedId != "168" && !fishData.ContainsKey(unqualifiedId)) continue;//skip if not fish, except trash

                        int percent = fishChancesSlow.ContainsKey(fish) ? (int)Math.Round((float)fishChancesSlow[fish] / fishChancesSlow["-1"] * 100f) : 0; //chance of this fish

                        if (iconCount < maxIcons[screen] && percent > 0)
                        {
                            iconCount++;                   //BACKUP MAX SIZES instead of loading this each tick
                            bool fishNeedsMaxSize = false;
                            bool fishNeedsAquarium = false;
                            bool fishNeedsBundle = false;
                            var data = ItemRegistry.GetDataOrErrorItem(fish);
                            bool caught = who.fishCaught.TryGetValue(data.QualifiedItemId, out var fishCaughtData);
                            if (showExtras && (caught || showExtraIcons[screen] == 1))
                            {
                                try
                                {
                                    if (data.Category == Object.FishCategory)
                                    {
                                        fishNeedsMaxSize = !caught || fishData.TryGetValue(unqualifiedId, out var array) && !(fishCaughtData[1] > Convert.ToInt32(array.Split('/')[4]));
                                        fishNeedsAquarium = modAquarium && !Game1.MasterPlayer.hasOrWillReceiveMail("AquariumDonated:" + data.InternalName.Replace(" ", string.Empty, StringComparison.Ordinal));
                                    }
                                    FieldInfo bundlesIngredientsInfoField = typeof(CommunityCenter).GetField("bundlesIngredientsInfo", BindingFlags.NonPublic | BindingFlags.Instance);
                                    var bundlesIngredientsInfo = (Dictionary<string, List<List<int>>>)bundlesIngredientsInfoField.GetValue(Game1.RequireLocation<CommunityCenter>("CommunityCenter"));
                                    fishNeedsBundle = bundlesIngredientsInfo.ContainsKey(fish) || bundlesIngredientsInfo.ContainsKey(data.Category.ToString());
                                }
                                catch { }
                            }
                            caught |= !uncaughtDark[screen] || (data.Category != Object.FishCategory);
                            string fishNameLocalized = caught ? data.DisplayName : "???";
                            Texture2D txt2d = data.GetTexture();
                            source = data.GetSourceRect();

                            if (data.IsErrorItem) continue;

                            if (unqualifiedId == "168") batch.Draw(txt2d, boxBottomLeft + new Vector2(FixIconScale(2), FixIconScale(-5)), source, Color.White,
                                0f, Vector2.Zero, FixRectScale(source, FixIconScale(1.9f)), SpriteEffects.None, 0.98f);//icon trash
                            else batch.Draw(txt2d, FixSourceOffset(source, boxBottomLeft), source,
                                caught ? Color.White : Color.DarkSlateGray, 0f, Vector2.Zero, FixIconScale(FixRectScale(source, 1.9f)), SpriteEffects.None, 0.98f);//icon

                            if (showPercent) //percent text
                            {
                                DrawStringWithBorder(batch, font, percent + "%", boxBottomLeft + new Vector2(8f * iconScale, FixIconScale(27f)),
                                    caught ? colorText : colorText * 0.8f, 0f, new Vector2(font.MeasureString(percent + "%").X / 2f, 0f), FixIconScale(0.58f), SpriteEffects.None, 1f, colorBg);
                            }
                            if (showExtras) //completion icons
                            {
                                if (fishNeedsMaxSize) batch.Draw(Game1.mouseCursors, boxBottomLeft + new Vector2(FixIconScale(31), FixIconScale(6)), new(338, 400, 8, 8), Color.Cyan, 0f, Vector2.Zero, FixIconScale(1.1f), SpriteEffects.None, 0.98f);
                                if (fishNeedsBundle) batch.Draw(Game1.mouseCursors, boxBottomLeft + new Vector2(FixIconScale(30.5f), FixIconScale(15)), new(330, 373, 16, 16), Color.White, 0f, Vector2.Zero, FixIconScale(0.6f), SpriteEffects.None, 0.98f);
                                if (fishNeedsAquarium) batch.Draw(Game1.objectSpriteSheet, boxBottomLeft + new Vector2(FixIconScale(31), FixIconScale(24)), new(128, 80, 16, 16), Color.White, 0f, Vector2.Zero, FixIconScale(0.6f), SpriteEffects.None, 1.98f);
                            }

                            if (unqualifiedId == miniFish && miniMode[screen] < 3) //green minigame outline on bar
                            {
                                batch.Draw(background[screen], new Rectangle((int)boxBottomLeft.X, (int)boxBottomLeft.Y, iconScale16, iconScale16), null, Color.GreenYellow, 0f, Vector2.Zero, SpriteEffects.None, 0.9f);
                            }

                            if (backgroundMode[screen] == 0) AddBackground(batch, boxTopLeft, boxBottomLeft, iconCount, defaultSource, iconScale, boxWidth, boxHeight, showPercent, showExtras);//circles

                            if (iconMode[screen] == 0)      //Horizontal Preview
                            {
                                if (iconCount % maxIconsPerRow[screen] == 0) boxBottomLeft = new Vector2(boxTopLeft.X, boxBottomLeft.Y + iconScale16 + (showPercent ? fixScale10 : 0)); //row switch
                                else boxBottomLeft += new Vector2(iconScale16 + (showExtras ? fixScale10 : 0), 0);
                            }
                            else                    //Vertical Preview
                            {
                                if (iconMode[screen] == 2 && !hideText)  // + text
                                {
                                    DrawStringWithBorder(batch, font, fishNameLocalized, boxBottomLeft + new Vector2(FixIconScale(35) + (showExtras ? fixScale10 : 0), 0 + (showPercent ? FixIconScale(5) : 0)),
                                        caught ? colorText : colorText * 0.8f, 0f, new Vector2(0, -3), FixIconScale(1f), SpriteEffects.None, 0.98f, colorBg); //text
                                    boxWidth = Math.Max(boxWidth + (showExtras ? fixScale10 : 0), boxBottomLeft.X + FixIconScale(font.MeasureString(fishNameLocalized).X) + iconScale16);
                                }

                                if (iconCount % maxIconsPerRow[screen] == 0) //row switch
                                {
                                    if (iconMode[screen] == 2) boxBottomLeft = new Vector2(boxWidth + FixIconScale(showExtras ? 30 : 20), boxTopLeft.Y);
                                    else boxBottomLeft = new Vector2(boxBottomLeft.X + iconScale16 + (showExtras ? fixScale10 : 0), boxTopLeft.Y);
                                }
                                else boxBottomLeft += new Vector2(0, iconScale16 + (showPercent ? fixScale10 : 0));
                                if (iconMode[screen] == 2 && iconCount <= maxIconsPerRow[screen]) boxHeight += iconScale16 + (showPercent ? fixScale10 : 0);
                            }
                        }
                    }
                }
                if (backgroundMode[screen] == 1) AddBackground(batch, boxTopLeft, boxBottomLeft, iconCount, defaultSource, iconScale, boxWidth, boxHeight, showPercent, showExtras);//rectangle
            }

            batch.End();
            batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise);
        }


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
            }
        }
        public void OnModMessageReceived(object sender, ModMessageReceivedEventArgs e)
        {
            if (e.FromModID == "barteke22.FishingMinigames")
            {
                if (e.Type == "whichFish")
                {
                    miniFish = e.ReadAs<string>();
                    isMinigameOther = miniFish != "-1";
                }
                if (e.Type == "hideText") hideText = e.ReadAs<bool>();
            }
        }

        private void AddGenericFishToList(Vector2 bobberTile)
        {
            if (Game1.ticks % (isMinigame ? 600 : 30) == 0 || oldGeneric == null)
            {
                oldGeneric = [];
                fishHere = ["(O)168"];
                fishChancesSlow = new() { { "-1", 100 }, { "(O)168", 0 } };//-1 represents the total

                var hard = AddHardcoded(bobberTile, false);
                if (hard != "-2")
                {
                    Dictionary<string, float> data = GetFishFromLocationData(who.currentLocation.Name, bobberTile, 5, who, !who.fishCaught.Any(), false, who.currentLocation, null);

                    if (hard != "-1")
                    {
                        if (hard.Contains('|'))
                        {
                            var d = hard.Split('|');
                            for (int i = 0; i < d.Length; i += 2)
                            {
                                var c = float.Parse(d[i + 1]);
                                data[d[i]] = c * (data.Sum(f => f.Value) + c);
                            }
                        }
                        else data[hard] = 1000;
                    }

                    float total = data.Sum(f => f.Value);
                    var j = data.FirstOrDefault(f => ItemRegistry.GetDataOrErrorItem(f.Key).Category == Object.junkCategory);
                    float notJunk = j.Key != null ? j.Value : 1f;
                    total -= notJunk;
                    foreach (var fish in data)
                    {
                        int itemCategory = ItemRegistry.GetDataOrErrorItem(fish.Key).Category;
                        if (fish.Value < 1f && itemCategory != Object.junkCategory)
                        {
                            notJunk *= 1f - fish.Value;
                        }
                    }
                    total += notJunk;
                    foreach (var fish in data)
                    {
                        int itemCategory = ItemRegistry.GetDataOrErrorItem(fish.Key).Category;
                        if (itemCategory != Object.junkCategory)
                        {
                            if (sortMode[screen] == 0) SortItemIntoListByDisplayName(fish.Key); //sort by name
                            else fishHere.Add(fish.Key);
                            fishChancesSlow[fish.Key] = (int)Math.Round(fish.Value / total * 100);
                        }
                    }
                    fishChancesSlow["(O)168"] = (int)Math.Round(notJunk / total * 100);
                }
                if (sortMode[screen] == 1) SortListByPercentages(); //sort by %
            }
        }

        /// <summary>
        /// MODIFIED COPY OF GameLocation.GetFishFromLocationData();
        /// </summary>
        internal static Dictionary<string, float> GetFishFromLocationData(string locationName, Vector2 bobberTile, int waterDepth, Farmer player, bool isTutorialCatch, bool isInherited, GameLocation location, ItemQueryContext itemQueryContext)
        {
            Dictionary<string, float> passed = [];

            location ??= Game1.getLocationFromName(locationName);
            LocationData locationData = location != null ? location.GetData() : GameLocation.GetData(locationName);
            Dictionary<string, string> allFishData = DataLoader.Fish(Game1.content);
            Season season = Game1.GetSeasonForLocation(location);
            if (location == null || !location.TryGetFishAreaForTile(bobberTile, out var fishAreaId, out var _))
            {
                fishAreaId = null;
            }
            bool usingMagicBait = false;
            bool hasCuriosityLure = false;
            string baitTargetFish = null;
            //bool usingGoodBait = false;//not important
            if (player?.CurrentTool is FishingRod rod)
            {
                hasCuriosityLure = rod.HasCuriosityLure();
                if (rod.GetBait() is Object bait)
                {
                    if (bait.QualifiedItemId == "(O)SpecificBait" && bait.preservedParentSheetIndex.Value != null)
                    {
                        baitTargetFish = "(O)" + bait.preservedParentSheetIndex.Value;
                    }
                    else if (rod.HasMagicBait()) usingMagicBait = true;
                    //else if (bait?.QualifiedItemId != "(O)685") usingGoodBait = true;
                }
            }
            Point playerTile = player.TilePoint;
            itemQueryContext ??= new ItemQueryContext(location, null, Game1.random, "location '" + locationName + "' > fish data");
            IEnumerable<SpawnFishData> possibleFish = Game1.locationData["Default"].Fish;
            if (locationData != null && locationData.Fish?.Count > 0)
            {
                possibleFish = possibleFish.Concat(locationData.Fish);
            }
            int targetedBaitTries = 0;
            HashSet<string> ignoreQueryKeys = usingMagicBait ? GameStateQuery.MagicBaitIgnoreQueryKeys : null;
            Item firstNonTargetFish = null;
            foreach (SpawnFishData spawn in possibleFish)
            {
                if ((isInherited && !spawn.CanBeInherited) || (spawn.FishAreaId != null && fishAreaId != spawn.FishAreaId) || (spawn.Season.HasValue && !usingMagicBait && spawn.Season != season))
                {
                    continue;
                }
                Rectangle? playerPosition = spawn.PlayerPosition;
                if (playerPosition.HasValue && !playerPosition.GetValueOrDefault().Contains(playerTile.X, playerTile.Y))
                {
                    continue;
                }
                playerPosition = spawn.BobberPosition;
                if ((playerPosition.HasValue && !playerPosition.GetValueOrDefault().Contains((int)bobberTile.X, (int)bobberTile.Y)) || player.FishingLevel < spawn.MinFishingLevel || waterDepth < spawn.MinDistanceFromShore || (spawn.MaxDistanceFromShore > -1 && waterDepth > spawn.MaxDistanceFromShore) || (spawn.RequireMagicBait && !usingMagicBait))
                {
                    continue;
                }
                float chance = spawn.GetChance(hasCuriosityLure, player.DailyLuck, player.LuckLevel, (value, modifiers, mode) => Utility.ApplyQuantityModifiers(value, modifiers, mode, location), spawn.ItemId == baitTargetFish);

                if (spawn.Condition != null && !GameStateQuery.CheckConditions(spawn.Condition, location, null, null, null, null, ignoreQueryKeys))
                {
                    continue;
                }

                List<string> queries = [];
                if (spawn.RandomItemId != null && spawn.RandomItemId.Count != 0)
                {
                    queries = spawn.RandomItemId;
                }
                else if (spawn.ItemId != null) queries.Add(spawn.ItemId);

                foreach (var q in queries)
                {
                    string query = q;
                    int max = 1;
                    if (!q.StartsWith('('))
                    {
                        max = 100;
                        query = q.Replace("BOBBER_X", ((int)bobberTile.X).ToString()).Replace("BOBBER_Y", ((int)bobberTile.Y).ToString()).Replace("WATER_DEPTH", waterDepth.ToString());
                    }

                    List<Item> items = [];
                    for (int i = 0; i < max; i++)
                    {
                        var item = ItemQueryResolver.TryResolve(query, itemQueryContext, ItemQuerySearchMode.FirstOfTypeItem, spawn.PerItemCondition, spawn.MaxItems, true);
                        if (item.FirstOrDefault()?.Item is Item fish && !items.Any(f => f.QualifiedItemId == fish.QualifiedItemId)) items.Add(fish);
                    }
                    foreach (var fish in items)
                    {
                        if (fish.Category == Object.junkCategory) fish.ItemId = "168";
                        if (!string.IsNullOrWhiteSpace(spawn.SetFlagOnCatch))
                        {
                            fish.SetFlagOnPickup = spawn.SetFlagOnCatch;
                        }
                        if (spawn.IsBossFish)
                        {
                            fish.SetTempData("IsBossFish", value: true);
                        }
                        if (spawn.CatchLimit <= -1 || !player.fishCaught.TryGetValue(fish.QualifiedItemId, out var values) || values[0] < spawn.CatchLimit)
                        {
                            float c = CheckGenericFishRequirements(fish, allFishData, location, player, spawn, waterDepth, usingMagicBait, hasCuriosityLure, spawn.ItemId == baitTargetFish, isTutorialCatch);
                            if (c > 0)
                            {
                                if (baitTargetFish == null || !(fish.QualifiedItemId != baitTargetFish) || targetedBaitTries >= 2)
                                {
                                    passed[fish.QualifiedItemId] = chance * c;
                                }
                                firstNonTargetFish ??= fish;
                                targetedBaitTries++;
                            }
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
                bool? canUseTrainingRod = spawn.CanUseTrainingRod;
                if (canUseTrainingRod.HasValue)
                {
                    if (!canUseTrainingRod.GetValueOrDefault())
                    {
                        return 0;
                    }
                }
                else
                {
                    if (!ArgUtility.TryGetInt(specificFishData, 1, out var difficulty, out _, "int difficulty"))
                    {
                        return 0;
                    }
                    if (difficulty >= 50)
                    {
                        return 0;
                    }
                }
            }
            if (isTutorialCatch)
            {
                if (!ArgUtility.TryGetOptionalBool(specificFishData, 13, out var isTutorialFish, out _, defaultValue: false, "bool isTutorialFish"))
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
                    if (!ArgUtility.TryGet(specificFishData, 5, out var rawTimeSpans, out _, allowBlank: true, "string rawTimeSpans"))
                    {
                        return 0;
                    }
                    string[] timeSpans = ArgUtility.SplitBySpace(rawTimeSpans);
                    bool found = false;
                    for (int i = 0; i < timeSpans.Length; i += 2)
                    {
                        if (!ArgUtility.TryGetInt(timeSpans, i, out var startTime, out _, "int startTime") || !ArgUtility.TryGetInt(timeSpans, i + 1, out var endTime, out _, "int endTime"))
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
                    if (!ArgUtility.TryGet(specificFishData, 7, out var weather, out _, allowBlank: true, "string weather"))
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
                if (!ArgUtility.TryGetInt(specificFishData, 12, out var minFishingLevel, out _, "int minFishingLevel"))
                {
                    return 0;
                }
                if (player.FishingLevel < minFishingLevel)
                {
                    return 0;
                }
                if (!ArgUtility.TryGetInt(specificFishData, 9, out var maxDepth, out _, "int maxDepth") || !ArgUtility.TryGetFloat(specificFishData, 10, out var chance, out _, "float chance") || !ArgUtility.TryGetFloat(specificFishData, 11, out var depthMultiplier, out _, "float depthMultiplier"))
                {
                    return 0;
                }
                float dropOffAmount = depthMultiplier * chance;
                chance -= Math.Max(0, maxDepth - waterDepth) * dropOffAmount;
                chance += player.FishingLevel / 50f;
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


        private void AddFishToListDynamic(Vector2 bobberTile)              //very performance intensive check for fish fish available in this area - simulates fishing
        {
            try
            {
                if (Game1.player.CurrentItem is FishingRod rod)  //dummy workaround for preventing player from getting special items
                {
                    who = new Farmer();
                    foreach (var m in Game1.player.mailReceived) who.mailReceived.Add(m);
                    who.currentLocation = Game1.player.currentLocation;
                    who.setTileLocation(Game1.player.Tile);
                    who.setSkillLevel("Fishing", Game1.player.FishingLevel);
                    //if there's ever any downside of referencing player rod directly, use below + add bait/tackle to it
                    //FishingRod rod = (FishingRod)(Game1.player.CurrentTool as FishingRod).getOne();
                    //who.CurrentTool = rod;
                    FishingRod rod2 = new() { UpgradeLevel = rod.UpgradeLevel };
                    foreach (var item in rod.attachments)
                    {
                        if (ItemRegistry.Create(item.QualifiedItemId, item.Stack, item.Quality, true) is Object obj)
                        {
                            rod2.attachments.Add(obj);
                        }
                    }
                    who.CurrentTool = rod2;
                    who.luckLevel.Value = Game1.player.LuckLevel;
                    foreach (var item in Game1.player.fishCaught) who.fishCaught.Add(item);
                    foreach (var m in Game1.player.secretNotesSeen) who.secretNotesSeen.Add(m);
                    who.stats.Values = [];
                    foreach (var m in Game1.player.stats.Values) who.stats.Set(m.Key, m.Value);

                    if (oldGeneric == null)
                    {
                        oldGeneric = [];
                        fishFailed = [];
                        fishHere = ["(O)168"];
                        fishChances = new() { { "-1", 0 }, { "(O)168", 0 } };
                        fishChancesSlow = [];
                        fishChancesModulo = 1;
                    }
                    int freq = isMinigame || isMinigameOther ? 6 / totalPlayersOnThisPC : extraCheckFrequency * 10 / totalPlayersOnThisPC; //minigame lowers frequency
                    for (int i = 0; i < freq; i++)
                    {
                        string fish = AddHardcoded(bobberTile, true);
                        if (fish != "-2")//not fully hardcoded
                        {
                            if (fish == "-1")//dynamic
                            {
                                //int nuts = 5;                                                                           //"fix" for preventing player from not getting specials       ----start1
                                //bool mail1 = false;
                                //bool mail2 = false;
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
                                rod2.isFishing = true;
                                item = who.currentLocation.getFish(0, rod2.GetBait()?.ItemId, 5, who, 100, bobberTile);
                                rod2.isFishing = false;
                                Game1.stats.TimesFished--;
                                try
                                {
                                    if (item.DisplayName.StartsWith("Error Item") == true)
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
                                //    if (nuts < 5) Game1.player.team.limitedNutDrops["IslandFishing"] = nuts;
                                //    if (mail1) Game1.player.mailReceived.Add("islandNorthCaveOpened");
                                //    if (mail2) Game1.player.mailForTomorrow.Add("islandNorthCaveOpened");                                                                                //-----end2
                                //}
                            }
                            else
                            {
                                item = ItemRegistry.Create(fish);
                            }
                            int val;
                            if (fishChances["-1"] < int.MaxValue) //percentages, slow version (the one shown) is updated less over time
                            {
                                if (item.Category == Object.junkCategory)
                                {
                                    fishChances.TryGetValue("(O)168", out val);
                                    fishChances["(O)168"] = val + 1;
                                }
                                else if (!fishHere.Contains(fish))
                                {
                                    fishChances = new() { { "-1", 0 } };//reset % on new fish added
                                    foreach (var f in fishHere) fishChances.Add(f, 1);
                                    fishChancesSlow = [];
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
                            if (item.Category != Object.junkCategory)
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
            }
            catch
            { }
        }

        private string AddHardcoded(Vector2 bobberTile, bool dynamic)//-2 skip dynamic, -1 dynamic, above -1 = item to add to dynamic
        {
            FishingRod rod = who.CurrentTool as FishingRod;
            if (who.currentLocation is IslandLocation)
            {
                if (Utility.CreateRandom(Game1.stats.DaysPlayed, Game1.stats.TimesFished + 1, Game1.uniqueIDForThisGame).NextDouble() < 0.15 && (!Game1.player.team.limitedNutDrops.ContainsKey("IslandFishing") || Game1.player.team.limitedNutDrops["IslandFishing"] < 5)) return "(O)73";//nut

                if (who.currentLocation is IslandSouthEast && bobberTile.X >= 17 && bobberTile.X <= 21 && bobberTile.Y >= 19 && bobberTile.Y <= 23)
                {
                    if (!(who.currentLocation as IslandSouthEast).fishedWalnut.Value)
                    {
                        fishHere = ["(O)73"];
                        fishChancesSlow = new() { { "-1", 1 }, { "(O)73", 1 }, { "(O)168", 0 } };
                    }
                    else
                    {
                        fishHere = ["(O)168"];
                        fishChancesSlow = new() { { "-1", 1 }, { "(O)168", 1 } };
                    }
                    oldGeneric = null;
                    return "-2";//prevents altering game state in dynamic
                }
            }
            else if (!dynamic)
            {
                if (who.currentLocation is Railroad)
                {
                    if (who.secretNotesSeen.Contains(GameLocation.NECKLACE_SECRET_NOTE_INDEX) && !who.hasOrWillReceiveMail(GameLocation.CAROLINES_NECKLACE_MAIL))
                    {
                        return GameLocation.CAROLINES_NECKLACE_ITEM_QID;
                    }
                }
                else if (who.currentLocation is Forest)
                {
                    if (bobberTile.X > 50f && bobberTile.X < 66f && bobberTile.Y > 100f)
                    {
                        if (!rod.QualifiedItemId.Contains("TrainingRod"))
                        {
                            float gobyChance = 0.15f;
                            if (rod != null)
                            {
                                if (rod.HasCuriosityLure())
                                {
                                    gobyChance += 0.15f;
                                }
                                if (rod.GetBait() != null && rod.GetBait().Name.Contains("Goby"))
                                {
                                    gobyChance += 0.2f;
                                }
                            }
                            if (gobyChance > 0)
                            {
                                return "(O)Goby|" + gobyChance;
                            }
                            if (Game1.IsFall)
                            {
                                return "(O)139|0.15";
                            }
                        }
                    }
                }
                else if (who.currentLocation is MineShaft mine)
                {
                    if (!rod.QualifiedItemId.Contains("TrainingRod"))
                    {
                        double chanceMultiplier = 1.5;
                        chanceMultiplier += 0.4 * who.FishingLevel;
                        string baitName = "";
                        if (rod != null)
                        {
                            if (rod.HasCuriosityLure())
                            {
                                chanceMultiplier += 5.0;
                            }
                            baitName = rod.GetBait()?.Name ?? "";
                        }
                        switch (mine.getMineArea())
                        {
                            case 0:
                            case 10:
                                chanceMultiplier += baitName.Contains("Stonefish") ? 10 : 0;
                                return "(O)158|" + (0.02 + 0.01 * chanceMultiplier);
                            case 40:
                                chanceMultiplier += baitName.Contains("Ice Pip") ? 10 : 0;
                                return "(O)161|" + (0.015 + 0.009 * chanceMultiplier);
                            case 80:
                                chanceMultiplier += baitName.Contains("Lava Eel") ? 10 : 0;
                                return "(O)162|" + (0.01 + 0.008 * chanceMultiplier)
                                    + "|(O)CaveJelly|" + (0.05 + who.LuckLevel * 0.05);
                        }
                    }
                }
            }
            return "-1";
        }

        private void AddCrabPotFish()
        {
            fishHere = [];

            bool isMariner = who.professions.Contains(10);
            if (!isMariner) fishHere.Add("(O)168");//trash
            fishChancesSlow = [];

            double fishChance = 1f;
            if (!isMariner && who.currentLocation.TryGetFishAreaForTile(who.Tile, out var _, out var data))
            {
                fishChance = 1f - (double?)data?.CrabPotJunkChance ?? 0.2;
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
                                    fishChance *= 1f - rawChance;
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

        private Item item;
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
        private static void DrawStringWithBorder(SpriteBatch batch, SpriteFont font, string text, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth, Color? borderColor = null)
        {
            Color border = borderColor ?? Color.Black;
            border.A = color.A;
            batch.DrawString(font, text, position + new Vector2(-1.2f * scale, -1.2f * scale), border, rotation, origin, scale, effects, layerDepth - 0.00001f);
            batch.DrawString(font, text, position + new Vector2(1.2f * scale, -1.2f * scale), border, rotation, origin, scale, effects, layerDepth - 0.00001f);
            batch.DrawString(font, text, position + new Vector2(-1.2f * scale, 1.2f * scale), border, rotation, origin, scale, effects, layerDepth - 0.00001f);
            batch.DrawString(font, text, position + new Vector2(1.2f * scale, 1.2f * scale), border, rotation, origin, scale, effects, layerDepth - 0.00001f);

            batch.DrawString(font, text, position + new Vector2(-0.2f * scale, -0.2f * scale), color, rotation, origin, scale, effects, layerDepth);
            batch.DrawString(font, text, position + new Vector2(0.2f * scale, 0.2f * scale), color, rotation, origin, scale, effects, layerDepth);
        }
        private void AddBackground(SpriteBatch batch, Vector2 boxTopLeft, Vector2 boxBottomLeft, int iconCount, Rectangle source, float iconScale, float boxWidth, float boxHeight, bool showPercent, bool showExtraIcons)
        {
            float fixScale10 = FixIconScale(10f);
            if (backgroundMode[screen] == 0)
            {
                batch.Draw(background[backgroundMode[screen]], new Rectangle((int)boxBottomLeft.X - 1, (int)boxBottomLeft.Y - 1, (int)(source.Width * iconScale + (showExtraIcons ? fixScale10 : 0)) + 1, (int)(source.Width * iconScale + (showPercent ? fixScale10 : 0)) + 1),
                    null, colorBg, 0f, Vector2.Zero, SpriteEffects.None, 0.5f);
            }
            else if (backgroundMode[screen] == 1)
            {
                if (iconMode[screen] == 0) batch.Draw(background[backgroundMode[screen]], new Rectangle((int)boxTopLeft.X - 2, (int)boxTopLeft.Y - 2, (int)(source.Width * iconScale * Math.Min(iconCount, maxIconsPerRow[screen]) + (showExtraIcons ? fixScale10 : 0)) + 5,
               (int)((source.Width * iconScale + (showPercent ? fixScale10 : 0)) * Math.Ceiling(iconCount / (maxIconsPerRow[screen] * 1.0))) + 5), null, colorBg, 0f, Vector2.Zero, SpriteEffects.None, 0.5f);
                else if (iconMode[screen] == 1) batch.Draw(background[backgroundMode[screen]], new Rectangle((int)boxTopLeft.X - 2, (int)boxTopLeft.Y - 2, (int)(source.Width * iconScale * Math.Ceiling(iconCount / (maxIconsPerRow[screen] * 1.0)) + (showExtraIcons ? fixScale10 : 0)) + 5,
                    (int)((source.Width * iconScale + (showPercent ? fixScale10 : 0)) * Math.Min(iconCount, maxIconsPerRow[screen])) + 5), null, colorBg, 0f, Vector2.Zero, SpriteEffects.None, 0.5f);
                else if (iconMode[screen] == 2) batch.Draw(background[backgroundMode[screen]], new Rectangle((int)boxTopLeft.X - 2, (int)boxTopLeft.Y - 2, (int)(boxWidth - boxTopLeft.X + 6), (int)boxHeight + 4),
                    null, colorBg, 0f, Vector2.Zero, SpriteEffects.None, 0.5f);
            }
        }

        public static float FixRectScale(Rectangle source, float scale)
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
                float scale = FixRectScale(source, 1f);

                float w = scale * source.Width;
                float h = scale * source.Height;
                if (w < 16f)
                {
                    offset.X += (int)(16f - w / 2f);
                }
                if (h < 16f)
                {
                    offset.Y += (int)(16f - h / 2f);
                }
            }
            return offset;
        }
        public float FixIconScale(float scale)
        {
            return scale * barScale[screen] / fixedZooom;
        }
    }
}
