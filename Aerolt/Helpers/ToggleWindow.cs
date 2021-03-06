using System;
using Aerolt.Enums;
using Aerolt.Managers;
using RoR2;
using UnityEngine;

namespace Aerolt.Classes
{
    public class ToggleWindow : MonoBehaviour
    {
        private bool _menuIsOpen = true;

        public GameObject panel;
        [NonSerialized] public NetworkUser owner;
        private PanelManager panelManager;

        public void Init(NetworkUser owner, PanelManager pm)
        {
            this.owner = owner;
            panelManager = pm;
            var profile = owner.localUser?.userProfile;
            if (profile == null)
            {
                // Delete ui objects that dont have a profile attached.
                Destroy(transform.parent.gameObject);
                return;
            }
            Invoke(nameof(WindowToggle), 0.01f);
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
            if (_menuIsOpen)
                panelManager.FuckingUnitySorting();
        }

        private void OnDestroy()
        {
            Load.aeroltUIs.Remove(owner);
        }
    }
}
