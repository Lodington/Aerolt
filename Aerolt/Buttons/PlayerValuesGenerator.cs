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


        public void Start()
        {
            var user = GetUser.FetchUser(GetComponentInParent<HUD>());
            var body = user.cachedMaster.GetBody();

            foreach (var type in typeof(CharacterBody).GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (!type.IsAbstract)
                    continue;
                
                foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)) {
                   
                    CreateNewStatPrefab(field.Name, field.GetValue(null));
                }
                
            }
            
        }

        private void CreateNewStatPrefab(string statName, object statValue)
        {
            var prefab = Instantiate(playerValuePrefab, parent.transform);
            prefab.GetComponent<TMP_Text>().text = statName;
            //prefab.GetComponent<TMP_InputField>().text = statValue.ToString();
        }
        
        IEnumerable<FieldInfo> GetAllFields(Type type) {
            return type.GetNestedTypes().SelectMany(GetAllFields)
                .Concat(type.GetFields());
        }
        
    }
}