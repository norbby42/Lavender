using Lavender.FurnitureLib;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Lavender.Test
{
    public class FurnitureTest : MonoBehaviour
    {
        private bool _trigger = true;

        private void Update()
        {
            if (Lavender.instance.firstUpdateFinished && _trigger)
            {
                _trigger = false;

                string path = Path.Combine(Assembly.GetExecutingAssembly().Location.Substring(0, Assembly.GetExecutingAssembly().Location.Length - 17), "osml_box.json");

                Furniture? f = FurnitureCreator.Create(path);
                if (f != null)
                {
                    f.GiveItem();
                }
            }
        }
    }
}
