using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Lavender.StorageLib
{
    public static class StorageCreator
    {
        /// <summary>
        /// Setups a Storage component without any spawn information.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="title">Name of the Storage</param>
        /// <param name="useMessage">Text seen when trying to interact with it. e.g. search</param>
        /// <param name="storageCategoryName"></param>
        /// <returns></returns>
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
