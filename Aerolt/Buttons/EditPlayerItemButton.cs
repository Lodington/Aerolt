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

        public static GameObject _globalButtonPrefab;
        public static GameObject _globalitemListParent;

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
            _globalButtonPrefab = buttonPrefab;
            _globalitemListParent = itemListParent;

            foreach (var def in ContentManager._itemDefs)
                new ButtonStruct(def, buttonParent,false);
        }
    }
    public class ButtonStruct
    {
        public ItemDef _def;
        public GameObject _button;
        public ButtonStruct CountButton;
        public ButtonStruct(ItemDef def, GameObject ButtonParent, bool doDestroy = true)
        {
            _def = def;
            _button = CreateButton(ButtonParent);
            _button.GetComponent<Button>().onClick.AddListener(!doDestroy ? AddItemToList : DestroyButton);
        }
        public GameObject CreateButton(GameObject buttonParent)
        {
            var newButton = GameObject.Instantiate(EditPlayerItemButton._globalButtonPrefab, buttonParent.transform);
            
            newButton.GetComponent<CustomButton>().ButtonText.text = Language.GetString(_def.nameToken);
            newButton.GetComponent<Image>().sprite = _def.pickupIconSprite;
            return newButton;
        }
        public void DestroyButton()
        {
            LobbyManager.ItemDef[_def]--;
            _button.GetComponent<CustomButton>().ButtonText.text = $"{Language.GetString(_def.nameToken)} x{LobbyManager.ItemDef[_def]}";

            if (LobbyManager.ItemDef[_def] == 0)
            {
                LobbyManager.ItemDef.Remove(_def);
                GameObject.Destroy(_button);
            }
            
        }
        public void AddItemToList()
        {
            if (!LobbyManager.ItemDef.ContainsKey(_def))
            {
                CountButton = new ButtonStruct(_def, EditPlayerItemButton._globalitemListParent);
                LobbyManager.ItemDef.Add(_def, 0);
            }
            LobbyManager.ItemDef[_def]++;
            CountButton._button.GetComponent<CustomButton>().ButtonText.text = $"{Language.GetString(_def.nameToken)} x{LobbyManager.ItemDef[_def]}";

        }
    }
}