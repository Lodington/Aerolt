using System;
using System.Collections.Generic;
using System.Linq;
using Aerolt.Helpers;
using Aerolt.Messages;
using BepInEx;
using RoR2;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Aerolt.Buttons
{
    public class MonsterButtonGenerator : MonoBehaviour
    {
        public GameObject buttonPrefab;
        public GameObject buttonParent;

        public GameObject editItemsPrefab;

        public TMP_Dropdown teamIndexDropDown;
        public TMP_Dropdown eliteIndexDropDown;
        public TMP_InputField searchFilter;

        public static Dictionary<ItemDef, int> ItemDef = new Dictionary<ItemDef, int>();
        private List<string> options = new List<string>();
        private Dictionary<string, EquipmentIndex> eliteMap;
        public Toggle brainDead;
        private Dictionary<CharacterMaster, CustomButton> masterDefRef = new();

        private void Awake()
        {
            foreach (var master in MasterCatalog.allAiMasters)
            {
                var body = master.bodyPrefab.GetComponent<CharacterBody>();
                
                GameObject newButton = Instantiate(buttonPrefab, buttonParent.transform);
                var buttonComponet = newButton.GetComponent<CustomButton>();
                buttonComponet.buttonText.text = master.bodyPrefab ? Language.GetString(master.bodyPrefab.GetComponent<CharacterBody>().baseNameToken) : master.name;
                buttonComponet.image.sprite = Sprite.Create((Texture2D)body.portraitIcon, new Rect(0, 0, body.portraitIcon.width, body.portraitIcon.height), new Vector2(0.5f, 0.5f));
                buttonComponet.button.onClick.AddListener(() => SpawnMonster(master));
                masterDefRef[master] = buttonComponet;
            }
            foreach (string team in Enum.GetNames(typeof(TeamIndex))) options.Add(team);
            teamIndexDropDown.AddOptions(options);

            eliteMap = new Dictionary<string, EquipmentIndex> {{"None", EquipmentIndex.None}};
            foreach (var eliteDef in EliteCatalog.eliteDefs)
            {
                if (eliteDef.eliteEquipmentDef)
                    eliteMap[Language.GetStringFormatted(eliteDef.modifierToken, "Monster")] = eliteDef.eliteEquipmentDef.equipmentIndex;
            }
            if(eliteIndexDropDown)
                eliteIndexDropDown.AddOptions(eliteMap.Keys.ToList());
            if(searchFilter)
                searchFilter.onValueChanged.AddListener(FilterUpdated);
        }

        public void SpawnMonster(CharacterMaster monsterMaster)
        {
            var localUser = LocalUserManager.GetFirstLocalUser();
            if(localUser == null || !localUser.cachedBody)
                return;
            
            var location = localUser.cachedBody.transform.position;
            var body = monsterMaster.GetComponent<CharacterMaster>().bodyPrefab;
            var eliteIndex = eliteMap[eliteIndexDropDown.options[eliteIndexDropDown.value].text];
            var teamIndex = (TeamIndex) teamIndexDropDown.value;
            new MonsterSpawnMessage(monsterMaster.name, body.name, location, teamIndex, eliteIndex, brainDead.isOn, ItemDef.ToDictionary(x => x.Key, x => (uint) x.Value)).SendToServer();
        }
        
        private void FilterUpdated(string text)
        {
            if (text.IsNullOrWhiteSpace())
            {
                foreach (var buttonGen in masterDefRef)
                {
                    buttonGen.Value.gameObject.SetActive(true);
                }
                return;
            }
            
            var arr = masterDefRef.Values.ToArray();
            var matches = Tools.FindMatches(arr, x => x.buttonText.text, text);
            foreach (var buttonGen in arr)
            {
                buttonGen.gameObject.SetActive(matches.Contains(buttonGen));
            }
        }
    }
}