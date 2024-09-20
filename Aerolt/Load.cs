using System.Collections.Generic;
using System.Net;
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
using Rewired;
using RiskOfOptions;
using RoR2;
using RoR2.UI;
using UnityEngine;
using ZioConfigFile;
using ZioRiskOfOptions;
using LogLevel = Aerolt.Enums.LogLevel;


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
        public const string Version = "4.1.0";
        public static ManualLogSource Log;
        public static GameObject Co;
        public static AssetBundle Assets;

        public static Load Instance = null!;

        public static Dictionary<ButtonNames, ZioConfigEntry<KeyboardShortcut>> KeyBinds = new();

        public static Dictionary<NetworkUser, GameObject> AeroltUIs = new();
        private static GameObject _settingsUI;
        public static ZioConfigFile.ZioConfigFile ConfigFile;
        public static NetworkUser TempViewer;
        public static HUD TempHud;
        public static string Path;


        public void Awake()
        {
            Instance = this;
            Log = Logger;

            Path = System.IO.Path.GetDirectoryName(Info.Location)!;
            Assets = AssetBundle.LoadFromFile(System.IO.Path.Combine(Path!, "aeroltbundle"));
            Tools.Log(LogLevel.Information, "Loaded AssetBundle");
            Co = Assets.LoadAsset<GameObject>("PlayerCanvas");

            Assets.LoadAsset<GameObject>("AeroltUI");

            Tools.Log(LogLevel.Information, Tools.SendCount());
            NetworkManager.Initialize();
        }

        public void Start()
        {
            RoR2Application.onLoad += GameLoad;
            HUD.shouldHudDisplay += CreateHud;
        }

        public void OnGUI()
        {
            if (!Esp.Instance)
                return;
            Esp.Draw();
        }

        public static bool GetKeyPressed(ZioConfigEntry<KeyboardShortcut> entry)
        {
            foreach (var item in entry.Value.Modifiers)
                if (!Input.GetKey(item))
                    return false;
            return Input.GetKeyDown(entry.Value.MainKey);
        }

        private void GameLoad()
        {
            ConfigFile = new ZioConfigFile.ZioConfigFile(RoR2Application.cloudStorage, "/Aerolt/Settings.cfg", true);
            CreateKeyBindSettings();
            Colors.InitColors();

            var harm = new Harmony(Info.Metadata.GUID);
            new PatchClassProcessor(harm, typeof(Hooks)).Patch();
        }

        private void CreateKeyBindSettings()
        {
            KeyBinds.Add(ButtonNames.OpenMenu,
                ConfigFile.Bind("Keybinds", "OpenMenu", new KeyboardShortcut(KeyCode.F1)));
            if (Chainloader.PluginInfos.ContainsKey("bubbet.zioriskofoptions"))
                MakeRiskOfOptions();
        }

        private void MakeRiskOfOptions()
        {
            foreach (var value in KeyBinds.Values) ModSettingsManager.AddOption(new ZioKeyBindOption(value));
            ModSettingsManager.SetModIcon(Assets.LoadAsset<Sprite>("NewLogo"));
        }


        public static void CreateHud(HUD hud, ref bool shoulddisplay)
        {
            if (!hud.cameraRigController) return;
            
            if (hud.gameObject.GetComponent<AeroltHudLoader>()) return;
            
            var viewer = hud.cameraRigController.viewer;
            if (AeroltUIs.ContainsKey(viewer)) return;
            
            var loader = hud.gameObject.AddComponent<AeroltHudLoader>();
            loader.hud = hud;
            
            loader.Invoke(nameof(AeroltHudLoader.SpawnHud), 3);
        }
        public class AeroltHudLoader : MonoBehaviour
        {
            public HUD hud;

            public void SpawnHud()
            {
                if (!hud.cameraRigController) return;
                var viewer = hud.cameraRigController.viewer;

                if (AeroltUIs.ContainsKey(viewer)) return;
                if (_settingsUI && _settingsUI.activeSelf) _settingsUI.SetActive(false);

                TempViewer = viewer;
                TempHud = hud;
                var ui = Instantiate(Co);
                ui.GetComponent<MPEventSystemProvider>().eventSystem = hud.eventSystemProvider.eventSystem;
                TempViewer = null;
                TempHud = null;
                AeroltUIs.Add(viewer, ui);
                Tools.Log(LogLevel.Information, "Created UI");
            }
        }
    }
}