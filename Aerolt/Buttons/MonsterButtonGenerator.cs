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
        public static Dictionary<ItemDef, int> ItemDef = new();
        public GameObject buttonPrefab = null!;
        public GameObject buttonParent = null!;

        public GameObject editItemsPrefab = null!;

        public TMP_Dropdown teamIndexDropDown = null!;
        public TMP_Dropdown eliteIndexDropDown = null!;
        public TMP_InputField searchFilter = null!;
        public Toggle brainDead = null!;
        private Dictionary<string, EquipmentIndex> eliteMap = null!;
        private readonly Dictionary<CharacterMaster, CustomButton> masterDefRef = new();
        private readonly List<string> options = new();

        private void Awake()
        {
            // Initialize dropdowns immediately (lightweight)
            foreach (var team in Enum.GetNames(typeof(TeamIndex))) options.Add(team);
            teamIndexDropDown.AddOptions(options);

            eliteMap = new Dictionary<string, EquipmentIndex> { { "None", EquipmentIndex.None } };
            foreach (var eliteDef in EliteCatalog.eliteDefs)
                if (eliteDef.eliteEquipmentDef)
                    eliteMap[Language.GetStringFormatted(eliteDef.modifierToken, "Monster")] =
                        eliteDef.eliteEquipmentDef.equipmentIndex;
            if (eliteIndexDropDown)
                eliteIndexDropDown.AddOptions(eliteMap.Keys.ToList());
            if (searchFilter)
                searchFilter.onValueChanged.AddListener(FilterUpdated);
        }

        private Coroutine? _initCoroutine;

        private void OnEnable()
        {
            if (masterDefRef.Count == 0)
            {
                if (_initCoroutine != null) StopCoroutine(_initCoroutine);
                _initCoroutine = StartCoroutine(InitializeButtons());
            }
        }

        private void ClearButtons()
        {
            if (_initCoroutine != null)
            {
                StopCoroutine(_initCoroutine);
                _initCoroutine = null;
            }
            foreach (var entry in masterDefRef)
                if (entry.Value) Destroy(entry.Value.gameObject);
            masterDefRef.Clear();
        }

        public void Repopulate()
        {
            ClearButtons();
            _initCoroutine = StartCoroutine(InitializeButtons());
        }

        private System.Collections.IEnumerator InitializeButtons()
        {
            int count = 0;
            const int batchSize = 10; // Create 10 buttons per frame

            foreach (var master in MasterCatalog.allMasters
                         .Where(x => x.bodyPrefab && x.bodyPrefab.GetComponent<CharacterBody>()?.portraitIcon)
                         .OrderBy(x => Language.GetString(x.bodyPrefab.GetComponent<CharacterBody>().baseNameToken)))
            {
                if (masterDefRef.ContainsKey(master)) continue;

                var body = master.bodyPrefab.GetComponent<CharacterBody>();

                var newButton = Instantiate(buttonPrefab, buttonParent.transform);
                var buttonComponet = newButton.GetComponent<CustomButton>();
                buttonComponet.buttonText.text = Language.GetString(body.baseNameToken);
                buttonComponet.image.sprite = Sprite.Create((Texture2D)body.portraitIcon,
                    new Rect(0, 0, body.portraitIcon.width, body.portraitIcon.height), new Vector2(0.5f, 0.5f));
                buttonComponet.button.onClick.AddListener(() => SpawnMonster(master));
                masterDefRef[master] = buttonComponet;

                count++;
                if (count >= batchSize)
                {
                    count = 0;
                    yield return null; // Wait one frame
                }
            }

            _initCoroutine = null;
        }

        public void SpawnMonster(CharacterMaster monsterMaster)
        {
            var localUser = LocalUserManager.GetFirstLocalUser();
            if (localUser == null || !localUser.cachedBody)
                return;

            var location = localUser.cachedBody.transform.position;
            var body = monsterMaster.GetComponent<CharacterMaster>().bodyPrefab;
            var eliteIndex = eliteMap[eliteIndexDropDown.options[eliteIndexDropDown.value].text];
            var teamIndex = (TeamIndex)teamIndexDropDown.value;
            new MonsterSpawnMessage(monsterMaster.name, body.name, location, teamIndex, eliteIndex, brainDead.isOn,
                ItemDef.ToDictionary(x => x.Key, x => (uint)x.Value)).SendToServer();
        }

        private void FilterUpdated(string text)
        {
            if (text.IsNullOrWhiteSpace())
            {
                foreach (var buttonGen in masterDefRef) buttonGen.Value.gameObject.SetActive(true);
                return;
            }

            var arr = masterDefRef.Values.ToArray();
            var matches = Tools.FindMatches(arr, x => x.buttonText.text, text);
            foreach (var buttonGen in arr) buttonGen.gameObject.SetActive(matches.Contains(buttonGen));
        }
    }
}