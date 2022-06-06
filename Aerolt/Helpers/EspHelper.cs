using UnityEngine;

namespace Aerolt.Managers
{
    public class EspHelper
    {
        public bool value;
        public string name;
        public void ToggleOption(bool toggle)
        {
            value = toggle;
        }
        
        public bool InScreenView(Vector3 scrnpt)
        {
            if (scrnpt.z <= 0f || scrnpt.x <= 0f || scrnpt.x >= 1f || scrnpt.y <= 0f || scrnpt.y >= 1f)
                return false;

            return true;
        }

        public float GetDistance(Vector3 endpos)
        {
            return (float)System.Math.Round(Vector3.Distance(Camera.main.transform.position, endpos));
        }
        public void DrawESPLabel(Vector3 worldpos, Color textcolor, Color outlinecolor, string text, string outlinetext = null)
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