using System.Collections;
using System.Collections.Generic;
using Aerolt.Classes;
using UnityEngine;
using UnityEngine.UI;
using ZioConfigFile;

namespace Aerolt.Managers
{
    public class WindowManager : MonoBehaviour
    {
        public List<Toggle> buttons = null!;
        public List<GameObject> panels = null!;
        private readonly List<ZioConfigEntry<bool>> windowOpen = new();

        public void Start()
        {
            var info = GetComponentInParent<MenuInfo>();
            var i = 0;
            var savedStates = new List<(Toggle toggle, bool savedValue)>();
            foreach (var panel in panels)
            {
                if (i >= buttons.Count) break;
                var toggle = buttons[i];
                var entry = info.ConfigFile.Bind("Window Open", panel.name, toggle.isOn, "");
                windowOpen.Add(entry);
                savedStates.Add((toggle, entry.Value));
                toggle.onValueChanged.AddListener(on => entry.Value = on);
                i++;
            }

            StartCoroutine(RestorePanelStates(savedStates));
        }

        private IEnumerator RestorePanelStates(List<(Toggle toggle, bool savedValue)> savedStates)
        {
            // Wait a frame so catalogs and other systems are ready before activating panels
            yield return null;
            foreach (var (toggle, savedValue) in savedStates)
                toggle.Set(savedValue);
        }
    }
}