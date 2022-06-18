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
            var user = GetUser.FetchUser(GetComponentInParent<HUD>());
            var body = user.cachedMaster.GetBody();

            
        }

        private void CreateNewStatPrefab(string statName, float statValue)
        {
            var prefab = Instantiate(playerValuePrefab, parent.transform);
            prefab.GetComponent<TMP_Text>().text = statName;
            prefab.GetComponent<TMP_InputField>().text = statValue.ToString();
        }

    }
}