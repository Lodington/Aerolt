using System.Collections.Generic;
using System.Security;
using System.Security.Permissions;
using Aerolt.Classes;
using Aerolt.Enums;
using Aerolt.Helpers;
using Aerolt.Managers;
using Aerolt.Overrides;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using RiskOfOptions;
using RoR2;
using RoR2.UI;
using UnityEngine;
using ZioConfigFile;
using ZioRiskOfOptions;

#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete
[module: UnverifiableCode]
namespace Aerolt 
{
    [BepInPlugin(Guid, Name, Version)]
    [BepInDependency("bubbet.zioriskofoptions")]
    public class Load : BaseUnityPlugin
    {
        public const string Name = "Aerolt";
        private const string Guid = "com.Lodington." + Name;
        public const string Version = "1.4.0";
        public static ManualLogSource Log;
        private static GameObject _co;
        private static GameObject _op;
        private static GameObject _popup;
        private static AssetBundle _assets;
        public static Load Instance;
        public static Dictionary<ButtonNames, ZioConfigEntry<KeyboardShortcut>> KeyBinds = new();

        public void Awake()
        {
            Instance = this;
            Log = Logger;

            var path = System.IO.Path.GetDirectoryName(Info.Location);
            _assets = AssetBundle.LoadFromFile(System.IO.Path.Combine(path!, "aeroltbundle"));
            Tools.Log(Enums.LogLevel.Information, "Loaded AssetBundle");
            _co = _assets.LoadAsset<GameObject>("AeroltUI");
            _op = _assets.LoadAsset<GameObject>("SettingUI");
            _popup = _assets.LoadAsset<GameObject>("Popup");

            var harm = new Harmony(Info.Metadata.GUID);
            new PatchClassProcessor(harm, typeof(Hooks)).Patch();
        }
        public void OnGUI()
        {
            if(!Esp.Instance)
                return;
            Esp.Draw();
        }

        public static void CallPopup(string title, string message, Transform parent)
        {
            GameObject popup = Instantiate(_popup, parent);
            popup.GetComponent<PopupManager>().SetupPopup(title, message);
        }
        
        
        public void Start()
        {
            RoR2Application.onLoad += GameLoad;
            SceneDirector.onPostPopulateSceneServer += Hooks.GetEspData;
            HUD.shouldHudDisplay += CreateHud;

        }

        private void Update()
        {
            if (!settingsUI) return;
            //if (GetKeyPressed(KeyBinds[ButtonNames.OpenMenu]) && aeroltUIs.Count == 0) SettingsToggle(); Disabled for now.
        }

        public static bool GetKeyPressed(ZioConfigEntry<KeyboardShortcut> entry)
        {
            foreach (var item in entry.Value.Modifiers)
            {
                if (!Input.GetKey(item))
                {
                    return false;
                }
            }
            return Input.GetKeyDown(entry.Value.MainKey);
        }

        private void SettingsToggle()
        {
            if (!settingsUI) return;
            settingsUI.SetActive(!settingsUI.activeSelf);
        }

        private void GameLoad()
        {
            configFile = new ZioConfigFile.ZioConfigFile(RoR2Application.cloudStorage, "/Aerolt/Settings.cfg", true);
            // create settings menu;
            CreateKeyBindSettings();
            var settingsRoot = Instantiate(_op);
            settingsUI = settingsRoot.transform.Find("SettingUIPanel").gameObject;
            settingsUI.SetActive(false);
            DontDestroyOnLoad(settingsRoot);
            CallPopup($"Welcome To Aerolt {Version}", "The menu will be available in game! \nJoin my discord \nNow With controller support", settingsRoot.transform);
        }

        private void CreateKeyBindSettings()
        {
            KeyBinds.Add(ButtonNames.OpenMenu, configFile.Bind("Keybinds", "OpenMenu", new KeyboardShortcut(KeyCode.F1)));
            if (Chainloader.PluginInfos.ContainsKey("bubbet.zioriskofoptions"))
                MakeRiskOfOptions();
        }

        private void MakeRiskOfOptions()
        {
            foreach (var value in KeyBinds.Values)
            {
                ModSettingsManager.AddOption(new ZioKeyBindOption(value));
            }
        }

        public static Dictionary<NetworkUser, GameObject> aeroltUIs = new();
        private static GameObject settingsUI;
        public ZioConfigFile.ZioConfigFile configFile;
        public static NetworkUser tempViewer;

        public static void CreateHud(HUD hud, ref bool shoulddisplay)
        {
            if (!hud.cameraRigController) return;
            var viewer = hud.cameraRigController.viewer;

            if (aeroltUIs.ContainsKey(viewer)) return;
            if (settingsUI && settingsUI.activeSelf) settingsUI.SetActive(false);

            tempViewer = viewer;
            var ui = Instantiate(_co, hud.mainContainer.transform, true);
            tempViewer = null;
            ui.transform.localScale = Vector3.one;
            var rect = (RectTransform) ui.transform;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMax = Vector2.zero;
            rect.offsetMin = Vector2.zero;
            ui.transform.localPosition = Vector3.zero;
            //rect.anchoredPosition = rect.GetParentSize() * 0.5f;
            aeroltUIs.Add(viewer, ui);
            Tools.Log(Enums.LogLevel.Information, "Created UI");
        }
    }
}
