using HarmonyLib;
using System;
using System.Collections.Generic;

namespace Lavender.StorageLib
{
    public static class StoragePatches
    {
        [HarmonyPatch(typeof(StorageCategoryDatabase), nameof(StorageCategoryDatabase.DeSerialize))]
        [HarmonyPostfix]
        static void StorageCategoryDatabase_DeSerialize_Postfix(ref object __result, Type type, string serializedState)
        {
            if(type == typeof(StorageCategory))
            {
                List<StorageCategory>? vanilla_db = __result as List<StorageCategory>;

                if(vanilla_db != null)
                {
                    List<StorageCategory> db = new List<StorageCategory>();
                    db.AddRange(vanilla_db);

                    List<int> category_ids = new List<int>();
                    foreach(StorageCategory category in vanilla_db)
                    {
                        category_ids.Add(category.ID);
                    }

                    foreach(StorageCategory category in Lavender.customStorageCategoryDatabase)
                    {
                        if(category_ids.Contains(category.ID))
                        {
                            LavenderLog.Log($"Replacing vanilla StorageCategory id={category.ID} with mod StorageCategory!");
                            db.Remove(db.Find((StorageCategory sc) => sc.ID == category.ID));
                            db.Add(category);
                        }
                        else
                        {
                            db.Add(category);
                        }
                    }

                    __result = db;

                    LavenderLog.Log($"Successfully added {Lavender.customStorageCategoryDatabase.Count} Mod StorageCategories");
                }
                else
                {
                    LavenderLog.Error("StorageCategory.DeSerialize: This shouldn't happen!");
                }
            }
        }

        [HarmonyPatch(typeof(StorageSpawnCategoryDatabase), nameof(StorageSpawnCategoryDatabase.DeSerialize))]
        [HarmonyPostfix]
        static void StorageSpawnCategoryDatabase_DeSerialize_Postfix(ref object __result, Type type, string serializedState)
        {
            if (type == typeof(StorageSpawnCategory))
            {
                List<StorageSpawnCategory>? vanilla_db = __result as List<StorageSpawnCategory>;

                if (vanilla_db != null)
                {
                    List<StorageSpawnCategory> db = new List<StorageSpawnCategory>();
                    db.AddRange(vanilla_db);

                    List<int> category_ids = new List<int>();
                    foreach (StorageSpawnCategory category in vanilla_db)
                    {
                        category_ids.Add(category.ID);
                    }

                    foreach (StorageSpawnCategory category in Lavender.customStorageSpawnCategoryDatabase)
                    {
                        if (category_ids.Contains(category.ID))
                        {
                            LavenderLog.Log($"Replacing vanilla StorageSpawnCategory id={category.ID} with mod StorageSpawnCategory!");
                            db.Remove(db.Find((StorageSpawnCategory sc) => sc.ID == category.ID));
                            db.Add(category);
                        }
                        else
                        {
                            db.Add(category);
                        }
                    }

                    __result = db;

                    LavenderLog.Log($"Successfully added {Lavender.customStorageSpawnCategoryDatabase.Count} Mod StorageSpawnCategories");
                }
                else
                {
                    LavenderLog.Error("StorageSpawnCategory.DeSerialize: This shouldn't happen!");
                }
            }
        }
    }
}
