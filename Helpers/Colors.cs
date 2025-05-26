using BepInEx.Bootstrap;
using UnityEngine;

namespace Aerolt_External.Helpers;

 public static class Colors
    {
        public static readonly Dictionary<string, Color32> DefaultColors = new()
        {
            { "Chest", Color.blue },
            { "Equipment", Color.yellow },
            { "Shop", Color.blue },
            { "Secret_Plates", Color.cyan },
            { "Barrels", new Color32(255, 128, 0, 255) },
            { "Scrappers", Color.blue },
            { "NewtAlter", Color.white },
            { "Shrine of Blood", Color.white },
            { "Shrine of Chance", Color.white },
            { "Cleansing Pool", Color.white },
            { "Shrine of Combat", Color.white },
            { "Altar of Gold", Color.white },
            { "Shrine of the Mountain", Color.white },
            { "Shrine of Order", Color.white },
            { "Shrine of the Woods", Color.white },
            { "Shrine", Color.white },
            { "Drone", Color.white },
            { "Printer", Color.white },
            { "Teleporter Idle", Color.magenta },
            { "Teleporter Charging", Color.yellow },
            { "Teleporter Charged", Color.green },
            { "Teleporter Finished", Color.red }
        };
        

        public static string GenerateColoredString(string text, Color color)
        {
            return "<color=#" + ColorUtility.ToHtmlStringRGB(color) + ">" + text + "</color>";
        }

        public static Color32 GetColor(string identifier)
        {
            return DefaultColors.TryGetValue(identifier, out var color) ? color : Color.white;
        }
        

        public static string ColorToHex(Color32 color)
        {
            var hex = color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
            return hex;
        }

        public static void InitColors()
        {
            foreach (var key in DefaultColors.Keys) GetColor(key);
        }
    }