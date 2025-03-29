using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Lavender.StorageLib
{
    public static class StorageCreator
    {
        /* Doesn't work :(
        public static GameObject AddStorage(GameObject gameObject, string title, string useMessage,string storageCategoryName, string storageSpawnCategoryName, SpawnSettings spawnSettings, bool spawnAtStart, bool spawnItemsFromCategory)
        {
            Storage storage = gameObject.AddComponent<Storage>();
            gameObject.layer = 17; // Needs to be on the 'useable' layer so the player can interact with it

            storage.title = title;
            storage.useMessage = useMessage;

            storage.StorageSettings = storageCategoryName;
            storage.SetCategorySettings(storageCategoryName);

            storage.itemSpawnCategory = storageSpawnCategoryName;

            storage.spawnSettings = spawnSettings;

            storage.spawnAtStart = spawnAtStart;
            storage.spawnItemsFromCategory = spawnItemsFromCategory;

            return gameObject;
        }

        public static SpawnSettings CreateSimpleSpawnSettings(bool respawnEnabled, SpawnSettings.SpawnTime spawnTime)
        {
            SpawnSettings spawnSettings = new SpawnSettings();

            spawnSettings.respawnEnabled = respawnEnabled;
            spawnSettings.spawnTime = spawnTime;
            spawnSettings.useCustomSpawnTime = false;

            return spawnSettings;
        }*/
    }
}
