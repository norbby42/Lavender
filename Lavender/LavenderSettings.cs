using System;
using BepInEx.Configuration;

namespace Lavender
{
    public class LavenderSettings(ConfigFile config)
    {
        public ConfigEntry<bool> DetailedLog = config.Bind<bool>("Log", "DetailedLog", false, "Enable Detailed Log Output");
        public ConfigEntry<bool> SceneRuntimeObjectNotification = config.Bind<bool>("Log", "SceneRuntimeObjectNotification", true, "Enable SceneRuntimeObject Notification");

        #region UNSTABLE PATCH SETTINGS

        public ConfigEntry<bool> FurnitureShop_AddFurniture_Prefix_SkipOriginal = config.Bind<bool>("UNSTABLE!", "FurnitureShop_AddFurniture_Prefix_SkipOriginal", true, "Only set this to false if you know what your doing!");
        public ConfigEntry<bool> FurnitureShop_Restock_Prefix_SkipOriginal = config.Bind<bool>("UNSTABLE!", "FurnitureShop_Restock_Prefix_SkipOriginal", true, "Only set this to false if you know what your doing!");
        public ConfigEntry<bool> BuildingSystem_AddFurniture_Prefix_SkipOriginal = config.Bind<bool>("UNSTABLE!", "BuildingSystem_AddFurniture_Prefix_SkipOriginal", true, "Only set this to false if you know what your doing!");

        public void LogUnstableStatus()
        {
            if (!FurnitureShop_AddFurniture_Prefix_SkipOriginal.Value) LavenderLog.Log("WARNING! FurnitureShop_AddFurniture_Prefix_SkipOriginal = FALSE");
            if (!FurnitureShop_Restock_Prefix_SkipOriginal.Value) LavenderLog.Log("WARNING! FurnitureShop_Restock_Prefix_SkipOriginal = FALSE");
            if (!BuildingSystem_AddFurniture_Prefix_SkipOriginal.Value) LavenderLog.Log("WARNING! BuildingSystem_AddFurniture_Prefix_SkipOriginal = FALSE");
        }

        #endregion
    }
}
