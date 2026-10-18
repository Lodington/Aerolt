using Aerolt.UI;
using HarmonyLib;
using RoR2;

namespace Aerolt.Overrides
{
    [HarmonyPatch]
    public class Hooks
    {
        [HarmonyPatch(typeof(LocalUser), nameof(LocalUser.isUIFocused), MethodType.Getter)]
        [HarmonyPostfix]
        private static void ForceUIFocusWhileMenuOpen(ref bool __result)
        {
            if (AeroltUniverseUI.IsOpen)
                __result = true;
        }
    }
}
