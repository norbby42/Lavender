using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using HarmonyLib;
using Lavender.FurnitureLib;
using Lavender.ItemLib;
using FullSerializer;

namespace Lavender
{
    public class Lavender
    {
        public static Lavender instance;

        public Harmony harmony;

        public bool isInitialized;

        /// <summary>
        /// The build index of the last scene during the "SceneManager.sceneLoaded" callback
        /// </summary>
        public int lastLoadedScene = 0;

        /// <summary>
        /// You want to execute your mod logic only when LoadingDone = true to make sure that all game logic is already initialized!
        /// </summary>
        public bool LoadingDone;

        public Lavender()
        {
            if (instance == null) instance = this;
            else return;

            harmony = new Harmony(LCMPluginInfo.PLUGIN_GUID);

            harmony.PatchAll(typeof(FurniturePatches));
            harmony.PatchAll(typeof(ItemPatches));
        }

        #region FurnitureLib

        public delegate Furniture FurnitureHandler(Furniture furniture);
        public delegate List<BuildingSystem.FurnitureInfo> FurnitureShopRestockHandler(FurnitureShopName name);

        public static Dictionary<string, FurnitureHandler> furnitureHandlers;
        public static Dictionary<string, FurnitureShopRestockHandler> furnitureShopRestockHandlers;

        public static List<Furniture> createdFurniture;

        public static Furniture? GetFurnitureByTitel(string titel)
        {
            return createdFurniture.Find((Furniture f) => f.title.Equals(titel));
        }

        /// <summary>
        /// Gets all FurnitureHandler methods defined in the given Type: type and registers them for the Handler callback
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool AddFurnitureHandlers(Type type)
        {
            MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance).Where(m => m.GetCustomAttributes(typeof(FurnitureHandlerAttribute), false).Length > 0).ToArray();

            foreach (MethodInfo method in methods)
            {
                FurnitureHandlerAttribute attribute = method.GetCustomAttribute<FurnitureHandlerAttribute>();

                if (!method.IsStatic)
                {
                    LavenderLog.Error($"'{method.DeclaringType.Name}.{method.Name}' is an instance method, but furniture handler methods must be static");
                    return false;
                }

                Delegate furnitureHandler = Delegate.CreateDelegate(typeof(FurnitureHandler), method, false);
                if (furnitureHandler != null)
                {
                    if (furnitureHandlers.ContainsKey(attribute.FurnitureTitle))
                    {
                        LavenderLog.Error($"DuplicateHandlerException: '{method.DeclaringType}.{method.Name}' Only one handler method is allowed per furniture!");
                        return false;
                    }
                    else
                    {
                        furnitureHandlers.Add(attribute.FurnitureTitle, (FurnitureHandler)furnitureHandler);
                    }
                }
                else
                {
                    LavenderLog.Error($"InvalidHandlerSignatureException: '{method.DeclaringType}.{method.Name}' doesn't match any acceptable furniture handler method signatures! Furniture handler methods should have a 'Furniture' parameter and should return 'Furniture'.");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Gets all FurnitureShopRestockHandler methods defined in the given Type: type and registers them for the Handler callback
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool AddFurnitureShopRestockHandlers(Type type)
        {
            MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance).Where(m => m.GetCustomAttributes(typeof(FurnitureShopRestockHandlerAttribute), false).Length > 0).ToArray();

            foreach (MethodInfo method in methods)
            {
                FurnitureShopRestockHandlerAttribute attribute = method.GetCustomAttribute<FurnitureShopRestockHandlerAttribute>();

                if (!method.IsStatic)
                {
                    LavenderLog.Error($"'{method.DeclaringType.Name}.{method.Name}' is an instance method, but furniture shop restock handler methods must be static");
                    return false;
                }

                Delegate furnitureHandler = Delegate.CreateDelegate(typeof(FurnitureShopRestockHandler), method, false);
                if (furnitureHandler != null)
                {
                    if (furnitureShopRestockHandlers.ContainsKey(attribute.HandlerUID))
                    {
                        LavenderLog.Error($"DuplicateHandlerException: '{method.DeclaringType}.{method.Name}' Only one handler method is allowed per UID!");
                        return false;
                    }
                    else
                    {
                        furnitureShopRestockHandlers.Add(attribute.HandlerUID, (FurnitureShopRestockHandler)furnitureHandler);
                    }
                }
                else
                {
                    LavenderLog.Error($"InvalidHandlerSignatureException: '{method.DeclaringType}.{method.Name}' doesn't match any acceptable furniture shop restock handler method signatures! Furniture handler methods should have a 'FurnitureShopName' parameter and should return 'List<BuildingSystem.FurnitureInfo>'.");
                    return false;
                }
            }

            return true;
        }

        #endregion

        #region ItemLib

        private static readonly fsSerializer JSON_serializer = new fsSerializer();

        public static List<Item> customItemDatabase;

        public static void AddCustomItem(Item item, string mod_name)
        {
            item.Categories.AddToArray("Lavender ModItem");
            item.Categories.AddToArray(mod_name);

            customItemDatabase.Add(item);
        }

        public static void AddCustomItemsFromJson(string jsonPath, string mod_name)
        {
            if (!File.Exists(jsonPath)) { LavenderLog.Error($"AddCustomItemsFromJson(): File at path '{jsonPath}' doesn't exists!");  return; }

            try
            {
                fsData data = fsJsonParser.Parse(File.ReadAllText(jsonPath));
                object result = null;
                ItemDatabase.JSON_serializer.TryDeserialize(data, typeof(List<Item>), ref result).AssertSuccessWithoutWarnings();

                List<Item> Items = result as List<Item>;

                foreach (Item i in Items)
                {
                    string path = jsonPath.Substring(0, jsonPath.Length - Path.GetFileName(jsonPath).Length);

                    // Sprite Path
                    if(i.Appearance.SpritePath.EndsWith(".png") || i.Appearance.SpritePath.EndsWith(".jpg"))
                    {
                        i.Appearance.SpritePath = "Lavender_SRC#" + path + i.Appearance.SpritePath;
                    }
                    else if(i.Appearance.SpritePath.Contains("#AB"))
                    {
                        // path to assetbundle + #AB<Sprite_Name>
                        i.Appearance.SpritePath = "Lavender_AB#" + path + i.Appearance.SpritePath;
                    }

                    // Prefab Path
                    if(i.Appearance.PrefabPath.EndsWith(".obj"))
                    {
                        i.Appearance.PrefabPath = "Lavender_SRC#" + path + i.Appearance.PrefabPath;
                    }
                    else if (i.Appearance.PrefabPath.Contains("#AB")) 
                    {
                        i.Appearance.PrefabPath = "Lavender_AB#" + path + i.Appearance.PrefabPath;
                    }

                    // Prefab Path Many
                    if (i.Appearance.PrefabPathMany.EndsWith(".obj"))
                    {
                        i.Appearance.PrefabPathMany = "Lavender_SRC#" + path + i.Appearance.PrefabPathMany;
                    }
                    else if (i.Appearance.PrefabPathMany.Contains("#AB"))
                    {
                        i.Appearance.PrefabPathMany = "Lavender_AB#" + path + i.Appearance.PrefabPathMany;
                    }

                    AddCustomItem(i, mod_name);
                }
            }
            catch(Exception e)
            {
                LavenderLog.Error($"Error while loading '{mod_name}'s Item Database!\nException: {e}");
            }
        }

        #endregion
    }
}
