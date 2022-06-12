using System;
using RoR2;
using RoR2.ContentManagement;
using UnityEngine;
using UnityEngine.UI;
using LobbyManager = Aerolt.Managers.LobbyManager;

namespace Aerolt.Buttons
{
    public class EditPlayerItemButton : MonoBehaviour
    {
        public GameObject buttonPrefab;
        public GameObject buttonParent;

        public GameObject itemListParent;
        
        private NetworkUser _user;
        
        public EditPlayerItemButton(NetworkUser user)
        {
            _user = user;
        }

        private void OnEnable()
        {
            var inventory = _user.master.inventory;
            //foreach (var def in inventory)
           // {
                
          //  }
        }

        private void Awake()
        {
            foreach (var def in ContentManager._itemDefs)
                new InventoryItemAddRemoveButtonGen(def, buttonPrefab, LobbyManager.ItemDef, itemListParent, buttonParent, false);
        }
    }
}