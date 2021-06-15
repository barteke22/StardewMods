//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Threading;
//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Graphics;
//using StardewModdingAPI;
//using StardewModdingAPI.Events;
//using StardewModdingAPI.Utilities;
//using StardewValley;
//using StardewValley.Tools;

//namespace StardewMods
//{
//    /// <summary>The mod entry point.</summary>
//    public class ModEntryOldTests : Mod
//    {
//        /*********
//        ** Public methods
//        *********/
//        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
//        /// <param name="helper">Provides simplified APIs for writing mods.</param>
//        public override void Entry(IModHelper helper)
//        {
//            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
//            helper.Events.GameLoop.DayStarted += this.OnDayStarted;
//            helper.Events.Display.RenderedHud += this.OnOneSecondUpdateTicked;
//        }

       

//        /*********
//        ** Private methods
//        *********/
//        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
//        /// <param name="sender">The event sender.</param>
//        /// <param name="e">The event data.</param>
//        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
//        {
//            // ignore if player hasn't loaded a save yet
//            if (!Context.IsWorldReady || !(e.Button == SButton.F7))
//                return;

//            //    // print button presses to the console window
//            //    foreach (System.Collections.Generic.KeyValuePair<int, string> achieve in Game1.achievements)
//            //    {
//            //        //this.Monitor.Log(achieve.Value + ", " + achieve.Key, LogLevel.Debug);
//            //    }

//            //    foreach (KeyValuePair<int, string> kvp in Game1.achievements)
//            //    {
//            //        bool farmerHas = Game1.player.achievements.Contains(kvp.Key);
//            //        string[] split2 = kvp.Value.Split('^');
//            //        if (farmerHas || (split2[2].Equals("true") && (split2[3].Equals("-1") || this.farmerHasAchievements(split2[3]))))
//            //        {
//            //            this.Monitor.Log(kvp.Value + ", DONE, " + kvp.Key, LogLevel.Debug);
//            //            //int xPos3 = baseX + widthUsed[5] % collectionWidth * 68;
//            //            //int yPos3 = baseY + widthUsed[5] / collectionWidth * 68;
//            //            //this.collections[5][0].Add(new ClickableTextureComponent(kvp.Key + " " + farmerHas.ToString(), new Rectangle(xPos3, yPos3, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 25), 1f));
//            //            //widthUsed[5]++;
//            //        }
//            //        else  this.Monitor.Log(kvp.Value + ", " + kvp.Key, LogLevel.Debug);
//            //    }
//            //}

//            //private bool farmerHasAchievements(string listOfAchievementNumbers)
//            //{
//            //    string[] array = listOfAchievementNumbers.Split(' ');
//            //    foreach (string s in array)
//            //    {
//            //        if (!Game1.player.achievements.Contains(Convert.ToInt32(s)))
//            //        {
//            //            return false;
//            //        }
//            //    }
//            //    return true;

//            this.Monitor.Log("\n\nSHIPPING:\n", LogLevel.Debug);

//            List<KeyValuePair<int, string>> list = new List<KeyValuePair<int, string>>(Game1.objectInformation);
//            list.Sort((KeyValuePair<int, string> a, KeyValuePair<int, string> b) => a.Key.CompareTo(b.Key));
//            foreach (KeyValuePair<int, string> kvp2 in list)
//            {
//                string typeString = kvp2.Value.Split('/')[3];
//                //if (typeString.Contains("Fish"))
//                //{
//                //    if ((kvp2.Key >= 167 && kvp2.Key <= 172) || (kvp2.Key >= 898 && kvp2.Key <= 902)) //Trash and Legendaries II
//                //    {
//                //        continue;
//                //    }
//                //    if (Game1.player.fishCaught.ContainsKey(kvp2.Key))
//                //    {
//                //        this.Monitor.Log(kvp2.Value.Split('/')[0] + ", " + kvp2.Key, LogLevel.Debug); //Value = Walleye/105/12/Fish - 4/Walleye/A freshwater fish caught at night./Night^Fall Winter
//                //    }                                                                                 //Key = 140
//                //    else
//                //    {
//                //        this.Monitor.Log(kvp2.Value.Split('/')[0] + ", " + kvp2.Key + ", NOT", LogLevel.Debug); //Menu idea: Total Fish Caught: 33, Done: ..., Missing: ...
//                //    }
//                //}

