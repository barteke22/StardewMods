using System;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Tools;

namespace FishingInfoOverlays
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        private ITranslationHelper translate;
        private ModConfig config;
        private readonly PerScreen<Overlay> overlays;

        public ModEntry()
        {
            overlays = new(() => new Overlay(this));
        }

        public override void Entry(IModHelper helper)
        {
            config = helper.ReadConfig<ModConfig>();
            translate = helper.Translation;
            Overlay.modAquarium = helper.ModRegistry.IsLoaded("Cherry.StardewAquarium");
            Overlay.nonFishCaughtID = helper.ModRegistry.ModID + "/nfc";
            Overlay.getAddedDistance = (Func<FishingRod, Farmer, int>)Delegate.CreateDelegate(typeof(Func<FishingRod, Farmer, int>), null,
                typeof(FishingRod).GetMethod("getAddedDistance", BindingFlags.NonPublic | BindingFlags.Instance), true);

            helper.Events.Input.ButtonPressed += ButtonPressed;
            helper.Events.GameLoop.UpdateTicked += UpdateTicked;
            helper.Events.Display.MenuChanged += MenuChanged;
            helper.Events.Display.Rendered += Rendered;
            helper.Events.Display.RenderedActiveMenu += RenderedActiveMenu;
            helper.Events.Display.RenderedActiveMenu += GenericModConfigMenuIntegration;
            helper.Events.Player.Warped += Warped;
            helper.Events.GameLoop.SaveLoaded += SaveLoaded;
            helper.Events.Multiplayer.PeerConnected += PeerConnected;
            helper.Events.Multiplayer.ModMessageReceived += ModMessageReceived;
        }

        private void GenericModConfigMenuIntegration(object sender, RenderedActiveMenuEventArgs e)     //Generic Mod Config Menu API
        {
            Helper.Events.Display.RenderedActiveMenu -= GenericModConfigMenuIntegration;
            if (Context.IsSplitScreen) return;
            translate = Helper.Translation;
            var GenericMC = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (GenericMC != null)
            {
                var GenericExtraOptions = Helper.ModRegistry.GetApi<IGMCMOptionsAPI>("jltaylor-us.GMCMOptions");

                GenericMC.Register(ModManifest, () => config = new ModConfig(), () => Helper.WriteConfig(config));

                try
                {
                    GenericMCPerScreen(GenericMC, GenericExtraOptions, 0);
                    AddSeparator(GenericMC, GenericExtraOptions, ModManifest);
                    GenericMC.AddPageLink(ModManifest, "colors", () => translate.Get("GenericMC.barColors"), () => translate.Get("GenericMC.barColors"));

                    GenericMC.AddPageLink(ModManifest, "s2", () => translate.Get("GenericMC.SplitScreen2"), () => translate.Get("GenericMC.SplitScreenDesc"));
                    GenericMC.AddPageLink(ModManifest, "s3", () => translate.Get("GenericMC.SplitScreen3"), () => translate.Get("GenericMC.SplitScreenDesc"));
                    GenericMC.AddPageLink(ModManifest, "s4", () => translate.Get("GenericMC.SplitScreen4"), () => translate.Get("GenericMC.SplitScreenDesc"));
                    GenericMCPerScreen(GenericMC, GenericExtraOptions, 1);
                    GenericMCPerScreen(GenericMC, GenericExtraOptions, 2);
                    GenericMCPerScreen(GenericMC, GenericExtraOptions, 3);

                    GenericMC.AddPage(ModManifest, "colors", () => translate.Get("GenericMC.barColors"));
                    if (GenericExtraOptions != null)//new pickers
                    {
                        GenericExtraOptions.AddColorOption(ModManifest,
                            () => new(config.BarBackgroundColorRGBA[0], config.BarBackgroundColorRGBA[1], config.BarBackgroundColorRGBA[2], config.BarBackgroundColorRGBA[3]),
                            (col) => { config.BarBackgroundColorRGBA[0] = col.R; config.BarBackgroundColorRGBA[1] = col.G; config.BarBackgroundColorRGBA[2] = col.B; config.BarBackgroundColorRGBA[3] = col.A; },
                            () => translate.Get("GenericMC.barBackgroundColor"), colorPickerStyle: (uint)IGMCMOptionsAPI.ColorPickerStyle.RGBSliders);
                        GenericExtraOptions.AddColorOption(ModManifest,
                            () => new(config.BarTextColorRGBA[0], config.BarTextColorRGBA[1], config.BarTextColorRGBA[2], config.BarTextColorRGBA[3]),
                            (col) => { config.BarTextColorRGBA[0] = col.R; config.BarTextColorRGBA[1] = col.G; config.BarTextColorRGBA[2] = col.B; config.BarTextColorRGBA[3] = col.A; },
                            () => translate.Get("GenericMC.barTextColor"), () => translate.Get("GenericMC.barTextColorDesc"), colorPickerStyle: (uint)IGMCMOptionsAPI.ColorPickerStyle.RGBSliders);
                    }
                    else//old pickers
                    {
                        GenericMC.AddSectionTitle(ModManifest, () => translate.Get("GenericMC.barBackgroundColor"));
                        GenericMC.AddNumberOption(ModManifest, () => config.BarBackgroundColorRGBA[0], (val) => config.BarBackgroundColorRGBA[0] = val, () => "R", null, 0, 255);
                        GenericMC.AddNumberOption(ModManifest, () => config.BarBackgroundColorRGBA[1], (val) => config.BarBackgroundColorRGBA[1] = val, () => "G", null, 0, 255);
                        GenericMC.AddNumberOption(ModManifest, () => config.BarBackgroundColorRGBA[2], (val) => config.BarBackgroundColorRGBA[2] = val, () => "B", null, 0, 255);
                        GenericMC.AddNumberOption(ModManifest, () => config.BarBackgroundColorRGBA[3], (val) => config.BarBackgroundColorRGBA[3] = val, () => "A", null, 0, 255);
                        GenericMC.AddSectionTitle(ModManifest, () => translate.Get("GenericMC.barTextColor"));
                        GenericMC.AddNumberOption(ModManifest, () => config.BarTextColorRGBA[0], (val) => config.BarTextColorRGBA[0] = val, () => "R", () => translate.Get("GenericMC.barTextColorDesc"), 0, 255);
                        GenericMC.AddNumberOption(ModManifest, () => config.BarTextColorRGBA[1], (val) => config.BarTextColorRGBA[1] = val, () => "G", () => translate.Get("GenericMC.barTextColorDesc"), 0, 255);
                        GenericMC.AddNumberOption(ModManifest, () => config.BarTextColorRGBA[2], (val) => config.BarTextColorRGBA[2] = val, () => "B", () => translate.Get("GenericMC.barTextColorDesc"), 0, 255);
                        GenericMC.AddNumberOption(ModManifest, () => config.BarTextColorRGBA[3], (val) => config.BarTextColorRGBA[3] = val, () => "A", () => translate.Get("GenericMC.barTextColorDesc"), 0, 255);
                    }

                    //dummy value validation trigger - must be the last thing, so all values are saved before validation
                    GenericMC.AddComplexOption(ModManifest, () => "", (b, pos) => { }, afterSave: () => UpdateConfig(true));

                    //void AddComplexOption(IManifest mod, Func<string> name, Func<string> tooltip, Action<SpriteBatch, Vector2> draw, Action saveChanges, Func<int> height = null, string fieldId = null);
                }
                catch (Exception)
                {
                    Monitor.Log("Error parsing config data. Please either fix your config.json, or delete it to generate a new one.", LogLevel.Error);
                }
            }
        }
        private void GenericMCPerScreen(IGenericModConfigMenuApi GenericMC, IGMCMOptionsAPI GenericExtraOptions, int screen)
        {
            if (screen == 0)//only page 0
            {
                GenericMC.AddSectionTitle(ModManifest, () => translate.Get("GenericMC.barLabel"));
                GenericMC.AddParagraph(ModManifest, () => "Translation: barteke22".Equals(translate.Get("GenericMC.translation"), StringComparison.Ordinal) ? "" : translate.Get("GenericMC.translation"));
                GenericMC.AddParagraph(ModManifest, () => translate.Get("GenericMC.barDescription"));
                GenericMC.AddParagraph(ModManifest, () => translate.Get("GenericMC.barDescription2"));

                GenericMC.AddTextOption(ModManifest, name: () => translate.Get("GenericMC.barSonarMode"), tooltip: () => translate.Get("GenericMC.barSonarModeDesc"), //All of these strings are stored in the traslation files.
                    getValue: () => config.BarSonarMode.ToString(),
                    setValue: value => config.BarSonarMode = int.Parse(value),
                    allowedValues: ["0", "1", "2", "3"],
                    formatAllowedValue: value => value == "3" ? translate.Get($"GenericMC.Disabled") : translate.Get($"GenericMC.barSonarMode{value}"));
                AddSeparator(GenericMC, GenericExtraOptions, ModManifest);
            }
            else //make new page
            {
                GenericMC.AddPage(ModManifest, "s" + (screen + 1), () => translate.Get("GenericMC.SplitScreen" + (screen + 1)));
                GenericMC.AddSectionTitle(ModManifest, () => translate.Get("GenericMC.barLabel"));
            }
            GenericMC.AddTextOption(ModManifest, name: () => translate.Get("GenericMC.barIconMode"), tooltip: () => translate.Get("GenericMC.barIconModeDesc"),
                getValue: () => config.BarIconMode[screen].ToString(),
                setValue: value => config.BarIconMode[screen] = int.Parse(value),
                allowedValues: ["0", "1", "2", "3"],
                formatAllowedValue: value => value == "3" ? translate.Get($"GenericMC.Disabled") : translate.Get($"GenericMC.barIconMode{value}"));

            GenericMC.AddNumberOption(ModManifest, () => config.BarTopLeftLocationX[screen], (val) => config.BarTopLeftLocationX[screen] = val,
                () => translate.Get("GenericMC.barPosX"), () => translate.Get("GenericMC.barPosXDesc"), 0);
            GenericMC.AddNumberOption(ModManifest, () => config.BarTopLeftLocationY[screen], (val) => config.BarTopLeftLocationY[screen] = val,
                () => translate.Get("GenericMC.barPosY"), () => translate.Get("GenericMC.barPosYDesc"), 0);
            GenericMC.AddNumberOption(ModManifest, () => config.BarScale[screen], (val) => config.BarScale[screen] = val,
                () => translate.Get("GenericMC.barScale"), () => translate.Get("GenericMC.barScaleDesc"), 0.1f, 3f, 0.05f);
            GenericMC.AddNumberOption(ModManifest, () => config.BarMaxIcons[screen], (val) => config.BarMaxIcons[screen] = val,
                () => translate.Get("GenericMC.barMaxIcons"), () => translate.Get("GenericMC.barMaxIconsDesc"), 4, 500);
            GenericMC.AddNumberOption(ModManifest, () => config.BarMaxIconsPerRow[screen], (val) => config.BarMaxIconsPerRow[screen] = val,
                () => translate.Get("GenericMC.barMaxIconsPerRow"), () => translate.Get("GenericMC.barMaxIconsPerRowDesc"), 4, 500);

            GenericMC.AddTextOption(ModManifest, name: () => translate.Get("GenericMC.barBackgroundMode"), tooltip: () => translate.Get("GenericMC.barBackgroundModeDesc"),
                getValue: () => config.BarBackgroundMode[screen].ToString(),
                setValue: value => config.BarBackgroundMode[screen] = int.Parse(value),
                allowedValues: ["0", "1", "2"],
                formatAllowedValue: value => value == "2" ? translate.Get($"GenericMC.Disabled") : translate.Get($"GenericMC.barBackgroundMode{value}"));

            AddSeparator(GenericMC, GenericExtraOptions, ModManifest);
            GenericMC.AddBoolOption(ModManifest, () => config.BarExtraIconsMaxSize[screen], (val) => config.BarExtraIconsMaxSize[screen] = val,
                () => translate.Get("GenericMC.barExtraIconsMaxSize"), () => translate.Get("GenericMC.barExtraIconsMaxSizeDesc"));
            GenericMC.AddBoolOption(ModManifest, () => config.BarExtraIconsBundles[screen], (val) => config.BarExtraIconsBundles[screen] = val,
                () => translate.Get("GenericMC.barExtraIconsBundles"), () => translate.Get("GenericMC.barExtraIconsBundlesDesc"));
            GenericMC.AddBoolOption(ModManifest, () => config.BarExtraIconsAquarium[screen], (val) => config.BarExtraIconsAquarium[screen] = val,
                () => translate.Get("GenericMC.barExtraIconsAquarium"), () => translate.Get("GenericMC.barExtraIconsAquariumDesc"));
            GenericMC.AddBoolOption(ModManifest, () => config.BarExtraIconsAlwaysShow[screen], (val) => config.BarExtraIconsAlwaysShow[screen] = val,
                () => translate.Get("GenericMC.barExtraIconsAlwaysShow"), () => translate.Get("GenericMC.barExtraIconsAlwaysShowDesc"));

            GenericMC.AddTextOption(ModManifest, name: () => translate.Get("GenericMC.barUncaughtEffect"), tooltip: () => translate.Get("GenericMC.barUncaughtEffectDesc"),
                getValue: () => config.BarUncaughtFishEffect[screen].ToString(),
                setValue: value => config.BarUncaughtFishEffect[screen] = int.Parse(value),
                allowedValues: ["0", "1", "2"],
                formatAllowedValue: value => value == "2" ? translate.Get($"GenericMC.Disabled") : translate.Get($"GenericMC.barUncaughtEffect{value}"));
            AddSeparator(GenericMC, GenericExtraOptions, ModManifest);

            GenericMC.AddBoolOption(ModManifest, () => config.BarShowBaitAndTackleInfo[screen], (val) => config.BarShowBaitAndTackleInfo[screen] = val,
                () => translate.Get("GenericMC.barShowBaitTackle"), () => translate.Get("GenericMC.barShowBaitTackleDesc"));
            GenericMC.AddTextOption(ModManifest, name: () => translate.Get("GenericMC.barShowPercentages"), tooltip: () => translate.Get("GenericMC.barShowPercentagesDesc"),
                getValue: () => config.BarShowPercentagesMode[screen].ToString(),
                setValue: value => config.BarShowPercentagesMode[screen] = int.Parse(value),
                allowedValues: ["0", "1", "2"],
                formatAllowedValue: value => value == "2" ? translate.Get($"GenericMC.Disabled") : translate.Get($"GenericMC.barShowPercentages{value}"));

            GenericMC.AddTextOption(ModManifest, name: () => translate.Get("GenericMC.barSortMode"), tooltip: () => translate.Get("GenericMC.barSortModeDesc"),
                getValue: () => config.BarSortMode[screen].ToString(),
                setValue: value => config.BarSortMode[screen] = int.Parse(value),
                allowedValues: ["0", "1", "2"],
                formatAllowedValue: value => value == "2" ? translate.Get($"GenericMC.Disabled") : translate.Get($"GenericMC.barSortMode{value}"));

            GenericMC.AddTextOption(ModManifest, name: () => translate.Get("GenericMC.barNonFishMode"), tooltip: () => translate.Get("GenericMC.barNonFishModeDesc"),
                getValue: () => config.BarNonFishMode[screen].ToString(),
                setValue: value => config.BarNonFishMode[screen] = int.Parse(value),
                allowedValues: ["0", "1", "2"],
                formatAllowedValue: value => value == "2" ? translate.Get($"GenericMC.Disabled") : translate.Get($"GenericMC.barNonFishMode{value}"));

            GenericMC.AddBoolOption(ModManifest, () => config.BarCrabPotEnabled[screen], (val) => config.BarCrabPotEnabled[screen] = val,
                () => translate.Get("GenericMC.barCrabPotEnabled"), () => translate.Get("GenericMC.barCrabPotEnabledDesc"));

            AddSeparator(GenericMC, GenericExtraOptions, ModManifest);
            GenericMC.AddNumberOption(ModManifest, () => config.BarScanRadius[screen], (val) => config.BarScanRadius[screen] = val,
                () => translate.Get("GenericMC.barScanRadius"), () => translate.Get("GenericMC.barScanRadiusDesc"), 1, 60);

            if (screen == 0)//only page 0
            {
                GenericMC.AddNumberOption(ModManifest, () => config.BarExtraCheckFrequency, (val) => config.BarExtraCheckFrequency = val,
                    () => translate.Get("GenericMC.barExtraCheckFrequency"), () => translate.Get("GenericMC.barExtraCheckFrequencyDesc"), 0, 22);
            }
            AddSeparator(GenericMC, GenericExtraOptions, ModManifest);
            GenericMC.AddSectionTitle(ModManifest, () => translate.Get("GenericMC.MinigameLabel"));
            if (screen == 0) GenericMC.AddParagraph(ModManifest, () => translate.Get("GenericMC.MinigameDescription"));
            GenericMC.AddBoolOption(ModManifest, () => config.MinigamePreviewBar[screen], (val) => config.MinigamePreviewBar[screen] = val,
                () => translate.Get("GenericMC.MinigameBar"), () => translate.Get("GenericMC.MinigameBarDesc"));
            GenericMC.AddBoolOption(ModManifest, () => config.MinigamePreviewRod[screen], (val) => config.MinigamePreviewRod[screen] = val,
                () => translate.Get("GenericMC.MinigameRod"), () => translate.Get("GenericMC.MinigameRodDesc"));
            GenericMC.AddBoolOption(ModManifest, () => config.MinigamePreviewWater[screen], (val) => config.MinigamePreviewWater[screen] = val,
                () => translate.Get("GenericMC.MinigameWater"), () => translate.Get("GenericMC.MinigameWaterDesc"));
            GenericMC.AddBoolOption(ModManifest, () => config.MinigamePreviewSonar[screen], (val) => config.MinigamePreviewSonar[screen] = val,
                () => translate.Get("GenericMC.MinigameSonar"), () => translate.Get("GenericMC.MinigameSonarDesc"));

        }




        private void ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady || !(e.Button == SButton.F5)) return; // ignore if player hasn't loaded a save yet
            config = Helper.ReadConfig<ModConfig>();
            translate = Helper.Translation;
            UpdateConfig(false);
        }

        private void SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            UpdateConfig(false);
        }

        private void UpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (Context.IsWorldReady) overlays.Value.UpdateTicked(e);
        }

        private void Rendered(object sender, RenderedEventArgs e)
        {
            if (Context.IsWorldReady)
            {
                if (!Overlay.hudMode)
                {
                    Helper.Events.Display.RenderedHud -= RenderedHud;
                    overlays.Value.RenderedBoth();
                }
                overlays.Value.RenderedMinigame(e);
            }
        }
        private void RenderedHud(object sender, RenderedHudEventArgs e)
        {
            if (Context.IsWorldReady) overlays.Value.RenderedBoth();
        }

        private void MenuChanged(object sender, MenuChangedEventArgs e)   //Minigame data
        {
            if (Context.IsWorldReady)
            {
                UpdateRenderMode();
                overlays.Value.MenuChanged(sender, e);
            }
        }
        private void RenderedActiveMenu(object sender, RenderedActiveMenuEventArgs e)
        {
            if (Context.IsWorldReady) overlays.Value.RenderedActiveMenu(sender, e);
        }

        private void ModMessageReceived(object sender, ModMessageReceivedEventArgs e)
        {
            if (Context.IsWorldReady) overlays.Value.ModMessageReceived(sender, e);
        }

        private void Warped(object sender, WarpedEventArgs e)
        {
            UpdateRenderMode();
            overlays.Value.OnWarped(sender, e);
        }

        private void PeerConnected(object sender, PeerConnectedEventArgs e)
        {
            if (e.Peer.IsSplitScreen) UpdateConfigPerScreenID(false, e.Peer.ScreenID.Value);
        }

        private void UpdateConfig(bool GMCM)
        {
            Overlay.trash = ItemRegistry.GetData("(O)168");

            UpdateConfigPerScreen(GMCM);

            if (config.BarExtraCheckFrequency > 22) config.BarExtraCheckFrequency /= 10;
            Overlay.extraCheckFrequency = config.BarExtraCheckFrequency;                     //config: 0-22: Bad performance dynamic check to see if there's modded/hardcoded fish
            Overlay.sonarMode = config.BarSonarMode;                                         //config: Sonar requirement: 0=everything, 1=minigame, 2=shift scan, 3=not needed

            Overlay.colorBg = new Color(config.BarBackgroundColorRGBA[0], config.BarBackgroundColorRGBA[1], config.BarBackgroundColorRGBA[2], config.BarBackgroundColorRGBA[3]);
            Overlay.colorText = new Color(config.BarTextColorRGBA[0], config.BarTextColorRGBA[1], config.BarTextColorRGBA[2], config.BarTextColorRGBA[3]);

            if (!GMCM)
            {
                Overlay.locationData = DataLoader.Locations(Game1.content);       //gets location data (which fish are here)
                Overlay.fishData = DataLoader.Fish(Game1.content);                //gets fish data
                Overlay.background[0] = WhiteCircle(17, 30);
                Overlay.background[1] = WhitePixel();
            }

            UpdateRenderMode(true);
        }
        private void UpdateConfigPerScreen(bool GMCM)
        {
            if (Context.IsWorldReady)
            {
                if (Context.IsMultiplayer)
                {
                    foreach (IMultiplayerPeer peer in Helper.Multiplayer.GetConnectedPlayers())
                    {
                        if (peer.IsSplitScreen)
                        {
                            UpdateConfigPerScreenID(GMCM, peer.ScreenID.Value);
                        }
                    }
                }
                else UpdateConfigPerScreenID(GMCM, Context.ScreenId);
            }
        }
        private void UpdateConfigPerScreenID(bool GMCM, int index)
        {
            var overlay = overlays.GetValueForScreen(index);

            overlay.barPosition = new Vector2(config.BarTopLeftLocationX[index] + 2, config.BarTopLeftLocationY[index] + 2); //config: Position of bar
            overlay.backgroundMode = config.BarBackgroundMode[index];                                                        //config: 0=Circles (dynamic), 1=Rectangle (single), 2=Off
            overlay.barCrabEnabled = config.BarCrabPotEnabled[index];                                                        //config: If bait/tackle/bait preview is enabled when holding a fishing rod
            overlay.barScale = config.BarScale[index];                                                                       //config: Custom scale for the location bar.
            overlay.iconMode = config.BarIconMode[index];                                                                    //config: 0=Horizontal Icons, 1=Vertical Icons, 2=Vertical Icons + Text, 3=Off
            overlay.maxIcons = config.BarMaxIcons[index];                                                                    //config: ^Max amount of tackle + trash + fish icons
            overlay.maxIconsPerRow = config.BarMaxIconsPerRow[index];                                                        //config: ^How many per row/column.
            overlay.nonFishMode = config.BarNonFishMode[index];                                                              //config: Whether to hide things like furniture.
            overlay.scanRadius = config.BarScanRadius[index];                                                                //config: 0: Only checks if can fish, 1-50: also checks if there's water within X tiles around player.
            overlay.showPercentagesMode = config.BarShowPercentagesMode[index];                                              //config: Whether it should show catch percentages.
            overlay.showTackles = config.BarShowBaitAndTackleInfo[index];                                                    //config: Whether it should show Bait and Tackle info.
            overlay.extraIconsShowAlways = config.BarExtraIconsAlwaysShow[index];                                            //config: Show extra icons when uncaught.
            overlay.extraIconsMaxSize = config.BarExtraIconsMaxSize[index];                                                  //config: Show star when fish isn't maxed size.
            overlay.extraIconsBundles = config.BarExtraIconsBundles[index];                                                  //config: Show chest when fish needed for bundle.
            overlay.extraIconsAquarium = config.BarExtraIconsAquarium[index];                                                //config: Show pufferfish when needed for Aquarium mod.
            overlay.sortMode = config.BarSortMode[index];                                                                    //config: 0= By Name (text mode only), 1= By Percentage, 2=Off
            overlay.uncaughtFishEffect = config.BarUncaughtFishEffect[index];                                                //config: Whether uncaught fish are displayed as ??? and use dark icons
            overlay.minigameBar = config.MinigamePreviewBar[index];                                                          //config: Fish preview in bar.
            overlay.minigameRod = config.MinigamePreviewRod[index];                                                          //config: Fish preview in minigame.
            overlay.minigameWater = config.MinigamePreviewWater[index];                                                      //config: Fish preview in water.
            overlay.minigameSonar = config.MinigamePreviewSonar[index];                                                      //config: Fish preview on sonar display.
        }

        private void UpdateRenderMode(bool reset = false) //checks whether UI or Zoom is closer to 100% and applies rendering in that mode
        {
            float dist = Math.Abs(Game1.options.zoomLevel - 1f);
            float distHud = Math.Abs(Game1.options.uiScale - 1f);
            bool hud = dist > distHud;

            if (hud != Overlay.hudMode || reset)
            {
                Overlay.hudMode = hud;
                if (hud) Helper.Events.Display.RenderedHud += RenderedHud;
                else Helper.Events.Display.RenderedHud -= RenderedHud;
            }
        }


        private static Texture2D WhitePixel() //returns a single pixel texture that can be recoloured and resized to make up a background
        {
            Texture2D whitePixel = new(Game1.graphics.GraphicsDevice, 1, 1);
            whitePixel.SetData([Color.White]);
            return whitePixel;
        }
        private static Texture2D WhiteCircle(int width, int thickness) //returns a circle texture that can be recoloured and resized to make up a background. Width works better with Odd Numbers.
        {
            Texture2D whitePixel = new(Game1.graphics.GraphicsDevice, width, width);

            Color[] data = new Color[width * width];

            float radiusSquared = width / 2 * (width / 2);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < width; y++)
                {
                    float dx = x - width / 2;
                    float dy = y - width / 2;
                    float distanceSquared = dx * dx + dy * dy;

                    if (distanceSquared <= radiusSquared + thickness)
                    {
                        data[x + y * width] = Color.White;
                    }
                }
            }

            whitePixel.SetData(data);
            return whitePixel;
        }

        private static void AddSeparator(IGenericModConfigMenuApi GenericMC, IGMCMOptionsAPI GenericExtraOptions, IManifest ModManifest)
        {
            if (GenericExtraOptions != null)
            {
                GenericExtraOptions.AddHorizontalSeparator(ModManifest, () => 1f);
            }
            else GenericMC.AddParagraph(ModManifest, () => "____________________________________________________________________");
        }
    }
}
