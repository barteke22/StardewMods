using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods;
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

            helper.Events.Input.ButtonPressed += OnButtonPressed;
            helper.Events.GameLoop.DayStarted += OnDayStarted;
            helper.Events.Display.Rendered += Rendered;
            helper.Events.Display.MenuChanged += OnMenuChanged;
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
                GenericMC.AddSectionTitle(ModManifest, () => translate.Get("GenericMC.barLabel")); //All of these strings are stored in the traslation files.
                GenericMC.AddParagraph(ModManifest, () => "Translation: barteke22".Equals(translate.Get("GenericMC.translation"), StringComparison.Ordinal) ? "" : translate.Get("GenericMC.translation"));
                GenericMC.AddParagraph(ModManifest, () => translate.Get("GenericMC.barDescription"));
                GenericMC.AddParagraph(ModManifest, () => translate.Get("GenericMC.barDescription2"));

                try
                {
                    GenericMC.AddTextOption(ModManifest, name: () => translate.Get("GenericMC.barSonarMode"), tooltip: () => translate.Get("GenericMC.barSonarModeDesc"),
                        getValue: () => config.BarSonarMode.ToString(),
                        setValue: value => config.BarSonarMode = int.Parse(value),
                        allowedValues: ["0", "1", "2", "3"],
                        formatAllowedValue: value => value == "3" ? translate.Get($"GenericMC.Disabled") : translate.Get($"GenericMC.barSonarMode{value}"));

                    GenericMCPerScreen(GenericMC, 0);
                    GenericMC.AddPageLink(ModManifest, "colors", () => translate.Get("GenericMC.barColors"), () => translate.Get("GenericMC.barColors"));

                    GenericMC.AddPageLink(ModManifest, "s2", () => translate.Get("GenericMC.SplitScreen2"), () => translate.Get("GenericMC.SplitScreenDesc"));
                    GenericMC.AddPageLink(ModManifest, "s3", () => translate.Get("GenericMC.SplitScreen3"), () => translate.Get("GenericMC.SplitScreenDesc"));
                    GenericMC.AddPageLink(ModManifest, "s4", () => translate.Get("GenericMC.SplitScreen4"), () => translate.Get("GenericMC.SplitScreenDesc"));
                    GenericMCPerScreen(GenericMC, 1);
                    GenericMCPerScreen(GenericMC, 2);
                    GenericMCPerScreen(GenericMC, 3);

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
        private void GenericMCPerScreen(IGenericModConfigMenuApi GenericMC, int screen)
        {
            if (screen > 0)//make new page
            {
                GenericMC.AddPage(ModManifest, "s" + (screen + 1), () => translate.Get("GenericMC.SplitScreen" + (screen + 1)));
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
                () => translate.Get("GenericMC.barScale"), () => translate.Get("GenericMC.barScaleDesc"), 0.1f, 5f, 0.1f);
            GenericMC.AddNumberOption(ModManifest, () => config.BarMaxIcons[screen], (val) => config.BarMaxIcons[screen] = val,
                () => translate.Get("GenericMC.barMaxIcons"), () => translate.Get("GenericMC.barMaxIconsDesc"), 4, 500);
            GenericMC.AddNumberOption(ModManifest, () => config.BarMaxIconsPerRow[screen], (val) => config.BarMaxIconsPerRow[screen] = val,
                () => translate.Get("GenericMC.barMaxIconsPerRow"), () => translate.Get("GenericMC.barMaxIconsPerRowDesc"), 4, 500);

            GenericMC.AddTextOption(ModManifest, name: () => translate.Get("GenericMC.barBackgroundMode"), tooltip: () => translate.Get("GenericMC.barBackgroundModeDesc"),
                getValue: () => config.BarBackgroundMode[screen].ToString(),
                setValue: value => config.BarBackgroundMode[screen] = int.Parse(value),
                allowedValues: ["0", "1", "2"],
                formatAllowedValue: value => value == "2" ? translate.Get($"GenericMC.Disabled") : translate.Get($"GenericMC.barBackgroundMode{value}"));

            GenericMC.AddBoolOption(ModManifest, () => config.BarShowBaitAndTackleInfo[screen], (val) => config.BarShowBaitAndTackleInfo[screen] = val,
                () => translate.Get("GenericMC.barShowBaitTackle"), () => translate.Get("GenericMC.barShowBaitTackleDesc"));
            GenericMC.AddBoolOption(ModManifest, () => config.BarShowPercentages[screen], (val) => config.BarShowPercentages[screen] = val,
                () => translate.Get("GenericMC.barShowPercentages"), () => translate.Get("GenericMC.barShowPercentagesDesc"));

            GenericMC.AddTextOption(ModManifest, name: () => translate.Get("GenericMC.barSortMode"), tooltip: () => translate.Get("GenericMC.barSortModeDesc"),
                getValue: () => config.BarSortMode[screen].ToString(),
                setValue: value => config.BarSortMode[screen] = int.Parse(value),
                allowedValues: ["0", "1", "2"],
                formatAllowedValue: value => value == "2" ? translate.Get($"GenericMC.Disabled") : translate.Get($"GenericMC.barSortMode{value}"));

            GenericMC.AddNumberOption(ModManifest, () => config.BarScanRadius[screen], (val) => config.BarScanRadius[screen] = val,
                () => translate.Get("GenericMC.barScanRadius"), () => translate.Get("GenericMC.barScanRadiusDesc"), 1, 60);
            GenericMC.AddBoolOption(ModManifest, () => config.BarCrabPotEnabled[screen], (val) => config.BarCrabPotEnabled[screen] = val,
                () => translate.Get("GenericMC.barCrabPotEnabled"), () => translate.Get("GenericMC.barCrabPotEnabledDesc"));
            GenericMC.AddBoolOption(ModManifest, () => config.UncaughtFishAreDark[screen], (val) => config.UncaughtFishAreDark[screen] = val,
                () => translate.Get("GenericMC.barUncaughtDarker"), () => translate.Get("GenericMC.barUncaughtDarkerDesc"));
            GenericMC.AddBoolOption(ModManifest, () => config.OnlyFish[screen], (val) => config.OnlyFish[screen] = val,
                () => translate.Get("GenericMC.barOnlyFish"), () => translate.Get("GenericMC.barOnlyFishDesc"));

            if (screen == 0)//only page 0
            {
                GenericMC.AddNumberOption(ModManifest, () => config.BarExtraCheckFrequency, (val) => config.BarExtraCheckFrequency = val,
                    () => translate.Get("GenericMC.barExtraCheckFrequency"), () => translate.Get("GenericMC.barExtraCheckFrequencyDesc"), 0, 22);

                GenericMC.AddSectionTitle(ModManifest, () => translate.Get("GenericMC.MinigameLabel"));
                GenericMC.AddParagraph(ModManifest, () => translate.Get("GenericMC.MinigameDescription"));
                GenericMC.AddParagraph(ModManifest, () => translate.Get("GenericMC.MinigameDescription2"));
            }
            GenericMC.AddTextOption(ModManifest, name: () => translate.Get("GenericMC.MinigameMode"), tooltip: () => translate.Get("GenericMC.MinigameModeDesc"),
                getValue: () => config.MinigamePreviewMode[screen].ToString(),
                setValue: value => config.MinigamePreviewMode[screen] = int.Parse(value),
                allowedValues: ["0", "1", "2", "3"],
                formatAllowedValue: value => value == "3" ? translate.Get($"GenericMC.Disabled") : translate.Get($"GenericMC.MinigameMode{value}"));
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
            if (overlay.Value == null) overlay.Value = new Overlay(this);
            if (Context.IsWorldReady) overlay.Value.Rendered(sender, e);
        }

        private void OnMenuChanged(object sender, MenuChangedEventArgs e)   //Minigame data
        {
            if (overlay.Value == null) overlay.Value = new Overlay(this);
            if (Context.IsWorldReady) overlay.Value.OnMenuChanged(sender, e);
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
            overlay.Value?.OnWarped(sender, e);
        }

        private void UpdateConfig(bool GMCM)
        {
            for (int i = 0; i < 4; i++)
            {
                Overlay.barPosition[i] = new Vector2(config.BarTopLeftLocationX[i] + 2, config.BarTopLeftLocationY[i] + 2); //config: Position of bar
            }

            Overlay.backgroundMode = config.BarBackgroundMode;                                                              //config: 0=Circles (dynamic), 1=Rectangle (single), 2=Off
            Overlay.barCrabEnabled = config.BarCrabPotEnabled;                                                              //config: If bait/tackle/bait preview is enabled when holding a fishing rod
            Overlay.barScale = config.BarScale;                                                                             //config: Custom scale for the location bar.
            Overlay.sonarMode = config.BarSonarMode;                                                                        //config: Sonar requirement: 0=everything, 1=minigame, 2=shift scan, 3=not needed
            Overlay.iconMode = config.BarIconMode;                                                                          //config: 0=Horizontal Icons, 1=Vertical Icons, 2=Vertical Icons + Text, 3=Off
            Overlay.maxIcons = config.BarMaxIcons;                                                                          //config: ^Max amount of tackle + trash + fish icons
            Overlay.maxIconsPerRow = config.BarMaxIconsPerRow;                                                              //config: ^How many per row/column.
            Overlay.onlyFish = config.OnlyFish;                                                                             //config: Whether to hide things like furniture.
            Overlay.miniMode = config.MinigamePreviewMode;                                                                  //config: Fish preview in minigame: 0=Full, 1=Simple, 2=BarOnly, 3=Off
            Overlay.scanRadius = config.BarScanRadius;                                                                      //config: 0: Only checks if can fish, 1-50: also checks if there's water within X tiles around player.
            Overlay.showPercentages = config.BarShowPercentages;                                                            //config: Whether it should show catch percentages.
            Overlay.showTackles = config.BarShowBaitAndTackleInfo;                                                          //config: Whether it should show Bait and Tackle info.
            Overlay.sortMode = config.BarSortMode;                                                                          //config: 0= By Name (text mode only), 1= By Percentage, 2=Off
            Overlay.uncaughtDark = config.UncaughtFishAreDark;                                                              //config: Whether uncaught fish are displayed as ??? and use dark icons

            if (config.BarExtraCheckFrequency > 22) config.BarExtraCheckFrequency /= 10;
            Overlay.extraCheckFrequency = config.BarExtraCheckFrequency;                                                    //config: 20-220: Bad performance dynamic check to see if there's modded/hardcoded fish

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
    }
}
