using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Lavender.StorageLib
{
    public static class StorageCreator
    {
        public static void AddStorage(GameObject gameObject, string title, string useMessage,string storageCategoryName, string storageSpawnCategoryName, SpawnSettings spawnSettings, bool spawnAtStart, bool spawnItemsFromCategory)
        {
            Storage storage = gameObject.AddComponent<Storage>();

            storage.title = title;
            storage.useMessage = useMessage;

            storage.StorageSettings = storageCategoryName;
            storage.SetCategorySettings(storageCategoryName);

            storage.itemSpawnCategory = storageSpawnCategoryName;

            storage.spawnSettings = spawnSettings;

            storage.spawnAtStart = spawnAtStart;
            storage.spawnItemsFromCategory = spawnItemsFromCategory;
        }

        public static SpawnSettings CreateSimpleSpawnSettings(bool respawnEnabled, SpawnSettings.SpawnTime spawnTime)
        {
            SpawnSettings spawnSettings = new SpawnSettings();

            spawnSettings.respawnEnabled = respawnEnabled;
            spawnSettings.spawnTime = spawnTime;
            spawnSettings.useCustomSpawnTime = false;

            return spawnSettings;
        }
    }
}
