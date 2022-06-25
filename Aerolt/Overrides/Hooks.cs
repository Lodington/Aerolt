using System;
using System.Linq;
using Aerolt.Classes;
using Aerolt.Managers;
using HarmonyLib;
using RoR2;
using RoR2.UI;
using Object = UnityEngine.Object;

namespace Aerolt.Overrides
{
    [HarmonyPatch]
    public class Hooks
    {
        public static void GetEspData(SceneDirector director)
        {
            Esp.BarrelInteractions = Object.FindObjectsOfType<BarrelInteraction>().ToList();
            Esp.PurchaseInteractions = Object.FindObjectsOfType<PurchaseInteraction>().ToList();
            Esp.SecretButtons = Object.FindObjectsOfType<PressurePlateController>().ToList();
            Esp.Scrappers = Object.FindObjectsOfType<ScrapperController>().ToList();
            Esp.MultiShops = Object.FindObjectsOfType<MultiShopController>().ToList();
            
            InteractableManager.Cards = director.GenerateInteractableCardSelection().choices.Where(x => x.value != null).Select(x => x.value.spawnCard).ToList();
            
        }
    }
}
