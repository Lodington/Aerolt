using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Aerolt.Helpers;
using Aerolt.Managers;
using RoR2;
using RoR2.UI;
using TMPro;
using UnityEngine;

namespace Aerolt.Buttons
{
    public class PlayerValuesGenerator : MonoBehaviour
    {
        public GameObject parent;
        public GameObject playerValuePrefab;
        private bool setup;

        public void Update()
        {
            if (setup) return;
            var body = GetBody();
            if (!body) return;
            FieldInfo[] fields = typeof(CharacterBody).GetFields();
            foreach (var field in fields)
                if (field.FieldType == typeof(float))
                    CreateNewStatPrefab(field);
            setup = true;
        }

        private CharacterBody GetBody()
        {
            var panel = GetComponentInParent<PanelManager>();
            if (!panel) return null;
            var user = GetUser.FetchUser(panel.hud);
            return user.cachedMaster.GetBody();
        }

        private void CreateNewStatPrefab(FieldInfo field)
        {
            var prefab = Instantiate(playerValuePrefab, parent.transform);
            prefab.GetComponentInChildren<TextMeshProUGUI>().text = field.Name;
            var input = prefab.GetComponentInChildren<TMP_InputField>();
            input.text = field.GetValue(GetBody()).ToString();
            input.m_OnEndEdit.AddListener(result =>
            {
                var body = GetBody();
                if (body && float.TryParse(result, out var value)) field.SetValue(body, value);
            });
        }

    }
}