using System.Collections.Generic;
using UnityEngine;

namespace Aerolt.Managers
{
    public static class EspHelper
    {
        public static void DrawESPLabel(Vector3 worldpos, Color textcolor, Color outlinecolor, string text,
            string outlinetext = null)
        {
            var content = new GUIContent(text);
            if (outlinetext == null) outlinetext = text;
            var content1 = new GUIContent(outlinetext);
            var style = GUI.skin.label;
            style.alignment = TextAnchor.MiddleCenter;
            var size = style.CalcSize(content);
            var pos = Camera.main.WorldToScreenPoint(worldpos);
            pos.y = Screen.height - pos.y;
            if (pos.z >= 0)
            {
                GUI.color = Color.black;
                GUI.Label(new Rect(pos.x - size.x / 2 + 1, pos.y + 1, size.x, size.y), content1);
                GUI.Label(new Rect(pos.x - size.x / 2 - 1, pos.y - 1, size.x, size.y), content1);
                GUI.Label(new Rect(pos.x - size.x / 2 + 1, pos.y - 1, size.x, size.y), content1);
                GUI.Label(new Rect(pos.x - size.x / 2 - 1, pos.y + 1, size.x, size.y), content1);
                GUI.color = textcolor;
                GUI.Label(new Rect(pos.x - size.x / 2, pos.y, size.x, size.y), content);
                GUI.color = Color.black;
            }
        }

        public static void DrawRarityESPLabel(Vector3 worldpos, Color textcolor, Color outlinecolor, string text,
            Color itemColor, string itemName = "",  string outlinetext = null)
        {
            var content = new GUIContent(text);
            if (outlinetext == null) outlinetext = text;
            var outlineContent = new GUIContent(outlinetext);
            var style = GUI.skin.label;
            style.alignment = TextAnchor.MiddleCenter;
            var size = style.CalcSize(content);
            var pos = Camera.main.WorldToScreenPoint(worldpos);
            pos.y = Screen.height - pos.y;
            if (pos.z >= 0)
            {
                GUI.color = Color.black;
                GUI.Label(new Rect(pos.x - size.x / 2 + 1, pos.y + 1, size.x, size.y), outlineContent);
                GUI.Label(new Rect(pos.x - size.x / 2 - 1, pos.y - 1, size.x, size.y), outlineContent);
                GUI.Label(new Rect(pos.x - size.x / 2 + 1, pos.y - 1, size.x, size.y), outlineContent);
                GUI.Label(new Rect(pos.x - size.x / 2 - 1, pos.y + 1, size.x, size.y), outlineContent);
                GUI.color = textcolor;
                GUI.Label(new Rect(pos.x - size.x / 2, pos.y, size.x, size.y), content);

                if (!string.IsNullOrEmpty(itemName))
                {
                    var itemContent = new GUIContent(itemName);
                    var outlineItemName = itemName;
                    var outlineItemContent = new GUIContent(outlineItemName);
                    var itemSize = style.CalcSize(itemContent);

                    GUI.color = Color.black;
                    GUI.Label(new Rect(pos.x - itemSize.x / 2 + 1, pos.y + 1 + size.y, itemSize.x, itemSize.y), outlineItemContent);
                    GUI.Label(new Rect(pos.x - itemSize.x / 2 - 1, pos.y - 1 + size.y, itemSize.x, itemSize.y), outlineItemContent);
                    GUI.Label(new Rect(pos.x - itemSize.x / 2 + 1, pos.y - 1 + size.y, itemSize.x, itemSize.y), outlineItemContent);
                    GUI.Label(new Rect(pos.x - itemSize.x / 2 - 1, pos.y + 1 + size.y, itemSize.x, itemSize.y), outlineItemContent);
                    GUI.color = itemColor;
                    GUI.Label(new Rect(pos.x - itemSize.x / 2, pos.y + size.y, itemSize.x, itemSize.y), itemContent);
                }

                GUI.color = Color.black;
            }
        }

        public static void DrawMultiShopRarityESPLabel(Vector3 worldpos, Color textcolor, Color outlinecolor, string text,
                List<Color> itemColors, List<string> itemNames, string outlinetext = null)
        {
            var content = new GUIContent(text);
            if (outlinetext == null) outlinetext = text;
            var outlineContent = new GUIContent(outlinetext);
            var style = GUI.skin.label;
            style.alignment = TextAnchor.MiddleCenter;
            var size = style.CalcSize(content);
            var pos = Camera.main.WorldToScreenPoint(worldpos);
            pos.y = Screen.height - pos.y;
            if (pos.z >= 0)
            {
                GUI.color = Color.black;
                GUI.Label(new Rect(pos.x - size.x / 2 + 1, pos.y + 1, size.x, size.y), outlineContent);
                GUI.Label(new Rect(pos.x - size.x / 2 - 1, pos.y - 1, size.x, size.y), outlineContent);
                GUI.Label(new Rect(pos.x - size.x / 2 + 1, pos.y - 1, size.x, size.y), outlineContent);
                GUI.Label(new Rect(pos.x - size.x / 2 - 1, pos.y + 1, size.x, size.y), outlineContent);
                GUI.color = textcolor;
                GUI.Label(new Rect(pos.x - size.x / 2, pos.y, size.x, size.y), content);

                for (int i = 0; i < itemNames.Count; i++)
                {
                    var itemName = itemNames[i];  
                    
                    if (!string.IsNullOrEmpty(itemName))
                    {
                        var itemColor = itemColors[i];
                        var itemContent = new GUIContent(itemName);
                        var outlineItemName = itemName;
                        var outlineItemContent = new GUIContent(outlineItemName);
                        var itemSize = style.CalcSize(itemContent);

                        GUI.color = Color.black;
                        GUI.Label(new Rect(pos.x - itemSize.x / 2 + 1, pos.y + 1 + size.y + i * itemSize.y, itemSize.x, itemSize.y), outlineItemContent);
                        GUI.Label(new Rect(pos.x - itemSize.x / 2 - 1, pos.y - 1 + size.y + i * itemSize.y, itemSize.x, itemSize.y), outlineItemContent);
                        GUI.Label(new Rect(pos.x - itemSize.x / 2 + 1, pos.y - 1 + size.y + i * itemSize.y, itemSize.x, itemSize.y), outlineItemContent);
                        GUI.Label(new Rect(pos.x - itemSize.x / 2 - 1, pos.y + 1 + size.y + i * itemSize.y, itemSize.x, itemSize.y), outlineItemContent);
                        GUI.color = itemColor;
                        GUI.Label(new Rect(pos.x - itemSize.x / 2, pos.y + size.y + i * itemSize.y, itemSize.x, itemSize.y), itemContent);
                    }
                }

            GUI.color = Color.black;
            }
        }

    public static Vector3 WorldToScreen(Vector3 worldpos)
        {
            var pos = Camera.main.WorldToScreenPoint(worldpos);
            pos.y = Screen.height - pos.y;
            return new Vector3(pos.x, pos.y);
        }
    }
}