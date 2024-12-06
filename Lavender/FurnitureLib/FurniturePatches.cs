using BehaviorDesigner.Runtime.Tasks;
using HarmonyLib;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using static Money;

namespace Lavender.FurnitureLib
{
    public static class FurniturePatches
    {
        [HarmonyPatch(typeof(SavableScriptableObject), nameof(SavableScriptableObject.LoadFromPath))]
        [HarmonyPrefix]
        static bool SavableScriptableObject_LoadFromPath_Prefix(SavableScriptableObject __instance, ref ScriptableObject __result)
        {
            if (!string.IsNullOrEmpty(__instance.addressableAssetPath))
            {
                if(__instance.addressableAssetPath.StartsWith("Lavender"))
                {
                    string sep = "<#>";
                    string path = __instance.addressableAssetPath.Substring(__instance.addressableAssetPath.IndexOf(sep) + 3);

                    try
                    {
                        if (File.Exists(path))
                        {
                            string rawFurnitureConfig = File.ReadAllText(path);

                            FurnitureConfig furnitureConfig = JsonConvert.DeserializeObject<FurnitureConfig>(rawFurnitureConfig);
                            furnitureConfig.assetBundlePath = path.Substring(0, path.Length - Path.GetFileName(path).Length) + furnitureConfig.assetBundlePath;
                            Furniture f = FurnitureCreator.FurnitureConfigToFurniture(furnitureConfig);
                            f.addressableAssetPath = $"Lavender<#>{path}";

                            if (Lavender.furnitureHandlers.TryGetValue(f.title, out Lavender.FurnitureHandler handler))
                            {
                                f = handler.Invoke(f);
                            }

                            __result = f;
                        }

                        __result = null;
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                        __result = null;
                    }

                    return false;
                }
            }

            return true;
        }

        [HarmonyPatch(typeof(FurnitureShop), nameof(FurnitureShop.AddFurniture))]
        [HarmonyPrefix]
        static bool FurnitureShop_AddFurniture_Prefix_Skip(FurnitureShop __instance,ref bool __result, Furniture furniture, int amount)
        {
            BuildingSystem.FurnitureInfo furnitureInfo = __instance.availableFurnitures.Find((BuildingSystem.FurnitureInfo f) => f.furniture.title == furniture.title);
            if (furnitureInfo == null || furnitureInfo.furniture == null)
            {
                TaskItem taskItem = (TaskItem)ScriptableObject.CreateInstance(typeof(TaskItem));
                taskItem.itemName = furniture.title;
                taskItem.itemDetails = furniture.details;
                taskItem.image = furniture.image;
                taskItem.itemType = TaskItem.Type.Furnitures;
                __instance.availableFurnitures.Add(new BuildingSystem.FurnitureInfo(furniture, taskItem, null, amount, null));
                __result = true;
            }
            furnitureInfo.amount += amount;
            __result = true;

            return false;
        }

        [HarmonyPatch(typeof(FurnitureShop), nameof(FurnitureShop.Restock))]
        [HarmonyPrefix]
        static bool FurnitureShop_Restock_Prefix(FurnitureShop __instance)
        {
            __instance.MoneyRestock();

            MethodInfo methodInfo = typeof(FurnitureShop).GetMethod("UpdateShopItems", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
            methodInfo.Invoke(__instance, new object[] { });

            Debug.Log($"[Lavender] Restocking '{(__instance.title != "" ? __instance.title : "One Stop Shop")}'");

            FurnitureShopName name = (__instance.title == "" ? FurnitureShopName.OneStopShop : (__instance.title == "Möbelmann Furnitures" ? FurnitureShopName.MoebelmannFurnitures : (__instance.title == "Jonasson's Shop" ? FurnitureShopName.SamuelJonasson : FurnitureShopName.None)));

            if (name != FurnitureShopName.None)
            {
                foreach (var pair in Lavender.furnitureShopRestockHandlers)
                {
                    __instance.availableFurnitures.AddRange(pair.Value.Invoke(name));
                }
            }

            return false;
        }

        [HarmonyPatch(typeof(BuildingSystem), nameof(BuildingSystem.AddFurniture))]
        [HarmonyPrefix]
        static bool BuildingSystem_AddFurniture_Prefix(BuildingSystem __instance, ref bool __result, Furniture furniture, GameObject gameObject, out GameObject savedGameObject, int amount)
        {
            BuildingSystem.FurnitureInfo furnitureInfo = __instance.availableFurnitures.Find((BuildingSystem.FurnitureInfo f) => f.furniture.title == furniture.title && f.gameObject == null);
            if (gameObject != null)
            {
                gameObject.transform.SetParent((__instance.inventoryLocation != null) ? __instance.inventoryLocation : __instance.transform);
                gameObject.transform.localPosition = Vector3.zero;
                if (!__instance.HasSaveableContent(gameObject))
                {
                    UnityEngine.Object.Destroy(gameObject);
                    gameObject = null;
                }
            }
            savedGameObject = gameObject;
            BuildingSystem.FurnitureInfo info = __instance.availableFurnitures.Find((BuildingSystem.FurnitureInfo f) => f.furniture.title == furniture.title);
            TaskItem taskItem = BSAddTaskItem(furniture, info, amount);
            if (furnitureInfo == null || furnitureInfo.furniture == null || gameObject != null)
            {
                __instance.availableFurnitures.Add(new BuildingSystem.FurnitureInfo(furniture, taskItem, gameObject, amount, null));
                __instance.availableFurnitures.Sort((BuildingSystem.FurnitureInfo slot1, BuildingSystem.FurnitureInfo slot2) => slot1.furniture.name.CompareTo(slot2.furniture.name));
                return true;
            }
            furnitureInfo.amount += amount;
            return true;
        }

        static TaskItem BSAddTaskItem(Furniture furniture, BuildingSystem.FurnitureInfo info, int amount)
        {
            TaskItem taskItem;
            if (info == null || info.furniture == null)
            {
                taskItem = (TaskItem)ScriptableObject.CreateInstance(typeof(TaskItem));
                taskItem.itemName = furniture.title;
                taskItem.itemDetails = furniture.details;
                taskItem.image = furniture.image;
                taskItem.itemType = TaskItem.Type.Furnitures;
                TaskItemsManager.instance.AddTaskItem(taskItem, amount, false, null, false);
            }
            else
            {
                taskItem = info.taskItem;
                TaskItemsManager.instance.AddTaskItem(info.taskItem, amount, false, null, false);
            }
            return taskItem;
        }
    }
}
