using GenericModConfigMenu;
using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace FishingMinigames
{
    public class ModEntry : Mod, IAssetEditor
    {
        public static ITranslationHelper translate;
        public static ModConfig config;
        private readonly PerScreen<Minigames> minigame = new PerScreen<Minigames>();



        public override void Entry(IModHelper helper)
        {
            UpdateConfig();

            helper.Events.Display.RenderedWorld += Display_RenderedWorld;
            helper.Events.GameLoop.UpdateTicking += GameLoop_UpdateTicking;
            helper.Events.Input.ButtonsChanged += Input_ButtonsChanged;
            helper.Events.GameLoop.GameLaunched += GenericModConfigMenuIntegration;
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            helper.Events.Multiplayer.ModMessageReceived += OnModMessageReceived;

            var harmony = HarmonyInstance.Create(ModManifest.UniqueID);//this might summon Cthulhu
            harmony.PatchAll();
        }

        private void GenericModConfigMenuIntegration(object sender, GameLaunchedEventArgs e)     //Generic Mod Config Menu API
        {
            if (Context.IsSplitScreen) return;
            translate = Helper.Translation;
            var GenericMC = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (GenericMC != null)
            {
                GenericMC.RegisterModConfig(ModManifest, () => config = new ModConfig(), () => Helper.WriteConfig(config));
                GenericMC.SetDefaultIngameOptinValue(ModManifest, true);
                GenericMC.RegisterLabel(ModManifest, translate.Get("GenericMC.MainLabel"), ""); //All of these strings are stored in the traslation files.
                GenericMC.RegisterParagraph(ModManifest, translate.Get("GenericMC.MainDesc"));
                GenericMC.RegisterParagraph(ModManifest, translate.Get("GenericMC.MainDesc2"));
                if (Constants.TargetPlatform != GamePlatform.Android) GenericMC.RegisterParagraph(ModManifest, translate.Get("GenericMC.MainDescPC"));
                else GenericMC.RegisterParagraph(ModManifest, translate.Get("GenericMC.MainDescOther"));
                GenericMC.RegisterParagraph(ModManifest, translate.Get("GenericMC.MainDesc3"));

                try
                {
                    GenericMC.RegisterClampedOption(ModManifest, translate.Get("GenericMC.Volume"), translate.Get("GenericMC.VolumeDesc"),
                        () => config.VoiceVolume, (int val) => config.VoiceVolume = val, 0, 100);

                    GenericMCPerScreen(GenericMC, 0);
                    for (int i = 2; i < 5; i++)
                    {
                        GenericMC.RegisterPageLabel(ModManifest, translate.Get("GenericMC.SplitScreen" + i), translate.Get("GenericMC.SplitScreenDesc"), translate.Get("GenericMC.SplitScreen" + i));
                    }
                    GenericMCPerScreen(GenericMC, 1);
                    GenericMCPerScreen(GenericMC, 2);
                    GenericMCPerScreen(GenericMC, 3);
                }
                catch (Exception)
                {
                    this.Monitor.Log("Error parsing config data. Please either fix your config.json, or delete it to generate a new one.", LogLevel.Error);
                }
            }
        }
        private void GenericMCPerScreen(IGenericModConfigMenuApi GenericMC, int screen)
        {
            if (screen > 0)//make new page
            {
                GenericMC.StartNewPage(ModManifest, translate.Get("GenericMC.SplitScreen" + (screen + 1)));
            }
            GenericMC.RegisterClampedOption(ModManifest, translate.Get("GenericMC.Pitch"), translate.Get("GenericMC.PitchDesc"),
                () => config.VoicePitch[screen], (int val) => config.VoicePitch[screen] = val, -100, 100);

            GenericMC.RegisterSimpleOption(ModManifest, translate.Get("GenericMC.KeyBinds"), translate.Get("GenericMC.KeyBindsDesc"),
                () => config.KeyBinds[screen], (string val) => config.KeyBinds[screen] = val);

            GenericMC.RegisterSimpleOption(ModManifest, translate.Get("GenericMC.FreeAim"), translate.Get("GenericMC.FreeAimDesc"),
                () => config.FreeAim[screen], (bool val) => config.FreeAim[screen] = val);

            GenericMC.RegisterChoiceOption(ModManifest, translate.Get("GenericMC.StartMinigameStyle"), translate.Get("GenericMC.StartMinigameStyleDesc"),
                () => (config.StartMinigameStyle[screen] == 0) ? translate.Get("GenericMC.Disabled") : (config.StartMinigameStyle[screen] == 1) ? translate.Get("GenericMC.StartMinigameStyle1") : (config.StartMinigameStyle[screen] == 2) ? translate.Get("GenericMC.StartMinigameStyle2") : translate.Get("GenericMC.StartMinigameStyle3"),
                (string val) => config.StartMinigameStyle[screen] = Int32.Parse((val.Equals(translate.Get("GenericMC.Disabled"), StringComparison.Ordinal)) ? "0" : (val.Equals(translate.Get("GenericMC.StartMinigameStyle1"), StringComparison.Ordinal)) ? "1" : (val.Equals(translate.Get("GenericMC.StartMinigameStyle2"), StringComparison.Ordinal)) ? "2" : "3"),
                new string[] { translate.Get("GenericMC.Disabled"), translate.Get("GenericMC.StartMinigameStyle1") });//, translate.Get("GenericMC.StartMinigameStyle2"), translate.Get("GenericMC.StartMinigameStyle3") });//small 'hack' so options appear as name strings, while config.json stores them as integers

            GenericMC.RegisterChoiceOption(ModManifest, translate.Get("GenericMC.EndMinigameStyle"), translate.Get("GenericMC.EndMinigameStyleDesc"),
                () => (config.EndMinigameStyle[screen] == 0) ? translate.Get("GenericMC.Disabled") : (config.EndMinigameStyle[screen] == 1) ? translate.Get("GenericMC.EndMinigameStyle1") : (config.EndMinigameStyle[screen] == 2) ? translate.Get("GenericMC.EndMinigameStyle2") : translate.Get("GenericMC.EndMinigameStyle3"),
                (string val) => config.EndMinigameStyle[screen] = Int32.Parse((val.Equals(translate.Get("GenericMC.Disabled"), StringComparison.Ordinal)) ? "0" : (val.Equals(translate.Get("GenericMC.EndMinigameStyle1"), StringComparison.Ordinal)) ? "1" : (val.Equals(translate.Get("GenericMC.EndMinigameStyle2"), StringComparison.Ordinal)) ? "2" : "3"),
                new string[] { translate.Get("GenericMC.Disabled"), translate.Get("GenericMC.EndMinigameStyle1"), translate.Get("GenericMC.EndMinigameStyle2"), translate.Get("GenericMC.EndMinigameStyle3") });

            GenericMC.RegisterClampedOption(ModManifest, translate.Get("GenericMC.EndDamage"), translate.Get("GenericMC.EndDamageDesc"),
                () => config.EndMinigameDamage[screen], (float val) => config.EndMinigameDamage[screen] = val, 0f, 2f);
            GenericMC.RegisterClampedOption(ModManifest, translate.Get("GenericMC.Difficulty"), translate.Get("GenericMC.DifficultyDesc"),
                () => config.MinigameDifficulty[screen], (float val) => config.MinigameDifficulty[screen] = val, 0.1f, 2f);

            if (screen == 0)//only page 0
            {
                GenericMC.RegisterClampedOption(ModManifest, translate.Get("GenericMC.StartMinigameScale"), translate.Get("GenericMC.StartMinigameScale"),
                    () => config.StartMinigameScale, (float val) => config.StartMinigameScale = val, 0.5f, 5f);

                GenericMC.RegisterSimpleOption(ModManifest, translate.Get("GenericMC.RealisticSizes"), translate.Get("GenericMC.RealisticSizesDesc"),
                    () => config.RealisticSizes, (bool val) => config.RealisticSizes = val);

                if (LocalizedContentManager.CurrentLanguageCode == 0) GenericMC.RegisterSimpleOption(ModManifest, translate.Get("GenericMC.ConvertToMetric"), translate.Get("GenericMC.ConvertToMetricDesc"),
                    () => config.ConvertToMetric, (bool val) => config.ConvertToMetric = val);

                GenericMC.RegisterLabel(ModManifest, translate.Get("GenericMC.FestivalLabel"), "");
                GenericMC.RegisterParagraph(ModManifest, translate.Get("GenericMC.FestivalDesc"));
                GenericMC.RegisterParagraph(ModManifest, translate.Get("GenericMC.FestivalDesc2"));
            }
            GenericMC.RegisterChoiceOption(ModManifest, translate.Get("GenericMC.FestivalMode"), translate.Get("GenericMC.FestivalModeDesc"),
                () => (config.FestivalMode[screen] == 0) ? translate.Get("GenericMC.FestivalModeVanilla") : (config.FestivalMode[screen] == 1) ? translate.Get("GenericMC.FestivalModeSimple") : translate.Get("GenericMC.FestivalModePerfectOnly"),
                (string val) => config.FestivalMode[screen] = Int32.Parse((val.Equals(translate.Get("GenericMC.FestivalModeVanilla"), StringComparison.Ordinal)) ? "0" : (val.Equals(translate.Get("GenericMC.FestivalModeSimple"), StringComparison.Ordinal)) ? "1" : "2"),
                new string[] { translate.Get("GenericMC.FestivalModeVanilla"), translate.Get("GenericMC.FestivalModeSimple"), translate.Get("GenericMC.FestivalModePerfectOnly") });
        }


        private void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            UpdateConfig();
        }
        private void OnModMessageReceived(object sender, ModMessageReceivedEventArgs e)
        {
            minigame.Value.OnModMessageReceived(sender, e);
        }

        private void Input_ButtonsChanged(object sender, ButtonsChangedEventArgs e)  //this.Monitor.Log(locationName, LogLevel.Debug);
        {
            if (e.Pressed.Contains(SButton.F5))
            {
                UpdateConfig();
            }
            if (Game1.player.IsLocalPlayer) minigame.Value.Input_ButtonsChanged(sender, e);
        }

        private void GameLoop_UpdateTicking(object sender, UpdateTickingEventArgs e) //adds item to inv
        {
            if (!Game1.player.IsLocalPlayer) return;

            if (minigame.Value == null)
            {
                minigame.Value = new Minigames(this);
            }

            try
            {
                minigame.Value.GameLoop_UpdateTicking(sender, e);
            }
            catch (Exception ex)
            {
                Monitor.Log("Handled Exception in UpdateTicking. Festival: " + Game1.isFestival() + ", Message: " + ex.Message, LogLevel.Trace);
                minigame.Value.EmergencyCancel();
            }
        }

        private void Display_RenderedWorld(object sender, RenderedWorldEventArgs e)
        {
            try
            {
                if (Game1.player.IsLocalPlayer) minigame.Value.Display_RenderedWorld(sender, e);
            }
            catch (Exception ex)
            {
                Monitor.Log("Handled Exception in RenderedWorld. Festival: " + Game1.isFestival() + ", Message: " + ex.Message, LogLevel.Trace);
                minigame.Value.EmergencyCancel();
            }
        }



        /// <summary>Get whether this instance can edit the given asset.</summary>
        public bool CanEdit<T>(IAssetInfo asset)
        {
            if (asset.AssetNameEquals("TileSheets/tools") || asset.AssetNameEquals("Maps/springobjects") || asset.AssetNameEquals("Strings/StringsFromCSFiles") || asset.AssetNameEquals("Data/ObjectInformation")) return true;
            return false;
        }
        /// <summary>Edits the asset if CanEdit</summary>
        public void Edit<T>(IAssetData asset)
        {
            if (asset.AssetNameEquals("Strings/StringsFromCSFiles"))
            {
                translate = Helper.Translation;
                IDictionary<string, string> data = asset.AsDictionary<string, string>().Data;
                foreach (string itemID in data.Keys.ToArray())
                {
                    try
                    {
                        switch (itemID)
                        {
                            case "FishingRod.cs.14041":
                                data[itemID] = translate.Get("net.fishing");
                                break;
                            case "FishingRod.cs.trainingRodDescription":
                                data[itemID] = AddEffectDescriptions("Training Rod", translate.Get("net.trainingDesc"));
                                break;
                            //case "FishingRod.cs.14042":
                            //    Minigames.rodDescription = data[itemID];
                            //    break;
                            case "FishingRod.cs.14045":
                                data[itemID] = translate.Get("net.bamboo");
                                break;
                            case "FishingRod.cs.14046":
                                data[itemID] = translate.Get("net.training");
                                break;
                            case "FishingRod.cs.14047":
                                data[itemID] = translate.Get("net.fiberglass");
                                break;
                            case "FishingRod.cs.14048":
                                data[itemID] = translate.Get("net.iridium");
                                break;
                            case "SkillPage.cs.11598":
                                data[itemID] = translate.Get("net.skill");
                                break;
                            case "FishingRod.cs.14083":
                                if (config.ConvertToMetric) data[itemID] = "{0} cm";
                                break;
                        }
                    }
                    catch (Exception)
                    {
                        Monitor.Log("Could not load string for Rod to Net change, line: " + data[itemID] + ". Are the translations missing? Ignore if you removed them intentionally.", LogLevel.Warn);
                    }
                }

            }
            else if (asset.AssetNameEquals("Data/ObjectInformation"))
            {
                translate = Helper.Translation;
                IDictionary<int, string> data = asset.AsDictionary<int, string>().Data;
                foreach (int itemID in data.Keys.ToArray())
                {
                    try
                    {
                        string[] itemData = data[itemID].Split('/');
                        switch (itemID)
                        {
                            //bait
                            case 685:
                                itemData[5] = AddEffectDescriptions("Bait");
                                break;
                            case 703:
                                itemData[5] = AddEffectDescriptions("Magnet", itemData[5]);
                                break;
                            case 774:
                                itemData[5] = AddEffectDescriptions("Wild Bait", itemData[5]);
                                break;
                            case 908:
                                itemData[5] = AddEffectDescriptions("Magic Bait", itemData[5]);
                                break;
                            //tackle
                            case 686:
                                itemData[4] = "Spinner Megaphone";
                                itemData[5] = AddEffectDescriptions("Spinner");
                                break;
                            case 687:
                                itemData[4] = "Dressed Megaphone";
                                itemData[5] = AddEffectDescriptions("Dressed Spinner");
                                break;
                            case 694:
                                itemData[4] = "Trap Megaphone";
                                itemData[5] = AddEffectDescriptions("Trap Bobber");
                                break;
                            case 695:
                                itemData[4] = "Cork Megaphone";
                                itemData[5] = AddEffectDescriptions("Cork Bobber");
                                break;
                            case 692:
                                itemData[4] = "Lead Megaphone";
                                itemData[5] = AddEffectDescriptions("Lead Bobber");
                                break;
                            case 693:
                                itemData[4] = "Treasure Megaphone";
                                itemData[5] = AddEffectDescriptions("Treasure Hunter");
                                break;
                            case 691:
                                itemData[4] = "Barbed Megaphone";
                                itemData[5] = AddEffectDescriptions("Barbed Hook");
                                break;
                            case 856:
                                itemData[4] = "Curiosity Megaphone";
                                itemData[5] = AddEffectDescriptions("Curiosity Lure", itemData[5]);
                                break;
                            case 877:
                                itemData[4] = "Quality Megaphone";
                                itemData[5] = AddEffectDescriptions("Quality Bobber");
                                break;
                        }
                        data[itemID] = string.Join("/", itemData);
                    }
                    catch (Exception)
                    {
                        Monitor.Log("Could not load string for Rod to Net change, line: " + data[itemID] + ". Are the translations missing? Ignore if you removed them intentionally.", LogLevel.Warn);
                    }
                }
            }
            else
            {
                var editor = asset.AsImage();

                Texture2D sourceImage;
                try
                {
                    if (asset.AssetNameEquals("Maps/springobjects"))
                    {
                        sourceImage = Helper.Content.Load<Texture2D>("assets/bait_magnet.png", ContentSource.ModFolder);
                        editor.PatchImage(sourceImage, targetArea: new Rectangle(112, 464, 16, 16));
                        sourceImage = Helper.Content.Load<Texture2D>("assets/tackle_basic.png", ContentSource.ModFolder);
                        editor.PatchImage(sourceImage, targetArea: new Rectangle(304, 448, 80, 16));
                        sourceImage = Helper.Content.Load<Texture2D>("assets/tackle_curiosity.png", ContentSource.ModFolder);
                        editor.PatchImage(sourceImage, targetArea: new Rectangle(256, 560, 16, 16));
                        sourceImage = Helper.Content.Load<Texture2D>("assets/tackle_quality.png", ContentSource.ModFolder);
                        editor.PatchImage(sourceImage, targetArea: new Rectangle(208, 576, 16, 16));
                        sourceImage = Helper.Content.Load<Texture2D>("assets/tackle_spinners.png", ContentSource.ModFolder);
                        editor.PatchImage(sourceImage, targetArea: new Rectangle(224, 448, 32, 16));
                    }
                    else
                    {
                        sourceImage = Helper.Content.Load<Texture2D>("assets/rod_sprites.png", ContentSource.ModFolder);
                        editor.PatchImage(sourceImage, targetArea: new Rectangle(128, 0, 64, 16));
                        sourceImage = Helper.Content.Load<Texture2D>("assets/rod_farmer.png", ContentSource.ModFolder);
                        editor.PatchImage(sourceImage, targetArea: new Rectangle(0, 289, 295, 95));
                    }
                    sourceImage.Dispose();
                }
                catch (Exception)
                {
                    Monitor.Log("Could not load images for the " + ((asset.AssetNameEquals("Maps/springobjects")) ? "bait/tackles" : "fishing nets") + "! Are the assets missing? Ignore if you removed them intentionally.", LogLevel.Warn);
                }
            }
        }
        public static string AddEffectDescriptions(string itemName, string initialText = null)
        {
            foreach (string effectPair in Minigames.itemData[itemName])
            {
                string value = "";
                float tempVal = float.Parse(effectPair.Split(':')[1]);
                switch (effectPair.Split(':')[0])
                {
                    case "DAMAGE":
                    case "DIFFICULTY":
                    case "SPEED":
                        if (1f - tempVal == 0f) continue;
                        if (initialText != null) initialText += "\n";
                        value = ((1f - tempVal) * 100f).ToString();
                        break;
                    case "AREA":
                    case "SIZE":
                        if (tempVal - 1f == 0f) continue;
                        if (initialText != null) initialText += "\n";
                        value = ((tempVal - 1f) * 100f).ToString();
                        break;
                    case "DOUBLE":
                        if (tempVal == 0f) continue;
                        if (initialText != null) initialText += "\n";
                        value = (tempVal * 100f).ToString();
                        break;
                    case "LIFE":
                    case "QUALITY":
                    case "UNBREAKING":
                        if (tempVal == 0f || itemName.EndsWith("Rod") || itemName.EndsWith("Pole")) continue;
                        if (initialText != null) initialText += "\n";
                        value = tempVal.ToString();
                        break;
                    case "TREASURE":
                        if (tempVal == 0f) continue;
                        if (initialText != null) initialText += "\n";
                        value = (tempVal * StardewValley.Tools.FishingRod.baseChanceForTreasure * 100f).ToString("0.#");
                        break;
                }
                initialText += translate.Get("Effects." + effectPair.Split(':')[0]).Tokens(new { val = value });
            }
            return initialText;
        }


        private void UpdateConfig()
        {
            config = Helper.ReadConfig<ModConfig>();

            //item configs
            Minigames.itemData = new Dictionary<string, string[]>();
            foreach (var file in Directory.GetFiles(Helper.DirectoryPath + "/itemConfigs", "*.json").Select(Path.GetFileName).OrderBy(f => f))
            {
                (Helper.Data.ReadJsonFile<Dictionary<string, string[]>>("itemConfigs/" + file) ?? new Dictionary<string, string[]>()).ToList().ForEach(x => Minigames.itemData[x.Key] = x.Value);
            }
            Minigames.itemData.Remove("Example Item Name");


            try
            {
                Minigames.fishySound = SoundEffect.FromStream(new FileStream(Path.Combine(Helper.DirectoryPath, "assets", "fishy.wav"), FileMode.Open));
            }
            catch (Exception ex)
            {
                Monitor.Log($"error loading fishy.wav: {ex}", LogLevel.Error);
            }

            for (int i = 0; i < 4; i++)
            {
                if (!config.Voice_Test_Ignore_Me[i].Equals(config.VoiceVolume + "/" + config.VoicePitch[i], StringComparison.Ordinal)) //play voice and save it if changed
                {
                    config.Voice_Test_Ignore_Me[i] = config.VoiceVolume + "/" + config.VoicePitch[i];
                    Minigames.fishySound.Play(config.VoiceVolume / 100f * 0.98f, config.VoicePitch[i] / 100f, 0f);
                    Helper.WriteConfig(config);
                }

                try //keybinds
                {
                    if (config.KeyBinds.Equals("") || config.KeyBinds.Equals(" ")) throw new FormatException("String can't be empty.");
                    Minigames.keyBinds[i] = KeybindList.Parse(config.KeyBinds[i]);
                }
                catch (Exception e)
                {
                    string def = "MouseLeft, Space, ControllerX";
                    Minigames.keyBinds[i] = KeybindList.Parse(def);
                    config.KeyBinds[i] = def;
                    Helper.WriteConfig(config);
                    Monitor.Log(e.Message + " Resetting KeyBinds for screen " + (i + 1) + " to default. For key names, see: https://stardewcommunitywiki.com/Modding:Player_Guide/Key_Bindings", LogLevel.Error);
                }

                Minigames.voicePitch[i] = config.VoicePitch[i] / 100f;

                if (Context.IsWorldReady)
                {
                    Minigames.freeAim[i] = config.FreeAim[i];
                    Minigames.startMinigameStyle[i] = config.StartMinigameStyle[i];
                    Minigames.endMinigameStyle[i] = config.EndMinigameStyle[i];
                    Minigames.minigameDamage[i] = config.EndMinigameDamage[i];
                    Minigames.minigameDifficulty[i] = config.MinigameDifficulty[i];
                    Minigames.festivalMode[i] = config.FestivalMode[i];
                }
            }
            if (Context.IsWorldReady)
            {
                Minigames.voiceVolume = config.VoiceVolume / 100f;
                Minigames.startMinigameScale = config.StartMinigameScale;
                Minigames.realisticSizes = config.RealisticSizes;
                if (LocalizedContentManager.CurrentLanguageCode == 0) Minigames.metricSizes = config.ConvertToMetric;
                else Minigames.metricSizes = false;
                Helper.Content.InvalidateCache("Strings/StringsFromCSFiles");
            }
        }
    }
}



