using System;
using System.Collections.Generic;
using Aerolt.Helpers;
using Aerolt.Managers;
using AK.Wwise;
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
        public Toggle showMultiShopToggle;
        public Toggle showBarrelToggle;
        public Toggle showScrapperToggle;
        public Toggle ShowSecretToggle;
        
        public static Esp Instance;

        public static void Draw()
        {
            if (Instance.showTeleporterToggle.isOn)
                ShowTeleporter();
            if (Instance.showChestToggle.isOn || Instance.showMultiShopToggle.isOn)
                DrawPurchaseInteractables();
            if (Instance.showBarrelToggle.isOn)
                DrawBarrelInteractables();
            if (Instance.showScrapperToggle.isOn)
                DrawScrapperInteractables();
            if (Instance.ShowSecretToggle.isOn)
                DrawSecretInteractables();
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

                Color teleporterColor;
                
                switch(TeleporterInteraction.instance.currentState)
                {
                    case TeleporterInteraction.IdleState:
                        teleporterColor = Color.magenta;
                        break;
                    case TeleporterInteraction.ChargingState:
                    case TeleporterInteraction.IdleToChargingState :
                        teleporterColor = Color.yellow;
                        break;
                    case TeleporterInteraction.ChargedState:
                        teleporterColor = Color.green;
                        break;
                    default:
                        teleporterColor = Color.red;
                        break;
                }
                
                string status = TeleporterInteraction.instance.activationState.ToString();
                
                string boxText = $"{friendlyName}\n{status}\n{distance}m";
                Helper.DrawESPLabel(teleporterInteraction.transform.position, teleporterColor, Color.clear, boxText);
                
            }
        }
        
        private static void DrawBarrelInteractables()
        {
            foreach (BarrelInteraction barrel in BarrelInteractions)
            {
                if (!barrel.Networkopened)
                {
                    string friendlyName = "Barrel";
                    float distance = (int)Vector3.Distance(Camera.main.transform.position, barrel.transform.position);
                    string boxText = $"{friendlyName}\n{distance}m";
                    
                    if (Instance.showBarrelToggle.isOn || CheckCursorPosition(barrel.transform.position))
                        Helper.DrawESPLabel(barrel.transform.position, Colors.GetColor("Barrels"), Color.clear,boxText);
                    else
                        Helper.DrawESPLabel(barrel.transform.position, Colors.GetColor("Barrels"), Color.clear, "+");
                }
            }
        }
        private static void DrawSecretInteractables()
        {
            foreach (PressurePlateController secretButton in SecretButtons)
            {
                if (secretButton)
                {
                    string friendlyName = "Secret Button";

                    float distance = (int)Vector3.Distance(Camera.main.transform.position, secretButton.transform.position);
                    string boxText = $"{friendlyName}\n{distance}m";
                    Helper.DrawESPLabel(secretButton.transform.position, Colors.GetColor("Secret_Plates"), Color.clear, boxText);
                    
                }
            }
        }
        private static void DrawScrapperInteractables()
        {
            foreach (ScrapperController scrapper in Scrappers)
            {
                if (scrapper)
                {
                    string friendlyName = "Scrapper";

                    var position = scrapper.transform.position;
                    float distance = (int)Vector3.Distance(Camera.main.transform.position, position);
                    string boxText = $"{friendlyName}\n{distance}m";
                    Helper.DrawESPLabel(position, Colors.GetColor("Scrappers"), Color.clear, boxText);
                }
            }
        }
        public static void DrawPurchaseInteractables()
        {
            foreach (PurchaseInteraction purchaseInteraction in PurchaseInteractions)
            {
                if (purchaseInteraction.available)
                {
                    var chest = purchaseInteraction.GetComponent<ChestBehavior>();
                    var multiShop = purchaseInteraction.GetComponent<ShopTerminalBehavior>();
                    if (chest && Instance.showChestToggle.isOn) 
                        ShowChest(chest, purchaseInteraction);

                    if (multiShop && Instance.showMultiShopToggle.isOn) 
                        ShowShop(multiShop, purchaseInteraction);
                }
            }
        }
        public static void ShowChest(ChestBehavior chest, PurchaseInteraction purchaseInteraction)
        {
            if (Instance.showChestAdvancedToggle.isOn || CheckCursorPosition(purchaseInteraction.transform.position))
                Helper.DrawESPLabel(purchaseInteraction.transform.position, Colors.GetColor("Chest"), Color.clear, GetDistance(purchaseInteraction));
            else
                Helper.DrawESPLabel(purchaseInteraction.transform.position, Colors.GetColor("Chest"), Color.clear, "+");
        }
        public static string GetDistance(PurchaseInteraction purchaseInteraction)
        {
            float distanceToObject = Vector3.Distance(Camera.main.transform.position, purchaseInteraction.transform.position);
            int distance = (int)distanceToObject;
            String friendlyName = purchaseInteraction.GetDisplayName();
            int cost = purchaseInteraction.cost;

            return $"{friendlyName}\n${cost}\n{distance}m";
        }
        private static void ShowShop(ShopTerminalBehavior multiShop, PurchaseInteraction purchaseInteraction)
        {
            string boxText = GetDistance(purchaseInteraction);

            if (CheckCursorPosition(purchaseInteraction.transform.position))
                Helper.DrawESPLabel(purchaseInteraction.transform.position, Colors.GetColor("Chest"), Color.clear, boxText);
            else
                Helper.DrawESPLabel(purchaseInteraction.transform.position, Colors.GetColor("Chest"), Color.clear, "+");

            
        }

        public static bool CheckCursorPosition(Vector3 worldPos)
        {
            var mpEventSystem = LocalUserManager.GetFirstLocalUser().eventSystem;
            mpEventSystem.GetCursorPosition(out var cursorPos);
            cursorPos.y = Camera.main.pixelRect.height - cursorPos.y;
            if (!mpEventSystem.isCursorVisible) 
                cursorPos = Camera.main.pixelRect.center;
            return Vector3.Distance(EspHelper.WorldToScreen(worldPos), cursorPos) < 50;
        }
        
    }
}