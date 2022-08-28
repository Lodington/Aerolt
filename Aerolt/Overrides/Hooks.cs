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
            var connectionWarningRectTransform = Load.ConnectionWarning.GetComponent(typeof(RectTransform)) as RectTransform;
            connectionWarningRectTransform.SetSize(746, 480);
            Object.Instantiate(Load.ConnectionWarning, transform);
            
            var chatwindowRect = Load.ConnectionWarning.GetComponent(typeof(RectTransform)) as RectTransform;
            chatwindowRect.SetSize(746, 480);
            Object.Instantiate(Load.chatWindow, transform);
        }
    }
}