using GenericModConfigMenu;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewMods;
using StardewValley;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CustomSpouseLocation
{
    class DictionaryEditor
    {
        public Dictionary<string, List<string>> enabledNPCs;
        public Dictionary<string, NPC> datableNPCs = new Dictionary<string, NPC>();
        public Dictionary<string, NPC> otherNPCs = new Dictionary<string, NPC>();
        public Dictionary<string, Rectangle> hoverNames = new Dictionary<string, Rectangle>();
        public Dictionary<string, string> dataStrings = new Dictionary<string, string>();
        public string dataEditing = null;
        public Vector2 scrollBar;
        public Rectangle boundsLeftRight;
        public Rectangle boundsTopBottom;
        public int scrollState;
        public int contentBottom;
        public Regex spaceRemover = new Regex(@"\s+");
        public Regex digitRemover = new Regex(@"\d*");
        public Regex animChecker = new Regex(@"^\d+\:\d+(\.\d+)?$");

        public DictionaryEditor(Dictionary<string, List<KeyValuePair<string, Vector2>>> dictionary, Vector2 pos)
        {
            enabledNPCs = new Dictionary<string, List<string>>();
            foreach (var npc in dictionary)
            {
                for (int i = 0; i < npc.Value.Count; i++)
                {
                    dataStrings[npc.Key + i] = npc.Value[i].Key + " / " + npc.Value[i].Value.X + ", " + npc.Value[i].Value.Y;
                    if (enabledNPCs.ContainsKey(npc.Key)) enabledNPCs[npc.Key].Add(npc.Key + i);
                    else enabledNPCs[npc.Key] = new List<string>() { npc.Key + i };
                }
            }

            foreach (var item in Game1.content.Load<Dictionary<string, string>>("Data\\NPCDispositions"))
            {
                if (item.Value.Split('/')[5].Equals("datable", StringComparison.Ordinal)) datableNPCs.Add(item.Key, Game1.getCharacterFromName(item.Key));
                else otherNPCs.Add(item.Key, Game1.getCharacterFromName(item.Key));
            }
            scrollState = Game1.input.GetMouseState().ScrollWheelValue;
        }
    }

    //class DictionaryEditorWidget : IKeyboardSubscriber
    //{
    //    private DictionaryEditor state;
    //    public static bool resized = true;
    //    IMonitor Monitor;

    //    public virtual string String { get; set; }

    //    private bool SelectedImpl;
    //    public bool Selected
    //    {
    //        get => this.SelectedImpl;
    //        set
    //        {
    //            if (this.SelectedImpl == value)
    //                return;

    //            this.SelectedImpl = value;
    //            if (this.SelectedImpl)
    //                Game1.keyboardDispatcher.Subscriber = this;
    //            else
    //            {
    //                if (Game1.keyboardDispatcher.Subscriber == this)
    //                    Game1.keyboardDispatcher.Subscriber = null;
    //            }
    //        }
    //    }
    //    protected virtual void ReceiveInput(string str)
    //    {
    //        if (state?.dataEditing != null) state.dataStrings[state.dataEditing] += str;
    //    }

    //    public void RecieveCommandInput(char command)
    //    {
    //        if (command == '\b' && state?.dataStrings[state.dataEditing].Length > 1)
    //        {
    //            Game1.playSound("tinyWhip");
    //            state.dataStrings[state.dataEditing] = state.dataStrings[state.dataEditing].Substring(0, state.dataStrings[state.dataEditing].Length - 1);
    //        }
    //        else if (command == '\b' && state?.dataStrings[state.dataEditing].Length < 2)
    //        {
    //            Game1.playSound("tinyWhip");
    //            state.dataStrings[state.dataEditing] = "";
    //        }
    //    }

    //    public void RecieveSpecialInput(Keys key)
    //    {
    //        //?
    //    }

    //    public void RecieveTextInput(char inputChar)
    //    {
    //        ReceiveInput(inputChar.ToString());
    //    }

    //    public void RecieveTextInput(string text)
    //    {
    //        ReceiveInput(text);
    //    }

    //    public void GenericMCDictionaryEditor(IGenericModConfigMenuApi GenericMC, IManifest mod, string optionName, string optionDesc, IMonitor mon, IModHelper Helper, Dictionary<string, List<KeyValuePair<string, Vector2>>> dict)
    //    {
    //        this.Monitor = mon;

    //        Func<Vector2, object, object> editorUpdate =
    //            (Vector2 pos, object state_) =>
    //            {
    //                KeybindList reset = KeybindList.Parse("F5");

    //                state = state_ as DictionaryEditor;
    //                if (state == null || reset.JustPressed()) state = new DictionaryEditor(dict, pos);

    //                KeybindList click = KeybindList.Parse("MouseLeft");
    //                KeybindList delete = KeybindList.Parse("LeftControl + X");


    //                if (new Rectangle(state.bounds.X - 220, state.bounds.Y, state.bounds.Width + 440, state.bounds.Height).Contains(Game1.getMouseX(), Game1.getMouseY()))
    //                {
    //                    if (state.scrollState != Game1.input.GetMouseState().ScrollWheelValue)
    //                    {
    //                        int scroll = Game1.input.GetMouseState().ScrollWheelValue;
    //                        float line = Game1.smallFont.MeasureString("ImLost").Y * 1.2f;

    //                        if (scroll > state.scrollState && state.scrollBar.Y + state.bounds.Height < pos.Y + state.bounds.Height) state.scrollBar.Y += line;
    //                        else if (scroll < state.scrollState && state.contentBottom + state.scrollBar.Y - state.bounds.Height - line > pos.Y) state.scrollBar.Y -= line;

    //                        state.scrollState = scroll;
    //                    }
    //                }
    //                else state.scrollState = Game1.input.GetMouseState().ScrollWheelValue;


    //                if (click.JustPressed())
    //                {
    //                    foreach (var button in state.hoverNames)
    //                    {
    //                        if (button.Value.Contains(Game1.getMouseX(), Game1.getMouseY()))
    //                        {
    //                            if (state.enabledNPCs.ContainsKey(button.Key))
    //                            {
    //                                int numb = int.Parse(state.dataStrings.Keys.Where(val => val.StartsWith(button.Key)).OrderBy(val => int.Parse(val.Replace(button.Key, ""))).Last().Replace(button.Key, "")) + 1;
    //                                state.enabledNPCs[button.Key].Add(button.Key + numb);
    //                                state.dataStrings.Add(button.Key + numb, "2, 0, 0");
    //                                state.dataEditing = null;
    //                                break;
    //                            }
    //                            else if (state.datableNPCs.ContainsKey(button.Key))
    //                            {
    //                                state.enabledNPCs[button.Key] = new List<string>() { button.Key + 0 };
    //                                state.dataStrings.Add(button.Key + 0, "2, 0, 0");
    //                                state.dataEditing = null;
    //                                break;
    //                            }
    //                            else if (state.dataStrings.ContainsKey(button.Key))
    //                            {
    //                                state.dataEditing = button.Key;
    //                                break;
    //                            }
    //                            else state.dataEditing = null;
    //                        }
    //                    }
    //                    Selected = state.dataEditing != null;
    //                }
    //                else if (delete.JustPressed())
    //                {
    //                    state.dataEditing = null;
    //                    foreach (var button in state.hoverNames)
    //                    {
    //                        if (button.Value.Contains(Game1.getMouseX(), Game1.getMouseY()))
    //                        {
    //                            if (state.enabledNPCs.ContainsKey(button.Key))
    //                            {
    //                                if (!button.Key.Equals("Default", StringComparison.Ordinal) && state.dataStrings.Keys.Where(val => val.StartsWith(button.Key, StringComparison.Ordinal)).Count() < 2)
    //                                {
    //                                    state.enabledNPCs.Remove(button.Key);//delete name if only 1 entry + delete entries
    //                                    foreach (var entry in state.hoverNames)
    //                                    {
    //                                        if (entry.Key.StartsWith(button.Key, StringComparison.Ordinal)) state.dataStrings.Remove(entry.Key);
    //                                    }
    //                                    break;
    //                                }
    //                            }
    //                            else if (state.dataStrings.ContainsKey(button.Key) && state.dataStrings.Keys.Where(val => val.StartsWith(Regex.Replace(button.Key, @"\d*", ""), StringComparison.Ordinal)).Count() > 1)
    //                            {
    //                                state.dataStrings.Remove(button.Key);//otherwise delete selected entry
    //                                break;
    //                            }
    //                        }
    //                    }
    //                }


    //                return state;
    //            };
    //        Func<SpriteBatch, Vector2, object, object> editorDraw =
    //            (SpriteBatch b, Vector2 pos, object state_) =>
    //            {
    //                var state = state_ as DictionaryEditor;
    //                if (resized)
    //                {
    //                    state.scrollBar = pos;
    //                    int width = Math.Min(Game1.uiViewport.Width / 4, 400);
    //                    state.bounds = new Rectangle((int)pos.X - width, (int)pos.Y, width * 2, -300 + (int)(Math.Min(Game1.uiViewport.Height, 1300f) * 0.5f));//test //0.8f
    //                    resized = false;
    //                }
    //                Vector2 left = new Vector2(-100f - (state.bounds.Width / 2f), 0f);
    //                b.DrawString(Game1.smallFont, "_____________" + state.scrollBar.Y, state.scrollBar + new Vector2(0f, state.bounds.Height), Color.Red, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);

    //                state.hoverNames = new Dictionary<string, Rectangle>();

    //                foreach (var npc in state.enabledNPCs.OrderBy(val => val.Key))
    //                {
    //                    Vector2 nameS = Game1.smallFont.MeasureString(npc.Key);
    //                    Rectangle nameR = new Rectangle((int)(state.scrollBar + left).X, (int)(state.scrollBar + left).Y, state.bounds.Width, (int)nameS.Y);

    //                    if (!state.bounds.Contains(state.bounds.Width, (int)(state.scrollBar.Y + left.Y))) //out of bounds?
    //                    {
    //                        left.Y += nameS.Y;
    //                    }
    //                    else
    //                    {
    //                        state.hoverNames[npc.Key] = nameR;

    //                        b.DrawString(Game1.smallFont, npc.Key, state.scrollBar + left, (nameR.Contains(Game1.getMouseX(), Game1.getMouseY())) ? Color.DarkSlateGray : Color.ForestGreen, 0f, Vector2.Zero, 1.2f, SpriteEffects.None, 1f);
    //                        left.Y += nameS.Y;
    //                    }
    //                    foreach (var text in state.dataStrings)
    //                    {
    //                        if (!state.bounds.Contains(state.bounds.Width, (int)(state.scrollBar.Y + left.Y))) //out of bounds?
    //                        {
    //                            left.Y += nameS.Y;
    //                            continue;
    //                        }
    //                        if (text.Key.StartsWith(npc.Key, StringComparison.Ordinal))
    //                        {
    //                            nameR = new Rectangle((int)(state.scrollBar + left).X, (int)(state.scrollBar + left).Y, state.bounds.Width, (int)nameS.Y);
    //                            state.hoverNames[text.Key] = nameR;
    //                            Color color = Color.Red;
    //                            string[] temp = text.Value.Split(',');

    //                            if (int.TryParse(temp[0], out int spriteIndex) && temp.Length > 2 && float.TryParse(temp[1], out float x) && float.TryParse(temp[2], out float y)) color = Color.ForestGreen;
    //                            if (nameR.Contains(Game1.getMouseX(), Game1.getMouseY()))
    //                            {
    //                                if (color == Color.Red) color = Color.Maroon;
    //                                else color = Color.DarkSlateGray;
    //                            }

    //                            b.DrawString(Game1.smallFont, text.Value, state.scrollBar + left + new Vector2(50f, 0f), color, 0f, Vector2.Zero, 1.2f, SpriteEffects.None, 1f);

    //                            if (Selected && text.Key.Equals(state.dataEditing, StringComparison.Ordinal) && DateTime.UtcNow.Millisecond % 1000 >= 500)
    //                                b.Draw(Game1.staminaRect, new Rectangle((int)(state.scrollBar.X + left.X + 50f) + 8 + (int)(Game1.smallFont.MeasureString(text.Value).X * 1.2f), (int)(state.scrollBar.Y + left.Y), 4, 32), Color.Black);

    //                            left.Y += nameS.Y;
    //                        }
    //                    }
    //                }

    //                foreach (var npc in state.datableNPCs.OrderBy(val => val.Key))
    //                {
    //                    Vector2 nameS = Game1.smallFont.MeasureString(npc.Key);
    //                    if (!state.bounds.Contains(state.bounds.Width, (int)(state.scrollBar.Y + left.Y))) //out of bounds?
    //                    {
    //                        left.Y += nameS.Y;
    //                        continue;
    //                    }
    //                    if (!state.enabledNPCs.ContainsKey(npc.Key))
    //                    {
    //                        Rectangle nameR = new Rectangle((int)(state.scrollBar + left).X, (int)(state.scrollBar + left).Y, state.bounds.Width, (int)nameS.Y);
    //                        state.hoverNames[npc.Key] = nameR;

    //                        b.DrawString(Game1.smallFont, npc.Key, state.scrollBar + left, (nameR.Contains(Game1.getMouseX(), Game1.getMouseY())) ? Color.Maroon : Color.Red, 0f, Vector2.Zero, 1.2f, SpriteEffects.None, 1f);
    //                        left.Y += nameS.Y;
    //                    }
    //                }
    //                state.contentBottom = (int)left.Y;

    //                //ui//top/bottom + arrows if not met requirements in scroll
    //                //b.Draw(Game1.staminaRect, new Rectangle((int)(state.scrollBar.X + left.X + 50f) + 8 + (int)(Game1.smallFont.MeasureString(text.Value).X * 1.2f), (int)(state.scrollBar.Y + left.Y), 4, 32), Color.Black);



    //                return state;
    //            };
    //        Action<object> editorSave =
    //            (object state_) =>
    //            {
    //                if (state_ == null) return;
    //                ModConfig config = Helper.ReadConfig<ModConfig>();
    //                config.Kitchen_TileOffsets = new Dictionary<string, List<KeyValuePair<string, Vector2>>>();
    //                var state = (state_ as DictionaryEditor);
    //                foreach (var npc in state.enabledNPCs)
    //                {
    //                    config.Kitchen_TileOffsets[npc.Key] = new List<KeyValuePair<string, Vector2>>();

    //                    foreach (var entry in state.dataStrings.Where(val => val.Key.StartsWith(npc.Key)))
    //                    {
    //                        string[] data = entry.Value.Split(',');
    //                        if (data.Length > 2 && int.TryParse(data[0], out int spriteIndex) && float.TryParse(data[1], out float x) && float.TryParse(data[2], out float y)) config.Kitchen_TileOffsets[npc.Key].Add(new KeyValuePair<string, Vector2>(spriteIndex, new Vector2(x, y)));
    //                        else config.Kitchen_TileOffsets[npc.Key].Add(new KeyValuePair<string, Vector2>(2, Vector2.Zero));
    //                    }
    //                }
    //                foreach (var item in config.Kitchen_TileOffsets)
    //                {
    //                    foreach (var item2 in item.Value)
    //                    {
    //                        Monitor.Log(item.Key + ": " + item2.Value.X, LogLevel.Error);
    //                    }
    //                }
    //                Helper.WriteConfig(config);
    //            };

    //        //GenericMC.RegisterLabel(mod, ".   " + optionName, optionDesc);
    //        //GenericMC.RegisterComplexOption(mod, "A", "", editorUpdate, editorDraw, editorSave);
    //        //GenericMC.RegisterLabel(mod, ".", "");
    //        //GenericMC.RegisterLabel(mod, ".", "");
    //    }
    //}
}
