using UnityEngine;
using ZioConfigFile;

namespace Aerolt.UI
{

    public static class AeroltIcon
    {
        private const int Size = 48;
        private const int Margin = 8;

        private static Texture? _logo;
        private static bool _loaded;

        public static ZioConfigEntry<bool> ShowIcon = null!;

        public static void Init()
        {
            ShowIcon ??= Load.ConfigFile.Bind("General", "Show Menu Icon", true,
                "Show the clickable Aerolt logo in the top-left corner. The menu can still be opened with its keybind.");
        }

        public static void Draw()
        {

            if (ShowIcon != null && !ShowIcon.Value)
                return;

            if (!_loaded)
            {
                _loaded = true;
                var sprite = Load.Assets ? Load.Assets.LoadAsset<Sprite>("NewLogo") : null;
                if (sprite) _logo = sprite!.texture;
            }

            var rect = new Rect(Margin, Margin, Size, Size);
            var clicked = _logo
                ? GUI.Button(rect, _logo, GUIStyle.none)
                : GUI.Button(rect, "Aerolt");

            if (clicked)
                AeroltUniverseUI.Toggle();
        }
    }
}
