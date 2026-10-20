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
        private static ItemEditorWindow? _itemEditor;

        public static bool IsReady => _uiBase != null && _window != null;
        public static UIBase? UiBase => _uiBase;

        public static AeroltUniverseWindow? Window => _window;

    
        public static void OpenItemEditor(IItemEditTarget? target = null)
        {
            if (_uiBase == null) return;

            _itemEditor ??= new ItemEditorWindow(_uiBase);
            _uiBase.Enabled = true;
            _itemEditor.SetActive(true);
            _itemEditor.SetTarget(target ?? new PlayerInventoryTarget());
        }

        public static void Bootstrap()
        {
            var config = new UniverseLibConfig { Force_Unlock_Mouse = true };
            Universe.Init(1f, OnReady, LogHandler, config);
        }

        private static void OnReady()
        {
            _uiBase = UniversalUI.RegisterUI(UiId, OnUpdate);
            _window = new AeroltUniverseWindow(_uiBase);
            Tools.Log(LogLevel.Information, "Aerolt UniverseLib UI ready.");

            // Start hidden.
            SetVisible(false);

            AeroltAPI.NotifyReady(_window);
        }

        private static void OnUpdate()
        {
        }
        public static bool IsOpen => _window != null && _window.Enabled;

        public static void Toggle()
        {
            SetVisible(!IsOpen);
        }

        public static void SetVisible(bool visible)
        {
            if (_uiBase == null || _window == null) return;
            _uiBase.Enabled = visible;
            _window.SetActive(visible);
        }

        private static void LogHandler(string message, LogType type)
        {
            Tools.Log(type == LogType.Error ? LogLevel.Error : LogLevel.Information, $"[UniverseLib] {message}");
        }
    }
}
