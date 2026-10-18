using Aerolt.Classes;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib.UI;
using ZioConfigFile;

namespace Aerolt.UI.Sections
{
    public class EspSection : IAeroltSection
    {
        public string Title => "ESP";
        public GameObject Root { get; private set; } = null!;

        public void Build(GameObject contentParent)
        {
            Root = UIFactory.CreateVerticalGroup(contentParent, "EspSection", false, false, true, true, 4,
                default, new Color(0.14f, 0.14f, 0.14f));
            UIFactory.SetLayoutElement(Root, flexibleWidth: 9999, flexibleHeight: 9999);

            UIFactory.CreateScrollView(Root, "EspScroll", out var content, out _, new Color(0.12f, 0.12f, 0.12f));
            UIFactory.SetLayoutGroup<VerticalLayoutGroup>(content, false, false, true, true, 4, 4, 4, 4, 4);

            AddToggle(content, "Teleporter", Esp.ShowTeleporter);
            AddToggle(content, "Chests", Esp.ShowChest);
            AddToggle(content, "Multishops", Esp.ShowMultiShop);
            AddToggle(content, "Barrels", Esp.ShowBarrel);
            AddToggle(content, "Scrappers", Esp.ShowScrapper);
            AddToggle(content, "Secret Buttons", Esp.ShowSecret);
            AddToggle(content, "Duplicators", Esp.ShowDuplicator);
            AddToggle(content, "Drones", Esp.ShowDrone);
            AddToggle(content, "Shrines", Esp.ShowShrine);
            AddToggle(content, "Newt Altars", Esp.ShowNewtAlter);
            AddToggle(content, "Advanced Info", Esp.ShowAdvanced);
        }

        public void OnShown()
        {
        }

        private static void AddToggle(GameObject parent, string label, ZioConfigEntry<bool> entry)
        {
            var obj = UIFactory.CreateToggle(parent, $"Esp_{label}", out var toggle, out var text);
            UIFactory.SetLayoutElement(obj, minHeight: 28, flexibleWidth: 9999);
            text.text = label;
            toggle.isOn = entry.Value;
            toggle.onValueChanged.AddListener(on => entry.Value = on);
        }
    }
}
