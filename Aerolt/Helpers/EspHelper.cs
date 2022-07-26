using UnityEngine;

namespace Aerolt.Managers
{
    public static class EspHelper
    {

        public static void DrawESPLabel(Vector3 worldpos, Color textcolor, Color outlinecolor, string text, string outlinetext = null)
        {
            GUIContent content = new GUIContent(text);
            if (outlinetext == null) outlinetext = text;
            GUIContent content1 = new GUIContent(outlinetext);
            GUIStyle style = GUI.skin.label;
            style.alignment = TextAnchor.MiddleCenter;
            Vector2 size = style.CalcSize(content);
            Vector3 pos = Camera.main.WorldToScreenPoint(worldpos);
            pos.y = Screen.height - pos.y;
            if (pos.z >= 0)
            {
                GUI.color = Color.black;
                GUI.Label(new Rect((pos.x - size.x / 2) + 1, pos.y + 1, size.x, size.y), content1);
                GUI.Label(new Rect((pos.x - size.x / 2) - 1, pos.y - 1, size.x, size.y), content1);
                GUI.Label(new Rect((pos.x - size.x / 2) + 1, pos.y - 1, size.x, size.y), content1);
                GUI.Label(new Rect((pos.x - size.x / 2) - 1, pos.y + 1, size.x, size.y), content1);
                GUI.color = textcolor;
                GUI.Label(new Rect(pos.x - size.x / 2, pos.y, size.x, size.y), content);
                GUI.color = Color.black;
            }
        }
        public static Vector3 WorldToScreen(Vector3 worldpos)
        {
            Vector3 pos = Camera.main.WorldToScreenPoint(worldpos);
            pos.y = Screen.height - pos.y;
            return new Vector3(pos.x, pos.y);
        }
    }
}