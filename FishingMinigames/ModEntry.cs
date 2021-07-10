using GenericModConfigMenu;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using StardewValley.Objects;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Object = StardewValley.Object;

namespace FishingMinigames
{
    public class ModEntry : Mod, IAssetEditor
    {
        ITranslationHelper translate;
        private SoundEffect fishySound;

        private List<TemporaryAnimatedSprite> animations = new List<TemporaryAnimatedSprite>();
        private SpriteBatch batch;
        private SparklingText sparklingText;
        private bool caughtDoubleFish;
        private Farmer who;
        private int whichFish;
        private int minFishSize;
        private int maxFishSize;
        private float fishSize;
        private bool recordSize;
        private bool perfect;
        private int fishQuality;
        private bool fishCaught;
        private bool bossFish;
        private int difficulty;
        private bool treasureCaught;
        private bool showPerfect;
        private bool fromFishPond;
        private int clearWaterDistance;
        private Object item;


        private bool hereFishying;
        private bool itemIsInstantCatch;
        private int oldFacingDirection;
        private int oldGameTimeInterval;
        private float itemSpriteSize;
        private int fishingFestivalMinigame;//0=none, 1=fall16, 2=winter8

        private int startMinigameStage;
        private int endMinigameStage;
        private string endMinigameKey;
        private int endMinigameTimer;
        private bool endMinigameAnimate;
        private int infoTimer;

        //config values
        public ModConfig config;
        private KeybindList keyBinds;
        private float voiceVolume;
        private float voicePitch;
        private float minigameDamage;
        private int startMinigameStyle;
        private int endMinigameStyle;
        private bool realisticSizes;
        private bool metricSizes;
        private int festivalMode;
        private float minigameDifficulty;//?




        /*  
         *  instead of where clicked, soundwave anim ahead? would be hard to aim at pools, could use swing effect anim?
         */


        public override void Entry(IModHelper helper)
        {
            UpdateConfig();

            helper.Events.Display.RenderedWorld += Display_RenderedWorld;
            helper.Events.GameLoop.UpdateTicking += GameLoop_UpdateTicking;
            helper.Events.Input.ButtonsChanged += Input_ButtonsChanged;
            helper.Events.GameLoop.GameLaunched += GenericModConfigMenuIntegration;
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
        }


        private void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            if (!Context.IsSplitScreen) UpdateConfig();
        }

        private void GenericModConfigMenuIntegration(object sender, GameLaunchedEventArgs e)     //Generic Mod Config Menu API
        {
            translate = Helper.Translation;
            var GenericMC = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (GenericMC != null)
            {
                GenericMC.RegisterModConfig(ModManifest, () => config = new ModConfig(), () => Helper.WriteConfig(config));
                GenericMC.SetDefaultIngameOptinValue(ModManifest, true);
                GenericMC.RegisterLabel(ModManifest, translate.Get("GenericMC.MainLabel"), ""); //All of these strings are stored in the traslation files.
                GenericMC.RegisterParagraph(ModManifest, translate.Get("GenericMC.MainDesc"));
                GenericMC.RegisterParagraph(ModManifest, translate.Get("GenericMC.MainDesc2"));
                GenericMC.RegisterParagraph(ModManifest, translate.Get("GenericMC.MainDescPC"));
                GenericMC.RegisterParagraph(ModManifest, translate.Get("GenericMC.MainDescPC2"));
                GenericMC.RegisterClampedOption(ModManifest, translate.Get("GenericMC.Volume"), translate.Get("GenericMC.VolumeDesc"),
                    () => config.VoiceVolume, (int val) => config.VoiceVolume = val, 0, 100);
                GenericMC.RegisterClampedOption(ModManifest, translate.Get("GenericMC.Pitch"), translate.Get("GenericMC.PitchDesc"),
                    () => config.VoicePitch, (int val) => config.VoicePitch = val, -100, 100);

                GenericMC.RegisterSimpleOption(ModManifest, translate.Get("GenericMC.KeyBinds"), translate.Get("GenericMC.KeyBindsDesc"),
                    () => config.KeyBinds, (string val) => config.KeyBinds = val);

                GenericMC.RegisterChoiceOption(ModManifest, translate.Get("GenericMC.StartMinigameStyle"), translate.Get("GenericMC.StartMinigameStyleDesc"),
                    () => (config.StartMinigameStyle == 0) ? translate.Get("GenericMC.Disabled") : (config.StartMinigameStyle == 1) ? translate.Get("GenericMC.StartMinigameStyle1") : (config.StartMinigameStyle == 2) ? translate.Get("GenericMC.StartMinigameStyle2") : translate.Get("GenericMC.StartMinigameStyle3"),
                    (string val) => config.StartMinigameStyle = Int32.Parse((val.Equals(translate.Get("GenericMC.Disabled"), StringComparison.Ordinal)) ? "0" : (val.Equals(translate.Get("GenericMC.StartMinigameStyle1"), StringComparison.Ordinal)) ? "1" : (val.Equals(translate.Get("GenericMC.StartMinigameStyle2"), StringComparison.Ordinal)) ? "2" : "3"),
                    new string[] { translate.Get("GenericMC.Disabled"), translate.Get("GenericMC.StartMinigameStyle1"), translate.Get("GenericMC.StartMinigameStyle2"), translate.Get("GenericMC.StartMinigameStyle3") });//small 'hack' so options appear as name strings, while config.json stores them as integers

                GenericMC.RegisterChoiceOption(ModManifest, translate.Get("GenericMC.EndMinigameStyle"), translate.Get("GenericMC.EndMinigameStyleDesc"),
                    () => (config.EndMinigameStyle == 0) ? translate.Get("GenericMC.Disabled") : (config.EndMinigameStyle == 1) ? translate.Get("GenericMC.EndMinigameStyle1") : (config.EndMinigameStyle == 2) ? translate.Get("GenericMC.EndMinigameStyle2") : translate.Get("GenericMC.EndMinigameStyle3"),
                    (string val) => config.EndMinigameStyle = Int32.Parse((val.Equals(translate.Get("GenericMC.Disabled"), StringComparison.Ordinal)) ? "0" : (val.Equals(translate.Get("GenericMC.EndMinigameStyle1"), StringComparison.Ordinal)) ? "1" : (val.Equals(translate.Get("GenericMC.EndMinigameStyle2"), StringComparison.Ordinal)) ? "2" : "3"),
                    new string[] { translate.Get("GenericMC.Disabled"), translate.Get("GenericMC.EndMinigameStyle1"), translate.Get("GenericMC.EndMinigameStyle2"), translate.Get("GenericMC.EndMinigameStyle3") });

                GenericMC.RegisterClampedOption(ModManifest, translate.Get("GenericMC.EndDamage"), translate.Get("GenericMC.EndDamageDesc"),
                    () => config.EndMinigameDamage, (float val) => config.EndMinigameDamage = val, 0f, 2f);
                GenericMC.RegisterClampedOption(ModManifest, translate.Get("GenericMC.Difficulty"), translate.Get("GenericMC.DifficultyDesc"),
                    () => config.MinigameDifficulty, (float val) => config.MinigameDifficulty = val, 0f, 2f);
                GenericMC.RegisterSimpleOption(ModManifest, translate.Get("GenericMC.RealisticSizes"), translate.Get("GenericMC.RealisticSizesDesc"),
                    () => config.RealisticSizes, (bool val) => config.RealisticSizes = val);

                if (LocalizedContentManager.CurrentLanguageCode == 0) GenericMC.RegisterSimpleOption(ModManifest, translate.Get("GenericMC.ConvertToMetric"), translate.Get("GenericMC.ConvertToMetricDesc"),
                    () => config.ConvertToMetric, (bool val) => config.ConvertToMetric = val);

                GenericMC.RegisterLabel(ModManifest, "", "");
                GenericMC.RegisterLabel(ModManifest, translate.Get("GenericMC.FestivalLabel"), "");
                GenericMC.RegisterParagraph(ModManifest, translate.Get("GenericMC.FestivalDesc"));
                GenericMC.RegisterChoiceOption(ModManifest, translate.Get("GenericMC.FestivalMode"), "",
                    () => (config.FestivalMode == 0) ? translate.Get("GenericMC.FestivalModeVanilla") : (config.FestivalMode == 1) ? translate.Get("GenericMC.FestivalModeSimple") : translate.Get("GenericMC.FestivalModePerfectOnly"),
                    (string val) => config.FestivalMode = Int32.Parse((val.Equals(translate.Get("GenericMC.FestivalModeVanilla"), StringComparison.Ordinal)) ? "0" : (val.Equals(translate.Get("GenericMC.FestivalModeSimple"), StringComparison.Ordinal)) ? "1" : "2"),
                    new string[] { translate.Get("GenericMC.FestivalModeVanilla"), translate.Get("GenericMC.FestivalModeSimple"), translate.Get("GenericMC.FestivalModePerfectOnly") });
            }
        }