//                if (!StardewValley.Object.isPotentialBasicShippedCategory(kvp2.Key, typeString.Substring(typeString.Length - 3)))
//                {
//                    continue;
//                }
//                if (Game1.player.basicShipped.ContainsKey(kvp2.Key))
//                {
//                    //    this.Monitor.Log(kvp2.Value.Split('/')[0] + ": " + (Game1.player.basicShipped.ContainsKey(kvp2.Key) ? Game1.player.basicShipped[kvp2.Key] : 0), LogLevel.Debug);    
//                    //    //Value = Walleye/105/12/Fish - 4/Walleye/A freshwater fish caught at night./Night^Fall Winter
//                    //    //Stuff after + is shipment count.
//                    //if ((Game1.player.basicShipped.ContainsKey(kvp2.Key) ? Game1.player.basicShipped[kvp2.Key] : 0) < 15) //THIS ONE SHOULD ONLY APPLY TO CROPS
//                    //{
//                    //    this.Monitor.Log(kvp2.Value.Split('/')[0] + ": " + (Game1.player.basicShipped.ContainsKey(kvp2.Key) ? Game1.player.basicShipped[kvp2.Key] : 0), LogLevel.Debug);
//                    //}
//                }   //Key = 140
//                else
//                {
//                    this.Monitor.Log(kvp2.Value.Split('/')[0] + ": 0", LogLevel.Debug); //Menu idea: Total Fish Caught: 33, Done: ..., Missing: ...
//                }

//            }

//            //this.Monitor.Log("\n\nCRAFTING:\n", LogLevel.Debug);

//            //foreach (KeyValuePair<int, string> kvp2 in list)
//            //{
//            //    if (Game1.player.basicShipped.ContainsKey(kvp2.Key))
//            //    {

//            //    }
//            //    else
//            //    {
//            //       // this.Monitor.Log(kvp2.Value.Split('/')[0] + ": 0", LogLevel.Debug);
//            //    }

//            //}
//        }


//        Boolean dayStarted = false;
//        Dictionary<string, string> locationData;
//        Dictionary<int, string> fishData;

//        private void OnDayStarted(object sender, DayStartedEventArgs e)
//        {
//            dayStarted = false;

//            locationData = Game1.content.Load<Dictionary<string, string>>("Data\\Locations")
//                .Where(x => !(x.Value.Split('/')[4 + Utility.getSeasonNumber(Game1.currentSeason)].Equals("-1")))
//                .ToDictionary(x => x.Key, x => x.Value);

//            //locationData = Game1.content.Load<Dictionary<string, string>>("Data\\Locations");
//            //Dictionary<string, string> temp = new Dictionary<string, string>(locationData);
//            //foreach (KeyValuePair<string, string> data in temp)
//            //{
//            //    if (data.Value.Split('/')[4 + Utility.getSeasonNumber(Game1.currentSeason)].Equals("-1")) locationData.Remove(data.Key);   //removes locs without fish this season
//            //}

//            fishData = Game1.content.Load<Dictionary<int, string>>("Data\\Fish");
//            dayStarted = true;
//        }
        

//        /*  SEE IF CAN BE SIMPLIFIED, ADD LEGENDARIES/TRASH/STUFF FROM NON-Generic Locations (GetFish)... Look for ways to automate, otherwise hard code??
//         *  Minigame: Preview, if not caught dark. On catch quick swap image? Or enough for game to show it off on its own? Pokemon like reveal (who's that fishiee??)?
//         *  Tackle + bait preview with values?
//         *  Dark preview (???) if fish not caught.
//         *  Update instant when: Swapped to rod/minigame end (don't update during). Otherwise every X ticks.
//         */

//        private void OnOneSecondUpdateTicked(object sender, RenderedHudEventArgs e)
//        {
//            if (!dayStarted || Game1.eventUp || !Game1.player.currentLocation.canFishHere() || !(Game1.player.CurrentTool is FishingRod)) return; //code stop conditions
//            string locationName = Game1.player.currentLocation.Name;
//            if (!locationData.ContainsKey(locationName)) return;


//            //this.Monitor.Log("\n", LogLevel.Debug);

//            List<string> names = new List<string>();    //UI INIT
//            List<int> ids = new List<int>();

//            SpriteFont font = Game1.smallFont;
//            float boxWidth = 0;
//            float lineHeight = font.LineSpacing;

//            int iconMode = 0;                           //config: 0=Horizontal Icons, 1=Vertical Icons, 2=Vertical Icons + Text
//            bool questionMarkUncaught = true;           //config: Whether uncaught fish are displayed as ??? and use dark icons
//            bool recatchableLegendariesMod = true;      //config: Whether player has a mod that allow recatching legendaries. Displays them even if caught.
//            Vector2 boxTopLeft = new Vector2(20, 20);   //config: Display Location
//            Vector2 boxBottomLeft = boxTopLeft;
//            const float iconScale = Game1.pixelZoom / 2f;

//            Texture2D whitePixel = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
//            whitePixel.SetData(new[] { Color.White });

//            SpriteBatch batch = Game1.spriteBatch;
//            batch.End();
//            batch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend);


//            string[] rawFishData = locationData[locationName].Split('/')[4 + Utility.getSeasonNumber(Game1.currentSeason)].Split(' ');  //data

