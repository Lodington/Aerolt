using System.Collections.Generic;
using Aerolt.Classes;
using UnityEngine;
using UnityEngine.UI;
using ZioConfigFile;

namespace Aerolt.Managers
{
    public class WindowManager : MonoBehaviour
    {
        public List<Toggle> buttons;
        public List<GameObject> panels;
        private readonly List<ZioConfigEntry<bool>> windowOpen = new();

        public void Start()
        {
            var info = GetComponentInParent<MenuInfo>();
            var i = 0;
            foreach (var panel in panels)
            {
                if (i >= buttons.Count) break;
                var toggle = buttons[i];
                var entry = info.ConfigFile.Bind("Window Open", panel.name, toggle.isOn, "");
                windowOpen.Add(entry);
                toggle.Set(entry.Value);
                toggle.onValueChanged.AddListener(on => entry.Value = on);
                i++;
            }
        }
    }
}