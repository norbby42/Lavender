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

        private void Awake()
        {
            Log = Logger;

            Lavender.furnitureHandlers = new Dictionary<string, Lavender.FurnitureHandler>();
            Lavender.furnitureShopRestockHandlers = new Dictionary<string, Lavender.FurnitureShopRestockHandler>();

            new Lavender();

            SceneManager.sceneLoaded += OnSceneLoaded;

            Log.LogInfo($"Plugin {LCMPluginInfo.PLUGIN_NAME} version {LCMPluginInfo.PLUGIN_VERSION} is loaded!");
            Lavender.instance.isInitialized = true;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (!Lavender.instance.isInitialized) return;

            Debug.Log($"Scene: {scene.buildIndex}, {scene.name} loaded!");

            Lavender.instance.lastLoadedScene = scene.buildIndex;
            Lavender.instance.firstUpdateFinished = false;

            if (scene.buildIndex != 0)
            {
                GameObject sro = new GameObject("SceneRuntimeObject", typeof(SceneRuntimeObject));
            }
        }
    }
}
