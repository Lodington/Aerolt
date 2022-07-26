using Aerolt.Helpers;
using TMPro;
using UnityEngine;

namespace Aerolt.Managers
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
                    OpenMenu.text = e.keyCode.ToString();
                    currentKey = null;
                }
            }
        }

        public void UpdateKey(ButtonNameHelper clicked)
        {
            currentKey = clicked;
        }
        
        
    }
}