using BepInEx;
using BepInEx.Logging;
using Lavender.FurnitureLib;
using Lavender.CommandLib;
using System.Reflection;
using System.IO;

namespace Lavender.Test
{
    [BepInPlugin(LCMPluginInfo.PLUGIN_GUID, LCMPluginInfo.PLUGIN_NAME, LCMPluginInfo.PLUGIN_VERSION)]
    public class BepinexPlugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log = null!;

        void Awake()
        {
            Log = Logger;

            SaveController.LoadingDone += onLoadingDone;

            Lavender.AddFurniturePrefabHandlers(typeof(FurnitureHandlerTest));
            Lavender.AddFurnitureShopRestockHandlers(typeof(FurnitureHandlerTest));

            // Item test
            string itemsPath = Path.Combine(Assembly.GetExecutingAssembly().Location.Substring(0, Assembly.GetExecutingAssembly().Location.Length - 17), "Items.json");
            Lavender.AddCustomItemsFromJson(itemsPath, LCMPluginInfo.PLUGIN_NAME);

            string recipesPath = Path.Combine(Assembly.GetExecutingAssembly().Location.Substring(0, Assembly.GetExecutingAssembly().Location.Length - 17), "Recipes.json");
            Lavender.AddCustomRecipesFromJson(recipesPath, LCMPluginInfo.PLUGIN_NAME);

            //Console test
            CommandManager.RegisterCommand(new TestCommandEcho());

            //Storage test
            string storageSettingsPath = Path.Combine(Assembly.GetExecutingAssembly().Location.Substring(0, Assembly.GetExecutingAssembly().Location.Length - 17), "StorageSettings.json");
            Lavender.AddCustomStorageCategoryFromJson(storageSettingsPath, LCMPluginInfo.PLUGIN_NAME);

            Log.LogInfo($"Plugin {LCMPluginInfo.PLUGIN_NAME} version {LCMPluginInfo.PLUGIN_VERSION} is loaded!");
        }

        private void onLoadingDone()
        {
            // !Only add Furniture after Loading is done
            string path = Path.Combine(Assembly.GetExecutingAssembly().Location.Substring(0, Assembly.GetExecutingAssembly().Location.Length - 17), "osml_box.json");

            Furniture? f = FurnitureCreator.Create(path);
            if (f != null) f.GiveItem();
        }
    }
}
