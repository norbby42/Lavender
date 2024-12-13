using HarmonyLib;
using Lavender.RuntimeImporter;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Lavender.ItemLib
{
    public static class ItemPatches
    {
        [HarmonyPatch(typeof(ItemDatabase), nameof(ItemDatabase.DeSerialize))]
        [HarmonyPostfix]
        static void ItemDatabase_DeSerialize_Postfix(ref object __result, Type type, string serializedState)
        {
            if (type == typeof(List<Item>))
            {
                List<Item>? vanilla_db = __result as List<Item>;

                if (vanilla_db != null)
                {
                    List<Item> db = new List<Item>();
                    db.AddRange(vanilla_db);

                    List<int> item_ids = new List<int>();
                    foreach(Item item in vanilla_db)
                    {
                        item_ids.Add(item.ID);
                    }

                    foreach(Item item in Lavender.customItemDatabase)
                    {
                        if(item_ids.Contains(item.ID))
                        {
                            LavenderLog.Log($"Replacing vanilla Item id={item.ID} with mod Item!");
                            db.Remove(db.Find((Item i) => i.ID == item.ID));
                            db.Add(item);
                        }
                        else
                        {
                            db.Add(item);
                        }
                    }

                    __result = db;

                    LavenderLog.Log($"Successfully added {Lavender.customItemDatabase.Count} Mod Items");
                }
                else
                {
                    LavenderLog.Error("ItemDatabase.DeSerialize: This shouldn't happen!");
                }
            }

            return;
        }

        [HarmonyPatch(typeof(ItemOperations), nameof(ItemOperations.SetCollectibleItemValues))]
        [HarmonyPostfix]
        static void ItemOperations_SetCollectibleItemValues_Postfix(ItemStack item, GameObject gameObject)
        {
            CollectibleItem component = gameObject.GetComponent<CollectibleItem>();
            if (component != null)
            {
                component.Item = item.itemReference;
            }

            return;
        }

        [HarmonyPatch(typeof(Item.ItemAppearance), nameof(Item.ItemAppearance.LoadSprite))]
        [HarmonyPrefix]
        static bool Item_ItemAppearance_LoadSprite_Prefix(Item.ItemAppearance __instance)
        {
            if(!string.IsNullOrEmpty(__instance.SpritePath))
            {
                if(__instance.SpritePath.StartsWith("Lavender_SRC#"))
                {
                    string spritePath = __instance.SpritePath.Substring("Lavender_SRC#".Length);

                    __instance.Sprite = ImageLoader.LoadSprite(spritePath);

                    return false;
                }
                else if (__instance.SpritePath.StartsWith("Lavender_AB#"))
                {
                    string spritePath = __instance.SpritePath.Substring("Lavender_AB#".Length);

                    __instance.Sprite = ItemCreator.ItemSpriteFromAssetBundle(spritePath);

                    return false;
                }
            }

            return true;
        }

        [HarmonyPatch(typeof(Item.ItemAppearance), nameof(Item.ItemAppearance.Loadprefab))]
        [HarmonyPrefix]
        static bool Item_ItemAppearance_Loadprefab_Prefix(Item.ItemAppearance __instance, Item item)
        {
            bool noSkip = true;

            // Prefab
            if (__instance.PrefabPath.StartsWith("Lavender_SRC#"))
            {
                string prefabPath = __instance.PrefabPath.Substring("Lavender_SRC#".Length, __instance.PrefabPath.Length - "Lavender_SRC#".Length - 3);

                if(File.Exists(prefabPath + "png"))
                {
                    __instance.Prefab = ItemCreator.ItemPrefabFromOBJ(
                    prefabPath + "obj",
                    prefabPath + "png",
                    item.Title);
                }
                else
                {
                    __instance.Prefab = ItemCreator.ItemPrefabFromOBJ(
                    prefabPath + "obj",
                    prefabPath + "jpg",
                    item.Title);
                }

                noSkip = false;
            }
            else if (__instance.PrefabPath.StartsWith("Lavender_AB#"))
            {
                string prefabPath = __instance.PrefabPath.Substring("Lavender_AB#".Length);

                __instance.Prefab = ItemCreator.ItemPrefabFromAssetBundle(prefabPath);

                noSkip = false;
            }

            // Prefab Many
            if(!string.IsNullOrEmpty(__instance.PrefabPathMany))
            {
                if (__instance.PrefabPathMany.StartsWith("Lavender_SRC#"))
                {
                    string prefabPathMany = __instance.PrefabPathMany.Substring("Lavender_SRC#".Length, __instance.PrefabPathMany.Length - 3);

                    if (File.Exists(prefabPathMany + "png"))
                    {
                        __instance.PrefabMany = ItemCreator.ItemPrefabFromOBJ(
                        prefabPathMany + "obj",
                        prefabPathMany + "png",
                        item.Title);
                    }
                    else
                    {
                        __instance.PrefabMany = ItemCreator.ItemPrefabFromOBJ(
                        prefabPathMany + "obj",
                        prefabPathMany + "jpg",
                        item.Title);
                    }

                    noSkip = false;
                }
                else if (__instance.PrefabPathMany.StartsWith("Lavender_AB#"))
                {
                    string prefabPathMany = __instance.PrefabPathMany.Substring("Lavender_AB#".Length);

                    __instance.PrefabMany = ItemCreator.ItemPrefabFromAssetBundle(prefabPathMany);

                    noSkip = false;
                }
            }

            return noSkip;
        }
    }
}
