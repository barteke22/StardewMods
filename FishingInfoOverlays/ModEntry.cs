using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace FishingInfoOverlays
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        ITranslationHelper translate;
        private ModConfig config;
        private readonly PerScreen<Overlay> overlay = new();


        public override void Entry(IModHelper helper)
        {
            config = helper.ReadConfig<ModConfig>();
            translate = helper.Translation;
            Overlay.modAquarium = helper.ModRegistry.IsLoaded("Cherry.StardewAquarium");
            Overlay.nonFishCaughtID = helper.ModRegistry.ModID + "/nfc";

            helper.Events.Input.ButtonPressed += OnButtonPressed;
            helper.Events.GameLoop.DayStarted += OnDayStarted;
            helper.Events.Display.MenuChanged += OnMenuChanged;
            helper.Events.Display.Rendered += Rendered;
            helper.Events.Display.RenderedActiveMenu += OnRenderMenu;
            helper.Events.Display.RenderedActiveMenu += GenericModConfigMenuIntegration;
            helper.Events.Multiplayer.ModMessageReceived += OnModMessageReceived;
            helper.Events.Player.Warped += OnWarped;
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

            GenericMC.AddBoolOption(ModManifest, () => config.OnlyFish[screen], (val) => config.OnlyFish[screen] = val,
                () => translate.Get("GenericMC.barOnlyFish"), () => translate.Get("GenericMC.barOnlyFishDesc"));
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




        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady || !(e.Button == SButton.F5)) return; // ignore if player hasn't loaded a save yet
            config = Helper.ReadConfig<ModConfig>();
            translate = Helper.Translation;
            UpdateConfig(false);
        }


        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            UpdateConfig(false);
        }

        private void Rendered(object sender, RenderedEventArgs e)
        {
            overlay.Value ??= new Overlay(this);
            if (Context.IsWorldReady)
            {
                if (!Overlay.hudMode)
                {
                    Helper.Events.Display.RenderedHud -= RenderedHud;
                    overlay.Value.RenderedBoth();
                }
                overlay.Value.RenderedMinigame(e);
            }
        }
        private void RenderedHud(object sender, RenderedHudEventArgs e)
        {
            overlay.Value ??= new Overlay(this);
            if (Context.IsWorldReady) overlay.Value.RenderedBoth();
        }

        private void OnMenuChanged(object sender, MenuChangedEventArgs e)   //Minigame data
        {
            overlay.Value ??= new Overlay(this);
            if (Context.IsWorldReady)
            {
                UpdateRenderMode();
                overlay.Value.OnMenuChanged(sender, e);
            }
        }
        private void OnRenderMenu(object sender, RenderedActiveMenuEventArgs e)
        {
            if (Context.IsWorldReady) overlay.Value?.OnRenderMenu(sender, e);
        }

        private void OnModMessageReceived(object sender, ModMessageReceivedEventArgs e)
        {
            if (Context.IsWorldReady) overlay?.Value?.OnModMessageReceived(sender, e);
        }

        private void OnWarped(object sender, WarpedEventArgs e)
        {
            UpdateRenderMode();
            overlay.Value?.OnWarped(sender, e);
        }

        private void UpdateConfig(bool GMCM)
        {
            for (int i = 0; i < 4; i++)
            {
                Overlay.barPosition[i] = new Vector2(config.BarTopLeftLocationX[i] + 2, config.BarTopLeftLocationY[i] + 2); //config: Position of bar
            }

            Overlay.sonarMode = config.BarSonarMode;                                                                        //config: Sonar requirement: 0=everything, 1=minigame, 2=shift scan, 3=not needed
            Overlay.backgroundMode = config.BarBackgroundMode;                                                              //config: 0=Circles (dynamic), 1=Rectangle (single), 2=Off
            Overlay.barCrabEnabled = config.BarCrabPotEnabled;                                                              //config: If bait/tackle/bait preview is enabled when holding a fishing rod
            Overlay.barScale = config.BarScale;                                                                             //config: Custom scale for the location bar.
            Overlay.iconMode = config.BarIconMode;                                                                          //config: 0=Horizontal Icons, 1=Vertical Icons, 2=Vertical Icons + Text, 3=Off
            Overlay.maxIcons = config.BarMaxIcons;                                                                          //config: ^Max amount of tackle + trash + fish icons
            Overlay.maxIconsPerRow = config.BarMaxIconsPerRow;                                                              //config: ^How many per row/column.
            Overlay.onlyFish = config.OnlyFish;                                                                             //config: Whether to hide things like furniture.
            Overlay.scanRadius = config.BarScanRadius;                                                                      //config: 0: Only checks if can fish, 1-50: also checks if there's water within X tiles around player.
            Overlay.showPercentagesMode = config.BarShowPercentagesMode;                                                            //config: Whether it should show catch percentages.
            Overlay.showTackles = config.BarShowBaitAndTackleInfo;                                                          //config: Whether it should show Bait and Tackle info.
            Overlay.extraIconsShowAlways = config.BarExtraIconsAlwaysShow;                                                    //config: Show extra icons when uncaught.
            Overlay.extraIconsMaxSize = config.BarExtraIconsMaxSize;                                                        //config: Show star when fish isn't maxed size.
            Overlay.extraIconsBundles = config.BarExtraIconsBundles;                                                        //config: Show chest when fish needed for bundle.
            Overlay.extraIconsAquarium = config.BarExtraIconsAquarium;                                                      //config: Show pufferfish when needed for Aquarium mod.
            Overlay.sortMode = config.BarSortMode;                                                                          //config: 0= By Name (text mode only), 1= By Percentage, 2=Off
            Overlay.uncaughtFishEffect = config.BarUncaughtFishEffect;                                                          //config: Whether uncaught fish are displayed as ??? and use dark icons
            Overlay.minigameBar = config.MinigamePreviewBar;                                                                //config: Fish preview in bar.
            Overlay.minigameRod = config.MinigamePreviewRod;                                                                //config: Fish preview in minigame.
            Overlay.minigameWater = config.MinigamePreviewWater;                                                            //config: Fish preview in water.
            Overlay.minigameSonar = config.MinigamePreviewSonar;                                                            //config: Fish preview on sonar display.

            if (config.BarExtraCheckFrequency > 22) config.BarExtraCheckFrequency /= 10;
            Overlay.extraCheckFrequency = config.BarExtraCheckFrequency;                                                    //config: 0-22: Bad performance dynamic check to see if there's modded/hardcoded fish

            Overlay.colorBg = new Color(config.BarBackgroundColorRGBA[0], config.BarBackgroundColorRGBA[1], config.BarBackgroundColorRGBA[2], config.BarBackgroundColorRGBA[3]);
            Overlay.colorText = new Color(config.BarTextColorRGBA[0], config.BarTextColorRGBA[1], config.BarTextColorRGBA[2], config.BarTextColorRGBA[3]);

            if (!GMCM)
            {
                Overlay.locationData = DataLoader.Locations(Game1.content);       //gets location data (which fish are here)
                Overlay.fishData = DataLoader.Fish(Game1.content);                   //gets fish data
                Overlay.background[0] = WhiteCircle(17, 30);
                Overlay.background[1] = WhitePixel();
            }

            overlay.ResetAllScreens();
            UpdateRenderMode(true);
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