        private void Input_ButtonsChanged(object sender, ButtonsChangedEventArgs e)  //this.Monitor.Log(locationName, LogLevel.Debug);
        {
            infoTimer = 0;
            who = Game1.player;
            if (keyBinds.JustPressed()) //cancel regular rod use, if it's a shared keybind
            {
                if (!Context.IsPlayerFree) return;
                if (Game1.activeClickableMenu == null && who.CurrentItem is FishingRod)
                {
                    if (fishingFestivalMinigame > 0 && festivalMode == 0) return;
                    if (e.Pressed.Contains(Game1.options.useToolButton[0].ToSButton())) Helper.Input.Suppress(Game1.options.useToolButton[0].ToSButton());
                    else if (e.Pressed.Contains(Game1.options.useToolButton[1].ToSButton())) Helper.Input.Suppress(Game1.options.useToolButton[1].ToSButton());
                    else if (e.Pressed.Contains(SButton.ControllerX)) Helper.Input.Suppress(SButton.ControllerX);
                }
            }


            if (e.Pressed.Contains(SButton.F5))
            {
                if (Context.IsWorldReady) _ = EmergencyCancel(Game1.player);
                UpdateConfig();
            }
            if (!Context.IsWorldReady) return;

            if (e.Pressed.Contains(SButton.Z))
            {

            }


            if (endMinigameStage == 2 || endMinigameStage == 3) EndMinigame(1);
            else
            {
                if (keyBinds.JustPressed())
                {

                    if (Context.IsWorldReady && Context.CanPlayerMove && who.CurrentTool is FishingRod)
                    {
                        if (Game1.isFestival() && (fishingFestivalMinigame == 0 || festivalMode == 0)) return;
                        if (fishingFestivalMinigame == 1)
                        {
                            FestivalGameSkip(who, e);
                            return;
                        }

                        if (!hereFishying)
                        {
                            try
                            {
                                perfect = false;
                                Vector2 mouse = Game1.currentCursorTile;
                                oldFacingDirection = who.getGeneralDirectionTowards(new Vector2(mouse.X * 64, mouse.Y * 64));
                                who.faceDirection(oldFacingDirection);


                                if (who.currentLocation.canFishHere() && who.currentLocation.isTileFishable((int)mouse.X, (int)mouse.Y))
                                {
                                    Monitor.Log($"here fishy fishy {mouse.X},{mouse.Y}");
                                    HereFishyFishy(who, (int)mouse.X * 64, (int)mouse.Y * 64);
                                }
                            }
                            catch
                            {
                                Monitor.Log($"error getting water tile", LogLevel.Error);
                            }
                        }
                    }
                }
            }
        }

        private void GameLoop_UpdateTicking(object sender, UpdateTickingEventArgs e) //adds item to inv
        {
            if (Game1.isFestival())
            {
                fishingFestivalMinigame = 0;
                string data = Helper.Reflection.GetField<Dictionary<string, string>>(Game1.CurrentEvent, "festivalData").GetValue()["file"];
                if (data != null)
                {
                    who = Game1.player;
                    int timer = 0;
                    if (data.Equals("fall16") && Game1.currentMinigame is StardewValley.Minigames.FishingGame)
                    {
                        timer = Helper.Reflection.GetField<int>(Game1.currentMinigame as StardewValley.Minigames.FishingGame, "gameEndTimer").GetValue();
                        if (timer < 100000 && timer >= 500) fishingFestivalMinigame = 1;
                    }
                    else if (data.Equals("winter8"))
                    {
                        timer = Game1.CurrentEvent.festivalTimer;
                        if (timer < 120000 && timer >= 500) fishingFestivalMinigame = 2;
                    }
                    if (timer <= 500 && fishingFestivalMinigame > 0 && festivalMode > 0) _ = EmergencyCancel(who);
                }
            }

            for (int i = animations.Count - 1; i >= 0; i--)
            {
                if (endMinigameStage != 3 && animations[i].update(Game1.currentGameTime))
                {
                    animations.RemoveAt(i);
                }
            }
            if (sparklingText != null && sparklingText.update(Game1.currentGameTime))
            {
                sparklingText = null;
            }
            if (fishCaught)
            {
                if (fishingFestivalMinigame == 0) infoTimer = 1000;
                who.faceDirection(oldFacingDirection);
                if (fishingFestivalMinigame == 0) Helper.Multiplayer.SendMessage(who.UniqueMultiplayerID, "FishCaught", modIDs: new[] { "barteke22.FishingInfoOverlays" });//update overlay
                fishCaught = false;
            }
            if (infoTimer > 0) infoTimer--;
        }

