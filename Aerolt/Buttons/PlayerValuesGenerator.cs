using System.Reflection;
using Aerolt.Classes;
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
        private MenuInfo _info;

        public void Awake()
        {
            _info = GetComponentInParent<MenuInfo>();
        }

        public void Update()
        {
            if (setup) return;
            var body = _info.Body;
            if (!body) return;
            FieldInfo[] fields = typeof(CharacterBody).GetFields();
            foreach (var field in fields)
                if (field.FieldType == typeof(float))
                    CreateNewStatPrefab(field);
            setup = true;
        }

        private void CreateNewStatPrefab(FieldInfo field)
        {
            var prefab = Instantiate(playerValuePrefab, parent.transform);
            prefab.GetComponentInChildren<TextMeshProUGUI>().text = field.Name;
            var input = prefab.GetComponentInChildren<TMP_InputField>();
            var body = _info.Body;
            if (!body) return;
            input.text = field.GetValue(body).ToString();
            input.m_OnEndEdit.AddListener(result =>
            {
                var infoBody = _info.Body;
                if (infoBody && float.TryParse(result, out var value)) field.SetValue(infoBody, value);
            });
        }

    }
}