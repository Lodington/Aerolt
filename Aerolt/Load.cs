using System.Collections.Generic;
using System.Security;
using System.Security.Permissions;
using Aerolt.Classes;
using Aerolt.Enums;
using Aerolt.Helpers;
using Aerolt.Managers;
using Aerolt.Overrides;
using Aerolt.UI;
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
        public const string Version = "4.2.0";
        public static ManualLogSource Log = null!;
        public static AssetBundle Assets = null!;

        public static Load Instance = null!;

        public static Dictionary<ButtonNames, ZioConfigEntry<KeyboardShortcut>> KeyBinds = new();

        public static ZioConfigFile.ZioConfigFile ConfigFile = null!;
        public static string Path = null!;


        public void Awake()
        {
            Instance = this;
            Log = Logger;

            Path = System.IO.Path.GetDirectoryName(Info.Location)!;
            Assets = AssetBundle.LoadFromFile(System.IO.Path.Combine(Path!, "aeroltbundle"));
            Tools.Log(LogLevel.Information, "Loaded AssetBundle");

            Tools.SendCount();
            NetworkManager.Initialize();
        }

        public void Start()
        {
            RoR2Application.onLoad += GameLoad;
        }

        public void OnGUI()
        {
            if (AeroltUniverseUI.IsReady)
                AeroltIcon.Draw();

            if (!Esp.AnyEnabled)
                return;
            if (Event.current.type != EventType.Repaint)
                return;
            Esp.Draw();
        }

        public void Update()
        {
            if (AeroltUniverseUI.IsReady &&
                KeyBinds.TryGetValue(ButtonNames.OpenMenu, out var entry) &&
                GetKeyPressed(entry))
                AeroltUniverseUI.Toggle();
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

            IconCache.Warm(this);
            Esp.Init();
            AeroltIcon.Init();
            Run.onRunStartGlobal += OnRunStart;

            var harm = new Harmony(Info.Metadata.GUID);
            new PatchClassProcessor(harm, typeof(Hooks)).Patch();
        }

        private static void OnRunStart(Run run)
        {
            if (!AeroltUniverseUI.IsReady)
                AeroltUniverseUI.Bootstrap();
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
    }
}
