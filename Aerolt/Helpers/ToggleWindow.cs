using System;
using System.Collections.Generic;
using Aerolt.Enums;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using RiskOfOptions;
using RoR2;
using UnityEngine;
using UnityEngine.UI;
using ZioConfigFile;
using ZioRiskOfOptions;

namespace Aerolt.Classes
{
    public class ToggleWindow : MonoBehaviour
    {
        private static readonly List<ConfigDefinition> roo = new();

        public GameObject panel;
        private bool _menuIsOpen = true;
        private MenuInfo menuInfo;
        [NonSerialized] public NetworkUser owner;
        private ZioConfigEntry<bool> visible;

        public void Update()
        {
            if (!owner) return;
            var localUser = LocalUserManager.GetFirstLocalUser();
            var isFirst = localUser == owner.localUser;
            if (isFirst && Load.GetKeyPressed(Load.KeyBinds[ButtonNames.OpenMenu]))
                WindowToggle();
        }

        private void OnDestroy()
        {
            Load.AeroltUIs.Remove(owner);
            visible.SettingChanged -= VisibleOnSettingChanged;
        }

        public void Init(NetworkUser owner, MenuInfo info)
        {
            menuInfo = info;
            this.owner = owner;
            var profile = owner.localUser?.userProfile;
            if (profile == null)
            {
                // Delete ui objects that dont have a profile attached.
                Destroy(transform.parent.gameObject);
                return;
            }

            Invoke(nameof(WindowToggle), 0.01f);
            visible = menuInfo.ConfigFile.Bind("General", "Show Icon", true,
                "You should probably leave this on if you're using a gamepad.");
            visible.SettingChanged += VisibleOnSettingChanged;
            VisibleOnSettingChanged(null, null, false);
            if (!roo.Contains(visible.Definition) && Chainloader.PluginInfos.ContainsKey("bubbet.zioriskofoptions"))
                MakeRiskOfOptions(visible);
        }

        private void VisibleOnSettingChanged(ZioConfigEntryBase arg1, object arg2, bool arg3) => GetComponent<Image>().enabled = visible.Value;

        private void MakeRiskOfOptions(ZioConfigEntry<bool> visible)
        {
            var who = menuInfo ? Load.Name + " " + menuInfo.Owner.GetNetworkPlayerName().GetResolvedName() : Load.Guid;
            ModSettingsManager.AddOption(new ZioCheckBoxOption(visible), who, who);
            roo.Add(visible.Definition);
        }

        public void WindowToggle()
        {
            _menuIsOpen = !_menuIsOpen;
            panel.SetActive(_menuIsOpen);
            if (_menuIsOpen)
                menuInfo.FuckingUnitySorting();
        }
    }
}
