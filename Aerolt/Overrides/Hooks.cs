using System.Linq;
using Aerolt.Classes;
using HarmonyLib;
using RoR2;
using RoR2.UI;

namespace Aerolt.Overrides
{
    [HarmonyPatch]
    public class Hooks
    {
        public static void GetEspData(SceneDirector obj)
        {
            Esp.BarrelInteractions = UnityEngine.Object.FindObjectsOfType<BarrelInteraction>().ToList();
            Esp.PurchaseInteractions = UnityEngine.Object.FindObjectsOfType<PurchaseInteraction>().ToList();
            Esp.SecretButtons = UnityEngine.Object.FindObjectsOfType<PressurePlateController>().ToList();
            Esp.Scrappers = UnityEngine.Object.FindObjectsOfType<ScrapperController>().ToList();
            Esp.MultiShops = UnityEngine.Object.FindObjectsOfType<MultiShopController>().ToList();    
        }


    }
}
