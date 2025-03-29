using BepInEx;
using BepInEx.Logging;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Lavender
{
    [BepInPlugin(LCMPluginInfo.PLUGIN_GUID, LCMPluginInfo.PLUGIN_NAME, LCMPluginInfo.PLUGIN_VERSION)]
    public class BepinexPlugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log = null!;
        internal static LavenderSettings Settings = null!;

        private void Awake()
        {
            Log = Logger;
            Settings = new LavenderSettings(Config);

            Lavender.furnitureHandlers = new Dictionary<string, Lavender.FurnitureHandler>();
            Lavender.furnitureShopRestockHandlers = new Dictionary<string, Lavender.FurnitureShopRestockHandler>();
            Lavender.createdFurniture = new List<Furniture>();

            Lavender.customItemDatabase = new List<Item>();

            Lavender.customRecipeDatabase = new List<Recipe>();

            Lavender.customStorageCategoryDatabase = new List<StorageCategory>();
            Lavender.customStorageSpawnCategoryDatabase = new List<StorageSpawnCategory>();

            new Lavender();

            SceneManager.sceneLoaded += OnSceneLoaded;
            SaveController.LoadingDone += onLoadingDone;

            Log.LogInfo($"Plugin {LCMPluginInfo.PLUGIN_NAME} version {LCMPluginInfo.PLUGIN_VERSION} is loaded!");
            LavenderLog.Log($"{LCMPluginInfo.PLUGIN_NAME} version {LCMPluginInfo.PLUGIN_VERSION} is loaded!");
            Lavender.instance.isInitialized = true;

            Settings.LogUnstableStatus();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (!Lavender.instance.isInitialized) return;

            Debug.Log($"Scene: {scene.buildIndex}, {scene.name} loaded!");

            Lavender.instance.lastLoadedScene = scene.buildIndex;
            Lavender.instance.LoadingDone = false;
        }

        private void onLoadingDone()
        {
            Lavender.instance.LoadingDone = true;
            LavenderLog.Log("Scene Loading Done!");

            if (BepinexPlugin.Settings.SceneLoadingDoneNotification.Value)
            {
                Notifications.instance.CreateNotification("Lavender", "Scene Loading Done!", false);
            }
        }
    }
}
