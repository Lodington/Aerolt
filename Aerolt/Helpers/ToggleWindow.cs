using Aerolt.Enums;
using RoR2;
using UnityEngine;

namespace Aerolt.Classes
{
    public class ToggleWindow : MonoBehaviour
    {
        private bool _menuIsOpen = true;

        public GameObject panel;
        public NetworkUser owner;

        public void Init(NetworkUser owner)
        {
            this.owner = owner;
            var profile = owner.localUser?.userProfile;
            if (profile == null)
            {
                // Delete ui objects that dont have a profile attached.
                Destroy(transform.parent.gameObject);
                return;
            }
            WindowToggle();
        }
        public void Update()
        {
            var localUser = LocalUserManager.GetFirstLocalUser();
            var isFirst = localUser == owner.localUser;
            if (isFirst && Load.GetKeyPressed(Load.KeyBinds[ButtonNames.OpenMenu]))
                WindowToggle();
        }
        
        public void WindowToggle()
        {
            _menuIsOpen = !_menuIsOpen;
            panel.SetActive(_menuIsOpen);
        }

        private void OnDestroy()
        {
            Load.aeroltUIs.Remove(owner);
        }
    }
}
