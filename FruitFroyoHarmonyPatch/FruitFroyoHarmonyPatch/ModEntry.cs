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
            //harmony.Patch(
            //    original: AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.performObjectDropInAction)),
            //    prefix: new HarmonyMethod(typeof(ObjectPatches), nameof(ObjectPatches.performObjectDropInAction_prefix))
            //    );
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

        public static void performObjectDropInAction_postfix(StardewValley.Object __instance, Item dropInItem, bool probe, Farmer who)
        {
            // After putting an item in a machine, recolor fruit frozen yogurt items based on the color tags of the fruit
            try
            {
                if (!probe)
                {
                    //Monitor.Log("Item Name:"+dropInItem.DisplayName, LogLevel.Debug);
                    //Monitor.Log("Object Name:" + dropInItem.DisplayName, LogLevel.Debug);
                    StardewValley.Object heldObject = __instance.heldObject.Get();
                    if (heldObject == null) 
                    { 
                        //Monitor.Log("No Held Object", LogLevel.Debug); 
                    }
                    else
                    {
                        StardewValley.Object heldObjectBase = new StardewValley.Object(parentSheetIndex: heldObject.ParentSheetIndex, initialStack: 1);
                        if (heldObjectBase.Name == "Fruit Frozen Yogurt")
                        {
                            //Monitor.Log("Fruit Froyo!", LogLevel.Debug);
                            int heldObjectPPSI = heldObject.preservedParentSheetIndex.Get();
                            if (heldObjectPPSI != 0)
                            {
                                StardewValley.Object heldObjectParent = new StardewValley.Object(parentSheetIndex: heldObjectPPSI, initialStack: 1);
                                //Monitor.Log("Held Object Parent Name:" + heldObjectParent.DisplayName, LogLevel.Debug);

                                foreach (string contextTag in heldObjectParent.GetContextTagList())
                                {
                                    if (contextTag.StartsWith("color_")) {
                                        heldObject.GetContextTags().Add(contextTag);
                                        Color parentColor = (Color) StardewValley.Menus.TailoringMenu.GetDyeColor(heldObjectParent);

                                        // Color Adjustment
                                        switch (contextTag)
                                        {
                                            case "color_white":
                                            case "color_sand":
                                                parentColor.R = 240;
                                                parentColor.G = 160;
                                                parentColor.B = 80;
                                                break;
                                            case "color_yellow":
                                            case "color_light_yellow":
                                            case "color_dark_yellow":
                                                parentColor.R = 255;
                                                parentColor.G = 190;
                                                parentColor.B = 0;
                                                break;
                                            case "color_pink":
                                            case "color_light_pink":
                                            case "color_salmon":
                                                parentColor = Color.HotPink;
                                                break;
                                            case "color_black":
                                                parentColor = Color.DarkViolet;
                                                break;
                                            case "color_lime":
                                                parentColor.R = 150;
                                                parentColor.G = 220;
                                                parentColor.B = 50;
                                                break;
                                            default:
                                                parentColor.R = ChannelSigmoid(parentColor.R);
                                                parentColor.G = ChannelSigmoid(parentColor.G);
                                                parentColor.B = ChannelSigmoid(parentColor.B);
                                                break;
                                        }

                                        StardewValley.Objects.ColoredObject asColoredObject = (StardewValley.Objects.ColoredObject) __instance.heldObject.Get();
                                        asColoredObject.color.Set(parentColor);
                                        heldObject.GetContextTags().Add(contextTag);
                                        Monitor.Log("Recolored " + heldObject.DisplayName + " to:" + contextTag, LogLevel.Trace);
                                        break;
                                    }
                                }
                                

                            }
                        }


                        
                    }
                } 
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(performObjectDropInAction_postfix)}:\n{ex}", LogLevel.Error);
                //return true;
            }
        }

        private static byte ChannelSigmoid(byte r, int midpoint=128, double flatten=16, double bleachWeight=0.25)
        {
            double res = 255 / (
                1 + Math.Pow(
                    Math.E, 
                    (midpoint - (double)r)/ flatten
                    )
                );
            res = (bleachWeight*255 + (1-bleachWeight)*res);
            return Convert.ToByte(res);
        }
    }
}