//            Dictionary<string, string> rawFishDataWithLocation = new Dictionary<string, string>();
//            if (rawFishData.Length > 1)
//            {
//                for (int j = 0; j < rawFishData.Length; j += 2)
//                {
//                    rawFishDataWithLocation[rawFishData[j]] = rawFishData[j + 1];
//                }
//            }
//            string[] keys = rawFishDataWithLocation.Keys.ToArray();
//            for (int i = 0; i < keys.Length; i++)
//            {
//                bool fail = true;
//                string[] specificFishData = fishData[Convert.ToInt32(keys[i])].Split('/');
//                string[] timeSpans = specificFishData[5].Split(' ');
//                int location = Convert.ToInt32(rawFishDataWithLocation[keys[i]]);
//                if (location == -1 || Game1.player.currentLocation.getFishingLocation(Game1.player.getTileLocation()) == location)
//                {
//                    for (int l = 0; l < timeSpans.Length; l += 2)
//                    {
//                        if (Game1.timeOfDay >= Convert.ToInt32(timeSpans[l]) && Game1.timeOfDay < Convert.ToInt32(timeSpans[l + 1]))
//                        {
//                            fail = false;
//                            break;
//                        }
//                    }
//                }
//                if (!specificFishData[7].Equals("both"))
//                {
//                    if (specificFishData[7].Equals("rainy") && !Game1.IsRainingHere(Game1.player.currentLocation))
//                    {
//                        fail = true;
//                    }
//                    else if (specificFishData[7].Equals("sunny") && Game1.IsRainingHere(Game1.player.currentLocation))
//                    {
//                        fail = true;
//                    }
//                }
//                bool beginnersRod = Game1.player != null && Game1.player.CurrentTool != null && Game1.player.CurrentTool is StardewValley.Tools.FishingRod && (int)Game1.player.CurrentTool.upgradeLevel == 1;
//                if (Convert.ToInt32(specificFishData[1]) >= 50 && beginnersRod)
//                {
//                    fail = true;
//                }
//                if (Game1.player.FishingLevel < Convert.ToInt32(specificFishData[12]))
//                {
//                    fail = true;
//                }
//                if (!fail)
//                {
//                    int fish = Int32.Parse(keys[i]);
//                    //this.Monitor.Log(new StardewValley.Object(fish, 1).Name, LogLevel.Debug);
//                    names.Add(new StardewValley.Object(fish, 1).Name);
//                    ids.Add(Int32.Parse(keys[i]));

//                    //UI DRAW
//                    Rectangle source = GameLocation.getSourceRectForObject(fish);
//                    batch.Draw(Game1.objectSpriteSheet, boxBottomLeft, source, Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 1F);
//                    lineHeight = Math.Max(lineHeight, source.Height * iconScale);


//                    if (Game1.player.fishCaught.ContainsKey(fish))
//                    {

//                        batch.DrawString(font, new StardewValley.Object(fish, 1).Name, boxBottomLeft + new Vector2(source.Width * iconScale, 0), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1F);
//                        //Utility.drawTextWithColoredShadow(batch, new StardewValley.Object(Int32.Parse(keys[i]), 1).Name, font, boxBottomLeft + new Vector2(source.Width * iconScale, 0), Color.White, Color.Black, 1f, -1, -1, -1, 5);
//                        //Utility.drawTextWithColoredShadow(batch, new StardewValley.Object(Int32.Parse(keys[i]), 1).Name, font, boxBottomLeft + new Vector2(source.Width * iconScale, 0), Color.White, Color.Black, 1f, -1, 1, 1, 5);
//                    } else
//                    {
//                        batch.DrawString(font, "???", boxBottomLeft + new Vector2(source.Width * iconScale, 0), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1F);
//                    }
//                    boxBottomLeft += new Vector2(0, lineHeight);
//                    boxWidth = Math.Max(boxWidth, font.MeasureString(new StardewValley.Object(fish, 1).Name).X + source.Width * iconScale);
//                }
//            }

//            batch.Draw(whitePixel, new Rectangle((int)boxTopLeft.X-2, (int)boxTopLeft.Y-2, (int)boxWidth+6, (int)(boxBottomLeft.Y - lineHeight)+22), null, new Color(0, 0, 0, 0.5F), 0f, Vector2.Zero, SpriteEffects.None, 0);


//            batch.End();
//            batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise);

//            //if (names.Count > 0)//debug
//            //{
//            //    Game1.addHUDMessage(new HUDMessage(String.Join(", ", names)));
//            //    Game1.addHUDMessage(new HUDMessage(String.Join(", ", ids)));
//            //}
//        }
        

//        //public virtual int getFishingLocation(Vector2 tile)
//        //{
//        //    return -1;
//        //}
//    }
//}
