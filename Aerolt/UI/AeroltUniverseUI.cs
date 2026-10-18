using Aerolt.API;
using Aerolt.Enums;
using Aerolt.Helpers;
using UnityEngine;
using UniverseLib;
using UniverseLib.Config;
using UniverseLib.UI;

namespace Aerolt.UI
{
    public static class AeroltUniverseUI
    {
        private const string UiId = "com.Lodington.Aerolt.UniverseUI";

        private static UIBase? _uiBase;
        private static AeroltUniverseWindow? _window;

        public static bool IsReady => _uiBase != null && _window != null;
        public static UIBase? UiBase => _uiBase;

        public static AeroltUniverseWindow? Window => _window;

        public static void Bootstrap()
        {
            var config = new UniverseLibConfig { Force_Unlock_Mouse = true };
            Universe.Init(1f, OnReady, LogHandler, config);
        }

        private static void OnReady()
        {
            _uiBase = UniversalUI.RegisterUI(UiId, OnUpdate);
            _window = new AeroltUniverseWindow(_uiBase);
            _window.SetActive(true); // the panel stays active the UIBase root is what we toggle
            _uiBase.Enabled = false;
            Tools.Log(LogLevel.Information, "Aerolt UniverseLib UI ready.");

            AeroltAPI.NotifyReady(_window);
        }

        private static void OnUpdate()
        {
        }

        public static bool IsOpen => _uiBase != null && _uiBase.Enabled;

        public static void Toggle()
        {
            SetVisible(!IsOpen);
        }

        public static void SetVisible(bool visible)
        {
            if (_uiBase == null) return;
            _uiBase.Enabled = visible;
        }

        private static void LogHandler(string message, LogType type)
        {
            Tools.Log(type == LogType.Error ? LogLevel.Error : LogLevel.Information, $"[UniverseLib] {message}");
        }
    }
}
