using System;
using System.Collections.Generic;
using Aerolt.Helpers;
using Aerolt.Managers;
using RoR2;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Aerolt.Classes
{
    public class Esp : MonoBehaviour
    {
        private static readonly EspHelper Helper = new EspHelper();
        
        public static List<PurchaseInteraction> PurchaseInteractions = new List<PurchaseInteraction>();
        public static List<BarrelInteraction> BarrelInteractions = new List<BarrelInteraction>();
        public static List<PressurePlateController> SecretButtons = new List<PressurePlateController>();
        public static List<ScrapperController> Scrappers = new List<ScrapperController>();
        public static List<MultiShopController> MultiShops = new List<MultiShopController>();

        public Toggle showTeleporterToggle;
        public Toggle showChestToggle;
        public Toggle showChestAdvancedToggle;
        public static Esp Instance;

        public static void Draw()
        {
            if (Instance.showTeleporterToggle.isOn)
                ShowTeleporter();
            if(Instance.showChestToggle.isOn)
                ShowChest();
 
        }
        public void Start()
        {
            Instance = this;
        }

        private static void ShowTeleporter()
        {
            if (TeleporterInteraction.instance)
            {
                var teleporterInteraction = TeleporterInteraction.instance;
                float distanceToObject = Vector3.Distance(Camera.main.transform.position, teleporterInteraction.transform.position);
                int distance = (int)distanceToObject;
                String friendlyName = "Teleporter";

                Color teleporterColor =
                    teleporterInteraction.isIdle ? Color.magenta :
                    teleporterInteraction.isIdleToCharging || teleporterInteraction.isCharging ? Color.yellow :
                    teleporterInteraction.isCharged ? Color.green : Color.yellow;

                string status = "" + (
                    teleporterInteraction.isIdle ? "Idle" :
                    teleporterInteraction.isCharging ? "Charging" :
                    teleporterInteraction.isCharged ? "Charged" :
                    teleporterInteraction.isActiveAndEnabled ? "Idle" :
                    teleporterInteraction.isIdleToCharging ? "Idle-Charging" :
                    teleporterInteraction.isInFinalSequence ? "Final-Sequence" :
                    "???");

                string boxText = $"{friendlyName}\n{status}\n{distance}m";
                Helper.DrawESPLabel(teleporterInteraction.transform.position, teleporterColor, Color.clear, boxText);
                
            }
        }
        
        public static void ShowChest()
        {
            foreach (PurchaseInteraction purchaseInteraction in PurchaseInteractions)
            {
                if (purchaseInteraction.available)
                {
                    var chest = purchaseInteraction?.gameObject.GetComponent<ChestBehavior>();

                    float distanceToObject = Vector3.Distance(Camera.main.transform.position, purchaseInteraction.transform.position);
                    int distance = (int)distanceToObject;
                    String friendlyName = purchaseInteraction.GetDisplayName();
                    int cost = purchaseInteraction.cost;
                    var ItemToDrop = Language.GetString(chest.dropPickup.pickupDef.nameToken);
                    string boxText = $"{friendlyName}\n${cost}\n{ItemToDrop}\n{distance}m";
                    
                    if (Instance.showChestAdvancedToggle.isOn || CheckCursorPosition(purchaseInteraction.transform.position))
                        Helper.DrawESPLabel(purchaseInteraction.transform.position, Colors.GetColor("Chest"), Color.clear, boxText);

                    Helper.DrawESPLabel(purchaseInteraction.transform.position, Colors.GetColor("Chest"), Color.clear, "+");

                }
            }
        }

        public static bool CheckCursorPosition(Vector3 WorldPos)
        {
            var mpEventSystem = LocalUserManager.GetFirstLocalUser().eventSystem;
            mpEventSystem.GetCursorPosition(out var cursorPos);
            cursorPos.y = Camera.main.pixelRect.height - cursorPos.y;
            if (!mpEventSystem.isCursorVisible) 
                cursorPos = Camera.main.pixelRect.center;
            return Vector3.Distance(EspHelper.WorldToScreen(WorldPos), cursorPos) < 50;
        }
        
    }
}