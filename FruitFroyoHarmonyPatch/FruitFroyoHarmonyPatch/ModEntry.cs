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
        private IJsonAssetsApi jsonAssets;

        public override void Entry(IModHelper helper)
        {
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        }


        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            Harmony harmony = new Harmony(this.ModManifest.UniqueID);
            this.jsonAssets = Helper.ModRegistry.GetApi<IJsonAssetsApi>("spacechase0.JsonAssets");
            this.objectPatches = new ObjectPatches(this.Monitor, this.jsonAssets);
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

    public interface IJsonAssetsApi
    {
        int GetObjectId(string name);
    }

    public class ObjectPatches
    {
        private static IMonitor Monitor;
        private static IJsonAssetsApi jsonAssets;

        // For converting generic fruit froyo into special flavors
        // Key: Name of Preserved Item, Value: Name of Special Flavor
        private static readonly System.Collections.Generic.Dictionary<String, String> SpecialFruitFroyo = new() {
            { "Ancient Fruit","Ancient Fruit Frozen Yogurt" },
            { "Apple", "Apple Frozen Yogurt" },
            { "Banana", "Banana Frozen Yogurt" },
            { "Blackberry", "Blackberry Frozen Yogurt" },
            { "Blueberry", "Blueberry Frozen Yogurt" },
            { "Cactus Fruit", "Cactus Fruit Frozen Yogurt" },
            { "Cherry", "Cherry Frozen Yogurt" },
            { "Coconut", "Coconut Frozen Yogurt" },
            { "Cranberries", "Cranberry Frozen Yogurt" },
            { "Crystal Fruit", "Crystal Fruit Frozen Yogurt" },
            { "Grape", "Grape Frozen Yogurt" },
            { "Hot Pepper", "Hot Pepper Frozen Yogurt" },
            { "Mango", "Mango Frozen Yogurt" },
            { "Melon", "Pink Melon Frozen Yogurt" },
            { "Orange", "Orange Frozen Yogurt" },
            { "Peach", "Peach Frozen Yogurt" },
            { "Pineapple", "Pineapple Frozen Yogurt" },
            { "Pomegranate", "Pomegranate Frozen Yogurt" },
            { "Qi Fruit", "Qi's Frozen Yogurt" },
            { "Salmonberry", "Salmonberry Frozen Yogurt" },
            { "Spice Berry", "Spice Berry Frozen Yogurt" },
            { "Starfruit", "Starfruit Frozen Yogurt" },
            { "Strawberry", "Strawberry Frozen Yogurt" },
            { "Lemon", "Lemon Frozen Yogurt" },
            { "Lime", "Lime Frozen Yogurt" },
            { "Kiwi", "Kiwi Frozen Yogurt" },
            { "Raspberry", "Raspberry Frozen Yogurt"}
        };
        // For converting generic chocolate fruit froyo into special flavors
        // Key: Name of Preserved Item, Value: Name of Special Flavor
        private static readonly System.Collections.Generic.Dictionary<String, String> SpecialChocoFruitFroyo = new() {
            { "Banana", "Chocolate Banana Frozen Yogurt" },
            { "Blueberry", "Chocolate Blueberry Frozen Yogurt" },
            { "Cherry", "Chocolate Cherry Frozen Yogurt" },
            { "Coconut", "Chocolate Coconut Frozen Yogurt" },
            { "Hot Pepper", "Chili Chocolate Frozen Yogurt" },
            { "Orange", "Chocolate Orange Frozen Yogurt" },
            { "Pineapple", "Chocolate Pineapple Frozen Yogurt" },
            { "Pomegranate", "Chocolate Pomegranate Frozen Yogurt" },
            { "Strawberry", "Chocolate Strawberry Frozen Yogurt" },
            { "Raspberry", "Chocolate Raspberry Frozen Yogurt" },
        };

        // For special flavors to generic chocolate fruit swirl flavors
        // The preserved item is input as the preserved item, then is converted by this patch to the actual flavor
        // Key: Name of Special Flavor, Key: Name of Replacement Fruit
        private static readonly System.Collections.Generic.Dictionary<String, String> GenericChocoFruitFroyo = new() {
            { "Ancient Fruit Frozen Yogurt" , "Ancient Fruit"},
            { "Apple Frozen Yogurt", "Apple" },
            { "Blackberry Frozen Yogurt", "Blackberry" },
            { "Cactus Fruit Frozen Yogurt", "Cactus Fruit" },
            { "Cranberry Frozen Yogurt", "Cranberries" },
            { "Crystal Fruit Frozen Yogurt", "Crystal Fruit" },
            { "Grape Frozen Yogurt", "Grape" },
            { "Mango Frozen Yogurt", "Mango" },
            { "Pink Melon Frozen Yogurt", "Melon" },
            { "Peach Frozen Yogurt", "Peach" },
            { "Qi's Frozen Yogurt", "Qi Fruit"},
            { "Salmonberry Frozen Yogurt", "Salmonberry" },
            { "Spice Berry Frozen Yogurt", "Spice Berry" },
            { "Starfruit Frozen Yogurt", "Starfruit" },
            { "Lemon Frozen Yogurt", "Lemon" },
            { "Lime Frozen Yogurt", "Lime" },
            { "Kiwi Frozen Yogurt", "Kiwi" },
        };

        private static readonly System.Collections.Generic.Dictionary<String, int> VanillaFruitIDLookup = new()
        {
            { "Ancient Fruit", 454 },
            { "Apple", 613 },
            { "Banana", 91 },
            { "Blackberry", 410 },
            { "Blueberry", 258 },
            { "Cactus Fruit", 90 },
            { "Cherry", 638 },
            { "Coconut", 88 },
            { "Cranberries", 282 },
            { "Crystal Fruit", 414 },
            { "Grape", 398 },
            { "Hot Pepper", 260 },
            { "Mango", 834 },
            { "Melon", 254 },
            { "Orange", 635 },
            { "Peach", 636 },
            { "Pineapple", 832 },
            { "Pomegranate", 637 },
            { "Qi Fruit", 889 },
            { "Salmonberry", 296 },
            { "Spice Berry", 396 },
            { "Starfruit", 268 },
            { "Strawberry", 400 },
        };

        public ObjectPatches(IMonitor monitorIn, IJsonAssetsApi jsonAssetsIn)
        {
            Monitor = monitorIn;
            jsonAssets = jsonAssetsIn;
        }

        public static void performObjectDropInAction_postfix(StardewValley.Object __instance, Item dropInItem, bool probe, Farmer who)
        {
            try
            {
                if (!probe)
                {
                    //Monitor.Log("Item Name:"+dropInItem.DisplayName, LogLevel.Debug);
                    //Monitor.Log("Object Name:" + dropInItem.DisplayName, LogLevel.Debug);

                    // Check Machine
                    if (__instance.name != "Frozen Yogurt Machine" && __instance.name != "Chocolate Swirl Machine")
                    {
                        return;
                    }
                    // Check Held Object
                    StardewValley.Object heldObject = __instance.heldObject.Get();
                    if (heldObject == null) 
                    {
                        //Monitor.Log("No Held Object", LogLevel.Debug);
                        return; 
                    }
                    else
                    {
                        ApplyHeldItemChanges(__instance);
                    }
                } 
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(performObjectDropInAction_postfix)}:\n{ex}", LogLevel.Error);
                //return true;
            }
        }

        private static void ApplyHeldItemChanges(StardewValley.Object __instance)
        {
            StardewValley.Object heldObject = __instance.heldObject.Get();
            // Check Held Object is Generic Flavor Froyo
            StardewValley.Object heldObjectBase = new StardewValley.Object(parentSheetIndex: heldObject.ParentSheetIndex, initialStack: 1);
            if (heldObjectBase.Name == "Fruit Frozen Yogurt" || heldObjectBase.Name == "Chocolate Swirl Fruit Frozen Yogurt")
            {
                //Monitor.Log("Fruit Froyo!", LogLevel.Debug);
                int heldObjectPPSI = heldObject.preservedParentSheetIndex.Get();
                if (heldObjectPPSI != 0)
                {
                    StardewValley.Object heldObjectParent = new StardewValley.Object(parentSheetIndex: heldObjectPPSI, initialStack: 1);
                    //Monitor.Log("Held Object Parent Name:" + heldObjectParent.DisplayName, LogLevel.Debug);
                    bool needsRecolor = true;
                    if (
                        heldObjectBase.Name == "Fruit Frozen Yogurt"
                        && SpecialFruitFroyo.ContainsKey(heldObjectParent.Name)
                        ) // Replacement of Generic Fruit Froyo or Chocolate-Swirl Fruit Froyo with a specific variety
                    {
                        int replacementFroyoIndex = jsonAssets.GetObjectId(SpecialFruitFroyo[heldObjectParent.Name]);
                        if (replacementFroyoIndex != -1)
                        {
                            __instance.heldObject.Value = new StardewValley.Object(
                                parentSheetIndex: replacementFroyoIndex,
                                initialStack: 1
                                );
                            needsRecolor = false;
                        }
                        else
                        {
                            Monitor.Log($"Froyo Replacement failed to find item name: " + SpecialFruitFroyo[heldObjectParent.Name], LogLevel.Error);
                        }

                    }
                    else if (
                        heldObjectBase.Name == "Chocolate Swirl Fruit Frozen Yogurt"
                        && SpecialChocoFruitFroyo.ContainsKey(heldObjectParent.Name)
                        ) // Replacement of Generic Chocolate-Fruit Swirl Froyo with special flavors
                    {
                        int replacementFroyoIndex = jsonAssets.GetObjectId(SpecialChocoFruitFroyo[heldObjectParent.Name]);
                        __instance.heldObject.Value = new StardewValley.Object(
                                parentSheetIndex: replacementFroyoIndex,
                                initialStack: 1
                                );
                        needsRecolor = false;
                    }
                    else if (
                        heldObjectBase.Name == "Chocolate Swirl Fruit Frozen Yogurt"
                        && GenericChocoFruitFroyo.ContainsKey(heldObjectParent.Name)
                        ) // Replacement of special fruit froyo with a fruit, in a generic choco-fruit froyo
                    {
                        string replacementPreservedItem = GenericChocoFruitFroyo[heldObjectParent.Name];
                        int replacementPPSI;
                        if (VanillaFruitIDLookup.ContainsKey(replacementPreservedItem))
                        {
                            replacementPPSI = VanillaFruitIDLookup[replacementPreservedItem];
                        }
                        else
                        {
                            replacementPPSI = jsonAssets.GetObjectId(replacementPreservedItem);
                        }
                        __instance.heldObject.Value.preservedParentSheetIndex.Value = replacementPPSI;

                    }

                    if (needsRecolor) // Recolor generic fruit froyo/choco-fruit froyo
                    {
                        foreach (string contextTag in heldObjectParent.GetContextTagList())
                        {
                            if (contextTag.StartsWith("color_"))
                            {
                                heldObject.GetContextTags().Add(contextTag);
                                Color parentColor = (Color)StardewValley.Menus.TailoringMenu.GetDyeColor(heldObjectParent);

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

                                StardewValley.Objects.ColoredObject asColoredObject = (StardewValley.Objects.ColoredObject)__instance.heldObject.Get();
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