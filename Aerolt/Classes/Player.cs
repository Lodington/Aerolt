using R2API.Utils;
using RoR2;
using UnityEngine;

namespace Aerolt.Classes
{
    public class Player : MonoBehaviour
    {
        public void GodModeToggle()
        {
            var godToggleMethod = typeof(CharacterMaster).GetMethodCached("ToggleGod");
            bool hasNotYetRun = true;
            foreach (var playerInstance in PlayerCharacterMasterController.instances)
            {
                godToggleMethod.Invoke(playerInstance.master, null);
                if (hasNotYetRun)
                {
                    hasNotYetRun = false;
                }
            }
        }
        
    }
}