        private void Display_RenderedWorld(object sender, RenderedWorldEventArgs e)
        {
            if (batch == null) batch = e.SpriteBatch;
            who = Game1.player;
            if (!Game1.eventUp && !Game1.menuUp && !hereFishying && who.CurrentItem is FishingRod && who.currentLocation.isTileFishable((int)Game1.currentCursorTile.X, (int)Game1.currentCursorTile.Y))
            {
                e.SpriteBatch.Draw(Game1.mouseCursors, new Vector2(((Game1.getMouseX() / 64) * 64), ((Game1.getMouseY() / 64) * 64)), new Rectangle(652, 204, 44, 44), new Color(0, 255, 0, 0.4f), 0f, Vector2.Zero, 1.45f, SpriteEffects.None, 1f);
            }
            for (int i = animations.Count - 1; i >= 0; i--)
            {
                animations[i].draw(e.SpriteBatch, false, 0, 0, 1f);
                if (endMinigameStage > 0 && i == 0)
                {
                    if (endMinigameStage == 1)
                    {
                        Rectangle area = new Rectangle((int)who.Position.X - 200, (int)who.Position.Y - 400, 400, 400);
                        if (area.Contains((int)animations[0].Position.X, (int)animations[0].Position.Y) && animations[0].initialPosition != animations[0].Position)
                        {
                            endMinigameStage = 2;
                        }
                    }
                    if (endMinigameStage == 2)
                    {
                        Rectangle area = new Rectangle((int)who.Position.X - 70, (int)who.Position.Y - 185, 140, 205);
                        if (area.Contains((int)animations[0].Position.X, (int)animations[0].Position.Y) && animations[0].initialPosition != animations[0].Position)
                        {
                            EndMinigame(0);
                        }
                    }
                    else if (endMinigameStage == 3)
                    {
                        endMinigameTimer++;

                        int totalDifficulty = (int)(100f + ((endMinigameStyle == 3) ? 25f : 0f) - ((difficulty / 2f) - (fishSize / 10f) * minigameDifficulty));

                        if (endMinigameTimer > totalDifficulty)
                        {
                            endMinigameTimer = 0;
                            endMinigameStage = 8;
                            who.completelyStopAnimatingOrDoingAction();
                            List<FarmerSprite.AnimationFrame> animationFrames = new List<FarmerSprite.AnimationFrame>(){
                                new FarmerSprite.AnimationFrame(94, 500, false, false, null, false).AddFrameAction(delegate (Farmer f) { f.jitterStrength = 2f; }) };
                            who.FarmerSprite.setCurrentAnimation(animationFrames.ToArray());
                            who.FarmerSprite.PauseForSingleAnimation = true;
                            who.FarmerSprite.loop = true;
                            who.FarmerSprite.loopThisAnimation = true;
                            who.Sprite.currentFrame = 94;
                        }
                    }
                }
            }

            if (endMinigameAnimate) Game1.drawTool(who);

            if (showPerfect)
            {
                perfect = true;
                sparklingText = new SparklingText(Game1.dialogueFont, Game1.content.LoadString("Strings\\UI:BobberBar_Perfect"), Color.Yellow, Color.White, false, 0.1, 1500, -1, 500, 1f);
                Game1.playSound("jingle1");
                showPerfect = false;
            }

            if (sparklingText != null && who != null && !itemIsInstantCatch)
            {
                sparklingText.draw(e.SpriteBatch, Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(-64f, -400f)));
            }

