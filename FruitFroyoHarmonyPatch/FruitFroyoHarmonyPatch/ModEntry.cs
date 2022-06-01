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
        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            this.ApplyPatches(new Harmony(this.ModManifest.UniqueID));
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
        private static void Initialize(IMonitor monitor)
        {
            Monitor = monitor;
        }

        public static bool performObjectDropInAction_prefix(Item dropInItem, bool probe, Farmer who)
        {
            try
            {
                Monitor.Log(dropInItem.DisplayName, LogLevel.Debug);
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