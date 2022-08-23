using System.Collections.Generic;
using System.Security;
using System.Security.Permissions;
using System.Threading;
using System.Threading.Tasks;
using Aerolt.Classes;
using Aerolt.Enums;
using Aerolt.Helpers;
using Aerolt.Managers;
using Aerolt.Overrides;
using Aerolt.Social;
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
using LogLevel = Aerolt.Enums.LogLevel;
using Path = System.IO.Path;

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
        public const string Version = "4.0.5";
        public static ManualLogSource Log;
        public static GameObject _co;
        public static AssetBundle _assets;

        public static GameObject chatWindow;
        public static GameObject settingsRoot;

        public static Load Instance;
        public static Dictionary<ButtonNames, ZioConfigEntry<KeyboardShortcut>> KeyBinds = new();

        public static Dictionary<NetworkUser, GameObject> aeroltUIs = new();
        private static GameObject settingsUI;
        public static ZioConfigFile.ZioConfigFile configFile;
        public static NetworkUser tempViewer;
        public static HUD tempHud;
        public static string path;


        public void Awake()
        {
            Instance = this;
            Log = Logger;

            path = Path.GetDirectoryName(Info.Location);
            _assets = AssetBundle.LoadFromFile(Path.Combine(path!, "aeroltbundle"));
            Tools.Log(LogLevel.Information, "Loaded AssetBundle");
            _co = _assets.LoadAsset<GameObject>("PlayerCanvas");
            _assets.LoadAsset<GameObject>("AeroltUI");

            chatWindow = _assets.LoadAsset<GameObject>("ChatWindow");
            DontDestroyOnLoad(chatWindow);

            NetworkManager.Initialize();
        }

        public void Start()
        {
            RoR2Application.onLoad += GameLoad;
            HUD.shouldHudDisplay += CreateHud;
        }

        private void OnDestroy()
        {
            WebSocketClient.DisconnectClient();
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
            configFile = new ZioConfigFile.ZioConfigFile(RoR2Application.cloudStorage, "/Aerolt/Settings.cfg", true);
            CreateKeyBindSettings();
            Colors.InitColors();

            var harm = new Harmony(Info.Metadata.GUID);
            new PatchClassProcessor(harm, typeof(Hooks)).Patch();
        }

        private void CreateKeyBindSettings()
        {
            KeyBinds.Add(ButtonNames.OpenMenu,
                configFile.Bind("Keybinds", "OpenMenu", new KeyboardShortcut(KeyCode.F1)));
            if (Chainloader.PluginInfos.ContainsKey("bubbet.zioriskofoptions"))
                MakeRiskOfOptions();
        }

        private void MakeRiskOfOptions()
        {
            foreach (var value in KeyBinds.Values) ModSettingsManager.AddOption(new ZioKeyBindOption(value));
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
            Tools.Log(LogLevel.Information, "Created UI");
        }
    }
}