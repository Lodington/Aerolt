using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using RoR2;
using RoR2.UI;
using UnityEngine;

namespace Aerolt.Classes
{
    public class MenuInfo : MonoBehaviour
    {
        public static readonly Dictionary<LocalUser, ZioConfigFile.ZioConfigFile> Files = new();
        [NonSerialized] public ZioConfigFile.ZioConfigFile ConfigFile;
        [NonSerialized] public HUD Hud;
        [NonSerialized] public NetworkUser Owner;
        private Canvas parentCanvas;
        [CanBeNull] public LocalUser LocalUser => Owner.localUser;
        [CanBeNull] public CharacterBody Body => LocalUser?.cachedBody;
        [CanBeNull] public CharacterMaster Master => Owner.master;

        private void Awake()
        {
            parentCanvas = GetComponent<Canvas>();
            PauseManager.onPauseStartGlobal += FuckingUnitySorting;
            FuckingUnitySorting();

            Hud = Load.TempHud;
            Owner = Load.TempViewer;

            if (Owner.localUser == null) return;
            if (!Files.TryGetValue(Owner.localUser, out ConfigFile))
            {
                ConfigFile = new ZioConfigFile.ZioConfigFile(RoR2Application.cloudStorage,
                    $"/Aerolt/Profiles/{Owner.localUser.userProfile.fileName}.cfg", true);
                Files.Add(Owner.localUser, ConfigFile);
            }

            transform.GetComponentInChildren<ToggleWindow>().Init(Owner, this);

            foreach (var startup in GetComponentsInChildren<IModuleStartup>(true))
                try
                {
                    startup?.ModuleStart();
                }
                catch (Exception e)
                {
                    Load.Log.LogError(e);
                }
        }

        private void OnDestroy()
        {
            foreach (var startup in GetComponentsInChildren<IModuleStartup>(true))
                try
                {
                    startup?.ModuleEnd();
                }
                catch (Exception e)
                {
                    Load.Log.LogError(e);
                }

            PauseManager.onPauseStartGlobal -= FuckingUnitySorting;
        }

        public void FuckingUnitySorting()
        {
            StartCoroutine(Example());

            IEnumerator Example()
            {
                yield return new WaitForSecondsRealtime(0.01f);
                parentCanvas.sortingOrder = 1000; // something keeps fucking setting this back to 0
            }
        }
    }
}