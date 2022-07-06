using System;
using System.Collections.Generic;
using System.Linq;
using Aerolt.Classes;
using Aerolt.Managers;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using JetBrains.Annotations;
using RiskOfOptions;
using UnityEngine;
using UnityEngine.UI;
using ZioConfigFile;
using ZioRiskOfOptions;

namespace Aerolt.Helpers
{
    public class ImageColorer : MonoBehaviour
    {
        public string catagory;
        public string name;
        public string description;
        private ZioConfigFile.ZioConfigFile configFile;
        [NonSerialized] public Image image;
        [NonSerialized] public ZioConfigEntry<Color> configEntry;
        public static Dictionary<string, Dictionary<string, ZioConfigEntry<Color>>> instances = new();
        private string who;
        private ZioConfigEntry<Color> globalEntry;

        public void Start()
        {
            var menuInfo = transform.parent ? GetComponentInParent<MenuInfo>() : GetComponent<MenuInfo>(); 
            configFile = menuInfo ? menuInfo.ConfigFile : Load.Instance.configFile;
            who = menuInfo ? Load.Name + " " + menuInfo.Owner.GetNetworkPlayerName().GetResolvedName() : Load.Guid;
            image = GetComponent<Image>();
            if (!image) throw new ArgumentNullException($"Missing Image Component on object {gameObject} for ImageColorer.");
            var def = catagory + name;
            var instanceKey = configFile.FilePath.ToString();
            if (!instances.TryGetValue(instanceKey, out var instance))
            {
                instance = new Dictionary<string, ZioConfigEntry<Color>>();
                instances.Add(instanceKey, instance);
            }

            if (!instance.TryGetValue("globalcolor", out globalEntry))
            {
                globalEntry = configFile.Bind("Global Color", "Global Color", (Color) new Color32(35, 180, 151, 255), description);
                instance.Add("globalcolor", globalEntry);
                globalEntry.SettingChanged += (_, _, _) =>
                {
                    foreach(var config in instance.Values) config.Value = globalEntry.Value;
                };  
                if (Chainloader.PluginInfos.ContainsKey("bubbet.zioriskofoptions"))
                    MakeRiskOfOptionsGlobal();
            }
            
            if (!instance.TryGetValue(def, out configEntry))
            {
                configEntry = configFile.Bind(catagory, name, image.color, description);
                instance.Add(def, configEntry);
                if (Chainloader.PluginInfos.ContainsKey("bubbet.zioriskofoptions"))
                    MakeRiskOfOptions();
            }
            configEntry.SettingChanged += SettingChanged;
            SettingChanged(configEntry, default, false);
        }

        private void MakeRiskOfOptionsGlobal()
        {
            ModSettingsManager.AddOption(new ZioColorOption(globalEntry),who, who);
        }

        private void OnDestroy()
        {
            if (configEntry != null)
                configEntry.SettingChanged -= SettingChanged;
        }

        private void MakeRiskOfOptions()
        {
            ModSettingsManager.AddOption(new ZioColorOption(configEntry),who, who);
        }

        public void SettingChanged(ZioConfigEntryBase entryBase, object oldValue, bool ignoreSave)
        {
            var entry = (ZioConfigEntry<Color>) entryBase;
            if (entry.Value.Equals(oldValue)) return;
            image.color = entry.Value;
        }
    }
}