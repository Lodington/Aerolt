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

        public string[] stats = {
            "baseRegen","baseMaxShield","baseMoveSpeed","baseAccelerat","baseJumpPower","baseDamage","baseAttackSpe","baseCrit","baseArmor"
        };


        public void Awake()
        {
            var user = GetUser.FetchUser(GetComponentInParent<HUD>());
            var body = user.cachedMaster.GetBody();

            CreateNewStatPrefab(stats[0], body.baseRegen);
            CreateNewStatPrefab(stats[1], body.baseMaxHealth);
            CreateNewStatPrefab(stats[2], body.baseMoveSpeed);
            CreateNewStatPrefab(stats[3], body.baseAcceleration);
            CreateNewStatPrefab(stats[4], body.baseJumpPower);
            CreateNewStatPrefab(stats[5], body.baseDamage);
            CreateNewStatPrefab(stats[6], body.baseAttackSpeed);
            CreateNewStatPrefab(stats[7], body.baseCrit);
            CreateNewStatPrefab(stats[8], body.baseArmor);
            
        }

        private void CreateNewStatPrefab(string statName, float statValue)
        {
            var prefab = Instantiate(playerValuePrefab, parent.transform);
            prefab.GetComponent<TMP_Text>().text = statName;
            prefab.GetComponent<TMP_InputField>().text = statValue.ToString();
        }

    }
}