using DiagonalAim;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;

namespace DiagonalAim
{
    public class ModEntry : Mod
    {
        public static ModConfig config;

        public override void Entry(IModHelper helper)
        {
            Log.Monitor = Monitor;
            config = Helper.ReadConfig<ModConfig>();


            var harmony = new Harmony(ModManifest.UniqueID);//this might summon Cthulhu
            //harmony.Patch(
            //    original: AccessTools.Method(typeof(Character), "GetToolLocation", new Type[] { typeof(Vector2), typeof(bool) }),
            //    postfix: new HarmonyMethod(typeof(HarmonyPatches), "GetToolLocation_Diagonals")
            //);
            harmony.Patch(
                original: AccessTools.Method(typeof(Character), nameof(Character.GetToolLocation), new Type[] { typeof(Vector2), typeof(bool) }),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(HarmonyPatches.GetToolLocation_Diagonals))
            );
        }
    }
}
