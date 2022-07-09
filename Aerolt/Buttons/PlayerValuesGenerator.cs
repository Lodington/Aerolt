using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using RoR2;
using TMPro;
using UnityEngine;

namespace Aerolt.Buttons
{
    public class PlayerValuesGenerator : MonoBehaviour
    {
        public GameObject parent;
        public GameObject playerValuePrefab;
        private bool setup;
        private CharacterBody _target;
        private readonly Dictionary<FieldInfo, TMP_InputField> _entries = new();
        private static readonly FieldInfo[] Fields = _fields ??= typeof(CharacterBody).GetFields().Where(x => x.FieldType == typeof(float)).ToArray();
        [CanBeNull] private static readonly FieldInfo[] _fields;

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
            foreach (var (field, inputField) in _entries) ((TextMeshProUGUI) inputField.placeholder).text = field.GetValue(TargetBody).ToString();
        }

        public void Update()
        {
            if (setup) return;
            foreach (var field in Fields) CreateNewStatPrefab(field);
            if (TargetBody) UpdateText();
            setup = true;
        }

        private void CreateNewStatPrefab(FieldInfo field)
        {
            var prefab = Instantiate(playerValuePrefab, parent.transform);
            prefab.GetComponentInChildren<TextMeshProUGUI>().text = field.Name;
            var input = prefab.GetComponentInChildren<TMP_InputField>();
            input.m_OnEndEdit.AddListener(result =>
            {
                if (!TargetBody || !float.TryParse(result, out var value)) return;
                
                field.SetValue(TargetBody, value);
                TargetBody.statsDirty = true;
                input.text = string.Empty;
                ((TextMeshProUGUI) input.placeholder).text = value.ToString(CultureInfo.InvariantCulture);
            });
            _entries[field] = input;
        }

    }
}