using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Aerolt.Classes;
using JetBrains.Annotations;
using RoR2;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ZioConfigFile;

namespace Aerolt.Buttons
{
    public class PlayerValuesGenerator : MonoBehaviour
    {
        public GameObject parent;
        public GameObject playerValuePrefab;
        public ToggleGroup profileGroup;
        private bool setup;
        private CharacterBody _target;
        private readonly Dictionary<FieldInfo, TMP_InputField> _entries = new();
        private static readonly FieldInfo[] Fields = _fields ??= typeof(CharacterBody).GetFields().Where(x => x.FieldType == typeof(float)).ToArray();
        [CanBeNull] private static readonly FieldInfo[] _fields;
        private MenuInfo info;
        private ZioConfigEntry<int> selectedProfile;
        private List<StatProfile> profiles = new();
        public List<Toggle> toggles;

        public void Setup()
        {
            foreach (var field in Fields) CreateNewStatPrefab(field);
            
            info = GetComponentInParent<MenuInfo>();
            var i = 0;
            foreach (var toggle in toggles)
            {
                var i1 = i;
                if (i > 0) profiles.Add(new StatProfile(i, info.ConfigFile));
                toggle.SetToggleGroup(profileGroup, true);
                toggle.onValueChanged.AddListener(val =>
                {
                    if (!val) return;
                    ProfileSelected(i1);
                });
                i++;
            }

            selectedProfile = info.ConfigFile.Bind("Player", "Selected Profile", 0, "Which profile is selected for the body stats.");
            ProfileSelected(selectedProfile.Value);
        }

        public void SetTogglesActive(bool enable)
        {
            foreach (var toggle in toggles)
            {
                toggle.interactable = enable;
            }
        }

        public void Awake()
        {
            toggles[0].SetIsOnWithoutNotify(false);
            toggles[selectedProfile.Value].SetIsOnWithoutNotify(true);
        }

        public void ProfileSelected(int i, bool applyValuesToBody = true)
        {
            if (!TargetBody) return;
            selectedProfile.Value = i;
            if (i == 0)
            {
                var defaultBody = TargetBody.master.bodyPrefab.GetComponent<CharacterBody>();
                foreach (var (entry, text) in _entries)
                {
                    text.SetText(entry.GetValue(defaultBody).ToString());
                    if(applyValuesToBody)
                        text.ReleaseSelection();
                }
            }
            else
            {
                profiles[i-1].Apply(_entries, applyValuesToBody);
            }
        }

        public CharacterBody TargetBody
        {
            get => _target;
            set
            {
                _target = value;
                UpdateText();
            }
        }

        private void UpdateText()
        {
            foreach (var (field, inputField) in _entries) ((TextMeshProUGUI) inputField.placeholder).text = field.GetValue(TargetBody).ToString(); // this might but probably wont fuck up the profiles, if you die and come back to life
        }

        public void Update()
        {
            if (setup) return;
            if (!TargetBody) return;
            UpdateText();
            setup = true;
        }

        private void CreateNewStatPrefab(FieldInfo field)
        {
            var prefab = Instantiate(playerValuePrefab, parent.transform);
            prefab.GetComponentInChildren<TextMeshProUGUI>().text = field.Name;
            var input = prefab.GetComponentInChildren<TMP_InputField>();
            input.onEndEdit.AddListener(result =>
            {
                if (!TargetBody || !float.TryParse(result, out var value)) return;
                
                field.SetValue(TargetBody, value);
                if (selectedProfile.Value > 0) profiles[selectedProfile.Value - 1].entries[field].Value = value;
                TargetBody.statsDirty = true;
                input.text = string.Empty;
                ((TextMeshProUGUI) input.placeholder).text = value.ToString(CultureInfo.InvariantCulture);
            });
            _entries[field] = input;
        }

        public class StatProfile
        {
            public Dictionary<FieldInfo, ZioConfigEntry<float>> entries = new();
            public StatProfile(int profileIndex, ZioConfigFile.ZioConfigFile configFile)
            {
                foreach (var field in Fields) entries[field] = configFile.Bind("PlayerProfile" + profileIndex, field.Name, 1f, "");
            }
            public void Apply(Dictionary<FieldInfo, TMP_InputField> bodyEntries, bool applyValuesToBody)
            {
                foreach (var (entry, text) in bodyEntries)
                {
                    text.SetText(entries[entry].Value.ToString(CultureInfo.InvariantCulture));
                    if (applyValuesToBody)
                        text.ReleaseSelection();
                }
            }
        }
    }
}