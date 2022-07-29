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
        public const string Version = "4.0.0";
        public static ManualLogSource Log;
        public static GameObject _co;
        public static AssetBundle _assets;

        public static GameObject settingsRoot;
        
        public static Load Instance;
        public static Dictionary<ButtonNames, ZioConfigEntry<KeyboardShortcut>> KeyBinds = new();
        
        public static Dictionary<NetworkUser, GameObject> aeroltUIs = new();
        private static GameObject settingsUI;
        public ZioConfigFile.ZioConfigFile configFile;
        public static NetworkUser tempViewer;
        public static HUD tempHud;


        public void Awake()
        {
            Instance = this;
            Log = Logger;

            var path = System.IO.Path.GetDirectoryName(Info.Location);
            _assets = AssetBundle.LoadFromFile(System.IO.Path.Combine(path!, "aeroltbundle"));
            Tools.Log(Enums.LogLevel.Information, "Loaded AssetBundle");
            _co = _assets.LoadAsset<GameObject>("PlayerCanvas"); _assets.LoadAsset<GameObject>("AeroltUI");
            
            NetworkManager.Initialize();
        }
        public void OnGUI()
        {
            if(!Esp.Instance)
                return;
            Esp.Draw();
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
            CreateKeyBindSettings();
            Colors.InitColors();
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
            aeroltUIs.Add(viewer, ui);
            Tools.Log(Enums.LogLevel.Information, "Created UI");
        }
    }
}
