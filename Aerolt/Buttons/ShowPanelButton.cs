using Aerolt.Enums;
using Aerolt.Managers;
using UnityEngine;

namespace Aerolt.Buttons
{
    public class ShowPanelButton : MonoBehaviour
    {
        public string PanelId;

        public PanelShowBehaviour Behaviour;

        private PanelManager _panelManager;

        private void Start()
        {
            _panelManager = PanelManager.Instance;
        }

        public void DoShowPanel()
        {
            PanelManager.Instance.ShowPanel(PanelId, Behaviour);
        }
    }
}
