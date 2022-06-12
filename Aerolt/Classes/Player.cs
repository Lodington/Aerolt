using RoR2;
using UnityEngine;

namespace Aerolt.Classes
{
    public class Player : MonoBehaviour
    {
        public void GodModeToggle()
        {
            bool hasNotYetRun = true;
            foreach (var playerInstance in PlayerCharacterMasterController.instances)
            {
                playerInstance.master.ToggleGod();
                if (hasNotYetRun)
                {
                    hasNotYetRun = false;
                }
            }
        }
        
    }
}