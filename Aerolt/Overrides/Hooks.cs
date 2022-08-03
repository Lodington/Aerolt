using Aerolt.Helpers;
using HarmonyLib;
using RoR2.UI;
using RoR2.UI.MainMenu;
using UnityEngine;

namespace Aerolt.Overrides
{
    [HarmonyPatch]
    public class Hooks
    {
        [HarmonyPostfix, HarmonyPatch(typeof(BaseMainMenuScreen), nameof(BaseMainMenuScreen.Awake))]
        public static void MenuTransform(BaseMainMenuScreen __instance)
        {
            var transform = __instance.transform;

            GameObject.Instantiate(Load.chatWindow, transform);
            
            RectTransform chatwindowRect = Load.changeLogWindow.GetComponent (typeof (RectTransform)) as RectTransform;
            Load.changeLogWindow.transform.localPosition =
                Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height,
                    Camera.main.nearClipPlane));
            chatwindowRect.SetSize(746,480);
        }
    }
}