using Aerolt.Helpers;
using HarmonyLib;
using RoR2.UI.MainMenu;
using UnityEngine;

namespace Aerolt.Overrides
{
    [HarmonyPatch]
    public class Hooks
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(BaseMainMenuScreen), nameof(BaseMainMenuScreen.Awake))]
        public static void MenuTransform(BaseMainMenuScreen __instance)
        {
            var transform = __instance.transform;

            Object.Instantiate(Load.chatWindow, transform);

            var chatwindowRect = Load.chatWindow.GetComponent(typeof(RectTransform)) as RectTransform;
            Load.chatWindow.transform.localPosition =
                Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height,
                    Camera.main.nearClipPlane));
            chatwindowRect.SetSize(746, 480);
        }
    }
}