            if (endMinigameStyle == 3 && endMinigameTimer > 0 && endMinigameTimer < 100)
            {
                Vector2 position = new Vector2(who.getStandingX() - Game1.viewport.X, who.getStandingY() - 156 - Game1.viewport.Y);
                batch.Draw(Game1.mouseCursors, position + new Vector2(-24, 0), new Rectangle(473, 36, 24, 24), Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0.98f);//text bg box
                batch.DrawString(Game1.smallFont, endMinigameKey, position - (Game1.smallFont.MeasureString(endMinigameKey) / 2 * 1.2f) + new Vector2(0f, 28f), Color.Gold, 0f, Vector2.Zero, 1.2f, SpriteEffects.None, 1f); //text
            }
            if (infoTimer > 0)
            {
                CaughtBubble(who);
            }
        }


        private async void HereFishyFishy(Farmer who, int x, int y)
        {
            if (who.IsLocalPlayer && fishingFestivalMinigame != 2)
            {
                float oldStamina = who.Stamina;
                who.Stamina -= 8f - (float)who.FishingLevel * 0.1f;
                who.checkForExhaustion(oldStamina);
            }

            oldGameTimeInterval = Game1.gameTimeInterval;
            if (!Game1.IsMultiplayer && !Game1.isFestival()) Game1.gameTimeInterval = 0;
            startMinigameStage = 0;
            endMinigameStage = 0;

            CatchFish(who, x, y);


            if (!fromFishPond && fishingFestivalMinigame != 2 && startMinigameStyle > 0)
            {
                //startMinigameStage = 1;
                //await MINIGAME                    TODO
            }

            if (startMinigameStage == 5)
            {
                await SwingAndEmote (who, 2);
                return;
            }
            else await HereFishyStartingAnimation(who);


            if (!fromFishPond && endMinigameStyle > 0) endMinigameStage = 1;

            await HereFishyFlyingAnimation(who, x, y);
        }


        private void CatchFish(Farmer who, int x, int y)
        {
            FishingRod rod = who.CurrentTool as FishingRod;
            Vector2 bobberTile = new Vector2(x / 64, y / 64);
            fromFishPond = who.currentLocation.isTileBuildingFishable((int)bobberTile.X, (int)bobberTile.Y);

            clearWaterDistance = FishingRod.distanceToLand((int)bobberTile.X, (int)bobberTile.Y, who.currentLocation);
            double baitPotency = ((rod.attachments[0] != null) ? ((float)rod.attachments[0].Price / 10f) : 0f);
            
            Rectangle fishSplashRect = new Rectangle(who.currentLocation.fishSplashPoint.X * 64, who.currentLocation.fishSplashPoint.Y * 64, 64, 64);
            Rectangle bobberRect = new Rectangle(x - 80, y - 80, 64, 64);
            bool splashPoint = fishSplashRect.Intersects(bobberRect);

            item = who.currentLocation.getFish(0, (rod.attachments[0] != null) ? rod.attachments[0].ParentSheetIndex : (-1), clearWaterDistance + (splashPoint ? 1 : 0), who, baitPotency + (splashPoint ? 0.4 : 0.0), bobberTile); //all item data starts here, FishingRod.cs

            if (fromFishPond) //get whole fishpond stage in one go: 6-3-1 fish
            {
                foreach (Building b in Game1.getFarm().buildings)
                {
                    if (b is FishPond && b.isTileFishable(bobberTile))
                    {
                        for (int i = 0; i < (b as FishPond).currentOccupants.Value; i++)
                        {
                            (b as FishPond).CatchFish();
                            item.Stack++;
                        }
                        break;
                    }
                }
            }

            if (item != null) whichFish = item.ParentSheetIndex;//fix here for fishpond

            if (item == null || whichFish <= 0)
            {
                item = new Object(Game1.random.Next(167, 173), 1);//trash
                whichFish = item.ParentSheetIndex;
            }

            fishSize = 0f;
            fishQuality = 0;
            difficulty = 0;
            minFishSize = 0;
            maxFishSize = 0;

            Dictionary<int, string> data = Game1.content.Load<Dictionary<int, string>>("Data\\Fish");
            string[] fishData = null;
            if (data.ContainsKey(whichFish)) fishData = data[whichFish].Split('/');


            itemIsInstantCatch = false;
            if (item is Furniture) itemIsInstantCatch = true;
            else if (Utility.IsNormalObjectAtParentSheetIndex(item, whichFish) && data.ContainsKey(whichFish))
            {
                if (int.TryParse(fishData[1], out difficulty) && int.TryParse(fishData[3], out minFishSize) && int.TryParse(fishData[4], out maxFishSize)) itemIsInstantCatch = false;
                else itemIsInstantCatch = true;
            }
            else itemIsInstantCatch = true;

            if (itemIsInstantCatch || item.Category == -20 || item.ParentSheetIndex == 152 || item.ParentSheetIndex == 153 || item.parentSheetIndex == 157 || item.parentSheetIndex == 797 || item.parentSheetIndex == 79 || item.parentSheetIndex == 73 || item.ParentSheetIndex == 842 || (item.ParentSheetIndex >= 820 && item.ParentSheetIndex <= 828) || item.parentSheetIndex == GameLocation.CAROLINES_NECKLACE_ITEM || item.ParentSheetIndex == 890 || fromFishPond)
            {
                itemIsInstantCatch = true;
            }

            //special item handling
            if (whichFish == GameLocation.CAROLINES_NECKLACE_ITEM) item.questItem.Value = true;
            if (whichFish == 79 || whichFish == 842)
            {
                item = who.currentLocation.tryToCreateUnseenSecretNote(who);
            }
            if (fishingFestivalMinigame != 2 && !(item is Furniture) && !fromFishPond && who.team.specialOrders != null)
            {
                foreach (SpecialOrder order in who.team.specialOrders)
                {
                    order.onFishCaught?.Invoke(who, item);
                }
            }


            //sizes
            if (maxFishSize > 0)
            {
                fishSize = Math.Min((float)clearWaterDistance / 5f, 1f);
                int minimumSizeContribution = 1 + who.FishingLevel / 2;
                fishSize *= (float)Game1.random.Next(minimumSizeContribution, Math.Max(6, minimumSizeContribution)) / 5f;

                if (rod.getBaitAttachmentIndex() != -1) fishSize *= 1.2f;
                fishSize *= 1f + (float)Game1.random.Next(-10, 11) / 100f;
                fishSize = Math.Max(0f, Math.Min(1f, fishSize));


                fishSize = (int)((float)minFishSize + (float)(maxFishSize - minFishSize) * fishSize);
                fishSize++;
            }

            if (rod.Name.Equals("Training Rod", StringComparison.Ordinal)) fishSize = minFishSize;


            bossFish = FishingRod.isFishBossFish(whichFish);
        }

        private void CatchFishAfterMinigame(Farmer who)
        {
            //data calculations: quality, double, exp, treasure
            FishingRod rod = who.CurrentTool as FishingRod;

            treasureCaught = false;
            float reduction = 0f;

            if (!itemIsInstantCatch)
            {
                if (rod.Name.Equals("Training Rod", StringComparison.Ordinal))
                {
                    fishQuality = 0;
                    fishSize = minFishSize;
                }
                else
                {
                    fishQuality = (fishSize * (0.9 + (who.FishingLevel / 5.0)) < 0.33) ? 0 : ((fishSize * (0.9 + (who.FishingLevel / 5.0)) < 0.66) ? 1 : 2);//init quality
                    if (rod.getBobberAttachmentIndex() == 877) fishQuality++;

                    if (startMinigameStyle > 0 && endMinigameStyle > 0) //minigame score reductions
                    {
                        if (startMinigameStage == 10) reduction -= 0.4f;
                        else if (startMinigameStage == 9) reduction += 0.3f;
                        else if (startMinigameStage == 8) reduction += 0.5f;
                        else if (startMinigameStage == 7) reduction += 0.7f;
                        else if (startMinigameStage == 6) reduction += 0.8f;
                        if (endMinigameStage == 10) reduction -= 0.4f;
                        else if (endMinigameStage == 9) reduction += 0.6f;
                        else if (endMinigameStage == 8) reduction += 0.8f;
                    }
                    else if (startMinigameStyle > 0)
                    {
                        if (startMinigameStage == 10) reduction -= 1f;
                        else if (startMinigameStage == 9) reduction += 0f;
                        else if (startMinigameStage < 9) reduction += 1f;
                        else if (startMinigameStage == 6) reduction += 2f;
                    }
                    else if (endMinigameStyle > 0)
                    {
                        if (endMinigameStage == 10) reduction -= 1f;
                        else if (endMinigameStage == 9) reduction += (Game1.random.Next(0, 2) == 0) ? 0f : 1f;
                        else if (endMinigameStage < 8) reduction += 2f;
                    }
                    else
                    {
                        if (perfect) fishQuality++;
                    }
                    fishSize -= (int)Math.Round(reduction * 2);
                    fishQuality -= (int)Math.Round(reduction);
                }

                if (fishQuality < 0) fishQuality = 0;
                if (fishQuality > 2) fishQuality = 4;


                caughtDoubleFish = fishingFestivalMinigame != 2 && !bossFish && rod.getBaitAttachmentIndex() == 774 && !fromFishPond && Game1.random.NextDouble() < 0.1 + who.DailyLuck / 2.0 - reduction - 0.5f;


                if (who.IsLocalPlayer && fishingFestivalMinigame != 2)
                {
                    int experience = Math.Max(1, (fishQuality + 1) * 3 + difficulty / 3);
                    if (bossFish) experience *= 5;

                    if (startMinigameStyle + endMinigameStyle > 0) experience += (int)(experience - reduction - 0.5f);
                    else if (perfect) experience += (int)((float)experience * 1.4f);

                    who.gainExperience(1, experience);
                    if (minigameDamage > 0 && endMinigameStyle > 0 && endMinigameStage == 8) who.takeDamage((int)((10 + (difficulty / 10) + (int)(fishSize / 5) - who.FishingLevel) * minigameDamage), true, null);
                }


                treasureCaught = fishingFestivalMinigame != 2 && who.fishCaught != null && who.fishCaught.Count() > 1 && Game1.random.NextDouble() < FishingRod.baseChanceForTreasure + (double)who.LuckLevel * 0.005 + ((rod.getBaitAttachmentIndex() == 703) ? FishingRod.baseChanceForTreasure : 0.0) + ((rod.getBobberAttachmentIndex() == 693) ? (FishingRod.baseChanceForTreasure / 3.0) : 0.0) + who.DailyLuck / 2.0 + ((who.professions.Contains(9) ? FishingRod.baseChanceForTreasure : 0.0) - reduction - 0.5f);
                item.Quality = fishQuality;
                if (caughtDoubleFish) item.Stack = 2;
            }
            else if (who.IsLocalPlayer && fishingFestivalMinigame != 2)
            {
                who.gainExperience(1, 3);
                if (!fromFishPond && minigameDamage > 0 && endMinigameStyle > 0 && endMinigameStage == 8) who.takeDamage((int)((16 - who.FishingLevel) * minigameDamage), true, null);
            }
        }

        //player jumping and calling fish
        private async Task HereFishyStartingAnimation(Farmer who)
        {
            Game1.freezeControls = true;
            hereFishying = true;
            if (fishySound != null) fishySound.Play(voiceVolume, voicePitch, 0);

            who.completelyStopAnimatingOrDoingAction();
            who.jitterStrength = 2f;
            List<FarmerSprite.AnimationFrame> animationFrames = new List<FarmerSprite.AnimationFrame>(){
                new FarmerSprite.AnimationFrame(94, 100, false, false, null, false).AddFrameAction(delegate (Farmer f) { f.jitterStrength = 2f; }) };

            who.FarmerSprite.setCurrentAnimation(animationFrames.ToArray());
            who.FarmerSprite.PauseForSingleAnimation = true;
            who.FarmerSprite.loop = true;
            who.FarmerSprite.loopThisAnimation = true;
            who.Sprite.currentFrame = 94;


            await Task.Delay(1793);

            if (startMinigameStyle + endMinigameStyle == 0 && Game1.random.Next(who.FishingLevel, 20) > 16)
            {
                showPerfect = true;
            }

            who.synchronizedJump(8f);

            await Task.Delay(1000);

            who.stopJittering();
            who.completelyStopAnimatingOrDoingAction();
            who.forceCanMove();
            Game1.freezeControls = true;

            await Task.Delay(Game1.random.Next(500, 1000));

            animations.Clear();
        }

        //fish flying from xy to player
        private async Task HereFishyFlyingAnimation(Farmer who, int x, int y)
        {
            if (itemIsInstantCatch && !fromFishPond) //angory fish emote workaround
            {
                int interval = 200;
                Vector2 position = new Vector2(x, y - 32);
                animations.Add(new TemporaryAnimatedSprite(Game1.emoteSpriteSheet.ToString(), new Rectangle(12 * 16 % Game1.emoteSpriteSheet.Width, 12 * 16 / Game1.emoteSpriteSheet.Width * 16, 16, 16), interval, 1, 0, position, false, false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false));
                await Task.Delay(interval);
                animations.Add(new TemporaryAnimatedSprite(Game1.emoteSpriteSheet.ToString(), new Rectangle(13 * 16 % Game1.emoteSpriteSheet.Width, 12 * 16 / Game1.emoteSpriteSheet.Width * 16, 16, 16), interval, 1, 0, position, false, false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false));
                await Task.Delay(interval);
                animations.Add(new TemporaryAnimatedSprite(Game1.emoteSpriteSheet.ToString(), new Rectangle(14 * 16 % Game1.emoteSpriteSheet.Width, 12 * 16 / Game1.emoteSpriteSheet.Width * 16, 16, 16), interval, 1, 0, position, false, false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false));
                await Task.Delay(interval);
                animations.Add(new TemporaryAnimatedSprite(Game1.emoteSpriteSheet.ToString(), new Rectangle(15 * 16 % Game1.emoteSpriteSheet.Width, 12 * 16 / Game1.emoteSpriteSheet.Width * 16, 16, 16), interval, 1, 0, position, false, false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false));
                await Task.Delay(interval);
            }

            //realistic: divide fish size by 64 inches (around average human size) * 10f (how much you need to multiply item sprite to be player height (8*16 = 128 = 2 tiles + 20% for perspective))
            if (realisticSizes)
            {
                itemSpriteSize = 2.5f;
                if (fishSize > 0) itemSpriteSize = Math.Max(fishSize / 64f, 0.05f) * 10f;
            }
            else itemSpriteSize = 4f;
            if (item is Furniture) itemSpriteSize = 2.2f;

            float t;
            float distance2 = y - (float)(who.getStandingY() - 100);
            float height2 = Math.Abs(distance2 + 256f + 32f);
            if (who.FacingDirection == 0)
            {
                height2 += 96f;
            }
            float gravity2 = 0.002f;
            float velocity = (float)Math.Sqrt((double)(2f * gravity2 * height2));
            t = (float)(Math.Sqrt((double)(2f * (height2 - distance2) / gravity2)) + (double)(velocity / gravity2));
            float xVelocity2 = 0f;
            if (t != 0f)
            {
                xVelocity2 = (who.Position.X - x) / t;
            }
            animations.Add(new TemporaryAnimatedSprite((item is Furniture) ? Furniture.furnitureTexture.ToString() : "Maps\\springobjects", (item is Furniture) ? (item as Furniture).defaultSourceRect : Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, whichFish, 16, 16), t, 1, 0, new Vector2(x, y), false, false, y / 10000f, 0f, Color.White, itemSpriteSize, 0f, 0f, 0f, false)
            {
                motion = new Vector2(xVelocity2, -velocity),
                acceleration = new Vector2(0f, gravity2),
                timeBasedMotion = true,
                endFunction = new TemporaryAnimatedSprite.endBehavior(PlayerCaughtFishEndFunction),
                extraInfoForEndBehavior = whichFish,
                endSound = "tinyWhip"
            });
            for (int i = 1; i < item.Stack; i++)
            {
                await Task.Delay(100);
                animations.Add(new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, whichFish, 16, 16), t, 1, 0, new Vector2(x, y), false, false, y / 10000f, 0f, Color.White, itemSpriteSize, 0f, 0f, 0f, false)
                {
                    motion = new Vector2(xVelocity2, -velocity),
                    acceleration = new Vector2(0f, gravity2),
                    timeBasedMotion = true,
                    endSound = "tinyWhip",
                    Parent = who.currentLocation
                });

            }
            Game1.freezeControls = true;
        }

        private void EndMinigame(int stage)
        {
            if (config.EndMinigameStyle == 3 && (Game1.options.gamepadControls || Game1.options.gamepadMode == Options.GamepadModes.ForceOn || Game1.options.gamepadMode != Options.GamepadModes.ForceOff)) endMinigameStyle = 2;
            else endMinigameStyle = 3;

            if (stage == 0) //pick button + show alert/button sprite
            {
                endMinigameStage = 3;
                endMinigameTimer = 0;
                who.PlayFishBiteChime();

                string sprite = "LooseSprites\\Cursors";
                Rectangle rect = new Rectangle(395, 497, 3, 8);
                Vector2 offset = new Vector2(-7.5f, 0);
                Color color = Color.White;

                switch (endMinigameStyle)
                {
                    case 1:
                        break;
                    case 2:
                        color = Color.Gold;
                        offset = new Vector2(-25f, -20f);
                        int direction = Game1.random.Next(0, 4);
                        endMinigameKey = direction.ToString();
                        switch (direction)
                        {
                            case 0://up
                                rect = new Rectangle(407, 1651, 10, 10);
                                break;
                            case 1://right
                                rect = new Rectangle(416, 1660, 10, 10);
                                break;
                            case 2://down
                                rect = new Rectangle(407, 1660, 10, 10);
                                break;
                            case 3://left
                                rect = new Rectangle(398, 1660, 10, 10);
                                break;
                        }
                        break;
                    case 3:
                        endMinigameKey = item.DisplayName.Replace(" ", "")[Game1.random.Next(0, item.DisplayName.Replace(" ", "").Length)].ToString().ToUpper();
                        return;
                }
                Game1.screenOverlayTempSprites.Add(new TemporaryAnimatedSprite(sprite, rect, new Vector2(who.getStandingX() - Game1.viewport.X, who.getStandingY() - 136 - Game1.viewport.Y) + offset, flipped: false, 0.02f, color)
                {
                    scale = 5f,
                    scaleChange = -0.01f,
                    motion = new Vector2(0f, -0.5f),
                    shakeIntensityChange = -0.005f,
                    shakeIntensity = 1f
                });
            }
            else
            {
                bool passed = false;
                if (endMinigameStage == 3) //button press, if sprite appeared
                {
                    switch (endMinigameStyle)
                    {
                        case 1:
                            if (keyBinds.JustPressed()) passed = true;
                            break;
                        case 2:
                            switch (endMinigameKey)
                            {
                                case "0"://up
                                    if (KeybindList.Parse("W, Up, DPadUp").JustPressed()) passed = true;
                                    break;
                                case "1"://right
                                    if (KeybindList.Parse("D, Right, DPadRight").JustPressed()) passed = true;
                                    break;
                                case "2"://down
                                    if (KeybindList.Parse("S, Down, DPadDown").JustPressed()) passed = true;
                                    break;
                                case "3"://left
                                    if (KeybindList.Parse("A, Left, DPadLeft").JustPressed()) passed = true;
                                    break;
                            }
                            break;
                        case 3:
                            if (KeybindList.Parse(endMinigameKey).JustPressed()) passed = true;
                            break;
                    }
                }
                Game1.freezeControls = true;
                if (passed)
                {
                    int totalDifficulty = (int)((30f + ((endMinigameStyle == 3) ? 20f : (endMinigameStyle == 2) ? 10f : 0f)) - ((difficulty / 33f) - (fishSize / 33f) * minigameDifficulty));

                    if (endMinigameStage == 3 && endMinigameTimer < totalDifficulty) endMinigameStage = 10;
                    else endMinigameStage = 9;

                    animations.Clear();
                    PlayerCaughtFishEndFunction(whichFish);
                    endMinigameTimer = 0;
                }
                else //button press, too early or wrong
                {
                    Game1.playSound("fishEscape");
                    _ = SwingAndEmote(who, 1);
                    endMinigameStage = 3;
                    endMinigameTimer = 1000;
                }
            }
        }

        private async void PlayerCaughtFishEndFunction(int extraData)
        {
            if (endMinigameStage > 8) await SwingAndEmote(who, 0);

            CatchFishAfterMinigame(who);

            if (!fromFishPond)
            {
                if (endMinigameStage > 8) await Task.Delay(300);
                else if (endMinigameStage == 8)
                {
                    animations.Add(new TemporaryAnimatedSprite(10, who.Position - new Vector2(0, 100), Color.Blue));//water on face
                    await Task.Delay(100);
                }

                if (fishingFestivalMinigame == 0)
                {
                    recordSize = who.caughtFish(whichFish, (metricSizes) ? (int)(fishSize * 2.54f) : (int)fishSize, false, caughtDoubleFish ? 2 : 1);
                    if (bossFish)
                    {
                        Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingRod.cs.14068"));
                        string name = Game1.objectInformation[whichFish].Split('/')[4];

                        //Helper.Reflection.GetField<Multiplayer>(Game1.game1, "multiplayer").GetValue().globalChatInfoMessage("CaughtLegendaryFish", new string[] { who.Name, name }); //multiplayer class is not protected
                        if (Game1.IsMultiplayer || Game1.multiplayerMode != 0)
                        {
                            if (Game1.IsClient) Game1.client.sendMessage(15, "CaughtLegendaryFish", new string[] { who.Name, name });
                            else if (Game1.IsServer)
                            {
                                foreach (long id in Game1.otherFarmers.Keys)
                                {
                                    Game1.server.sendMessage(id, 15, who, "CaughtLegendaryFish", new string[] { who.Name, name });
                                }
                            }
                        }
                    }
                    else if (recordSize)
                    {
                        sparklingText = new SparklingText(Game1.dialogueFont, Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingRod.cs.14069"), Color.LimeGreen, Color.Azure, false, 0.1, 2500, -1, 500, 1f);
                        who.currentLocation.localSound("newRecord");
                    }
                }

                await SwingAndEmote(who, 2);
            }

            Monitor.Log($"caught fish end");
            who.Halt();
            who.armOffset = Vector2.Zero;


            if (fishingFestivalMinigame == 2)
            {
                if (!itemIsInstantCatch)
                {
                    if (endMinigameStage == 10 || festivalMode == 1) Game1.CurrentEvent.caughtFish(whichFish, (int)fishSize, who);
                }
            }
            else//adding items + chest
            {
                FishingRod rod = who.CurrentTool as FishingRod;
                Helper.Reflection.GetField<Farmer>(rod, "lastUser").SetValue(who);
                Helper.Reflection.GetField<int>(rod, "whichFish").SetValue(whichFish);
                Helper.Reflection.GetField<bool>(rod, "caughtDoubleFish").SetValue(caughtDoubleFish);
                Helper.Reflection.GetField<int>(rod, "fishQuality").SetValue(fishQuality);
                Helper.Reflection.GetField<int>(rod, "clearWaterDistance").SetValue(clearWaterDistance);
                Helper.Reflection.GetField<Farmer>(who.CurrentTool, "lastUser").SetValue(who);

                if (!treasureCaught || itemIsInstantCatch)
                {
                    if (!fromFishPond) rod.doneFishing(who, true);
                    who.addItemByMenuIfNecessary(item);
                }
                else
                {
                    await Task.Delay(1000);
                    who.currentLocation.localSound("openChest");
                    animations.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(64, 1920, 32, 32), 200f, 4, 0, who.Position + new Vector2(-32f, -228f), flicker: false, flipped: false, (float)who.getStandingY() / 10000f + 0.001f, 0f, Color.White, 4f, 0f, 0f, 0f)
                    {
                        motion = new Vector2(0f, -0.128f),
                        timeBasedMotion = true,
                        alpha = 0f,
                        alphaFade = -0.002f,
                        endFunction = rod.openTreasureMenuEndFunction,
                        extraInfoForEndBehavior = (!who.addItemToInventoryBool(item)) ? 1 : 0
                    });
                    await Task.Delay(1000);
                }
            }

            Game1.freezeControls = false;

            if (!Game1.IsMultiplayer && !Game1.isFestival()) Game1.gameTimeInterval = oldGameTimeInterval;
            hereFishying = false;
            fishCaught = true;
        }


        private async Task SwingAndEmote(Farmer who, int which) //send messages to other mods to do stuff: sounds/animations
        {
            if (which < 2)
            {
                if (!fromFishPond && endMinigameStyle > 0) //swing animation
                {
                    if (endMinigameStage == 10) showPerfect = true;
                    who.completelyStopAnimatingOrDoingAction();
                    endMinigameAnimate = true;
                    (who.CurrentTool as FishingRod).setTimingCastAnimation(who);
                    switch (oldFacingDirection)
                    {
                        case 0://up
                            who.FarmerSprite.animateOnce(295, 1f, 1);
                            who.CurrentTool.Update(0, 0, who);
                            break;
                        case 1://right
                            who.FarmerSprite.animateOnce(296, 1f, 1);
                            who.CurrentTool.Update(1, 0, who);
                            break;
                        case 2://down
                            who.FarmerSprite.animateOnce(297, 1f, 1);
                            who.CurrentTool.Update(2, 0, who);
                            break;
                        case 3://left
                            who.FarmerSprite.animateOnce(298, 1f, 1);
                            who.CurrentTool.Update(3, 0, who);
                            break;
                    }
                    await Task.Delay(300);
                    endMinigameAnimate = false;
                }
            }
            else if (which == 2)
            {
                if (startMinigameStyle + endMinigameStyle > 0)
                {
                    if (startMinigameStage == 4) //for now 4 = cancel = X
                    {
                        who.doEmote(36);
                        who.netDoEmote("x");
                    }
                    else if (startMinigameStage == 5) //for now 5 = fail = Angry
                    {
                        who.doEmote(12);
                        who.netDoEmote("angry");
                    }
                    else if (endMinigameStage == 8) //8 = hit = Uh
                    {
                        who.doEmote(10);
                        who.netDoEmote("angry");
                    }
                    else //otherwise = happy
                    {
                        who.doEmote(32);
                        who.netDoEmote("happy");
                    }
                }
            }
        }

        private void CaughtBubble(Farmer who)
        {
            if (!(who.CurrentItem is FishingRod))//cancel for scrolling
            {
                infoTimer = 0;
                return;
            }
            //arm up
            who.FacingDirection = 2;
            who.FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[1] { new FarmerSprite.AnimationFrame(84, 150) });

            if (maxFishSize > 0)
            {
                //bubble
                batch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(-130f, -75f)), new Rectangle(156, 465, 5, 20), Color.White * 0.8f, -1.57f, Vector2.Zero, 5f, SpriteEffects.None, 0.9f);
                batch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(-130f, -50f)), new Rectangle(149, 465, 5, 24), Color.White * 0.8f, -1.57f, Vector2.Zero, 5f, SpriteEffects.None, 0.9f);
                batch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(-130f, -25f)), new Rectangle(141, 465, 5, 20), Color.White * 0.8f, -1.57f, Vector2.Zero, 5f, SpriteEffects.None, 0.9f);
                //stars
                if (fishQuality > 0)
                {
                    Rectangle quality_rect = (fishQuality < 4) ? new Rectangle(338 + (fishQuality - 1) * 8, 400, 8, 8) : new Rectangle(346, 392, 8, 8);
                    batch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(-104f, -48f)), quality_rect, Color.White, 0f, new Vector2(4f, 4f), 2f, SpriteEffects.None, 1f);
                    batch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(-56f, -48f)), quality_rect, Color.White, 0f, new Vector2(4f, 4f), 2f, SpriteEffects.None, 1f);
                }
                //strings
                float offset = 0f;
                if (caughtDoubleFish || fishQuality > 0) offset = 13f;
                if (caughtDoubleFish)
                {
                    batch.DrawString(Game1.smallFont, "x2", Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(-91f, -60f)), Game1.textColor, 0f, Vector2.Zero, 0.8f, SpriteEffects.None, 1f);
                }
                string sizeString = Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingRod.cs.14083", (metricSizes || LocalizedContentManager.CurrentLanguageCode != 0) ? Math.Round(fishSize * 2.54f) : (fishSize));
                batch.DrawString(Game1.smallFont, sizeString, Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(-80f - Game1.smallFont.MeasureString(sizeString).X / 2f, -77f - offset)), recordSize ? Color.Blue : Game1.textColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
            }
            //fishing net
            batch.Draw(Game1.toolSpriteSheet, Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(+22f, +17f)), Game1.getSourceRectForStandardTileSheet(Game1.toolSpriteSheet, who.CurrentTool.IndexOfMenuItemView, 16, 16), Color.White, -3f, new Vector2(8f, 8f), 4f, SpriteEffects.None, 0.9f);

            //item(s) in hand
            if (!(item is Furniture))
            {
                batch.Draw(Game1.objectSpriteSheet, Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(12f, -44f)), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, whichFish, 16, 16), Color.White, (fishSize == -1 || whichFish == 800 || whichFish == 798 || whichFish == 149 || whichFish == 151) ? 0.5f : (caughtDoubleFish) ? 2.2f : 2.4f, new Vector2(8f, 8f), itemSpriteSize, SpriteEffects.None, 1f);

                if (caughtDoubleFish) batch.Draw(Game1.objectSpriteSheet, Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(4f, -44f)), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, whichFish, 16, 16), Color.White, (fishSize == -1 || whichFish == 800 || whichFish == 798 || whichFish == 149 || whichFish == 151) ? 1f : 2.6f, new Vector2(8f, 8f), itemSpriteSize, SpriteEffects.None, 1f);
            }
            else batch.Draw(Furniture.furnitureTexture, Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(0f, -56f)), (item as Furniture).defaultSourceRect, Color.White, 0f, new Vector2(8f, 8f), 2.25f, SpriteEffects.None, 1f);
        }

        private async void FestivalGameSkip(Farmer who, ButtonsChangedEventArgs button)
        {
            if (!hereFishying)
            {
                hereFishying = true;
                int direction = who.FacingDirection;
                Game1.freezeControls = true;

                if (fishySound != null) fishySound.Play(voiceVolume, voicePitch, 0);
                who.synchronizedJump(8f);
                await Task.Delay(Game1.random.Next(2000, 3000));

                Event ev = Game1.CurrentEvent;
                ev.caughtFish(137, Game1.random.Next(0, 20), who);
                if (Game1.random.Next(0, 5) == 0) ev.perfectFishing();

                await Task.Delay(Game1.random.Next(2000, 4000));
                who.synchronizedJump(6f);
                for (int i = 0; i < 4; i++)
                {
                    if (direction == 3) direction = 0;
                    else direction++;
                    who.faceDirection(direction);
                    await Task.Delay(200);
                }
                hereFishying = false;

                Game1.freezeControls = false;
                //if (fishingFestivalMinigame.Value == 1 && keyBinds.JustPressed()) FestivalGameSkip(who);
            }
            return;
        }

        private async Task EmergencyCancel(Farmer who)
        {
            endMinigameStage = 5;
            startMinigameStage = 0;
            animations.Clear();
            await Task.Delay(1000);
            who.UsingTool = false;
            who.Halt();
            who.completelyStopAnimatingOrDoingAction();
            hereFishying = false;
            Game1.freezeControls = false;
        }



        /// <summary>Get whether this instance can edit the given asset.</summary>
        public bool CanEdit<T>(IAssetInfo asset)
        {
            if (asset.AssetNameEquals("TileSheets/tools") || asset.AssetNameEquals("Strings/StringsFromCSFiles")) return true;
            return false;
        }
        /// <summary>Edits the asset if CanEdit</summary>
        public void Edit<T>(IAssetData asset)
        {
            if (asset.AssetNameEquals("TileSheets/tools"))
            {
                var editor = asset.AsImage();

                Texture2D sourceImage = Helper.Content.Load<Texture2D>("assets/rod_sprites.png", ContentSource.ModFolder);
                editor.PatchImage(sourceImage, targetArea: new Rectangle(128, 0, 64, 16));
                sourceImage = Helper.Content.Load<Texture2D>("assets/rod_farmer.png", ContentSource.ModFolder);
                editor.PatchImage(sourceImage, targetArea: new Rectangle(0, 289, 295, 95));
                sourceImage.Dispose();
            }
            else
            {
                translate = Helper.Translation;
                IDictionary<string, string> data = asset.AsDictionary<string, string>().Data;
                foreach (string itemID in data.Keys.ToArray())
                {
                    switch (itemID)
                    {
                        case "FishingRod.cs.14041":
                            data[itemID] = translate.Get("net.fishing");
                            break;
                        case "FishingRod.cs.trainingRodDescription":
                            data[itemID] = translate.Get("net.trainingDesc");
                            break;
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
                        default:
                            break;
                    }
                }
            }
        }


        private void UpdateConfig()
        {
            config = Helper.ReadConfig<ModConfig>();
            try
            {
                fishySound = SoundEffect.FromStream(new FileStream(Path.Combine(Helper.DirectoryPath, "assets", "fishy.wav"), FileMode.Open));
            }
            catch (Exception ex)
            {
                Monitor.Log($"error loading fishy.wav: {ex}", LogLevel.Error);
            }
            if (!config.Voice_Test_Ignore_Me.Equals(config.VoiceVolume + "/" + config.VoicePitch, StringComparison.Ordinal)) //play voice and save it if changed
            {
                config.Voice_Test_Ignore_Me = config.VoiceVolume + "/" + config.VoicePitch;
                fishySound.Play(config.VoiceVolume / 100f, config.VoicePitch / 100f, 0f);
                Helper.WriteConfig(config);
            }

            try //keybinds
            {
                if (config.KeyBinds.Equals("") || config.KeyBinds.Equals(" ")) throw new FormatException("String can't be empty.");
                keyBinds = KeybindList.Parse(config.KeyBinds);
            }
            catch (Exception e)
            {
                string def = "MouseLeft, Space, ControllerX";
                keyBinds = KeybindList.Parse(def);
                config.KeyBinds = def;
                Helper.WriteConfig(config);
                Monitor.Log(e.Message + " Resetting KeyBinds to default. For key names, see: https://stardewcommunitywiki.com/Modding:Player_Guide/Key_Bindings", LogLevel.Error);
            }

            if (Context.IsWorldReady)
            {
                voiceVolume = config.VoiceVolume / 100f;
                voicePitch = config.VoicePitch / 100f;
                startMinigameStyle = config.StartMinigameStyle;
                endMinigameStyle = config.EndMinigameStyle;
                minigameDamage = config.EndMinigameDamage;
                minigameDifficulty = config.MinigameDifficulty;
                realisticSizes = config.RealisticSizes;
                if (LocalizedContentManager.CurrentLanguageCode == 0) metricSizes = config.ConvertToMetric;
                else metricSizes = false;
                Helper.Content.InvalidateCache("Strings/StringsFromCSFiles");
                festivalMode = config.FestivalMode;
            }
        }
    }
}
