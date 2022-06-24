using Aerolt.Enums;
using Aerolt.Managers;
using UnityEngine;

namespace Aerolt.Buttons
{
    public class ShowPanelButton : MonoBehaviour
    {
        public string PanelId;
        public bool showBack = true;

        public PanelShowBehaviour Behaviour;

        private PanelManager _panelManager;

        private void Start()
        {
            _panelManager = GetComponentInParent<PanelManager>();
        }

        public void DoShowPanel()
        {
            _panelManager.ShowPanel(PanelId, Behaviour, showBack);
        }
    }
}
