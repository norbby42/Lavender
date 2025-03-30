using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Lavender.StorageLib
{
    public static class StorageCreator
    {
        public static Storage AddSimpleStorage(GameObject gameObject, string title, string useMessage, string storageCategoryName)
        {
            Storage storage = gameObject.AddComponent<Storage>();
            gameObject.layer = 17;

            storage.title = title;
            storage.useMessage = useMessage;
            storage.StorageSettings = storageCategoryName;

            return storage;
        }
    }
}
