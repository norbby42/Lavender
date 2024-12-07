using System;
using BepInEx;
using BepInEx.Logging;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Lavender.Test
{
    [BepInPlugin(LCMPluginInfo.PLUGIN_GUID, LCMPluginInfo.PLUGIN_NAME, LCMPluginInfo.PLUGIN_VERSION)]
    public class BepinexPlugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log = null!;

        void Awake()
        {
            Log = Logger;

            SceneManager.sceneLoaded += OnSceneLoaded;

            Lavender.AddFurnitureHandlers(typeof(FurnitureHandlerTest));
            Lavender.AddFurnitureShopRestockHandlers(typeof(FurnitureHandlerTest));

            Log.LogInfo($"Plugin {LCMPluginInfo.PLUGIN_NAME} version {LCMPluginInfo.PLUGIN_VERSION} is loaded!");
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (!Lavender.instance.isInitialized) return;

            if (scene.buildIndex != 0)
            {
                GameObject ft = new GameObject("FurnitureTest", typeof(FurnitureTest));
            }
        }
    }
}
