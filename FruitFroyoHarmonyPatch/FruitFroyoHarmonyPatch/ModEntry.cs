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
                    Monitor.Log(dropInItem.DisplayName, LogLevel.Debug);
                }
                return true;
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(performObjectDropInAction_prefix)}:\n{ex}", LogLevel.Error);
                return true;
            }
            
        }
    }

}