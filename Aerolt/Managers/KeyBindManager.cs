using System;
using System.Collections.Generic;
using Aerolt.Enums;
using Aerolt.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Aerolt.Buttons
{
    public class KeyBindManager : MonoBehaviour
    {
        public TMP_Text OpenMenu;
        
        private ButtonNameHelper currentKey;

        public void OnGUI()
        {
            if (currentKey != null)
            {
                Event e = Event.current;
                if (e.isKey)
                {
                    //Load.Instance.KeyBinds[currentKey.buttonName].Value = e.keyCode; Disabled for now;
                    OpenMenu.text = e.keyCode.ToString();
                    currentKey = null;
                }
            }
        }
        public void Awake()
        {
            //OpenMenu.text = Load.Instance.KeyBinds[ButtonNames.OpenMenu].Value.ToString(); Disabled for now;
        }

        public void UpdateKey(ButtonNameHelper clicked)
        {
            currentKey = clicked;
        }
        
        
    }
}