using UnityEngine;

namespace Aerolt.Classes
{
    public class ToggleWindow : MonoBehaviour
    {
        private bool MenuIsOpen;

        public GameObject Panel;

        public void Update()
        {
           if (Input.GetKeyDown(KeyCode.F1))
            {
                WindowToggle();
                Debug.Log("Toggled Main Window");
            }
        }
        public void WindowToggle()
        {
            MenuIsOpen = !MenuIsOpen;
            Panel.SetActive(MenuIsOpen);
        }
    }
}
