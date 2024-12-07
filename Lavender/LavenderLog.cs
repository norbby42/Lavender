using System;
using UnityEngine;

namespace Lavender
{
    static internal class LavenderLog
    {
        public static void Log(string message)
        {
            Debug.Log($"[<color=#9585f1>Lavender</color>] {message}");
        }

        public static void Detailed(string message)
        {
            if(BepinexPlugin.Settings.DetailedLog.Value)
            {
                Debug.Log($"[<color=#9585f1>Lavender</color>][Detailed] {message}");
            }
        }

        public static void Error(string message)
        {
            Debug.LogError($"[<color=#9585f1>Lavender</color>] {message}");
        }
    }
}
