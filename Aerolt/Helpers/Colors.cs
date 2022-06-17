using System.Collections.Generic;
using UnityEngine;
using ZioConfigFile;

namespace Aerolt.Helpers
{
    public static class Colors
    {
        
        public static readonly Dictionary<string, Color32> DefaultColors = new Dictionary<string, Color32>()
        {
            {"Chest", Color.red},
            {"Secret_Plates", Color.cyan},
            {"Barrels", new Color32(255, 128, 0, 255)},
            {"Scrappers", Color.blue}
        };

        public static readonly Dictionary<string, ZioConfigEntry<Color>> GlobalColors = new();

        public static string GenerateColoredString(string text, Color color)
        {
            return "<color=#" + ColorUtility.ToHtmlStringRGB(color) + ">" + text + "</color>";
        }
        public static Color32 GetColor(string identifier)
        {
            if (Load.Instance.configFile == null) return Color.magenta;
            if (GlobalColors.TryGetValue(identifier, out var color)) return color.Value;
            
            color = Load.Instance.configFile.Bind("EspColors", identifier, (Color) DefaultColors[identifier], "");
            GlobalColors[identifier] = color;
            return color.Value;
        }

        public static void SetColor(string id, Color32 c) => GlobalColors[id].Value = c;

        public static string ColorToHex(Color32 color)
        {
            string hex = color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
            return hex;
        }
    }
}