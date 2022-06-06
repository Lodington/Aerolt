using System.Collections.Generic;
using UnityEngine;

namespace Aerolt.Helpers
{
    public static class Colors
    {
        public static readonly Dictionary<string, Color32> GlobalColors = new Dictionary<string, Color32>()
        {
            {"Chest", Color.red},
            {"Secret_Plates", Color.cyan},
            {"Barrels", new Color32(255, 128, 0, 255)},
            {"Scrappers", Color.blue}
        };
        
        public static string GenerateColoredString(string text, Color color)
        {
            return "<color=#" + ColorUtility.ToHtmlStringRGB(color) + ">" + text + "</color>";
        }
        public static Color32 GetColor(string identifier)
        {
            if (GlobalColors.TryGetValue(identifier, out var toret))
                return toret;

            return Color.magenta;
        }

        public static void SetColor(string id, Color32 c) => GlobalColors[id] = c;

        public static string ColorToHex(Color32 color)
        {
            string hex = color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
            return hex;
        }
    }
}