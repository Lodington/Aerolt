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
            GameObject.Instantiate(Load.changeLogWindow, transform);
            
            RectTransform rt = Load.changeLogWindow.GetComponent (typeof (RectTransform)) as RectTransform;
            Load.changeLogWindow.transform.localPosition =
                Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2 + 50, Screen.height / 2,
                    Camera.main.nearClipPlane));
            rt.SetSize(746,480);
            

        }
    }
}