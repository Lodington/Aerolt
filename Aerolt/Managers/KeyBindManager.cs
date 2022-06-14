using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Aerolt.Buttons
{
    public class KeyBindManager : MonoBehaviour
    {
        public TMP_Text OpenMenu;
        
        private GameObject currentKey;

        public void OnGUI()
        {
            if (currentKey != null)
            {
                Event e = Event.current;
                if (e.isKey)
                {
                    Load.Instance.KeyBinds[currentKey.name] = e.keyCode;
                    OpenMenu.text = e.keyCode.ToString();
                    currentKey = null;
                }
            }
        }
        
        public void Awake()
        {
            OpenMenu.text = Load.Instance.KeyBinds["OpenMenu"].ToString();
        }

        public void UpdateKey(GameObject clicked)
        {
            currentKey = clicked;
        }
        
        
    }
}