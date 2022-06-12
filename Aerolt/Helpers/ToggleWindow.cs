using UnityEngine;

namespace Aerolt.Classes
{
    public class ToggleWindow : MonoBehaviour
    {
        private bool _menuIsOpen = false;

        public GameObject panel;

        public void Update()
        {
           if (Input.GetKeyDown(KeyCode.F1)) WindowToggle();
        }
        
        public void WindowToggle()
        {
            _menuIsOpen = !_menuIsOpen;
            panel.SetActive(_menuIsOpen);
        }
    }
}
