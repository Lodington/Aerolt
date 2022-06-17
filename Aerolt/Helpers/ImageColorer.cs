using System;
using System.Collections.Generic;
using System.Linq;
using Aerolt.Managers;
using BepInEx.Bootstrap;
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
        public static List<ImageColorer> instances = new();

        public void Start()
        {
            var panelManager = transform.parent ? GetComponentInParent<PanelManager>() : GetComponent<PanelManager>(); 
            configFile = panelManager ? panelManager.configFile : Load.Instance.configFile;
            image = GetComponent<Image>();
            configEntry = configFile.Bind(catagory, name, image.color, description);
            configEntry.SettingChanged += SettingChanged;
            SettingChanged(configEntry, default, false);
            if (instances.Any(x => x.configEntry == configEntry)) return;
            instances.Add(this);
            if (Chainloader.PluginInfos.ContainsKey("bubbet.zioriskofoptions"))
                MakeRiskOfOptions();
        }

        private void MakeRiskOfOptions()
        {
            ModSettingsManager.AddOption(new ZioColorOption(configEntry));
        }

        public void OnDestroy()
        {
            if (instances.Contains(this)) instances.Remove(this);
        }

        public void SettingChanged(ZioConfigEntryBase entryBase, object oldValue, bool ignoreSave)
        {
            var entry = (ZioConfigEntry<Color>) entryBase;
            if (entry.Value.Equals(oldValue)) return;
            image.color = entry.Value;
        }
    }
}