[HarmonyPatch(typeof(Tool), "getDescription")]
class Patch
{
    static void Postfix(ref string __result, ref Tool __instance)
    {
        if (__instance is StardewValley.Tools.FishingRod)
        {
            string desc = Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingRod.cs.14042");
            if (__instance.UpgradeLevel == 0)//bamboo
            {
                desc += "\n" + FishingMinigames.ModEntry.AddEffectDescriptions(__instance.Name);
                if (desc.EndsWith("\n")) desc = desc.Substring(0, desc.Length - 1);
                __result = Game1.parseText(desc, Game1.smallFont, desc.Length * 10);
            }
            else if (__instance.UpgradeLevel != 1)//fiber/iridium
            {
                desc += "\n" + FishingMinigames.ModEntry.AddEffectDescriptions(__instance.Name);

                if (__instance.attachments[0] != null)
                {
                    desc += "\n\n" + __instance.attachments[0].DisplayName + ((__instance.attachments[0].quality == 0) ? "" : " (" + FishingMinigames.ModEntry.translate.Get("mods.infinite") + ")")
                           + ":\n" + FishingMinigames.ModEntry.AddEffectDescriptions(__instance.attachments[0].Name);
                }
                if (__instance.attachments[1] != null)
                {
                    desc += "\n\n" + __instance.attachments[1].DisplayName + ((__instance.attachments[1].quality == 0) ? "" : " (" + FishingMinigames.ModEntry.translate.Get("mods.infinite") + ")")
                           + ":\n" + FishingMinigames.ModEntry.AddEffectDescriptions(__instance.attachments[1].Name);
                }

                if (desc.EndsWith("\n")) desc = desc.Substring(0, desc.Length - 1);
                __result = Game1.parseText(desc, Game1.smallFont, desc.Length * 10);
            }
        }
    }
}
