using System.Collections.Generic;
using Aerolt.Enums;
using Aerolt.Helpers;
using Aerolt.Managers;
using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib.UI;
using UniverseLib.UI.Models;
using ZioConfigFile;

namespace Aerolt.UI.Sections
{
    public class SettingsSection : IAeroltSection
    {
        private Toggle _hostOnly = null!;
        private ButtonRef? _rebinding;
        private ButtonNames _rebindingName;
        private bool _suppress;

        public string Title => "Settings";
        public GameObject Root { get; private set; } = null!;

        public void Build(GameObject contentParent)
        {
            Root = UIFactory.CreateVerticalGroup(contentParent, "SettingsSection", true, true, true, true, 0,
                default, new Color(0.14f, 0.14f, 0.14f));
            UIFactory.SetLayoutElement(Root, flexibleWidth: 9999, flexibleHeight: 9999);

            UIFactory.CreateScrollView(Root, "SettingsScroll", out var content, out _, new Color(0.12f, 0.12f, 0.12f));
            UIFactory.SetLayoutGroup<VerticalLayoutGroup>(content, false, false, true, true, 6, 4, 4, 4, 4);

            BuildGeneral(content);
            BuildKeybinds(content);
            BuildColors(content);

            NetworkManager.HostOnlyChanged += OnHostOnlyChangedExternally;
        }

        public void OnShown()
        {
            _suppress = true;
            if (_hostOnly) _hostOnly.isOn = NetworkManager.HostOnly;
            _suppress = false;
        }


        private void BuildGeneral(GameObject parent)
        {
            Header(parent, "General");

            var obj = UIFactory.CreateToggle(parent, "HostOnly", out _hostOnly, out var text);
            UIFactory.SetLayoutElement(obj, minHeight: 28, flexibleWidth: 9999);
            text.text = "Host Only (ignore Aerolt actions from clients)";
            _hostOnly.isOn = NetworkManager.HostOnly;
            _hostOnly.onValueChanged.AddListener(on =>
            {
                if (_suppress) return;
                NetworkManager.HostOnly = on;
            });
        }

        private void OnHostOnlyChangedExternally(bool value)
        {
            if (!_hostOnly) return;
            _suppress = true;
            _hostOnly.isOn = value;
            _suppress = false;
        }

        private void BuildKeybinds(GameObject parent)
        {
            Header(parent, "Keybinds");
            foreach (var kvp in Load.KeyBinds)
                BuildKeybindRow(parent, kvp.Key, kvp.Value);
        }

        private void BuildKeybindRow(GameObject parent, ButtonNames name, ZioConfigEntry<KeyboardShortcut> entry)
        {
            var row = Row(parent);
            var label = UIFactory.CreateLabel(row, "Label", PrettyName(name), TextAnchor.MiddleLeft);
            UIFactory.SetLayoutElement(label.gameObject, minWidth: 160, flexibleWidth: 9999);

            var btn = UIFactory.CreateButton(row, $"Rebind_{name}", entry.Value.ToString());
            UIFactory.SetLayoutElement(btn.Component.gameObject, minHeight: 28, minWidth: 140, flexibleWidth: 0);
            btn.OnClick += () =>
            {
                _rebinding = btn;
                _rebindingName = name;
                btn.ButtonText.text = "Press a key...";
            };
        }


        private void BuildColors(GameObject parent)
        {
            Header(parent, "ESP Colors");
            foreach (var kvp in Colors.GlobalColors)
                BuildColorRow(parent, kvp.Key, kvp.Value);
        }

        private void BuildColorRow(GameObject parent, string id, ZioConfigEntry<Color> entry)
        {
            var row = UIFactory.CreateHorizontalGroup(parent, $"Color_{id}", false, false, true, true, 4,
                new Vector4(2, 2, 2, 2));
            UIFactory.SetLayoutElement(row, minHeight: 28, flexibleWidth: 9999);

            var label = UIFactory.CreateLabel(row, "Label", id, TextAnchor.MiddleLeft);
            UIFactory.SetLayoutElement(label.gameObject, minWidth: 150, flexibleWidth: 9999);

            var swatchObj = UIFactory.CreateUIObject("Swatch", row);
            UIFactory.SetLayoutElement(swatchObj, minWidth: 26, minHeight: 22, preferredWidth: 26, preferredHeight: 22,
                flexibleWidth: 0, flexibleHeight: 0);
            var swatch = swatchObj.AddComponent<Image>();
            swatch.color = entry.Value;

            Slider MakeChannel(string ch, float value)
            {
                var col = UIFactory.CreateVerticalGroup(row, $"Ch_{ch}", false, false, true, true, 0);
                UIFactory.SetLayoutElement(col, minWidth: 70, flexibleWidth: 0);
                var chLabel = UIFactory.CreateLabel(col, "L", ch, TextAnchor.MiddleCenter);
                UIFactory.SetLayoutElement(chLabel.gameObject, minHeight: 12, flexibleWidth: 9999);
                UIFactory.CreateSlider(col, "S", out var slider);
                UIFactory.SetLayoutElement(slider.gameObject, minHeight: 14, minWidth: 60, flexibleWidth: 9999);
                slider.minValue = 0f;
                slider.maxValue = 1f;
                slider.value = value;
                return slider;
            }

            var r = MakeChannel("R", entry.Value.r);
            var g = MakeChannel("G", entry.Value.g);
            var b = MakeChannel("B", entry.Value.b);

            void Apply(float _)
            {
                var color = new Color(r.value, g.value, b.value, entry.Value.a);
                entry.Value = color; // writes to the same config the ESP + Risk of Options use
                swatch.color = color;
            }

            r.onValueChanged.AddListener(Apply);
            g.onValueChanged.AddListener(Apply);
            b.onValueChanged.AddListener(Apply);
        }

        public void TickRebind()
        {
            if (_rebinding == null) return;
            if (!Input.anyKeyDown) return;

            foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (!Input.GetKeyDown(key)) continue;
                if (key == KeyCode.Mouse0 || key == KeyCode.Mouse1) continue; // ignore clicks

                var entry = Load.KeyBinds[_rebindingName];
                entry.Value = new KeyboardShortcut(key);
                _rebinding.ButtonText.text = entry.Value.ToString();
                _rebinding = null;
                break;
            }
        }

        private static string PrettyName(ButtonNames name)
        {
            return name switch
            {
                ButtonNames.OpenMenu => "Open Menu",
                _ => name.ToString()
            };
        }

        private static void Header(GameObject parent, string text)
        {
            var label = UIFactory.CreateLabel(parent, "Header", text, TextAnchor.MiddleLeft);
            UIFactory.SetLayoutElement(label.gameObject, minHeight: 24, flexibleWidth: 9999);
            label.fontStyle = FontStyle.Bold;
        }

        private static GameObject Row(GameObject parent)
        {
            var row = UIFactory.CreateHorizontalGroup(parent, "Row", false, false, true, true, 4);
            UIFactory.SetLayoutElement(row, minHeight: 30, flexibleHeight: 0, flexibleWidth: 9999);
            return row;
        }
    }
}
