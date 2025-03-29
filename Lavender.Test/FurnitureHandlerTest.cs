using Lavender.FurnitureLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using UnityEngine;
using Lavender.StorageLib;

namespace Lavender.Test
{
    public class FurnitureHandlerTest
    {
        [FurnitureHandlerAttribute("OSML Box")]
        public static Furniture osmlBoxHandler(Furniture furniture)
        {
            Debug.Log($"[{furniture.title}] Hello!");

            return furniture;
        }

        [FurnitureShopRestockHandlerAttribute("LavenderTest")]
        public static List<BuildingSystem.FurnitureInfo> furShopRestockHandler(FurnitureShopName name)
        {
            List<BuildingSystem.FurnitureInfo> restock = new List<BuildingSystem.FurnitureInfo>();

            string path = Path.Combine(Assembly.GetExecutingAssembly().Location.Substring(0, Assembly.GetExecutingAssembly().Location.Length - 17), "osml_box.json");

            BuildingSystem.FurnitureInfo? info = FurnitureCreator.CreateShopFurniture(path, 20);
            if(info != null) restock.Add(info);

            return restock;
        }
    }
}
