using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Aerolt.Classes;
using Aerolt.Messages;
using RoR2;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib.UI;
using UniverseLib.UI.Models;
using ZioConfigFile;

namespace Aerolt.UI.Sections
{
    public class BodyStatsSection : IAeroltSection
    {
        private const int ProfileCount = 3; // saved profiles (profile 1..3); 0 = default/live

        private static readonly FieldInfo[] Fields = typeof(CharacterBody)
            .GetFields(BindingFlags.Public | BindingFlags.Instance)
            .Where(x => x.FieldType == typeof(float))
            .ToArray();

        private readonly Dictionary<FieldInfo, InputFieldRef> _inputs = new();
        private readonly List<StatProfile> _profiles = new();
        private ZioConfigEntry<int> _selectedProfile = null!;
        private bool _suppress;

        public string Title => "Body Stats";
        public GameObject Root { get; private set; } = null!;
        public bool ScrollableContent => false; 

        public void Build(GameObject contentParent)
        {
            Root = UIFactory.CreateVerticalGroup(contentParent, "BodyStatsSection", false, false, true, true, 4,
                default, new Color(0.14f, 0.14f, 0.14f));
            UIFactory.SetLayoutElement(Root, flexibleWidth: 9999, flexibleHeight: 9999);
            _selectedProfile = Load.ConfigFile.Bind("Player", "Selected Profile", 0,
                "Which body-stat profile is selected.");
            for (var i = 1; i <= ProfileCount; i++)
                _profiles.Add(new StatProfile(i, Load.ConfigFile));

            var profileRow = UIFactory.CreateHorizontalGroup(Root, "Profiles", false, false, true, true, 4);
            UIFactory.SetLayoutElement(profileRow, minHeight: 30, flexibleHeight: 0, flexibleWidth: 9999);
            AddProfileButton(profileRow, "Default", 0);
            for (var i = 1; i <= ProfileCount; i++)
                AddProfileButton(profileRow, $"Profile {i}", i);

            UIFactory.CreateScrollView(Root, "Stats", out var listContent, out _, new Color(0.1f, 0.1f, 0.1f));
            UIFactory.SetLayoutGroup<VerticalLayoutGroup>(listContent, false, false, true, true, 2, 2, 2, 2, 2);

            foreach (var field in Fields)
                CreateStatRow(listContent, field);
        }

        public void OnShown()
        {
            ApplyProfile(_selectedProfile.Value, false);
            RefreshPlaceholders();
        }

        private void AddProfileButton(GameObject parent, string label, int index)
        {
            var btn = UIFactory.CreateButton(parent, $"Profile_{index}", label);
            UIFactory.SetLayoutElement(btn.Component.gameObject, minHeight: 28, flexibleWidth: 9999);
            btn.OnClick += () =>
            {
                _selectedProfile.Value = index;
                ApplyProfile(index, true);
            };
        }

        private void CreateStatRow(GameObject parent, FieldInfo field)
        {
            var row = UIFactory.CreateHorizontalGroup(parent, $"Stat_{field.Name}", false, false, true, true, 4,
                new Vector4(2, 2, 2, 2));
            UIFactory.SetLayoutElement(row, minHeight: 28, flexibleWidth: 9999);

            var label = UIFactory.CreateLabel(row, "Name", field.Name, TextAnchor.MiddleLeft);
            UIFactory.SetLayoutElement(label.gameObject, minWidth: 180, flexibleWidth: 9999);

            var input = UIFactory.CreateInputField(row, "Value", "");
            UIFactory.SetLayoutElement(input.Component.gameObject, minHeight: 26, minWidth: 100, flexibleWidth: 0);
            input.Component.contentType = InputField.ContentType.DecimalNumber;
            input.Component.onEndEdit.AddListener(text => OnStatEdited(field, text));
            _inputs[field] = input;
        }

        private void OnStatEdited(FieldInfo field, string text)
        {
            if (_suppress) return;
            if (string.IsNullOrWhiteSpace(text)) return;
            var body = GetBody();
            if (!body || !float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
                return;

            if (_selectedProfile.Value > 0)
                _profiles[_selectedProfile.Value - 1].Entries[field].Value = value;

            new BodyStatMessage(body!, field.Name, value).SendToEveryone();
        }

        private void ApplyProfile(int index, bool applyToBody)
        {
            var body = GetBody();

            _suppress = true;
            if (index == 0)
            {
                if (body)
                    foreach (var (field, input) in _inputs)
                        input.Text = ((float)field.GetValue(body)).ToString(CultureInfo.InvariantCulture);
            }
            else
            {
                var profile = _profiles[index - 1];
                foreach (var (field, input) in _inputs)
                    input.Text = profile.Entries[field].Value.ToString(CultureInfo.InvariantCulture);
            }

            _suppress = false;

            if (applyToBody && body)
                foreach (var (field, input) in _inputs)
                    if (float.TryParse(input.Text, out var value))
                        new BodyStatMessage(body!, field.Name, value).SendToEveryone();
        }

        private void RefreshPlaceholders()
        {
            var body = GetBody();
            if (!body) return;
            foreach (var (field, input) in _inputs)
            {
                var ph = input.Component.placeholder as Text;
                if (ph) ph!.text = ((float)field.GetValue(body)).ToString(CultureInfo.InvariantCulture);
            }
        }

        private static CharacterBody? GetBody()
        {
            var user = PlayerSelection.Selected;
            if (!user || !user!.master) return null;

            var body = user.master.GetBody();
            if (!body && user.master.hasBody)
                body = user.master.GetBodyObject()?.GetComponent<CharacterBody>();

            return body ? body : null;
        }

        private class StatProfile
        {
            public readonly Dictionary<FieldInfo, ZioConfigEntry<float>> Entries = new();

            public StatProfile(int profileIndex, ZioConfigFile.ZioConfigFile configFile)
            {
                foreach (var field in Fields)
                    Entries[field] = configFile.Bind("PlayerProfile" + profileIndex, field.Name, 1f, "");
            }
        }
    }
}
