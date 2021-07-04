using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
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
        public static ModEntry context;
        ITranslationHelper translate;

        private static SoundEffect fishySound;
        private static List<TemporaryAnimatedSprite> animations = new List<TemporaryAnimatedSprite>();
        private static SparklingText sparklingText;
        private static bool caughtDoubleFish;
        private static Farmer who;
        private static int whichFish;
        private static int minFishSize;
        private static int maxFishSize;
        private static float fishSize;
        private static bool recordSize;
        private static bool perfect;
        private static int fishQuality;
        private static bool fishCaught;
        private static bool bossFish;
        private static int difficulty;
        private static bool treasureCaught;
        private static bool canPerfect;
        private static bool fromFishPond;
        private static int clearWaterDistance;
        private static Object item;

        private static SButton rodButton1;
        private static SButton rodButton2;

        private static bool hereFishying;
        private static bool itemIsInstantCatch;
        private static int oldFacingDirection;
        private static int oldGameTimeInterval;
        private static int fishingFestivalMinigame;//0=none, 1=fall16, 2=winter8

        private static int startMinigameStage;
        private static int endMinigameStage;
        private static int endMinigameTimer;
        private static bool endMinigameAnimate;

        //config values
        public static ModConfig config;
        private static bool overrideToolUse;
        private static float minigameDamage;
        private static int startMinigameStyle;
        private static int endMinigameStyle;
        private static int festivalMode;
        private static int minigameDifficulty;//?
        private static float voiceVolume;
        private static float voicePitch;




        /*  Stop other player interaction while 'fishing'
         *  fix desert
         *  instead of where clicked, soundwave anim ahead? would be hard to aim at pools, could use swing effect anim?
         */


        public override void Entry(IModHelper helper)
        {
            context = this;
            UpdateConfig();

            helper.Events.Display.RenderedWorld += Display_RenderedWorld;
            helper.Events.GameLoop.UpdateTicking += GameLoop_UpdateTicking;
            helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            helper.Events.GameLoop.GameLaunched += GenericModConfigMenuIntegration;
        }


        private void GenericModConfigMenuIntegration(object sender, GameLaunchedEventArgs e)     //Generic Mod Config Menu API
        {
            translate = context.Helper.Translation;
            var GenericMC = Helper.ModRegistry.GetApi<GenericModConfigMenuAPI>("spacechase0.GenericModConfigMenu");
            if (GenericMC != null)
            {
                GenericMC.RegisterModConfig(ModManifest, () => config = new ModConfig(), () => Helper.WriteConfig(config));
                GenericMC.SetDefaultIngameOptinValue(ModManifest, true);
                GenericMC.RegisterLabel(ModManifest, translate.Get("GenericMC.MainLabel"), ""); //All of these strings are stored in the traslation files.
                GenericMC.RegisterParagraph(ModManifest, translate.Get("GenericMC.MainDesc"));
                GenericMC.RegisterParagraph(ModManifest, translate.Get("GenericMC.MainDesc2") + "\n");
                GenericMC.RegisterClampedOption(ModManifest, translate.Get("GenericMC.Volume"), translate.Get("GenericMC.VolumeDesc"),
                    () => config.VoiceVolume, (int val) => config.VoiceVolume = val, 0, 100);
                GenericMC.RegisterClampedOption(ModManifest, translate.Get("GenericMC.Pitch"), translate.Get("GenericMC.PitchDesc"),
                    () => config.VoicePitch, (int val) => config.VoicePitch = val, -100, 100);
                GenericMC.RegisterSimpleOption(ModManifest, translate.Get("GenericMC.OverrideToolButton"), translate.Get("GenericMC.OverrideToolButtonDesc"),
                    () => config.OverrideToolUseButton, (bool val) => config.OverrideToolUseButton = val);

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
                    () => config.MinigameDifficulty, (int val) => config.MinigameDifficulty = val, -5, 5);

                GenericMC.RegisterLabel(ModManifest, "", "");
                GenericMC.RegisterLabel(ModManifest, translate.Get("GenericMC.FestivalLabel"), "");
                GenericMC.RegisterParagraph(ModManifest, translate.Get("GenericMC.FestivalDesc"));
                GenericMC.RegisterChoiceOption(ModManifest, translate.Get("GenericMC.FestivalMode"), "",
                    () => (config.FestivalMode == 0) ? translate.Get("GenericMC.FestivalModeVanilla") : (config.FestivalMode == 1) ? translate.Get("GenericMC.FestivalModeSimple") : translate.Get("GenericMC.FestivalModePerfectOnly"),
                    (string val) => config.FestivalMode = Int32.Parse((val.Equals(translate.Get("GenericMC.FestivalModeVanilla"), StringComparison.Ordinal)) ? "0" : (val.Equals(translate.Get("GenericMC.FestivalModeSimple"), StringComparison.Ordinal)) ? "1" : "2"),
                    new string[] { translate.Get("GenericMC.FestivalModeVanilla"), translate.Get("GenericMC.FestivalModeSimple"), translate.Get("GenericMC.FestivalModePerfectOnly") });
            }
        }


        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)  //this.Monitor.Log(locationName, LogLevel.Debug);
        {
            if (!Context.IsWorldReady || Game1.menuUp) return;

            who = Game1.player;
            if (e.Button == SButton.F5)
            {
                UpdateConfig();
            }

            if (endMinigameStage == 2)
            {
                Game1.freezeControls = true;
                if (endMinigameStyle == 1 && (e.Button == rodButton1 || e.Button == rodButton2))
                {
                    if (endMinigameStage == 2 && endMinigameTimer < 20) endMinigameStage = 10;
                    else endMinigameStage = 9;

                    animations.Clear();
                    PlayerCaughtFishEndFunction(whichFish);
                }
            }
            else
            {
                if (Context.IsWorldReady && Context.CanPlayerMove && who.CurrentTool is FishingRod && (e.Button == rodButton1 || e.Button == rodButton2))
                {
                    if (Game1.isFestival() && (fishingFestivalMinigame == 0 || festivalMode == 0)) return;
                    if (fishingFestivalMinigame == 1)
                    {
                        FestivalGameSkip(who, e);
                        return;
                    }

                    if (!hereFishying)
                    {
                        who.freezePause = 1; //stops fishing rod
                        try
                        {
                            perfect = false;
                            Vector2 mouse = Game1.currentCursorTile;
                            oldFacingDirection = who.getGeneralDirectionTowards(new Vector2(mouse.X * 64, mouse.Y * 64));
                            who.faceDirection(oldFacingDirection);


                            if (who.currentLocation.canFishHere() && who.currentLocation.isTileFishable((int)mouse.X, (int)mouse.Y))
                            {
                                context.Monitor.Log($"here fishy fishy {mouse.X},{mouse.Y}");
                                HereFishyFishy(who, (int)mouse.X * 64, (int)mouse.Y * 64);
                            }
                        }
                        catch
                        {
                            context.Monitor.Log($"error getting water tile", LogLevel.Error);
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
                string data = context.Helper.Reflection.GetField<Dictionary<string, string>>(Game1.CurrentEvent, "festivalData").GetValue()["file"];
                if (data != null)
                {
                    who = Game1.player;
                    int timer = 0;
                    if (data.Equals("fall16") && Game1.currentMinigame is StardewValley.Minigames.FishingGame)
                    {
                        timer = context.Helper.Reflection.GetField<int>(Game1.currentMinigame as StardewValley.Minigames.FishingGame, "gameEndTimer").GetValue();
                        if (timer < 100000 && timer >= 500) fishingFestivalMinigame = 1;
                    }
                    else if (data.Equals("winter8"))
                    {
                        timer = Game1.CurrentEvent.festivalTimer;
                        if (timer < 120000 && timer >= 500) fishingFestivalMinigame = 2;
                    }
                    if (timer <= 500 && fishingFestivalMinigame > 0 && festivalMode > 0) _ = EmergencyCancel(who);
                }
                context.Monitor.Log(fishingFestivalMinigame.ToString(), LogLevel.Debug);
            }

            for (int i = animations.Count - 1; i >= 0; i--)
            {
                if (endMinigameStage != 2 && animations[i].update(Game1.currentGameTime))
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
                who.faceDirection(oldFacingDirection);
                fishCaught = false;
            }
        }

        private void Display_RenderedWorld(object sender, RenderedWorldEventArgs e)
        {
            who = Game1.player;
            if (!Game1.eventUp && !Game1.menuUp && !hereFishying && who.CurrentItem is FishingRod)
            {
                Vector2 mouse = new Vector2((Game1.getMouseX()) / 64, (Game1.getMouseY()) / 64);
                Texture2D tile = Game1.content.Load<Texture2D>("LooseSprites\\buildingPlacementTiles");
                e.SpriteBatch.Draw(tile, mouse * 64, Game1.getSourceRectForStandardTileSheet(tile, (who.currentLocation.isTileFishable((int)Game1.currentCursorTile.X, (int)Game1.currentCursorTile.Y)) ? 0 : 1), new Color(255, 255, 255, 0.5f), 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
            }
            for (int i = animations.Count - 1; i >= 0; i--)
            {
                animations[i].draw(e.SpriteBatch, false, 0, 0, 1f);
                if (endMinigameStage > 0 && i == 0)
                {
                    if (endMinigameStage == 1)
                    {
                        Rectangle area = new Rectangle((int)who.Position.X - 70, (int)who.Position.Y - 185, 140, 205);
                        if (area.Contains((int)animations[0].Position.X, (int)animations[0].Position.Y))
                        {
                            endMinigameStage = 2;
                            endMinigameTimer = 0;
                            who.PlayFishBiteChime();
                            Game1.screenOverlayTempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(395, 497, 3, 8), new Vector2(who.getStandingX() - Game1.viewport.X, who.getStandingY() - 128 - 8 - Game1.viewport.Y), flipped: false, 0.02f, Color.White)
                            {
                                scale = 5f,
                                scaleChange = -0.01f,
                                motion = new Vector2(0f, -0.5f),
                                shakeIntensityChange = -0.005f,
                                shakeIntensity = 1f
                            });
                        }
                    }
                    else if (endMinigameStage == 2)
                    {
                        endMinigameTimer++;

                        if (endMinigameTimer > 100 - (difficulty / 2))
                        {
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

            if (canPerfect)
            {
                perfect = true;
                sparklingText = new SparklingText(Game1.dialogueFont, Game1.content.LoadString("Strings\\UI:BobberBar_Perfect"), Color.Yellow, Color.White, false, 0.1, 1500, -1, 500, 1f);
                Game1.playSound("jingle1");
                canPerfect = false;
            }

            if (sparklingText != null && who != null && !itemIsInstantCatch)
            {
                sparklingText.draw(e.SpriteBatch, Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(-64f, -400f)));
            }
        }


        private async static void HereFishyFishy(Farmer who, int x, int y)
        {
            if (fishingFestivalMinigame == 2) await FestivalCancelRod(who);
            else if (who.IsLocalPlayer)
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
                AnnounceAndEmote(who);
                return;
            }
            else await HereFishyStartingAnimation(who);


            if (!fromFishPond && endMinigameStyle > 0) endMinigameStage = 1;

            await HereFishyFlyingAnimation(who, x, y);
        }


        private static void CatchFish(Farmer who, int x, int y)
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

            fishSize = 1f;
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
                if (!int.TryParse(fishData[1], out difficulty) && !int.TryParse(fishData[3], out minFishSize) && !int.TryParse(fishData[4], out maxFishSize)) itemIsInstantCatch = true;
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
            fishSize *= Math.Min((float)clearWaterDistance / 5f, 1f);
            int minimumSizeContribution = 1 + who.FishingLevel / 2;
            fishSize *= (float)Game1.random.Next(minimumSizeContribution, Math.Max(6, minimumSizeContribution)) / 5f;

            if (rod.getBaitAttachmentIndex() != -1) fishSize *= 1.2f;
            fishSize *= 1f + (float)Game1.random.Next(-10, 11) / 100f;
            fishSize = Math.Max(0f, Math.Min(1f, fishSize));


            fishSize = (int)((float)minFishSize + (float)(maxFishSize - minFishSize) * fishSize);
            fishSize++;

            if (rod.Name.Equals("Training Rod", StringComparison.Ordinal)) fishSize = minFishSize;


            bossFish = FishingRod.isFishBossFish(whichFish);
        }

        private async static Task CatchFishAfterMinigame(Farmer who)
        {
            if (!fromFishPond && endMinigameStyle > 0 && endMinigameStage > 8)
            {
                if (endMinigameStage == 10) canPerfect = true;
                who.completelyStopAnimatingOrDoingAction();
                endMinigameAnimate = true;
                (who.CurrentTool as FishingRod).setTimingCastAnimation(who);
                switch (oldFacingDirection)
                {
                    case 0:
                        who.FarmerSprite.animateOnce(295, 1f, 1);
                        who.CurrentTool.Update(0, 0, who);
                        break;
                    case 1:
                        who.FarmerSprite.animateOnce(296, 1f, 1);
                        who.CurrentTool.Update(1, 0, who);
                        break;
                    case 2:
                        who.FarmerSprite.animateOnce(297, 1f, 1);
                        who.CurrentTool.Update(2, 0, who);
                        break;
                    case 3:
                        who.FarmerSprite.animateOnce(298, 1f, 1);
                        who.CurrentTool.Update(3, 0, who);
                        break;
                }
                await Task.Delay(300);
                endMinigameAnimate = false;
            }



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

        //fish flying from xy to player
        private async static Task HereFishyStartingAnimation(Farmer who)
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
                canPerfect = true;
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
        private async static Task HereFishyFlyingAnimation(Farmer who, int x, int y)
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
            animations.Add(new TemporaryAnimatedSprite((item is Furniture) ? Furniture.furnitureTexture.ToString() : "Maps\\springobjects", (item is Furniture) ? (item as Furniture).defaultSourceRect : Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, whichFish, 16, 16), t, 1, 0, new Vector2(x, y), false, false, y / 10000f, 0f, Color.White, (item is Furniture) ? 2f : 4f, 0f, 0f, 0f, false)
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
                animations.Add(new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, whichFish, 16, 16), t, 1, 0, new Vector2(x, y), false, false, y / 10000f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
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

        public async static void PlayerCaughtFishEndFunction(int extraData)
        {
            if (fishingFestivalMinigame == 2) await FestivalCancelRod(who);

            await CatchFishAfterMinigame(who);

            if (!fromFishPond)
            {
                if (endMinigameStage > 8) await Task.Delay(300);
                else if (endMinigameStage == 8)
                {
                    animations.Add(new TemporaryAnimatedSprite(10, who.Position - new Vector2(0, 100), Color.Blue));
                    await Task.Delay(100);
                }

                recordSize = who.caughtFish(whichFish, (int)fishSize, false, caughtDoubleFish ? 2 : 1);
                if (FishingRod.isFishBossFish(whichFish))
                {
                    Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingRod.cs.14068"));
                    string name = Game1.objectInformation[whichFish].Split('/')[4];

                    //context.Helper.Reflection.GetField<Multiplayer>(Game1.game1, "multiplayer").GetValue().globalChatInfoMessage("CaughtLegendaryFish", new string[] { who.Name, name }); //multiplayer class is not protected
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

                AnnounceAndEmote(who);
            }

            context.Monitor.Log($"caught fish end");
            who.Halt();
            who.armOffset = Vector2.Zero;


            if (fishingFestivalMinigame == 2)
            {
                if (!itemIsInstantCatch)
                {
                    if (festivalMode == 1 || endMinigameStage == 10) Game1.CurrentEvent.caughtFish(whichFish, 10, who);
                }
            }
            else//adding items + chest
            {
                FishingRod rod = who.CurrentTool as FishingRod;
                context.Helper.Reflection.GetField<Farmer>(rod, "lastUser").SetValue(who);
                context.Helper.Reflection.GetField<int>(rod, "whichFish").SetValue(whichFish);
                context.Helper.Reflection.GetField<bool>(rod, "caughtDoubleFish").SetValue(caughtDoubleFish);
                context.Helper.Reflection.GetField<int>(rod, "fishQuality").SetValue(fishQuality);
                context.Helper.Reflection.GetField<int>(rod, "clearWaterDistance").SetValue(clearWaterDistance);
                context.Helper.Reflection.GetField<Farmer>(who.CurrentTool, "lastUser").SetValue(who);

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


        private static void AnnounceAndEmote(Farmer who) //send messages to other mods to do stuff: sounds/animations
        {
            if (startMinigameStyle + endMinigameStyle > 0)
            {
                if (startMinigameStage == 4) //for now 4 = cancel = X
                {
                    who.doEmote(36);
                }
                else if (startMinigameStage == 5) //for now 5 = fail = Angry
                {
                    who.doEmote(12);
                }
                else if (endMinigameStage == 8) //8 = hit = Uh
                {
                    who.doEmote(10);
                }
                else //otherwise = happy
                {
                    who.doEmote(32);
                }
            }
        }


        private async static void FestivalGameSkip(Farmer who, ButtonPressedEventArgs button)
        {
            int direction = who.FacingDirection;
            Game1.freezeControls = true;
            await FestivalCancelRod(who);

            if (!hereFishying)
            {
                hereFishying = true;
                if (fishySound != null) fishySound.Play(voiceVolume, voicePitch, 0);
                who.synchronizedJump(8f);
                await Task.Delay(Game1.random.Next(2000, 3000));

                Event ev = Game1.CurrentEvent;
                //if (context.Helper.Reflection.GetField<Dictionary<string, string>>(ev, "festivalData").GetValue()["file"].Equals("fall16"))
                //{
                ev.caughtFish(137, Game1.random.Next(0, 20), who);
                if (Game1.random.Next(0, 5) == 0) ev.perfectFishing();
                //}
                //else ev.caughtFish(147, Game1.random.Next(0, 20), who);

                await Task.Delay(Game1.random.Next(2500, 4500));
                hereFishying = false;
            }
            who.FacingDirection = direction;
            Game1.freezeControls = false;

            if ((button.IsDown(rodButton1) || button.IsDown(rodButton2)) && fishingFestivalMinigame == 1) FestivalGameSkip(who, button);
            return;
        }

        private async static Task FestivalCancelRod(Farmer who)
        {
            FishingRod rod = (who.CurrentTool as FishingRod);
            for (int i = 0; i < 25; i++)
            {
                if (fishingFestivalMinigame == 0) return;
                rod.castingEndFunction(-1);
                rod.doneFishing(who);
                rod.resetState();
                who.UsingTool = false;
                who.Halt();
                who.forceCanMove();
                who.FacingDirection = oldFacingDirection;
                await Task.Delay(2);
            }
        }
        private async static Task EmergencyCancel(Farmer who)
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

                Texture2D sourceImage = context.Helper.Content.Load<Texture2D>("assets/rod_sprites.png", ContentSource.ModFolder);
                editor.PatchImage(sourceImage, targetArea: new Rectangle(128, 0, 64, 16));
                sourceImage = context.Helper.Content.Load<Texture2D>("assets/rod_farmer.png", ContentSource.ModFolder);
                editor.PatchImage(sourceImage, targetArea: new Rectangle(0, 289, 295, 95));
                sourceImage.Dispose();
            }
            else
            {
                translate = context.Helper.Translation;
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
                        default:
                            break;
                    }
                }
            }
        }


        private static void UpdateConfig()
        {
            config = context.Helper.ReadConfig<ModConfig>();
            voiceVolume = config.VoiceVolume / 100f;
            voicePitch = config.VoicePitch / 100f;
            overrideToolUse = config.OverrideToolUseButton;
            startMinigameStyle = config.StartMinigameStyle;
            endMinigameStyle = config.EndMinigameStyle;
            minigameDamage = config.EndMinigameDamage;
            minigameDifficulty = config.MinigameDifficulty;
            festivalMode = config.FestivalMode;

            try
            {
                fishySound = SoundEffect.FromStream(new FileStream(Path.Combine(context.Helper.DirectoryPath, "assets", "fishy.wav"), FileMode.Open));
            }
            catch (Exception ex)
            {
                context.Monitor.Log($"error loading fishy.wav: {ex}", LogLevel.Error);
            }


            if (!config.Voice_Test_Ignore_Me.Equals(config.VoiceVolume + "/" + config.VoicePitch, StringComparison.Ordinal))
            { //play voice and save it if changed
                config.Voice_Test_Ignore_Me = config.VoiceVolume + "/" + config.VoicePitch;
                fishySound.Play(voiceVolume, voicePitch, 0f);
                context.Helper.WriteConfig(config);
            }

            try
            {
                if (overrideToolUse)
                {
                    rodButton1 = Game1.options.useToolButton[0].ToSButton();
                    rodButton2 = (Game1.options.useToolButton.Length > 1) ? Game1.options.useToolButton[1].ToSButton() : SButton.None;
                }
                else
                {
                    rodButton1 = Game1.options.actionButton[0].ToSButton();
                    rodButton2 = (Game1.options.actionButton.Length > 1) ? Game1.options.actionButton[1].ToSButton() : SButton.None;
                }
            }
            catch (Exception)
            {
                context.Monitor.Log("There's no assigned " + ((overrideToolUse) ? "Tool Use" : "Action") + " button!", LogLevel.Error);
            }
        }
    }
}
