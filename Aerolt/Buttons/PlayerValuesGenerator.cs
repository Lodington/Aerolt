using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Aerolt.Helpers;
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

        public void Awake()
        {


        }

        private CharacterBody GetBody()
        {
            var user = GetUser.FetchUser(GetComponentInParent<HUD>());
            return user.cachedMaster.GetBody();
        }

        private void CreateNewStatPrefab(FieldInfo field)
        {
            var prefab = Instantiate(playerValuePrefab, parent.transform);
            prefab.GetComponent<TMP_Text>().text = field.Name;
            var input = prefab.GetComponent<TMP_InputField>();
            input.text = field.GetValue(GetBody()).ToString();
            input.m_OnEndEdit.AddListener(result =>
            {
                var body = GetBody();
                if (body && float.TryParse(result, out var flot))
                {
                    field.SetValue(body, flot);
                }
            });
        }

    }
}