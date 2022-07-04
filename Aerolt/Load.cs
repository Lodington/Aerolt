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
        public const string Guid = "com.Lodington." + Name;
        public const string Version = "2.0.1";
        public static ManualLogSource Log;
        public static GameObject _co;
        public static AssetBundle _assets;

        public static GameObject settingsRoot;
        
        public static Load Instance;
        public static Dictionary<ButtonNames, ZioConfigEntry<KeyboardShortcut>> KeyBinds = new();

        public void Awake()
        {
            Instance = this;
            Log = Logger;

            var path = System.IO.Path.GetDirectoryName(Info.Location);
            _assets = AssetBundle.LoadFromFile(System.IO.Path.Combine(path!, "aeroltbundle"));
            Tools.Log(Enums.LogLevel.Information, "Loaded AssetBundle");
            _co = _assets.LoadAsset<GameObject>("PlayerCanvas"); _assets.LoadAsset<GameObject>("AeroltUI");

            var harm = new Harmony(Info.Metadata.GUID);
            new PatchClassProcessor(harm, typeof(Hooks)).Patch();
            NetworkManager.Initialize();
        }
        public void OnGUI()
        {
            if(!Esp.Instance)
                return;
            Esp.Draw();
        }

        public static void CallPopup(string title, string message, Transform parent)
        {
            return;
            //GameObject popup = Instantiate(_popup, parent);
            //popup.GetComponent<PopupManager>().SetupPopup(title, message);
        }
        public static void CallPopup(string title, string body)
        {
            CallPopup(title,body, settingsRoot.transform);
        }
        
        public void Start()
        {
            RoR2Application.onLoad += GameLoad;
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
            Colors.InitColors();
            return;
            //settingsRoot = Instantiate(_op);
            settingsUI = settingsRoot.transform.Find("SettingUIPanel").gameObject;
            settingsUI.SetActive(false);
            DontDestroyOnLoad(settingsRoot);
            var welcomeMessage = DailyMessage.GetMessage();
            var stringConverter = TomlTypeConverter.GetConverter(typeof(string));
            welcomeMessage = (string) stringConverter.ConvertToObject(welcomeMessage, typeof(string));
            
            var config = configFile.Bind("DoNotTouch", "WelcomeMessage", "", "");
            if (config.Value != welcomeMessage)
            {
                CallPopup($"Welcome To Aerolt v{Version}", welcomeMessage, settingsRoot.transform);
                config.Value = welcomeMessage;
            }
            
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
        public static HUD tempHud;

        public static void CreateHud(HUD hud, ref bool shoulddisplay)
        {
            if (!hud.cameraRigController) return;
            var viewer = hud.cameraRigController.viewer;

            if (aeroltUIs.ContainsKey(viewer)) return;
            if (settingsUI && settingsUI.activeSelf) settingsUI.SetActive(false);

            tempViewer = viewer;
            tempHud = hud;
            var ui = Instantiate(_co);
            ui.GetComponent<MPEventSystemProvider>().eventSystem = hud.eventSystemProvider.eventSystem;
            tempViewer = null;
            tempHud = null;
            /*
            ui.transform.localScale = Vector3.one;
            var rect = (RectTransform) ui.transform;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMax = Vector2.zero;
            rect.offsetMin = Vector2.zero;
            ui.transform.localPosition = Vector3.zero;
            //rect.anchoredPosition = rect.GetParentSize() * 0.5f;
            */
            aeroltUIs.Add(viewer, ui);
            Tools.Log(Enums.LogLevel.Information, "Created UI");
        }
    }
}
