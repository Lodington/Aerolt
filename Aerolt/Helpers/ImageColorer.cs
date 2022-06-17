using System;
using System.Collections.Generic;
using System.Linq;
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

        public void Start()
        {
            var panelManager = transform.parent ? GetComponentInParent<PanelManager>() : GetComponent<PanelManager>(); 
            configFile = panelManager ? panelManager.configFile : Load.Instance.configFile;
            who = panelManager ? Load.Name + " " + panelManager.owner.GetNetworkPlayerName().GetResolvedName() : Load.Guid;
            image = GetComponent<Image>();
            var def = catagory + name;
            var instanceKey = configFile.FilePath.ToString();
            if (!instances.TryGetValue(instanceKey, out var instance))
            {
                instance = new Dictionary<string, ZioConfigEntry<Color>>();
                instances.Add(instanceKey, instance);
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

        private void OnDestroy()
        {
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