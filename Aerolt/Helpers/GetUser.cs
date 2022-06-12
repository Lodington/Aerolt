using RoR2;
using RoR2.UI;

namespace Aerolt.Helpers
{
    public class GetUser
    {
        // Hud wihch does the info to has the info on which user is connectedd to it, hud has info on camera contoller,
        //     
        //     camera rig contoller that is tied to network user, has list of local user ,
        //     
        //     find out whos hud belongs to whos body


        public static LocalUser FetchUser(HUD hud)
        {
            if (!hud || !hud.cameraRigController || hud.cameraRigController.localUserViewer == null)
                return LocalUserManager.GetFirstLocalUser();
            return hud.cameraRigController.localUserViewer;
        }
    }
}