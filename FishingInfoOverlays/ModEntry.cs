using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Tools;

namespace StardewMods
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        private bool dayStarted;    //world fish preview data 
        private Dictionary<string, string> locationData;
        private Dictionary<int, string> fishData;
        private List<int> fishHere;
        private Texture2D background;
        private string oldLoc = "";
        private int oldZone = 0;
        private int oldTime = 0;

        private bool modFound_StardewAquarium;

        private bool isMinigame = false;    //minigame fish preview data, Reflection
        private int miniFish;
        private float miniFishPos;
        private int miniXPositionOnScreen;
        private int miniYPositionOnScreen;
        private Vector2 miniFishShake;
        private Vector2 miniEverythingShake;
        private Vector2 miniBarShake;
        private Vector2 miniTreasureShake;
        private float miniScale;
        private bool miniBobberInBar;
        private float miniBobberBarPos;
        private float miniBobberBarHeight;
        private float miniTreasurePosition;
        private float miniTreasureScale;
        private float miniTreasureCatchLevel;
        private bool miniTreasureCaught;


        private ModConfig Config;   //config values
        private int miniMode = 0;
        private bool barCrabEnabled = true;
        private Vector2 barPosition;
        private int iconMode = 0;
        private float barScale = 0;
        private int maxIcons = 0;
        private int maxIconsPerRow = 0;
        private int backgroundMode = 0;
        private int extraCheckFrequency = 0;
        private bool showTackles = true;
        private bool showTrash = true;
        private bool uncaughtDark = true;
        private int legendaryMode = 0;


        public override void Entry(IModHelper helper)
        {
            modFound_StardewAquarium = helper.ModRegistry.IsLoaded("Cherry.StardewAquarium");

            Config = Helper.ReadConfig<ModConfig>();

            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            helper.Events.GameLoop.DayStarted += this.OnDayStarted;
            helper.Events.Display.RenderedHud += this.OnOneSecondUpdateTicked;
            helper.Events.Display.MenuChanged += this.OnMenuChanged;
            helper.Events.Display.RenderedActiveMenu += this.OnRenderMenu;
            helper.Events.GameLoop.GameLaunched += this.GenericModConfigMenuIntegration;
        }

        private void GenericModConfigMenuIntegration(object sender, GameLaunchedEventArgs e)     //Generic Mod Config Menu API
        {
            Config = Helper.ReadConfig<ModConfig>();
            ITranslationHelper Transl = Helper.Translation;
            var GenericMC = Helper.ModRegistry.GetApi<GenericModConfigMenuAPI>("spacechase0.GenericModConfigMenu");
            GenericMC.RegisterModConfig(ModManifest, () => Config = new ModConfig(), () => Helper.WriteConfig(Config));
            GenericMC.SetDefaultIngameOptinValue(ModManifest, true);
            GenericMC.RegisterLabel(ModManifest, Transl.Get("GenericMC.barLabel"), ""); //All of these strings are stored in the traslation files.
            GenericMC.RegisterParagraph(ModManifest, Transl.Get("GenericMC.barDescription") + "\n");
            GenericMC.RegisterChoiceOption(ModManifest, Transl.Get("GenericMC.barIconMode"), Transl.Get("GenericMC.barIconModeDesc"),
                () => (Config.BarIconMode == 0) ? Transl.Get("GenericMC.barIconModeHor") : (Config.BarIconMode == 1) ? Transl.Get("GenericMC.barIconModeVert") : (Config.BarIconMode == 2) ? Transl.Get("GenericMC.barIconModeVertText") : Transl.Get("GenericMC.Disabled"),
                (string val) => Config.BarIconMode = Int32.Parse((val.Equals(Transl.Get("GenericMC.barIconModeHor"))) ? "0" : (val.Equals(Transl.Get("GenericMC.barIconModeVert"))) ? "1" : (!val.Equals(Transl.Get("GenericMC.Disabled"))) ? "2" : "3"),
                new string[] { Transl.Get("GenericMC.barIconModeHor"), Transl.Get("GenericMC.barIconModeVert"), Transl.Get("GenericMC.barIconModeVertText"), Transl.Get("GenericMC.Disabled") });//small 'hack' so options appear as name strings, while config.json stores them as integers
            GenericMC.RegisterSimpleOption(ModManifest, Transl.Get("GenericMC.barPosX"), Transl.Get("GenericMC.barPosXDesc"),
                 () => Config.BarTopLeftLocationX, (int val) => Config.BarTopLeftLocationX = Math.Max(0, val));
            GenericMC.RegisterSimpleOption(ModManifest, Transl.Get("GenericMC.barPosY"), Transl.Get("GenericMC.barPosYDesc"),
                () => Config.BarTopLeftLocationY, (int val) => Config.BarTopLeftLocationY = Math.Max(0, val));
            GenericMC.RegisterSimpleOption(ModManifest, Transl.Get("GenericMC.barScale"), Transl.Get("GenericMC.barScaleDesc"),
                () => (float)Config.BarScale, (float val) => Config.BarScale = Math.Min(10, Math.Max(0.1f, val)));
            GenericMC.RegisterSimpleOption(ModManifest, Transl.Get("GenericMC.barMaxIcons"), Transl.Get("GenericMC.barMaxIconsDesc"),
               () => Config.BarMaxIcons, (int val) => Config.BarMaxIcons = (int)Math.Min(500, Math.Max(4, val)));
            GenericMC.RegisterSimpleOption(ModManifest, Transl.Get("GenericMC.barMaxIconsPerRow"), Transl.Get("GenericMC.barMaxIconsPerRowDesc"),
                () => Config.BarMaxIconsPerRow, (int val) => Config.BarMaxIconsPerRow = (int)Math.Min(500, Math.Max(4, val)));
            GenericMC.RegisterChoiceOption(ModManifest, Transl.Get("GenericMC.barBackgroundMode"), Transl.Get("GenericMC.barBackgroundModeDesc"),
                () => (Config.BarBackgroundMode == 0) ? Transl.Get("GenericMC.barBackgroundModeCircles") : (Config.BarBackgroundMode == 1) ? Transl.Get("GenericMC.barBackgroundModeRect") : Transl.Get("GenericMC.Disabled"),
                (string val) => Config.BarBackgroundMode = Int32.Parse((val.Equals(Transl.Get("GenericMC.barBackgroundModeCircles"))) ? "0" : (val.Equals(Transl.Get("GenericMC.barBackgroundModeRect"))) ? "1" : "2"),
                new string[] { Transl.Get("GenericMC.barBackgroundModeCircles"), Transl.Get("GenericMC.barBackgroundModeRect"), Transl.Get("GenericMC.Disabled") });
            GenericMC.RegisterSimpleOption(ModManifest, Transl.Get("GenericMC.barShowBaitTackle"), Transl.Get("GenericMC.barShowBaitTackleDesc"),
                () => Config.BarShowBaitAndTackleInfo, (bool val) => Config.BarShowBaitAndTackleInfo = val);
            GenericMC.RegisterSimpleOption(ModManifest, Transl.Get("GenericMC.barShowTrash"), Transl.Get("GenericMC.barShowTrashDesc"),
                () => Config.BarShowTrash, (bool val) => Config.BarShowTrash = val);
            GenericMC.RegisterChoiceOption(ModManifest, Transl.Get("GenericMC.barLegendaryMode"), Transl.Get("GenericMC.barLegendaryModeDesc"),
                () => (Config.BarLegendaryMode == 0) ? Transl.Get("GenericMC.barLegendaryModeVanilla") : (Config.BarLegendaryMode == 1) ? Transl.Get("GenericMC.barLegendaryModeAlways") : Transl.Get("GenericMC.Disabled"),
                (string val) => Config.BarLegendaryMode = Int32.Parse((val.Equals(Transl.Get("GenericMC.barLegendaryModeVanilla"))) ? "0" : (val.Equals(Transl.Get("GenericMC.barLegendaryModeAlways"))) ? "1" : "2"),
                new string[] { Transl.Get("GenericMC.barLegendaryModeVanilla"), Transl.Get("GenericMC.barLegendaryModeAlways"), Transl.Get("GenericMC.Disabled") });
            GenericMC.RegisterClampedOption(ModManifest, Transl.Get("GenericMC.barExtraCheckFrequency"), Transl.Get("GenericMC.barExtraCheckFrequencyDesc"),
                () => Config.BarExtraCheckFrequency, (int val) => Config.BarExtraCheckFrequency = val, 0, 200);
            GenericMC.RegisterSimpleOption(ModManifest, Transl.Get("GenericMC.barCrabPotEnabled"), Transl.Get("GenericMC.barCrabPotEnabledDesc"),
                () => Config.BarCrabPotEnabled, (bool val) => Config.BarCrabPotEnabled = val);
            GenericMC.RegisterSimpleOption(ModManifest, Transl.Get("GenericMC.barUncaughtDarker"), Transl.Get("GenericMC.barUncaughtDarkerDesc"),
                () => Config.UncaughtFishAreDark, (bool val) => Config.UncaughtFishAreDark = val);
            GenericMC.RegisterLabel(ModManifest, Transl.Get("GenericMC.MinigameLabel"), "");
            GenericMC.RegisterParagraph(ModManifest, Transl.Get("GenericMC.MinigameDescription"));
            GenericMC.RegisterChoiceOption(ModManifest, Transl.Get("GenericMC.MinigameMode"), Transl.Get("GenericMC.MinigameModeDesc"),
                () => (Config.MinigamePreviewMode == 0) ? Transl.Get("GenericMC.MinigameModeFull") : (Config.MinigamePreviewMode == 1) ? Transl.Get("GenericMC.MinigameModeSimple") : (Config.MinigamePreviewMode == 2) ? Transl.Get("GenericMC.MinigameModeBarOnly") : Transl.Get("GenericMC.Disabled"),
                (string val) => Config.MinigamePreviewMode = Int32.Parse((val.Equals(Transl.Get("GenericMC.MinigameModeFull"))) ? "0" : (val.Equals(Transl.Get("GenericMC.MinigameModeSimple"))) ? "1" : (val.Equals(Transl.Get("GenericMC.MinigameModeBarOnly"))) ? "2" : "3"),
                new string[] { Transl.Get("GenericMC.MinigameModeFull"), Transl.Get("GenericMC.MinigameModeSimple"), Transl.Get("GenericMC.MinigameModeBarOnly"), Transl.Get("GenericMC.Disabled") });
        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady || !(e.Button == SButton.F5)) // ignore if player hasn't loaded a save yet
                return;
            Config = Helper.ReadConfig<ModConfig>();
            this.UpdateConfig();
        }


        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            dayStarted = false;
            this.UpdateConfig();
            dayStarted = true;
        }


        /*  Add hardcoded + generic fish logic. Simplify/automate? DONE?
         *  maxIcons + maxIconsPerRow DONE
         *  make config update on day start + f5 only: update function. DONE
         *  Minigame: Preview, if not caught dark. DONE
         *  Tackle + bait preview with values? DONE
         *  Crab Pot preview? DONE
         *  better minigame: full, simple, just on preview, off DONE
         *  2ndary check for modded fish: Bad performance, setting is Check Frequency: 0-200 slider? DONE
         *  Dark preview (???) if fish not caught. DONE
         */

        private void OnOneSecondUpdateTicked(object sender, RenderedHudEventArgs e)
        {
            if (!dayStarted || Game1.eventUp || Game1.player.CurrentTool == null || (!(Game1.player.CurrentTool is FishingRod) && !(Game1.player.CurrentItem.Name.Equals("Crab Pot")))) return;//code stop conditions
            if (Game1.player.CurrentItem.Name.Equals("Crab Pot") && !barCrabEnabled) return;

            SpriteFont font = Game1.smallFont;                                                          //UI INIT
            Rectangle source = GameLocation.getSourceRectForObject(Game1.player.CurrentToolIndex);      //for average icon size
            SpriteBatch batch = Game1.spriteBatch;

            batch.End();    //stop current UI drawing and start mode where where layers work from 0f-1f
            batch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise);

            if (isMinigame) //MINIGAME PREVIEW
            {
                if (miniMode < 2)
                {
                    if (miniMode == 0) //Full minigame
                    {
                        //rod+bar textture cut to only cover the minigame bar
                        batch.Draw(Game1.mouseCursors, Utility.ModifyCoordinatesForUIScale(new Vector2(miniXPositionOnScreen + 126, miniYPositionOnScreen + 300) + miniEverythingShake),
                            new Rectangle(658, 2000, 15, 145), Color.White * miniScale, 0f, new Vector2(18.5f, 74f) * miniScale, Utility.ModifyCoordinateForUIScale(4f * miniScale), SpriteEffects.None, 0.01f);

                        if (miniScale == 1f)
                        {
                            //green moving bar player controls
                            batch.Draw(Game1.mouseCursors, Utility.ModifyCoordinatesForUIScale(new Vector2(miniXPositionOnScreen + 64, miniYPositionOnScreen + 12 + (int)miniBobberBarPos) + miniBarShake + miniEverythingShake),
                                new Rectangle(682, 2078, 9, 2), miniBobberInBar ? Color.White : (Color.White * 0.25f * ((float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 100.0), 2) + 2f)), 0f, Vector2.Zero, Utility.ModifyCoordinateForUIScale(4f), SpriteEffects.None, 0.89f);
                            batch.Draw(Game1.mouseCursors, Utility.ModifyCoordinatesForUIScale(new Vector2(miniXPositionOnScreen + 64, miniYPositionOnScreen + 12 + (int)miniBobberBarPos + 8) + miniBarShake + miniEverythingShake),
                                new Rectangle(682, 2081, 9, 1), miniBobberInBar ? Color.White : (Color.White * 0.25f * ((float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 100.0), 2) + 2f)), 0f, Vector2.Zero, Utility.ModifyCoordinatesForUIScale(new Vector2(4f, miniBobberBarHeight - 16)), SpriteEffects.None, 0.89f);
                            batch.Draw(Game1.mouseCursors, Utility.ModifyCoordinatesForUIScale(new Vector2(miniXPositionOnScreen + 64, miniYPositionOnScreen + 12 + (int)miniBobberBarPos + miniBobberBarHeight - 8) + miniBarShake + miniEverythingShake),
                                new Rectangle(682, 2085, 9, 2), miniBobberInBar ? Color.White : (Color.White * 0.25f * ((float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 100.0), 2) + 2f)), 0f, Vector2.Zero, Utility.ModifyCoordinateForUIScale(4f), SpriteEffects.None, 0.89f);
                            //treasure
                            batch.Draw(Game1.mouseCursors, Utility.ModifyCoordinatesForUIScale(new Vector2(miniXPositionOnScreen + 64 + 18, (float)(miniYPositionOnScreen + 12 + 24) + miniTreasurePosition) + miniTreasureShake + miniEverythingShake),
                                new Rectangle(638, 1865, 20, 24), Color.White, 0f, new Vector2(10f, 10f), Utility.ModifyCoordinateForUIScale(2f * miniTreasureScale), SpriteEffects.None, 0.9f);
                            if (miniTreasureCatchLevel > 0f && !miniTreasureCaught)//treasure progress
                            {
                                batch.Draw(Game1.staminaRect, new Rectangle((int)Utility.ModifyCoordinateForUIScale(miniXPositionOnScreen + 64), (int)Utility.ModifyCoordinateForUIScale(miniYPositionOnScreen + 12 + (int)miniTreasurePosition), (int)Utility.ModifyCoordinateForUIScale(40), (int)Utility.ModifyCoordinateForUIScale(8)), null, Color.DimGray * 0.5f, 0f, Vector2.Zero, SpriteEffects.None, 0.9f);
                                batch.Draw(Game1.staminaRect, new Rectangle((int)Utility.ModifyCoordinateForUIScale(miniXPositionOnScreen + 64), (int)Utility.ModifyCoordinateForUIScale(miniYPositionOnScreen + 12 + (int)miniTreasurePosition), (int)Utility.ModifyCoordinateForUIScale((miniTreasureCatchLevel * 40f)), (int)Utility.ModifyCoordinateForUIScale(8)), null, Color.Orange, 0f, Vector2.Zero, SpriteEffects.None, 0.9f);
                            }
                        }
                    }
                    else batch.Draw(Game1.mouseCursors, Utility.ModifyCoordinatesForUIScale(new Vector2(miniXPositionOnScreen + 82, (miniYPositionOnScreen + 36) + miniFishPos) + miniFishShake + miniEverythingShake),
                        new Rectangle(614 + (FishingRod.isFishBossFish(miniFish) ? 20 : 0), 1840, 20, 20), Color.Black, 0f, new Vector2(10f, 10f),
                        Utility.ModifyCoordinateForUIScale(2.05f), SpriteEffects.None, 0.9f);//Simple minigame

                    source = GameLocation.getSourceRectForObject(miniFish);
                    batch.Draw(Game1.objectSpriteSheet, Utility.ModifyCoordinatesForUIScale(new Vector2(miniXPositionOnScreen + 82, (miniYPositionOnScreen + 36) + miniFishPos) + miniFishShake + miniEverythingShake),
                        source, (!uncaughtDark || Game1.player.fishCaught.ContainsKey(miniFish)) ? Color.White : Color.DarkSlateGray, 0f, new Vector2(9.5f, 9f),
                        Utility.ModifyCoordinateForUIScale(3f), SpriteEffects.FlipHorizontally, 1f);
                }
            }



            if (iconMode != 3)
            {
                float iconScale = Game1.pixelZoom / 2f * barScale;
                int iconCount = 0;
                float boxWidth = 0;
                float boxHeight = 0;
                Vector2 boxTopLeft = barPosition;
                Vector2 boxBottomLeft = barPosition;

                if (showTackles && Game1.player.CurrentTool is FishingRod)    //BAIT AND TACKLE (BOBBERS) PREVIEW
                {
                    int bait = (Game1.player.CurrentTool as FishingRod).getBaitAttachmentIndex();
                    int tackle = (Game1.player.CurrentTool as FishingRod).getBobberAttachmentIndex();
                    if (bait > -1)
                    {
                        source = GameLocation.getSourceRectForObject(bait);
                        if (backgroundMode == 0) AddBackground(batch, boxTopLeft, boxBottomLeft, iconCount, source, iconScale, boxWidth, boxHeight);

                        int baitCount = (Game1.player.CurrentTool as FishingRod).attachments[0].Stack;
                        batch.Draw(Game1.objectSpriteSheet, boxBottomLeft, source, Color.White, 0f, Vector2.Zero, 1.9f * barScale, SpriteEffects.None, 0.9f);
                        Utility.drawTinyDigits(baitCount, batch, boxBottomLeft + new Vector2((source.Height * iconScale) - Utility.getWidthOfTinyDigitString(baitCount, 2f * barScale), 18 * barScale), 2f * barScale, 1f, Color.AntiqueWhite);

                        if (iconMode == 1) boxBottomLeft += new Vector2(0, source.Height * iconScale);
                        else boxBottomLeft += new Vector2(source.Height * iconScale, 0);
                        iconCount++;
                    }
                    if (tackle > -1)
                    {
                        source = GameLocation.getSourceRectForObject(tackle);
                        if (backgroundMode == 0) AddBackground(batch, boxTopLeft, boxBottomLeft, iconCount, source, iconScale, boxWidth, boxHeight);

                        int tackleCount = FishingRod.maxTackleUses - (Game1.player.CurrentTool as FishingRod).attachments[1].uses;
                        batch.Draw(Game1.objectSpriteSheet, boxBottomLeft, source, Color.White, 0f, Vector2.Zero, 1.9f * barScale, SpriteEffects.None, 0.9f);
                        Utility.drawTinyDigits(tackleCount, batch, boxBottomLeft + new Vector2((source.Height * iconScale) - Utility.getWidthOfTinyDigitString(tackleCount, 2f * barScale), 18 * barScale), 2f * barScale, 1f, Color.AntiqueWhite);

                        if (iconMode == 1) boxBottomLeft += new Vector2(0, source.Height * iconScale);
                        else boxBottomLeft += new Vector2(source.Height * iconScale, 0);
                        iconCount++;
                    }
                    if (iconMode == 2 && (bait + tackle) > -1)
                    {
                        boxBottomLeft = boxTopLeft + new Vector2(0, source.Height * iconScale);
                        boxHeight += (source.Width * iconScale);
                        if (bait > 0 && tackle > 0) iconCount--;
                    }
                }


                if (Game1.player.currentLocation.canFishHere())
                {
                    if (showTrash && (Game1.player.CurrentTool is FishingRod || (barCrabEnabled && !Game1.player.professions.Contains(11)))) //TRASH PREVIEW
                    {
                        source = GameLocation.getSourceRectForObject(168);
                        if (backgroundMode == 0) AddBackground(batch, boxTopLeft, boxBottomLeft, iconCount, source, iconScale, boxWidth, boxHeight);

                        batch.Draw(Game1.objectSpriteSheet, boxBottomLeft + new Vector2(2 * barScale, -5 * barScale), source, Color.White, 0f, Vector2.Zero, 1.9f * barScale, SpriteEffects.None, 0.99f);
                        if (iconMode == 2)
                        {
                            string trashLocalizedName = (new StardewValley.Object(168, 1).Name.Equals("Trash")) ? new StardewValley.Object(168, 1).DisplayName : "Trash";
                            if (backgroundMode == 0)
                            {
                                batch.DrawString(font, trashLocalizedName, boxBottomLeft + new Vector2(source.Width * iconScale, 0), Color.Black, 0f, new Vector2(1, -2), 1f * barScale, SpriteEffects.None, 0.9f); //textbg
                                batch.DrawString(font, trashLocalizedName, boxBottomLeft + new Vector2(source.Width * iconScale, 0), Color.Black, 0f, new Vector2(-1, -4), 1f * barScale, SpriteEffects.None, 0.9f); //textbg
                            }
                            batch.DrawString(font, trashLocalizedName, boxBottomLeft + new Vector2(source.Width * iconScale, 0), Color.White, 0f, new Vector2(0, -3), 1f * barScale, SpriteEffects.None, 1f); //text
                            boxWidth = Math.Max(boxWidth, boxBottomLeft.X + (font.MeasureString(trashLocalizedName).X * barScale) + (source.Width * iconScale));
                        }
                        if (iconMode == 0) boxBottomLeft += new Vector2(source.Height * iconScale, 0);
                        else boxBottomLeft += new Vector2(0, source.Height * iconScale);
                        boxHeight += (source.Width * iconScale);
                        iconCount++;
                    }


                    string locationName = Game1.player.currentLocation.Name;    //LOCATION FISH PREVIEW                 //this.Monitor.Log("\n", LogLevel.Debug);
                    if (!locationData.ContainsKey(locationName) && !locationName.StartsWith("UndergroundMine")              //rod
                        || (locationName.StartsWith("UndergroundMine") && Game1.player.CurrentItem.Name.Equals("Crab Pot")))//crab pot
                    {
                        if (locationName.StartsWith("UndergroundMine"))
                        {
                            string warning = Helper.Translation.Get("Bar.CrabMineWarning");
                            batch.DrawString(font, warning, boxBottomLeft + new Vector2(source.Width * iconScale, 0), Color.Black, 0f, new Vector2(1, -2), 1f * barScale, SpriteEffects.None, 0.9f); //textbg
                            batch.DrawString(font, warning, boxBottomLeft + new Vector2(source.Width * iconScale, 0), Color.Black, 0f, new Vector2(-1, -4), 1f * barScale, SpriteEffects.None, 0.9f); //textbg
                            batch.DrawString(font, warning, boxBottomLeft + new Vector2(source.Width * iconScale, 0), Color.Red, 0f, new Vector2(0, -3), 1f * barScale, SpriteEffects.None, 1f); //text
                        }
                        batch.End();
                        batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise);
                        return;
                    }

                    if (Game1.player.CurrentTool is FishingRod)
                    {
                        if (!isMinigame)//don't reset main list while minigame to prevent lag
                        {
                            if (oldTime != Game1.timeOfDay || !oldLoc.Equals(Game1.player.currentLocation.Name) || oldZone != Game1.player.currentLocation.getFishingLocation(Game1.player.getTileLocation()))
                            {
                                oldTime = Game1.timeOfDay;
                                oldLoc = Game1.player.currentLocation.Name;
                                oldZone = Game1.player.currentLocation.getFishingLocation(Game1.player.getTileLocation());
                                fishHere = new List<int>();

                                if (extraCheckFrequency == 0) AddHardcodedFishToList(locationName);
                                else AddGenericFishToList(locationName, Game1.player.currentLocation.getFishingLocation(Game1.player.getTileLocation()));
                            }
                        }
                        if (extraCheckFrequency > 0) AddFishToListDynamic();
                    }
                    else AddCrabPotFish();
                    //for (int i = 0; i < 20; i++)    //TEST ITEM INSERT
                    //{
                    //    fishHere.Add(100 + i);
                    //}

                    foreach (var fish in fishHere)
                    {
                        if (iconCount < maxIcons)
                        {
                            iconCount++;
                            string fishNameLocalized = "???";

                            if (new StardewValley.Object(fish, 1).Name.StartsWith("Error", StringComparison.Ordinal))   //Furniture
                            {
                                if (!uncaughtDark || Game1.player.fishCaught.ContainsKey(fish)) fishNameLocalized = new StardewValley.Objects.Furniture(fish, Vector2.Zero).DisplayName;

                                batch.Draw(StardewValley.Objects.Furniture.furnitureTexture, boxBottomLeft, new StardewValley.Objects.Furniture(fish, Vector2.Zero).defaultSourceRect, (!uncaughtDark || Game1.player.fishCaught.ContainsKey(fish))
                                    ? Color.White : Color.DarkSlateGray, 0f, Vector2.Zero, 0.95f * barScale, SpriteEffects.None, 1f);//icon
                            }
                            else                                                                                        //Item
                            {
                                if (!uncaughtDark || Game1.player.fishCaught.ContainsKey(fish)) fishNameLocalized = new StardewValley.Object(fish, 1).DisplayName;

                                source = GameLocation.getSourceRectForObject(fish);
                                batch.Draw(Game1.objectSpriteSheet, boxBottomLeft, source, (!uncaughtDark || Game1.player.fishCaught.ContainsKey(fish))
                                    ? Color.White : Color.DarkSlateGray, 0f, Vector2.Zero, 1.9f * barScale, SpriteEffects.None, 1f);//icon
                            }


                            if (fish == miniFish && miniMode < 3) batch.Draw(background, new Rectangle((int)boxBottomLeft.X - 3, (int)boxBottomLeft.Y - 3, (int)(source.Height * iconScale) + 3, (int)(source.Height * iconScale) + 3),
                                null, Color.GreenYellow, 0f, Vector2.Zero, SpriteEffects.None, 0.9f);//minigame outline

                            if (backgroundMode == 0) AddBackground(batch, boxTopLeft, boxBottomLeft, iconCount, source, iconScale, boxWidth, boxHeight);


                            if (iconMode == 0)      //Horizontal Preview
                            {
                                if (iconCount % maxIconsPerRow == 0) boxBottomLeft = new Vector2(boxTopLeft.X, boxBottomLeft.Y + (source.Height * iconScale)); //row switch
                                else boxBottomLeft += new Vector2(source.Height * iconScale, 0);
                            }
                            else                    //Vertical Preview
                            {
                                if (iconMode == 2)  // + text
                                {
                                    if (backgroundMode == 0)
                                    {
                                        batch.DrawString(font, fishNameLocalized, boxBottomLeft + new Vector2(source.Width * iconScale, 0), Color.Black, 0f, new Vector2(1, -2), 1f * barScale, SpriteEffects.None, 0.9f); //textbg
                                        batch.DrawString(font, fishNameLocalized, boxBottomLeft + new Vector2(source.Width * iconScale, 0), Color.Black, 0f, new Vector2(-1, -4), 1f * barScale, SpriteEffects.None, 0.9f); //textbg
                                    }
                                    batch.DrawString(font, fishNameLocalized, boxBottomLeft + new Vector2(source.Width * iconScale, 0), (!uncaughtDark || Game1.player.fishCaught.ContainsKey(fish))
                                        ? Color.White : Color.DarkGray, 0f, new Vector2(0, -3), 1f * barScale, SpriteEffects.None, 1f); //text
                                    boxWidth = Math.Max(boxWidth, boxBottomLeft.X + (font.MeasureString(fishNameLocalized).X * barScale) + (source.Width * iconScale));
                                }

                                if (iconCount % maxIconsPerRow == 0) //row switch
                                {
                                    if (iconMode == 2) boxBottomLeft = new Vector2(boxWidth + (20 * barScale), boxTopLeft.Y);
                                    else boxBottomLeft = new Vector2(boxBottomLeft.X + (source.Height * iconScale), boxTopLeft.Y);
                                }
                                else boxBottomLeft += new Vector2(0, (source.Height * iconScale));
                                if (iconMode == 2 && iconCount <= maxIconsPerRow) boxHeight += (source.Width * iconScale);
                            }
                        }
                    }
                    if (backgroundMode == 1) AddBackground(batch, boxTopLeft, boxBottomLeft, iconCount, source, iconScale, boxWidth, boxHeight);
                }
            }

            batch.End();
            batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise);
        }



        private void OnMenuChanged(object sender, MenuChangedEventArgs e)   //Minigame data
        {
            if (e.NewMenu is BobberBar) isMinigame = true;
            else
            {
                isMinigame = false;
                miniFish = -1;
                oldTime = 0;
            }
        }
        private void OnRenderMenu(object sender, RenderedActiveMenuEventArgs e)
        {
            if ((Game1.activeClickableMenu is BobberBar bar) && isMinigame)
            {
                miniFish = Helper.Reflection.GetField<int>(bar, "whichFish").GetValue();
                if (miniMode < 2)
                {
                    miniFishPos = Helper.Reflection.GetField<Single>(bar, "bobberPosition").GetValue();
                    miniXPositionOnScreen = Helper.Reflection.GetField<int>(bar, "xPositionOnScreen").GetValue();
                    miniYPositionOnScreen = Helper.Reflection.GetField<int>(bar, "yPositionOnScreen").GetValue();
                    miniFishShake = Helper.Reflection.GetField<Vector2>(bar, "fishShake").GetValue();
                    miniEverythingShake = Helper.Reflection.GetField<Vector2>(bar, "everythingShake").GetValue();
                }
                if (miniMode == 0)
                {
                    miniBarShake = Helper.Reflection.GetField<Vector2>(bar, "barShake").GetValue();
                    miniTreasureShake = Helper.Reflection.GetField<Vector2>(bar, "treasureShake").GetValue();
                    miniScale = Helper.Reflection.GetField<float>(bar, "scale").GetValue();
                    miniBobberInBar = Helper.Reflection.GetField<bool>(bar, "bobberInBar").GetValue();
                    miniBobberBarPos = Helper.Reflection.GetField<float>(bar, "bobberBarPos").GetValue();
                    miniBobberBarHeight = Helper.Reflection.GetField<int>(bar, "bobberBarHeight").GetValue();
                    miniTreasurePosition = Helper.Reflection.GetField<float>(bar, "treasurePosition").GetValue();
                    miniTreasureScale = Helper.Reflection.GetField<float>(bar, "treasureScale").GetValue();
                    miniTreasureCatchLevel = Helper.Reflection.GetField<float>(bar, "treasureCatchLevel").GetValue();
                    miniTreasureCaught = Helper.Reflection.GetField<bool>(bar, "treasureCaught").GetValue();
                }
            }
        }


        private void AddHardcodedFishToList(string locationName)                            //From all "locationName".cs getFish(), ignored if dynamic used
        {
            bool magicBait = Game1.player.currentLocation.IsUsingMagicBait(Game1.player);
            int fishingLocation = Game1.player.currentLocation.getFishingLocation(Game1.player.getTileLocation());

            switch (locationName)
            {
                case "Farm":
                    switch (Game1.whichFarm)
                    {
                        case 1:                                     //Riverland Farm
                            AddGenericFishToList("Forest", 1);
                            AddGenericFishToList("Town", 1);
                            break;
                        case 2:                                     //Forest Farm
                            fishHere.Add(734);//woodskip
                            AddGenericFishToList("Forest", 1);
                            break;
                        case 3:                                     //Hill-top Farm
                            AddGenericFishToList("Forest", 0);
                            break;
                        case 4:                                     //Mountain Farm
                            AddGenericFishToList("Mountain", -1);
                            break;
                        case 5:                                     //FourCourners Farm
                            AddGenericFishToList("Forest", 1);
                            break;
                        case 6:                                     //Beach Farm
                            fishHere.Add(152);//seaweed
                            fishHere.Add(723);//oyster
                            fishHere.Add(393);//coral
                            fishHere.Add(719);//mussel
                            fishHere.Add(718);//cockle
                            AddGenericFishToList("Beach", -1);
                            break;
                        default:                                    //Other Farms
                            AddGenericFishToList(locationName, fishingLocation);
                            break;
                    }
                    break;
                case "Beach":
                    if (legendaryMode != 2 && Game1.player.FishingLevel >= 5)//avoiding tile check for mod compatibility
                    {
                        if (Game1.player.team.SpecialOrderRuleActive("LEGENDARY_FAMILY")) fishHere.Add(898);//crimson jr
                        if ((legendaryMode == 1 || (legendaryMode == 0 && !Game1.player.fishCaught.ContainsKey(159)))
                            && (Game1.currentSeason.Equals("summer") || magicBait))
                        {
                            fishHere.Add(159);//crimsonfish
                        }
                    }
                    if (magicBait)
                    {
                        fishHere.Add(798);//midnight squid
                        fishHere.Add(799);//spook fish
                        fishHere.Add(800);//blobfish
                    }
                    AddGenericFishToList(locationName, fishingLocation);
                    break;
                case "Caldera":
                    fishHere.Add(162);//lava eel
                    AddGenericFishToList(locationName, fishingLocation);
                    break;
                case "Forest":
                    if (legendaryMode != 2 && Game1.player.FishingLevel >= 6)//avoiding tile check for mod compatibility
                    {
                        if (Game1.player.team.SpecialOrderRuleActive("LEGENDARY_FAMILY")) fishHere.Add(902);//glacier jr
                        if ((legendaryMode == 1 || (legendaryMode == 0 && !Game1.player.fishCaught.ContainsKey(775)))
                            && (Game1.currentSeason.Equals("winter") || magicBait))
                        {
                            fishHere.Add(775);//glacierfish
                        }
                    }
                    AddGenericFishToList(locationName, fishingLocation);
                    break;
                case "Mountains":
                    if (legendaryMode != 2 && (Game1.player.team.SpecialOrderRuleActive("LEGENDARY_FAMILY") || Game1.isRaining || magicBait) && Game1.player.FishingLevel >= 10)//avoiding tile check for mod compatibility
                    {
                        if (Game1.player.team.SpecialOrderRuleActive("LEGENDARY_FAMILY")) fishHere.Add(900);//legend jr
                        if ((legendaryMode == 1 || (legendaryMode == 0 && !Game1.player.fishCaught.ContainsKey(163)))
                            && (Game1.currentSeason.Equals("spring") || magicBait))
                        {
                            fishHere.Add(163);//legend
                        }
                    }
                    AddGenericFishToList(locationName, fishingLocation);
                    break;
                case "Sewer":
                    if (legendaryMode != 2)//avoiding tile check for mod compatibility
                    {
                        if (Game1.player.team.SpecialOrderRuleActive("LEGENDARY_FAMILY")) fishHere.Add(901);//radioactive carp
                        if (legendaryMode == 1 || (legendaryMode == 0 && !Game1.player.fishCaught.ContainsKey(682))) fishHere.Add(163);//mutant carp
                    }
                    AddGenericFishToList(locationName, fishingLocation);
                    break;
                case "Submarine":
                    fishHere.Add(800);//blobfish
                    fishHere.Add(799);//spook fish
                    fishHere.Add(798);//midnight squid
                    fishHere.Add(154);//sea cucumber
                    fishHere.Add(155);//super cucumber
                    fishHere.Add(149);//octopus
                    fishHere.Add(152);//seaweed
                    break;
                case "Town":
                    if (legendaryMode != 2 && Game1.player.FishingLevel >= 3)//avoiding tile check for mod compatibility
                    {
                        if (Game1.player.team.SpecialOrderRuleActive("LEGENDARY_FAMILY")) fishHere.Add(899);//ms angler
                        if ((legendaryMode == 1 || (legendaryMode == 0 && !Game1.player.fishCaught.ContainsKey(160)))
                            && (Game1.currentSeason.Equals("fall") || magicBait))
                        {
                            fishHere.Add(160);//mr angler
                        }
                    }
                    AddGenericFishToList(locationName, fishingLocation);
                    break;
                case "ExteriorMuseum":
                    if (modFound_StardewAquarium)
                    {
                        var JsonAssets = Helper.ModRegistry.GetApi<IJsonAssetsApi>("spacechase0.JsonAssets");
                        int PufferChickID = JsonAssets.GetObjectId("Pufferchick");
                        if ((legendaryMode == 1 || (legendaryMode == 0 && !Game1.player.fishCaught.ContainsKey(PufferChickID)))
                            && (Game1.player.fishCaught.ContainsKey(128) && Game1.player.stats.ChickenEggsLayed > 0))
                        {
                            fishHere.Add(PufferChickID);//pufferchick
                        }
                        AddGenericFishToList(locationName, fishingLocation);
                    }
                    break;
                default:
                    if (locationName.StartsWith("UndergroundMine"))
                    {
                        switch ((Game1.player.currentLocation as StardewValley.Locations.MineShaft).mineLevel)
                        {
                            case 20:
                                fishHere.Add(156);//ghostfish
                                fishHere.Add(158);//stonefish
                                fishHere.Add(157);//white algae
                                fishHere.Add(153);//green algae
                                break;
                            case 60:
                                fishHere.Add(156);//ghostfish
                                fishHere.Add(161);//ice pip
                                fishHere.Add(157);//white algae
                                fishHere.Add(153);//green algae
                                break;
                            case 100:
                                fishHere.Add(162);//lava eel
                                break;
                        }
                    }
                    else AddGenericFishToList(locationName, fishingLocation);
                    break;
            }

        }
        private void AddGenericFishToList(string locationName, int fishingLocation)         //From GameLocation.cs getFish()
        {
            bool magicBait = Game1.player.currentLocation.IsUsingMagicBait(Game1.player);
            if (locationName.Equals("BeachNightMarket")) locationName = "Beach";

            string[] rawFishData;
            if (!magicBait) rawFishData = locationData[locationName].Split('/')[4 + Utility.getSeasonNumber(Game1.currentSeason)].Split(' '); //fish by season
            else
            {
                List<string> all_season_fish = new List<string>(); //magic bait = all fish
                for (int k = 0; k < 4; k++)
                {
                    if (locationData[locationName].Split('/')[4 + k].Split(' ').Length > 1) all_season_fish.AddRange(locationData[locationName].Split('/')[4 + k].Split(' '));
                }
                rawFishData = all_season_fish.ToArray();
            }

            Dictionary<string, string> rawFishDataWithLocation = new Dictionary<string, string>();

            if (rawFishData.Length > 1) for (int j = 0; j < rawFishData.Length; j += 2) rawFishDataWithLocation[rawFishData[j]] = rawFishData[j + 1];

            string[] keys = rawFishDataWithLocation.Keys.ToArray();
            for (int i = 0; i < keys.Length; i++)
            {
                bool fail = true;
                string[] specificFishData = fishData[Convert.ToInt32(keys[i])].Split('/');
                string[] timeSpans = specificFishData[5].Split(' ');
                int location = Convert.ToInt32(rawFishDataWithLocation[keys[i]]);
                if (location == -1 || fishingLocation == location)
                {
                    for (int l = 0; l < timeSpans.Length; l += 2)
                    {
                        if (Game1.timeOfDay >= Convert.ToInt32(timeSpans[l]) && Game1.timeOfDay < Convert.ToInt32(timeSpans[l + 1]))
                        {
                            fail = false;
                            break;
                        }
                    }
                }
                if (!specificFishData[7].Equals("both"))
                {
                    if (specificFishData[7].Equals("rainy") && !Game1.IsRainingHere(Game1.player.currentLocation)) fail = true;
                    else if (specificFishData[7].Equals("sunny") && Game1.IsRainingHere(Game1.player.currentLocation)) fail = true;
                }
                if (magicBait) fail = false; //I guess magic bait check comes at this exact point because it overrides all conditions except rod and level?

                bool beginnersRod = Game1.player != null && Game1.player.CurrentTool != null && Game1.player.CurrentTool is StardewValley.Tools.FishingRod && (int)Game1.player.CurrentTool.upgradeLevel == 1;

                if (Convert.ToInt32(specificFishData[1]) >= 50 && beginnersRod) fail = true;
                if (Game1.player.FishingLevel < Convert.ToInt32(specificFishData[12])) fail = true;
                if (!fail && !fishHere.Contains(Int32.Parse(keys[i]))) fishHere.Add(Int32.Parse(keys[i]));
            }
        }
        private void AddFishToListDynamic()                                                  //very performance intensive check for fish fish available in this area - simulates fishing
        {
            int freq = (isMinigame) ? 1 : extraCheckFrequency; //minigame lowers frequency
            for (int i = 0; i < freq; i++)
            {
                int f = Helper.Reflection.GetMethod(Game1.player.currentLocation, "getFish").Invoke<StardewValley.Object>(0, -1, 10, Game1.player, 100, Game1.player.getTileLocation(), null).ParentSheetIndex;
                if ((f < 167 || f > 172) && !fishHere.Contains(f)) fishHere.Add(f);
            }
            fishHere.Sort();//must be sorted to avoid list icon jumps
        }

        private void AddCrabPotFish()
        {
            bool ocean = Game1.player.currentLocation is StardewValley.Locations.Beach;
            foreach (var fish in fishData)
            {
                if (!fish.Value.Contains("trap")) continue;

                string[] rawSplit = fish.Value.Split('/');
                if ((rawSplit[4].Equals("ocean") && ocean) || (rawSplit[4].Equals("freshwater") && !ocean))
                {
                    fishHere.Add(fish.Key);
                }
            }
        }

        private void UpdateConfig()
        {
            iconMode = Config.BarIconMode;                                                                  //config: 0=Horizontal Icons, 1=Vertical Icons, 2=Vertical Icons + Text, 3=Off
            miniMode = Config.MinigamePreviewMode;                                                          //config: Fish preview in minigame: 0=Full, 1=Simple, 2=BarOnly, 3=Off

            if (iconMode != 3)
            {
                barPosition = new Vector2(Config.BarTopLeftLocationX + 2, Config.BarTopLeftLocationY + 2);  //config: Position of bar
                barScale = Config.BarScale;                                                                 //config: Custom scale for the location bar.
                maxIcons = Config.BarMaxIcons;                                                              //config: ^Max amount of tackle + trash + fish icons
                maxIconsPerRow = Config.BarMaxIconsPerRow;                                                  //config: ^How many per row/column.
                backgroundMode = Config.BarBackgroundMode;                                                  //config: 0=Circles (dynamic), 1=Rectangle (single), 2=Off
                showTackles = Config.BarShowBaitAndTackleInfo;                                              //config: Whether it should show Bait and Tackle info.
                showTrash = Config.BarShowTrash;                                                            //config: Whether it should show trash icons.
                legendaryMode = Config.BarLegendaryMode;                                                    //config: Whether player has a mod that allow recatching legendaries. 0=Vanilla, 1=Van+Always, 2=Never
                extraCheckFrequency = Config.BarExtraCheckFrequency;                                        //config: 0-200: Bad performance dynamic check to see if there's modded/hardcoded fish
                uncaughtDark = Config.UncaughtFishAreDark;                                                  //config: Whether uncaught fish are displayed as ??? and use dark icons
                barCrabEnabled = Config.BarCrabPotEnabled;                                                  //config: If bait/tackle/bait preview is enabled when holding a fishing rod


                //locationData = Game1.content.Load<Dictionary<string, string>>("Data\\Locations");
                //fishData = Game1.content.Load<Dictionary<int, string>>("Data\\Fish");
                locationData = Helper.Content.Load<Dictionary<string, string>>("Data\\Locations", ContentSource.GameContent);   //gets location data (which fish are here)
                fishData = Helper.Content.Load<Dictionary<int, string>>("Data\\Fish", ContentSource.GameContent);               //gets fish data

                if (backgroundMode == 0) background = WhiteCircle(17, 30);
                else background = WhitePixel();
            }
        }


        private void AddBackground(SpriteBatch batch, Vector2 boxTopLeft, Vector2 boxBottomLeft, int iconCount, Rectangle source, float iconScale, float boxWidth, float boxHeight)
        {
            if (backgroundMode == 0)
            {
                batch.Draw(background, new Rectangle((int)boxBottomLeft.X - 1, (int)boxBottomLeft.Y - 1, (int)(source.Height * iconScale) + 1, (int)(source.Height * iconScale) + 1),
                    null, new Color(0, 0, 0, 0.4F), 0f, Vector2.Zero, SpriteEffects.None, 0.5f);
            }
            else if (backgroundMode == 1)
            {
                if (iconMode == 0) batch.Draw(background, new Rectangle((int)boxTopLeft.X - 2, (int)boxTopLeft.Y - 2, (int)(source.Width * iconScale * Math.Min(iconCount, maxIconsPerRow)) + 5,
               (int)(source.Width * iconScale * Math.Ceiling(iconCount / (maxIconsPerRow * 1.0))) + 5), null, new Color(0, 0, 0, 0.4F), 0f, Vector2.Zero, SpriteEffects.None, 0.5f);
                else if (iconMode == 1) batch.Draw(background, new Rectangle((int)boxTopLeft.X - 2, (int)boxTopLeft.Y - 2, (int)(source.Width * iconScale * Math.Ceiling(iconCount / (maxIconsPerRow * 1.0))) + 5,
                    (int)(source.Width * iconScale * Math.Min(iconCount, maxIconsPerRow)) + 5), null, new Color(0, 0, 0, 0.4F), 0f, Vector2.Zero, SpriteEffects.None, 0.5f);
                else if (iconMode == 2) batch.Draw(background, new Rectangle((int)boxTopLeft.X - 2, (int)boxTopLeft.Y - 2, (int)(boxWidth - boxTopLeft.X + 6), (int)boxHeight + 4),
                    null, new Color(0, 0, 0, 0.4F), 0f, Vector2.Zero, SpriteEffects.None, 0.5f);
            }
        }

        private Texture2D WhitePixel() //returns a single pixel texture that can be recoloured and resized to make up a background
        {
            Texture2D whitePixel = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
            whitePixel.SetData(new[] { Color.White });
            return whitePixel;
        }
        private Texture2D WhiteCircle(int width, int thickness) //returns a circle texture that can be recoloured and resized to make up a background. Width works better with Odd Numbers.
        {
            Texture2D whitePixel = new Texture2D(Game1.graphics.GraphicsDevice, width, width);

            Color[] data = new Color[width * width];

            float radiusSquared = (width / 2) * (width / 2);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < width; y++)
                {
                    float dx = x - (width / 2);
                    float dy = y - (width / 2);
                    float distanceSquared = dx * dx + dy * dy;

                    if (distanceSquared <= radiusSquared + thickness)
                    {
                        data[(x + y * width)] = Color.White;
                    }
                }
            }

            whitePixel.SetData(data);
            return whitePixel;
        }
    }
}
