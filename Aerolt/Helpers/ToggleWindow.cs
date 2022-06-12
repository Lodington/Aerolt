using System;
using RoR2;
using UnityEngine;

namespace Aerolt.Classes
{
    public class ToggleWindow : MonoBehaviour
    {
        private bool _menuIsOpen = false;

        public GameObject panel;
        public NetworkUser owner;

        public void Init(NetworkUser owner)
        {
            
        }
        public void Update()
        {
           if (Input.GetKeyDown(KeyCode.F1)) WindowToggle();
        }
        
        public void WindowToggle()
        {
            _menuIsOpen = !_menuIsOpen;
            panel.SetActive(_menuIsOpen);
        }

        private void OnDestroy()
        {
            Load.areoltUIs.Remove(owner);
        }
    }
}
