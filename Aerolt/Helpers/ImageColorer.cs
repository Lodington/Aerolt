using System.Collections.Generic;
using System.Linq;
using Aerolt.Managers;
using UnityEngine;
using ZioConfigFile;
using Image = UnityEngine.UI.Image;

namespace Aerolt.Helpers
{
    public class ImageColorer : MonoBehaviour
    {
        public string catagory;
        public string name;
        public string description;
        private ZioConfigFile.ZioConfigFile configFile;
        public Image image;
        public ZioConfigEntry<Color> configEntry;
        public static List<ImageColorer> instances = new();

        public void Awake()
        {
            configFile = transform.parent ? GetComponentInParent<PanelManager>().configFile : GetComponent<PanelManager>().configFile;
            image = GetComponent<Image>();
            configEntry = configFile.Bind(catagory, name, image.color, description);
            configEntry.SettingChanged += SettingChanged;
            SettingChanged(configEntry, null, false);
            if (instances.All(x => x.configEntry != configEntry))
                instances.Add(this);
        }

        public void OnDestroy()
        {
            if (instances.Contains(this)) instances.Remove(this);
        }

        public void SettingChanged(ZioConfigEntryBase entryBase, object oldValue, bool ignoreSave)
        {
            var entry = (ZioConfigEntry<Color>) entryBase;
            if (entry.Value == (Color) oldValue) return;
            image.color = entry.Value;
        }
    }
}