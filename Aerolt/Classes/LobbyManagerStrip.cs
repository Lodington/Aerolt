using System;
using System.Collections.Generic;
using RoR2;
using RoR2.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.Serialization;
using UnityEngine.UI;
using LobbyManager = Aerolt.Managers.LobbyManager;

namespace Aerolt.Classes
{
    public class LobbyManagerStrip : MonoBehaviour
    {
        private NetworkUser _user;
        
        public Image icon;
        public TMP_Text userName;
        public TMP_InputField moneyInputField;
        public TMP_InputField lunarCoinsInputField;
        public TMP_InputField voidMarkersInputField;
        public ItemInventoryDisplay itemInventoryDisplay;
        
        public void Init(NetworkUser user, LobbyManager lobbyManager)
        {
            CharacterBody body = user.master.GetBody();
            _user = user;
            
            itemInventoryDisplay.SetSubscribedInventory(user.master.inventory);
            
            icon.sprite = Sprite.Create((Texture2D)body.portraitIcon, new Rect(0, 0, body.portraitIcon.width, body.portraitIcon.height), new Vector2(0.5f, 0.5f));;
            userName.text = user.userName;

            moneyInputField.m_OnEndEdit.AddListener(UpdateMoney);
            lunarCoinsInputField.m_OnEndEdit.AddListener(UpdateLunarCoin);
            voidMarkersInputField.m_OnEndEdit.AddListener(UpdateVoidMarkers);
        }
        
        

        private void UpdateVoidMarkers(string arg0)
        {
            if (int.TryParse(voidMarkersInputField.text, out int value)) _user.master.voidCoins = (uint) value;
            voidMarkersInputField.text = string.Empty;
        }

        private void UpdateLunarCoin(string arg0)
        {
            if (int.TryParse(lunarCoinsInputField.text, out int value)) _user.NetworknetLunarCoins = (uint) value;
            lunarCoinsInputField.text = string.Empty;
        }


        public void UpdateMoney(string arg0)
        {
            if (int.TryParse(moneyInputField.text, out int value)) _user.master.money = (uint) value;
            moneyInputField.text = String.Empty;
        }
        
        
    }
}