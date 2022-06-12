﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Permissions;
using Aerolt.Classes;
using Aerolt.Helpers;
using Aerolt.Overrides;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using MonoMod.RuntimeDetour;
using RoR2;
using RoR2.UI;
using UnityEngine;

#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete
[module: UnverifiableCode]
namespace Aerolt 
{
    [BepInPlugin(Guid, Name, Version)]
    public class Load : BaseUnityPlugin
    {
        public const string Name = "Aerolt";
        private const string Guid = "com.Lodington." + Name;
        public const string Version = "1.4.0";
        public static ManualLogSource Log;
        private static GameObject _co;
        private static GameObject _op;
        private static AssetBundle _assets;
        private static GameObject _ui;
        public static Load Instance;

        public static bool MenuOpen = false;

        public void Awake()
        {
            Instance = this;
            Log = Logger;

            var path = System.IO.Path.GetDirectoryName(Info.Location);
            _assets = AssetBundle.LoadFromFile(System.IO.Path.Combine(path!, "aeroltbundle"));
            Tools.Log(Enums.LogLevel.Information, "Loaded AssetBundle");
            _co = _assets.LoadAsset<GameObject>("AeroltUI");
            _op = _assets.LoadAsset<GameObject>("SettingUI");

            var harm = new Harmony(Info.Metadata.GUID);
            new PatchClassProcessor(harm, typeof(Hooks)).Patch();

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
            SceneDirector.onPostPopulateSceneServer += Hooks.GetEspData;
            HUD.shouldHudDisplay += CreateHud;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1) && aeroltUIs.Count == 0) SettingsToggle();
        }

        private void SettingsToggle()
        {
            if (!settingsUI)
            {
                settingsUI = Instantiate(_op);
                DontDestroyOnLoad(settingsUI);
            }

            settingsUI.SetActive(!settingsUI.activeSelf);
        }

        private void GameLoad()
        {
            // create settings menu;
            Tools.Log(Enums.LogLevel.Information, "Created UI");
        }

        public static Dictionary<NetworkUser, GameObject> aeroltUIs = new();
        private static GameObject settingsUI;

        public static void CreateHud(HUD hud, ref bool shoulddisplay)
        {
            if (!hud.cameraRigController) return;
            var viewer = hud.cameraRigController.viewer;

            if (aeroltUIs.ContainsKey(viewer)) return;
            if (settingsUI && settingsUI.activeSelf) settingsUI.SetActive(false);

            var ui = Instantiate(_co, hud.mainContainer.transform, true);
            ui.GetComponentInChildren<ToggleWindow>().Init(viewer);
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
        }
    }   
}
