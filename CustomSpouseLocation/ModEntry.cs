using CustomSpouseLocation;
using GenericModConfigMenu;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Monsters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace StardewMods
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod, IKeyboardSubscriber
    {
        ITranslationHelper translate;
        private ModConfig config;

        private DictionaryEditor state;
        private bool resized = true;
        private bool SelectedImpl;



        public override void Entry(IModHelper helper)
        {
            config = helper.ReadConfig<ModConfig>();
            translate = helper.Translation;

            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            helper.Events.GameLoop.DayStarted += this.OnDayStarted;
            helper.Events.Display.WindowResized += this.OnResized;
            helper.Events.GameLoop.GameLaunched += this.GenericModConfigMenuIntegration;
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
                GenericMC.RegisterLabel(ModManifest, translate.Get("GenericMC.Label"), ""); //All of these strings are stored in the traslation files.
                GenericMC.RegisterParagraph(ModManifest, translate.Get("GenericMC.Description"));

                try
                {
                    GenericMC.RegisterSimpleOption(ModManifest, translate.Get("GenericMC.SpritePreviewMode"), translate.Get("GenericMC.SpritePreviewModeDesc"),
                        () => config.SpritePreviewMode, (bool val) => config.SpritePreviewMode = val);

                    GenericMC.RegisterPageLabel(ModManifest, translate.Get("GenericMC.SpouseRoom"), "", translate.Get("GenericMC.SpouseRoom"));
                    GenericMC.RegisterPageLabel(ModManifest, translate.Get("GenericMC.Kitchen"), "", translate.Get("GenericMC.Kitchen"));
                    GenericMC.RegisterPageLabel(ModManifest, translate.Get("GenericMC.Patio"), "", translate.Get("GenericMC.Patio"));
                    GenericMC.RegisterPageLabel(ModManifest, translate.Get("GenericMC.Porch"), "", translate.Get("GenericMC.Porch"));

                    //spouse room
                    GenericMC.StartNewPage(ModManifest, translate.Get("GenericMC.SpouseRoom"));

                    GenericMC.RegisterClampedOption(ModManifest, translate.Get("GenericMC.RandomTileChance"), translate.Get("GenericMC.RandomTileChanceDesc"),
                        () => config.SpouseRoom_RandomTileChance, (float val) => config.SpouseRoom_RandomTileChance = (int)val, 0f, 100f);
                    GenericMC.RegisterSimpleOption(ModManifest, translate.Get("GenericMC.FurnitureChairs"), translate.Get("GenericMC.FurnitureChairsDesc"),
                        () => config.SpouseRoom_RandomCanUse_FurnitureChairs_UpOnly, (bool val) => config.SpouseRoom_RandomCanUse_FurnitureChairs_UpOnly = val);
                    GenericMC.RegisterSimpleOption(ModManifest, translate.Get("GenericMC.MapChairs"), translate.Get("GenericMC.MapChairsDesc"),
                        () => config.SpouseRoom_RandomCanUse_MapChairs_DownOnly, (bool val) => config.SpouseRoom_RandomCanUse_MapChairs_DownOnly = val);
                    //spouse room auto tile config

                    //spouse room manual config

                    //kitchen config
                    GenericMC.StartNewPage(ModManifest, translate.Get("GenericMC.Kitchen"));
                    GenericMCDictionaryEditor(GenericMC, ModManifest, translate.Get("GenericMC.Kitchen"), "", 2);


                    //patio config
                    GenericMC.StartNewPage(ModManifest, translate.Get("GenericMC.Patio"));

                    //porch config
                    GenericMC.StartNewPage(ModManifest, translate.Get("GenericMC.Porch"));





                    //dummy value validation trigger - must be the last thing, so all values are saved before validation
                    GenericMC.RegisterComplexOption(ModManifest, "", "", (Vector2 pos, object state_) => null, (SpriteBatch b, Vector2 pos, object state_) => null, (object state) => UpdateConfig(true));
                }
                catch (Exception)
                {
                    this.Monitor.Log("Error parsing config data. Please either fix your config.json, or delete it to generate a new one.", LogLevel.Error);
                }
            }
        }

        public bool Selected
        {
            get => this.SelectedImpl;
            set
            {
                if (this.SelectedImpl == value)
                    return;

                this.SelectedImpl = value;
                if (this.SelectedImpl)
                    Game1.keyboardDispatcher.Subscriber = this;
                else
                {
                    if (Game1.keyboardDispatcher.Subscriber == this)
                        Game1.keyboardDispatcher.Subscriber = null;
                }
            }
        }
        protected virtual void ReceiveInput(string str)
        {
            if (state?.dataEditing != null) state.dataStrings[state.dataEditing] += str;
        }

        public void RecieveCommandInput(char command)
        {
            if (command == '\b' && state?.dataStrings[state.dataEditing].Length > 1)
            {
                Game1.playSound("tinyWhip");
                state.dataStrings[state.dataEditing] = state.dataStrings[state.dataEditing].Substring(0, state.dataStrings[state.dataEditing].Length - 1);
            }
            else if (command == '\b' && state?.dataStrings[state.dataEditing].Length < 2)
            {
                Game1.playSound("tinyWhip");
                state.dataStrings[state.dataEditing] = "";
            }
        }

        public void RecieveSpecialInput(Keys key)
        {
            //?
        }

        public void RecieveTextInput(char inputChar)
        {
            ReceiveInput(inputChar.ToString());
        }

        public void RecieveTextInput(string text)
        {
            ReceiveInput(text);
        }

        public void GenericMCDictionaryEditor(IGenericModConfigMenuApi GenericMC, IManifest mod, string optionName, string optionDesc, int which)
        {
            Func<Vector2, object, object> editorUpdate =
                (Vector2 pos, object state_) =>
                {
                    KeybindList reset = KeybindList.Parse("F5");

                    state = state_ as DictionaryEditor;
                    if (state == null || reset.JustPressed())
                    {
                        switch (which)
                        {
                            case 0:
                                //state = new DictionaryEditor(config.SpouseRoomRandomFaceTileOffset, pos);//todo - convert this to match the others?
                                break;
                            case 1:
                                state = new DictionaryEditor(config.SpouseRoom_ManualTileOffsets, pos);
                                break;
                            case 2:
                                state = new DictionaryEditor(config.Kitchen_TileOffsets, pos);
                                break;
                            case 3:
                                state = new DictionaryEditor(config.Patio_TileOffsets, pos);
                                break;
                            case 4:
                                state = new DictionaryEditor(config.Porch_TileOffsets, pos);
                                break;
                        }
                        resized = true;
                    }

                    KeybindList click = KeybindList.Parse("MouseLeft");
                    KeybindList delete = KeybindList.Parse("LeftControl + X");


                    if (state.boundsLeftRight.Contains(Game1.getMouseX(), Game1.getMouseY()))
                    {
                        if (state.scrollState != Game1.input.GetMouseState().ScrollWheelValue)
                        {
                            int scroll = Game1.input.GetMouseState().ScrollWheelValue;
                            float line = Game1.smallFont.LineSpacing * 1.5f;

                            if (scroll > state.scrollState && state.scrollBar.Y + state.boundsTopBottom.Height < pos.Y + state.boundsTopBottom.Height) state.scrollBar.Y += line;
                            else if (scroll < state.scrollState && state.contentBottom + state.scrollBar.Y - state.boundsTopBottom.Height > pos.Y) state.scrollBar.Y -= line;

                            state.dataEditing = null;
                            state.scrollState = scroll;
                        }
                    }
                    else state.scrollState = Game1.input.GetMouseState().ScrollWheelValue;


                    if (click.JustPressed())
                    {
                        foreach (var button in state.hoverNames)
                        {
                            if (button.Value.Contains(Game1.getMouseX(), Game1.getMouseY()))
                            {
                                if (state.enabledNPCs.ContainsKey(button.Key))
                                {
                                    int numb = int.Parse(state.dataStrings.Keys.Where(val => val.StartsWith(button.Key)).OrderBy(val => int.Parse(val.Replace(button.Key, ""))).Last().Replace(button.Key, "")) + 1;
                                    state.enabledNPCs[button.Key].Add(button.Key + numb);
                                    state.dataStrings.Add(button.Key + numb, "Down / 0, 0");
                                    state.dataEditing = null;
                                    break;
                                }
                                else if (state.datableNPCs.ContainsKey(button.Key))
                                {
                                    state.enabledNPCs[button.Key] = new List<string>() { button.Key + 0 };
                                    state.dataStrings.Add(button.Key + 0, "Down / 0, 0");
                                    state.dataEditing = null;
                                    break;
                                }
                                else if (state.dataStrings.ContainsKey(button.Key))
                                {
                                    state.dataEditing = button.Key;
                                    break;
                                }
                                else if (button.Key.StartsWith("Arrow", StringComparison.Ordinal))
                                {
                                    if (button.Key.Equals("ArrowUp", StringComparison.Ordinal)) state.scrollBar.Y += (int)(Game1.smallFont.LineSpacing * 1.5f);
                                    else state.scrollBar.Y -= (int)(Game1.smallFont.LineSpacing * 1.5f);
                                    state.dataEditing = null;
                                }
                                else state.dataEditing = null;
                            }
                        }
                        Selected = state.dataEditing != null;
                    }
                    else if (delete.JustPressed())
                    {
                        state.dataEditing = null;
                        foreach (var button in state.hoverNames)
                        {
                            if (button.Value.Contains(Game1.getMouseX(), Game1.getMouseY()))
                            {
                                if (state.enabledNPCs.ContainsKey(button.Key))
                                {
                                    if (!button.Key.Equals("Default", StringComparison.Ordinal) && state.dataStrings.Keys.Where(val => val.StartsWith(button.Key, StringComparison.Ordinal)).Count() < 2)
                                    {
                                        state.enabledNPCs.Remove(button.Key);//delete name if only 1 entry + delete entries
                                        foreach (var entry in state.hoverNames)
                                        {
                                            if (entry.Key.StartsWith(button.Key, StringComparison.Ordinal)) state.dataStrings.Remove(entry.Key);
                                        }
                                        break;
                                    }
                                }
                                else if (state.dataStrings.ContainsKey(button.Key) && state.dataStrings.Keys.Where(val => val.StartsWith(state.digitRemover.Replace(button.Key, ""), StringComparison.Ordinal)).Count() > 1)
                                {
                                    state.dataStrings.Remove(button.Key);//otherwise delete selected entry
                                    break;
                                }
                            }
                        }
                    }


                    return state;
                };
            Func<SpriteBatch, Vector2, object, object> editorDraw =
                (SpriteBatch b, Vector2 pos, object state_) =>
                {
                    var state = state_ as DictionaryEditor;
                    if (resized)
                    {
                        state.scrollBar = pos;
                        int width = Math.Min(Game1.uiViewport.Width / 4, 400);
                        state.boundsTopBottom = new Rectangle(100, (int)pos.Y, width * 2, -300 + (int)(Math.Min(Game1.uiViewport.Height, 1300f) * 0.8f));
                        state.boundsLeftRight = new Rectangle((int)(-550 + pos.X), state.boundsTopBottom.Y, 1100, state.boundsTopBottom.Height);
                        resized = false;
                    }
                    Vector2 left = new Vector2(-100f - (state.boundsTopBottom.Width / 2f), 10f);
                    float lineH = Game1.smallFont.LineSpacing * 1.5f;

                    state.hoverNames = new Dictionary<string, Rectangle>();

                    foreach (var npc in state.enabledNPCs.OrderBy(val => val.Key))//npcs in config
                    {
                        Rectangle nameR = new Rectangle((int)(state.scrollBar + left).X, (int)(state.scrollBar + left).Y, state.boundsTopBottom.Width, (int)lineH);

                        NPC current = null;
                        if (state.datableNPCs.TryGetValue(npc.Key, out current) || (npc.Key.Equals("Default", StringComparison.Ordinal)))
                        {
                            if (current == null && Game1.player.getSpouse()?.isVillager() != null) current = Game1.player.getSpouse();
                            else if (current == null && state.otherNPCs.TryGetValue("Pam", out current)) ;
                        }

                        if (!state.boundsTopBottom.Contains(state.boundsTopBottom.Width, (int)(state.scrollBar.Y + left.Y))) left.Y += lineH; //out of bounds?
                        else
                        {
                            state.hoverNames[npc.Key] = nameR;

                            if (current != null) b.Draw(current.Sprite.Texture, state.scrollBar + left, new Rectangle(1, 2, 14, 16), Color.White, 0f, new Vector2(16f, 1f), 2f, SpriteEffects.None, 1f);
                            b.DrawString(Game1.smallFont, npc.Key, state.scrollBar + left, (nameR.Contains(Game1.getMouseX(), Game1.getMouseY())) ? Color.DarkSlateGray : Color.ForestGreen, 0f, Vector2.Zero, 1.2f, SpriteEffects.None, 1f);
                            left.Y += lineH;
                        }
                        foreach (var text in state.dataStrings)//npc's entries
                        {
                            if (text.Key.StartsWith(npc.Key, StringComparison.Ordinal))
                            {
                                if (!state.boundsTopBottom.Contains(state.boundsTopBottom.Width, (int)(state.scrollBar.Y + left.Y))) //out of bounds?
                                {
                                    left.Y += lineH;
                                    continue;
                                }
                                nameR = new Rectangle((int)(state.scrollBar + left).X, (int)(state.scrollBar + left).Y, state.boundsTopBottom.Width, (int)lineH);
                                state.hoverNames[text.Key] = nameR;
                                Color color = Color.Red;

                                int spriteIndex = IsValidSprite(state, text.Value);
                                if (spriteIndex != -1) { 

                                    if (current != null)
                                    {
                                        b.Draw(current.Sprite.Texture, state.scrollBar + left + new Vector2(50f, 0f), Game1.getSquareSourceRectForNonStandardTileSheet(current.Sprite.Texture, 16, 32, spriteIndex), Color.White, 0f, new Vector2(18f, 6f), 1.4f, SpriteEffects.None, 1f);
                                    }
                                    if (IsValidVector2(state, text.Value) != new Vector2(-9999f)) color = Color.ForestGreen;
                                }
                                if (nameR.Contains(Game1.getMouseX(), Game1.getMouseY()))
                                {
                                    if (color == Color.Red) color = Color.Maroon;
                                    else color = Color.DarkSlateGray;
                                }

                                b.DrawString(Game1.smallFont, text.Value, state.scrollBar + left + new Vector2(50f, 0f), color, 0f, Vector2.Zero, 1.2f, SpriteEffects.None, 1f);

                                if (Selected && text.Key.Equals(state.dataEditing, StringComparison.Ordinal) && DateTime.UtcNow.Millisecond % 1000 >= 500)
                                    b.Draw(Game1.staminaRect, new Rectangle((int)(state.scrollBar.X + left.X + 56f) + (int)(Game1.smallFont.MeasureString(text.Value).X * 1.2f), (int)(state.scrollBar.Y + left.Y), 4, 32), Color.Black);

                                left.Y += lineH;
                            }
                        }
                    }

                    foreach (var npc in state.datableNPCs.OrderBy(val => val.Key))//other datable npcs (also add a list of non datables below?
                    {
                        if (!state.enabledNPCs.ContainsKey(npc.Key))
                        {
                            if (!state.boundsTopBottom.Contains(state.boundsTopBottom.Width, (int)(state.scrollBar.Y + left.Y))) //out of bounds?
                            {
                                left.Y += lineH;
                                continue;
                            }
                            Rectangle nameR = new Rectangle((int)(state.scrollBar + left).X, (int)(state.scrollBar + left).Y, state.boundsTopBottom.Width, (int)lineH);
                            state.hoverNames[npc.Key] = nameR;

                            b.DrawString(Game1.smallFont, npc.Key, state.scrollBar + left, (nameR.Contains(Game1.getMouseX(), Game1.getMouseY())) ? Color.Maroon : Color.Red, 0f, Vector2.Zero, 1.2f, SpriteEffects.None, 1f);
                            left.Y += lineH;
                        }
                    }
                    state.contentBottom = (int)left.Y;

                    //ui
                    b.Draw(Game1.staminaRect, new Rectangle((int)pos.X, (int)pos.Y, (int)(state.boundsTopBottom.Width * 1.4f), 1), null, Color.Black, 0f, new Vector2(0.5f), SpriteEffects.None, 1f);
                    b.Draw(Game1.staminaRect, new Rectangle((int)pos.X, (int)(pos.Y + state.boundsTopBottom.Height + Game1.smallFont.LineSpacing * 1.2f), (int)(state.boundsTopBottom.Width * 1.4f), 1), null, Color.Black, 0f, new Vector2(0.5f), SpriteEffects.None, 1f);
                    //arrows

                    if (state.scrollBar.Y + state.boundsTopBottom.Height < pos.Y + state.boundsTopBottom.Height)
                    {
                        Rectangle arrow = new Rectangle((int)(pos.X + state.boundsTopBottom.Width / 2f + 100f), (int)pos.Y, 32, 32);
                        state.hoverNames["ArrowUp"] = arrow;
                        b.Draw(Game1.mouseCursors, arrow, new Rectangle(421, 459, 12, 12), Color.White);
                    }
                    if (state.contentBottom + state.scrollBar.Y - state.boundsTopBottom.Height > pos.Y)
                    {
                        Rectangle arrow = new Rectangle((int)(pos.X + state.boundsTopBottom.Width / 2f + 100f), (int)pos.Y + state.boundsTopBottom.Height, 32, 32);
                        state.hoverNames["ArrowDown"] = arrow;
                        b.Draw(Game1.mouseCursors, arrow, new Rectangle(421, 472, 12, 12), Color.White);
                    }


                    return state;
                };
            Action<object> editorSave =
                (object state_) =>
                {
                    if (state_ == null) return;
                    var state = (state_ as DictionaryEditor);

                    Dictionary<string, List<KeyValuePair<string, Vector2>>> temp = new Dictionary<string, List<KeyValuePair<string, Vector2>>>();

                    foreach (var npc in state.enabledNPCs)
                    {
                        temp[npc.Key] = new List<KeyValuePair<string, Vector2>>();

                        foreach (var entry in state.dataStrings.Where(val => val.Key.StartsWith(npc.Key)))
                        {
                            int spriteIndex = IsValidSprite(state, entry.Value);
                            Vector2 offset = IsValidVector2(state, entry.Value);

                            if (spriteIndex != -1 && offset != new Vector2(-9999f))
                            {
                                temp[npc.Key].Add(new KeyValuePair<string, Vector2>(entry.Value.Split('/')[0], offset));
                            }
                            else temp[npc.Key].Add(new KeyValuePair<string, Vector2>("Down", Vector2.Zero));
                        }
                    }
                    switch (which)
                    {
                        case 0:
                            //todo - convert this to match the others?
                            break;
                        case 1:
                            config.SpouseRoom_ManualTileOffsets = temp;
                            break;
                        case 2:
                            config.Kitchen_TileOffsets = temp;
                            break;
                        case 3:
                            config.Patio_TileOffsets = temp;
                            break;
                        case 4:
                            config.Porch_TileOffsets = temp;
                            break;
                    }
                };

            GenericMC.RegisterLabel(mod, ".   " + optionName, optionDesc);
            GenericMC.RegisterComplexOption(mod, "", "", editorUpdate, editorDraw, editorSave);
        }
        private int IsValidSprite(DictionaryEditor editor, string input)
        {
            string[] data = editor.spaceRemover.Replace(input, "").Split('/');

            if (data.Length > 0)
            {
                if (int.TryParse(data[0], out int sprite)) return sprite;
                else if (data[0].StartsWith("A:", StringComparison.OrdinalIgnoreCase))
                {
                    List<FarmerSprite.AnimationFrame> anims = GetAnimations(data[0]);
                    if (anims.Count > 0) return anims[DateTime.UtcNow.Second % anims.Count].frame;
                }
                else
                {
                    switch (data[0].ToLower())
                    {
                        case "up":
                            return 0;
                        case "left":
                            return 3;
                        case "right":
                            return 1;
                        case "down":
                            return 2;
                    }
                }
            }
            return -1;
        }
        private List<FarmerSprite.AnimationFrame> GetAnimations(string animData)
        {
            List<FarmerSprite.AnimationFrame> anims = new List<FarmerSprite.AnimationFrame>();
            string[] data = animData.Split(':');
            foreach (var frame in data)
            {
                string[] d2 = frame.Split(',');
                if (d2.Length > 1 && int.TryParse(d2[0], out int f) && float.TryParse(d2[1], out float s))
                {
                    anims.Add(new FarmerSprite.AnimationFrame(f, (int)(s * 1000f)));
                }
            }
            return anims;
        }
        private Vector2 IsValidVector2(DictionaryEditor editor, string input)
        {
            string[] data = editor.spaceRemover.Replace(input, "").Split('/');
            if (data.Length > 1)
            {
                data = data[1].Split(',');
                if (data.Length == 2 && float.TryParse(data[0], out float x) && float.TryParse(data[1], out float y))
                {
                    return new Vector2(x, y);
                }
            }
            return new Vector2(-9999f);
        }




        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady || !(e.Button == SButton.F5)) return; // ignore if player hasn't loaded a save yet
            config = Helper.ReadConfig<ModConfig>();
            translate = Helper.Translation;
            UpdateConfig(false);

            OnDayStarted(null, null);//test
        }


        //private List<Vector2> tiles; //remember to set me to null after all monsters for this area are spawned!
        //private Vector2 GetMonsterCoordinates(Farmer who, int safetyRadius, int monsterRadius, int monsterSize = 1, SpriteBatch renderedWorldTestOnly = null)//size could maybe be used for bigger enemies somehow, or maybe use monster.GetBoundingBox().Intersects(value)
        //{
        //    if (tiles == null)
        //    {
        //        tiles = new List<Vector2>();
        //        Point mid = who.getTileLocationPoint();
        //        Point topLeft = new Point(mid.X - monsterRadius, mid.Y - monsterRadius);
        //        Point bottomRight = new Point(mid.X + monsterRadius, mid.Y + monsterRadius);
        //        for (int x = topLeft.X; x < bottomRight.X + 1; x++)
        //        {
        //            for (int y = topLeft.Y; y < bottomRight.Y + 1; y++)
        //            {
        //                Vector2 tile = new Vector2(x, y);
        //                if (Vector2.DistanceSquared(who.getTileLocation(), tile) > safetyRadius * safetyRadius 
        //                    && (!who.currentLocation.isTileOccupiedForPlacement(tile) && who.currentLocation.isTilePassable(new xTile.Dimensions.Location(x, y), Game1.viewport))) tiles.Add(tile);
        //            }
        //        }
        //        if (tiles.Count > 0) Monitor.Log("Found " + tiles.Count + " tiles!");
        //        else Monitor.Log("Found no free tiles!", LogLevel.Debug);
        //    }
        //    if (renderedWorldTestOnly != null)
        //    {
        //        foreach (var tile in tiles)
        //        {
        //            renderedWorldTestOnly.Draw(Game1.content.Load<Texture2D>("LooseSprites\\buildingPlacementTiles"), new Vector2((int)tile.X * 64 - Game1.viewport.X, (int)tile.Y * 64 - Game1.viewport.Y),
        //                        new Rectangle(0, 0, 64, 64), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
        //        }
        //    }
        //    if (tiles.Count > 0)
        //    {
        //        Vector2 tile = tiles[Game1.random.Next(0, tiles.Count)];
        //        tiles.Remove(tile);
        //        return tile * Game1.tileSize;
        //    }
        //    return -Vector2.One;
        //}

        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            UpdateConfig(false);

            Farmer who = Game1.player;
            NPC spouse = who.getSpouse();


            //List<NPC> test = new List<NPC>(){ Game1.getCharacterFromName("Abigail"), Game1.getCharacterFromName("Penny"), Game1.getCharacterFromName("Harvey"), Game1.getCharacterFromName("Maru"),//test
            //                                  Game1.getCharacterFromName("Sebastian"), Game1.getCharacterFromName("Elliott")};
            //foreach (var item in test)
            //{
            //    spouse = item;
            //    Game1.warpCharacter(spouse, who.currentLocation, who.getTileLocation());
            //    spouse.setTileLocation(new Vector2(38, 14));


            if (spouse?.isVillager() != null)
            {
                Vector2 tile = spouse.getTileLocation();
                KeyValuePair<string, Vector2> newTile = new KeyValuePair<string, Vector2>();
                GameLocation loc = spouse.currentLocation;
                int houseUpgradeLevel = who.HouseUpgradeLevel;
                bool changed = false;

                if (loc is FarmHouse)
                {
                    if (tile == Utility.PointToVector2((loc as FarmHouse).getKitchenStandingSpot())) //kitchen
                    {
                        List<KeyValuePair<string, Vector2>> tiles = config.Kitchen_TileOffsets[(config.Kitchen_TileOffsets.ContainsKey(spouse.Name) ? spouse.Name : "Default")].FindAll(val => !val.Key.Equals("Down", StringComparison.OrdinalIgnoreCase) && val.Value != Vector2.Zero);

                        if (changed = tiles.Count > 0) newTile = tiles[Game1.random.Next(0, tiles.Count)];
                    }
                    else
                    {
                        if (spouse.Name == "Sebastian" && Game1.netWorldState.Value.hasWorldStateID("sebastianFrog") && config.SpouseRoom_ManualTileOffsets.ContainsKey("sebastianFrog") //SpouseRoom (Sebastian after frog)
                            && tile == Utility.PointToVector2((loc as FarmHouse).GetSpouseRoomSpot()) + new Vector2(-1f, 1f))
                        {
                            List<KeyValuePair<string, Vector2>> tiles = config.SpouseRoom_ManualTileOffsets["sebastianFrog"].FindAll(val => !val.Key.Equals("Down", StringComparison.OrdinalIgnoreCase) && val.Value != Vector2.Zero);

                            if (changed = tiles.Count > 0) newTile = tiles[Game1.random.Next(0, tiles.Count)];

                            if (RandomTile(who, spouse, changed, loc, tile, ref newTile)) changed = true;
                        }
                        if (!changed && tile == Utility.PointToVector2((loc as FarmHouse).GetSpouseRoomSpot())) //SpouseRoom - everything else
                        {
                            List<KeyValuePair<string, Vector2>> tiles = config.SpouseRoom_ManualTileOffsets[(config.SpouseRoom_ManualTileOffsets.ContainsKey(spouse.Name) ? spouse.Name : "Default")].FindAll(val => !val.Key.Equals("Down", StringComparison.OrdinalIgnoreCase) && val.Value != Vector2.Zero);

                            if (changed = tiles.Count > 0) newTile = tiles[Game1.random.Next(0, tiles.Count)];

                            if (RandomTile(who, spouse, changed, loc, tile, ref newTile)) changed = true;
                        }
                    }
                }
                else if (Context.IsMainPlayer && loc is Farm)
                {
                    if (tile == Utility.PointToVector2((spouse.getHome() as FarmHouse).getPorchStandingSpot())) //porch
                    {
                        List<KeyValuePair<string, Vector2>> tiles = config.Porch_TileOffsets[(config.Porch_TileOffsets.ContainsKey(spouse.Name) ? spouse.Name : "Default")].FindAll(val => !val.Key.Equals("Down", StringComparison.OrdinalIgnoreCase) && val.Value != Vector2.Zero);

                        if (changed = tiles.Count > 0) newTile = tiles[Game1.random.Next(0, tiles.Count)];
                    }
                    else //patio - spouse area
                    {
                        Vector2 patio = (loc as Farm).GetSpouseOutdoorAreaCorner() + Vector2.One;
                        Rectangle area = new Rectangle((int)patio.X, (int)patio.Y, (int)patio.X + 4, (int)patio.Y + 3);
                        if (area.Contains((int)tile.X, (int)tile.Y))
                        {
                            List<KeyValuePair<string, Vector2>> tiles = config.Patio_TileOffsets[(config.Patio_TileOffsets.ContainsKey(spouse.Name) ? spouse.Name : "Default")].FindAll(val => !val.Key.Equals("Down", StringComparison.OrdinalIgnoreCase) && val.Value != Vector2.Zero);

                            if (changed = tiles.Count > 0) newTile = tiles[Game1.random.Next(0, tiles.Count)];
                        }
                    }
                }
                if (changed)
                {
                    spouse.Position = (tile + newTile.Value) * 64;
                    spouse.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>() { new FarmerSprite.AnimationFrame(40, 1000), new FarmerSprite.AnimationFrame(41, 1000), new FarmerSprite.AnimationFrame(42, 1000) });

                    //if (int.TryParse(newTile.Key, out int spriteIndex)) spouse.Sprite.CurrentFrame = spriteIndex;
                    //else
                    //{
                    //    switch (newTile.Key.ToLower())
                    //    {
                    //        case "tile":
                    //            if (config.SpouseRoomRandomFaceTileOffset.ContainsKey("sebastianFrog")) spouse.faceGeneralDirection((tile - config.SpouseRoomRandomFaceTileOffset["sebastianFrog"]) * 64);
                    //            else if (config.SpouseRoomRandomFaceTileOffset.ContainsKey(spouse.Name)) spouse.faceGeneralDirection((tile - config.SpouseRoomRandomFaceTileOffset[spouse.Name]) * 64);
                    //            else spouse.faceGeneralDirection((tile - config.SpouseRoomRandomFaceTileOffset["Default"]) * 64);
                    //            break;
                    //        case "up":
                    //            spouse.faceDirection(0);
                    //            break;
                    //        case "left":
                    //            spouse.faceDirection(3);
                    //            break;
                    //        case "right":
                    //            spouse.faceDirection(1);
                    //            break;
                    //        default://down
                    //            spouse.faceDirection(2);
                    //            break;
                    //    }
                    //}
                }
            }
            //}
        }

        private bool RandomTile(Farmer who, NPC spouse, bool changed, GameLocation loc, Vector2 tile, ref KeyValuePair<string, Vector2> newTile)
        {
            if ((!changed && config.SpouseRoom_RandomTileChance > 0) || config.SpouseRoom_RandomTileChance > Game1.random.Next(0, 99))
            {
                List<KeyValuePair<string, Vector2>> freeTiles = new List<KeyValuePair<string, Vector2>>();
                List<KeyValuePair<string, Vector2>> emergencyTiles = new List<KeyValuePair<string, Vector2>>();
                List<KeyValuePair<string, Vector2>> seatTiles = new List<KeyValuePair<string, Vector2>>();
                for (int x = -3; x < 3; x++)
                {
                    for (int y = -1; y < 5; y++)
                    {
                        Point potential = new Point(x + (int)tile.X, y + (int)tile.Y);
                        bool occupied = false;
                        for (int i = 0; i < loc.characters.Count; i++)
                        {
                            if (loc.characters[i] != null && loc.characters[i].GetBoundingBox().Intersects(new Rectangle(potential.X * 64 + 1, potential.Y * 64 + 1, 62, 62))
                                && !loc.characters[i].Name.Equals(spouse.Name, StringComparison.Ordinal))
                            {
                                occupied = true;
                                break;
                            }
                        }
                        if (!occupied)
                        {
                            //tiles
                            if (new PathFindController(spouse, loc, potential, 2).pathToEndPoint != null)
                            {
                                freeTiles.Add(new KeyValuePair<string, Vector2>("Tile", new Vector2(x, y)));
                            }
                            else if (!loc.isTileOccupiedForPlacement(new Vector2(x, y) + tile) && loc.getObjectAtTile(potential.X, potential.Y) == null)
                            {
                                emergencyTiles.Add(new KeyValuePair<string, Vector2>("Tile", new Vector2(x, y)));
                            }
                            //furniture
                            else if (config.SpouseRoom_RandomCanUse_FurnitureChairs_UpOnly && (loc.getObjectAtTile(potential.X, potential.Y) as ISittable)?.GetSittingDirection() == 0)
                            {
                                foreach (var seat in (loc.getObjectAtTile(potential.X, potential.Y) as ISittable).GetSeatPositions())
                                {
                                    if (Math.Abs(seat.X - potential.X) < 1f) seatTiles.Add(new KeyValuePair<string, Vector2>("Up", seat - tile + new Vector2(-0.01f, -0.11f)));
                                }
                            }
                            //mapChairs
                            else if (config.SpouseRoom_RandomCanUse_MapChairs_DownOnly)
                            {
                                foreach (MapSeat furniture in loc.mapSeats)
                                {
                                    if (furniture.OccupiesTile(potential.X, potential.Y) && !furniture.IsBlocked(loc) && furniture.GetSittingDirection() == 2)
                                    {
                                        foreach (var seat in furniture.GetSeatPositions())
                                        {
                                            if (Math.Abs(seat.X - potential.X) < 1f) seatTiles.Add(new KeyValuePair<string, Vector2>("Down", seat - tile + new Vector2(0.05f, 0.15f)));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                if (freeTiles.Count + seatTiles.Count + emergencyTiles.Count > 0)
                {
                    if (seatTiles.Count > 0 && Game1.random.Next(0, 3) == 0) newTile = seatTiles[Game1.random.Next(0, seatTiles.Count)];
                    else if (freeTiles.Count > 0) newTile = freeTiles[Game1.random.Next(0, freeTiles.Count)];
                    else if (emergencyTiles.Count > 0) newTile = emergencyTiles[Game1.random.Next(0, emergencyTiles.Count)];
                    else return false;
                    return true;
                }
            }
            return false;
        }


        private void OnResized(object sender, WindowResizedEventArgs e)
        {
            resized = true;
        }


        private void UpdateConfig(bool GMCM)
        {
            if (!GMCM) config = Helper.ReadConfig<ModConfig>();
        }
    }
}
