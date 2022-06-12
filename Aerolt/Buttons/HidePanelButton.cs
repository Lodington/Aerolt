using Aerolt.Managers;
using UnityEngine;

namespace Aerolt.Buttons
{
    public class HidePanelButton : MonoBehaviour
    {
        private PanelManager _panelManager;

        private void Start()
        {
            _panelManager = GetComponentInParent<PanelManager>();
        }


        public void DoHidePanel()
        {
            _panelManager.HideLastPanel();
        }
    }
}
