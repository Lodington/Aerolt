using UnityEngine;

namespace Aerolt.Classes
{
    public class ToggleWindow : MonoBehaviour
    {
        private bool MenuIsOpen = false;

        public GameObject Panel;

        public void Update()
        {
           if (Input.GetKeyDown(KeyCode.F1)) WindowToggle();
        }
        
        public void WindowToggle()
        {
            MenuIsOpen = !MenuIsOpen;
            Panel.SetActive(MenuIsOpen);
        }
    }
}
