using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using HarmonyLib;

namespace FruitFroyo
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {

        private ObjectPatches objectPatches;

        public override void Entry(IModHelper helper)
        {
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        }


        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            Harmony harmony = new Harmony(this.ModManifest.UniqueID);
            this.objectPatches = new ObjectPatches(this.Monitor);
            this.ApplyPatches(harmony);
        }

        private void ApplyPatches(Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.performObjectDropInAction)),
                prefix: new HarmonyMethod(typeof(ObjectPatches), nameof(ObjectPatches.performObjectDropInAction_prefix))
                );
            harmony.Patch(
                original: AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.performObjectDropInAction)),
                postfix: new HarmonyMethod(typeof(ObjectPatches), nameof(ObjectPatches.performObjectDropInAction_postfix))
                );
        }
    }

    public class ObjectPatches
    {
        private static IMonitor Monitor;

        public ObjectPatches(IMonitor monitor)
        {
            Monitor = monitor;
        }

        public static bool performObjectDropInAction_prefix(Item dropInItem, bool probe, Farmer who)
        {
            try
            {
                if (!probe) {
                    // Standing in for worky bits
                    Monitor.Log("Prefix, Not Probe:" + dropInItem.DisplayName, LogLevel.Debug);
                } 
                /*else
                {
                    Monitor.Log("Prefix, Probe:"+dropInItem.DisplayName, LogLevel.Debug);
                }*/
                return true;
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(performObjectDropInAction_prefix)}:\n{ex}", LogLevel.Error);
                return true;
            }
            
        }

        public static void performObjectDropInAction_postfix(StardewValley.Object __instance, Item dropInItem, bool probe, Farmer who)
        {
            try
            {
                if (!probe)
                {
                    Monitor.Log("Item Name:"+dropInItem.DisplayName, LogLevel.Debug);
                    Monitor.Log("Object Name:" + __instance.DisplayName, LogLevel.Debug);
                } 
                /*else
                {
                    Monitor.Log("Postfix, Probe:" + dropInItem.DisplayName, LogLevel.Debug);
                }*/
                //return true;
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(performObjectDropInAction_prefix)}:\n{ex}", LogLevel.Error);
                //return true;
            }

        }
    }

}