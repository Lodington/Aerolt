using System;
using System.Collections.Generic;
using System.Linq;
using Aerolt.Helpers;
using Aerolt.Managers;
using Aerolt.Messages;
using BepInEx;
using RoR2;
using RoR2.ContentManagement;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.UIElements;
using ZioConfigFile;

namespace Aerolt.Buttons
{
    public class EditPlayerItemButton : MonoBehaviour
    {
        public TMP_Dropdown sortMode;
        public TMP_InputField searchFilter;
        public GameObject buttonPrefab;
        public GameObject buttonParent;

        public GameObject itemListParent;
        
        private Dictionary<ItemDef, int> itemDef = new();
        private Dictionary<ItemDef, AddRemoveButtonGen<ItemDef>> itemDefRef = new();
        private NetworkUser user;
        private ZioConfigEntry<int> sortModeEntry;

        public void Awake()
        {
            if (sortMode)
            {
                sortMode.options.Clear();
                sortMode.AddOptions(new List<string>()
                {
                    "Tier Descending",
                    "Name Descending",
                    "Tier Ascending",
                    "Name Ascending",
                });
            }
            if(searchFilter)
                searchFilter.m_OnEndEdit.AddListener(FilterUpdated);
            foreach (var def in ContentManager._itemDefs)
                itemDefRef[def] = new AddRemoveButtonGen<ItemDef>(def, buttonPrefab, itemDef, buttonParent, itemListParent, false);
        }

        private void FilterUpdated(string text)
        {
            if (text.IsNullOrWhiteSpace())
            {
                foreach (var buttonGen in itemDefRef)
                {
                    buttonGen.Value.button.SetActive(true);
                }
                return;
            }

            var arr = itemDefRef.Values.ToArray();
            var matches = Tools.FindMatches(arr, (x) => Language.GetString(x.def.nameToken), text);
            foreach (var buttonGen in arr)
            {
                buttonGen.button.SetActive(matches.Contains(buttonGen));
            }
        }

        private void Sort()
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
            sortMode.value = sortModeEntry.Value;
            Sort();
        }

        public void GiveItems()
        {
            var inv = user.master.inventory;
            new SetItemCountMessage(inv, itemDef).SendToServer();
            GetComponentInParent<LobbyPlayerPageManager>().SwapViewState();
        }

        public void Initialize(NetworkUser currentUser)
        {
            user = currentUser;
            foreach (var def in ContentManager._itemDefs)
            {
                itemDefRef[def].SetAmount(user.master.inventory.GetItemCount(def));
            }
            
            Sort();
        }
    }
}