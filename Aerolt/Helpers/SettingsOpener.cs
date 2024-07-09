using System.Linq;
using Aerolt.Classes;
using BepInEx.Bootstrap;
using RiskOfOptions.Components.Panel;
using RoR2;
using RoR2.UI;
using UnityEngine;

namespace Aerolt.Helpers
{
    public class SettingsOpener : MonoBehaviour
    {
        private Transform headerTransform;
        private LocalUser localuser;
        private OpenState openSettings;

        public void Awake() => localuser = GetComponentInParent<MenuInfo>().Owner.localUser;

        private void Update()
        {
            if (openSettings == OpenState.Idle) return;
            var screen = PauseScreenController.instancesList.FirstOrDefault(x =>
                x.eventSystemProvider.resolvedEventSystem == localuser?.eventSystem);
            switch (openSettings)
            {
                case OpenState.WaitingForPause:
                    if (screen && Chainloader.PluginInfos.ContainsKey("bubbet.zioriskofoptions"))
                    {
                        screen.OpenSettingsMenu();
                        headerTransform =
                            screen.submenuObject.transform.Find("SafeArea/HeaderContainer/Header (JUICED)");
                        openSettings = OpenState.WaitingForRoO;
                        return;
                    }

                    break;
                case OpenState.WaitingForRoO:
                    var modSettings = headerTransform.Find("GenericHeaderButton (Mod Options)");
                    if (modSettings)
                    {
                        modSettings.GetComponent<HGButton>().InvokeClick();
                        openSettings = OpenState.WaitingForCategory;
                    }

                    return;
                case OpenState.WaitingForCategory:
                    OpenSettings(screen);
                    return;
            }

            openSettings = OpenState.Idle;
        }

        public void OpenSettingsForced()
        {
            Console.instance.SubmitCmd(null,
                "pause"); // This is how you do it, apparently splitscreen users do not have their own pause screen despite it being mostly implemented that way.
            if (localuser != null)
                openSettings = OpenState.WaitingForPause;
        }

        private void OpenSettings(PauseScreenController screen)
        {
            var category = screen.submenuObject.transform.Find(
                $"SafeArea/SubPanelArea/SettingsSubPanel, (Mod Options)/Mod List Panel(Clone)/Scroll View/Viewport/VerticalLayout/ModListButton ({Load.Name + " " + localuser.currentNetworkUser.GetNetworkPlayerName().GetResolvedName()})");
            if (!category) return;
            category.GetComponent<ModListButton>().InvokeClick();
            openSettings = OpenState.Idle;
        }

        private enum OpenState
        {
            Idle,
            WaitingForPause,
            WaitingForRoO,
            WaitingForCategory
        }
    }
}