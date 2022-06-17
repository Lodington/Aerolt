using System;
using System.Collections.Generic;
using System.Linq;
using Aerolt.Managers;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
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
        public static Dictionary<ConfigDefinition, ZioConfigEntry<Color>> instances = new();

        public void Start()
        {
            var panelManager = transform.parent ? GetComponentInParent<PanelManager>() : GetComponent<PanelManager>(); 
            configFile = panelManager ? panelManager.configFile : Load.Instance.configFile;
            image = GetComponent<Image>();
            var def = new ConfigDefinition(catagory, name);
            if (!instances.TryGetValue(def, out configEntry))
            {
                configEntry = configFile.Bind(def, image.color, new ConfigDescription(description));
                instances.Add(def, configEntry);
                if (Chainloader.PluginInfos.ContainsKey("bubbet.zioriskofoptions"))
                    MakeRiskOfOptions();
            }
            configEntry.SettingChanged += SettingChanged;
            SettingChanged(configEntry, default, false);
        }

        private void OnDestroy()
        {
            configEntry.SettingChanged -= SettingChanged;
        }

        private void MakeRiskOfOptions()
        {
            ModSettingsManager.AddOption(new ZioColorOption(configEntry));
        }

        public void SettingChanged(ZioConfigEntryBase entryBase, object oldValue, bool ignoreSave)
        {
            var entry = (ZioConfigEntry<Color>) entryBase;
            if (entry.Value.Equals(oldValue)) return;
            image.color = entry.Value;
        }
    }
}