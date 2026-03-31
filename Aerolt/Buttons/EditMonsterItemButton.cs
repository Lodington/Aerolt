using System;
using System.Collections.Generic;
using System.Linq;
using Aerolt.Helpers;
using BepInEx;
using RoR2;
using RoR2.ContentManagement;
using TMPro;
using UnityEngine;
using ZioConfigFile;

namespace Aerolt.Buttons
{
    public class EditMonsterItemButton : MonoBehaviour
    {
        public TMP_Dropdown sortMode = null!;
        public TMP_InputField searchFilter = null!;
        public GameObject buttonPrefab = null!;
        public GameObject buttonParent = null!;

        public GameObject itemListParent = null!;
        protected Dictionary<ItemDef, AddRemoveButtonGen<ItemDef>> itemDefRef = new();
#pragma warning disable CS0649
        private ZioConfigEntry<int>? sortModeEntry;
#pragma warning restore CS0649
        private bool initialized = false;

        public virtual Dictionary<ItemDef, int> itemDef => MonsterButtonGenerator.ItemDef;

        public void Awake()
        {
            if (sortMode)
            {
                sortMode.options.Clear();
                sortMode.AddOptions(new List<string>
                {
                    "Tier Descending",
                    "Name Descending",
                    "Tier Ascending",
                    "Name Ascending"
                });
                sortMode.onValueChanged.AddListener(Sort);
            }

            if (searchFilter)
                searchFilter.onValueChanged.AddListener(FilterUpdated);
        }

        private void OnEnable()
        {
            if (!initialized)
            {
                StartCoroutine(InitializeButtons());
            }
        }

        private System.Collections.IEnumerator InitializeButtons()
        {
            initialized = true;
            int count = 0;
            const int batchSize = 15; // Create 15 buttons per frame

            foreach (var def in ContentManager._itemDefs)
            {
                itemDefRef[def] =
                    new AddRemoveButtonGen<ItemDef>(def, buttonPrefab, itemDef, buttonParent, itemListParent, false);

                count++;
                if (count >= batchSize)
                {
                    count = 0;
                    yield return null; // Wait one frame
                }
            }
        }

        public void Sort(int _)
        {
            Sort();
        }

        private void FilterUpdated(string text)
        {
            if (text.IsNullOrWhiteSpace())
            {
                foreach (var buttonGen in itemDefRef) buttonGen.Value.button.SetActive(true);
                return;
            }

            var arr = itemDefRef.Values.ToArray();
            var matches = Tools.FindMatches(arr, x => Language.GetString(x.def.nameToken), text);
            foreach (var buttonGen in arr) buttonGen.button.SetActive(matches.Contains(buttonGen));
        }

        public void Sort()
        {
            var sorted = (sortMode ? sortMode.value : 0) switch
            {
                0 => itemDefRef.Values.OrderByDescending(x => x.def.tier),
                1 => itemDefRef.Values.OrderByDescending(x => Language.GetString(x.def.nameToken)),
                2 => itemDefRef.Values.OrderBy(x => x.def.tier),
                3 => itemDefRef.Values.OrderBy(x => Language.GetString(x.def.nameToken)),
                _ => throw new InvalidOperationException("Invalid sort mode.")
            };

            foreach (var buttonGen in sorted) buttonGen.button.transform.SetSiblingIndex(0);
        }

        private void SortModeChanged(ZioConfigEntryBase arg1, object arg2, bool arg3)
        {
            sortMode.value = sortModeEntry!.Value;
            Sort();
        }
    }
}