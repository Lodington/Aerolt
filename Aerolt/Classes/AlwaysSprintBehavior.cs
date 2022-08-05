using RoR2;
using UnityEngine;

namespace Aerolt.Classes
{
    public class AlwaysSprintBehavior : MonoBehaviour
    {
        private PlayerCharacterMasterController controller;
        private LocalUser localUser;

        public void Awake()
        {
            localUser = LocalUserManager.GetFirstLocalUser();
            if (localUser == null || localUser.cachedMasterController == null ||
                localUser.cachedMasterController.master == null) return;
            controller = localUser.cachedMasterController;
        }

        public void Update()
        {
            if (!controller)
            {
                Destroy(this);
                return;
            }

            var body = controller.master.GetBody();
            if (body && !body.isSprinting && !localUser.inputPlayer.GetButton("Sprint"))
                controller.sprintInputPressReceived = true;
        }
    }
}