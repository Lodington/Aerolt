using System;
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
        private const string Name = "Aerolt";
        private const string Guid = "com.Lodington." + Name;
        private const string Version = "1.4.0";
        public static ManualLogSource Log;
        private static GameObject _co;
        private static AssetBundle _assets;
        private static GameObject _ui;

        public static bool MenuOpen = false;

        public void Awake()
        {
            Log = Logger;

            var path = System.IO.Path.GetDirectoryName(Info.Location);
                _assets = AssetBundle.LoadFromFile(System.IO.Path.Combine(path!, "aeroltbundle"));
            Tools.Log(Enums.LogLevel.Information, "Loaded AssetBundle");
            _co = _assets.LoadAsset<GameObject>("AeroltUI");

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
            RoR2Application.onLoad += CreateUI;
            SceneDirector.onPostPopulateSceneServer += Hooks.GetEspData;
        }

        public void CreateUI()
        {
            _ui = Instantiate(_co);
            DontDestroyOnLoad(_ui);
            Tools.Log(Enums.LogLevel.Information, "Created UI");
        }
        
    }   
}
