using System;
using System.Collections.Generic;
using System.Text;
using Aerolt.Helpers;
using Aerolt.Managers;
using AK.Wwise;
using RoR2;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using ZioConfigFile;

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
        public Toggle showShopAdvancedToggle;
        public Toggle showBarrelToggle;
        public Toggle showScrapperToggle;
        public Toggle ShowSecretToggle;
        
        public static Esp Instance;
        private ZioConfigEntry<bool> teleporterEntry;
        private ZioConfigEntry<bool> chestEntry;
        private ZioConfigEntry<bool> shopEntry;
        private ZioConfigEntry<bool> chestAdvancedEntry;
        private ZioConfigEntry<bool> shopAdvancedEntry;
        private ZioConfigEntry<bool> barrelEntry;
        private ZioConfigEntry<bool> scrapperEntry;
        private ZioConfigEntry<bool> secretEntry;

        public static void Draw()
        {
            if (Instance.showTeleporterToggle.isOn)
                ShowTeleporter();
            if (Instance.showChestToggle.isOn)
                DrawPurchaseInteractables();
            if (Instance.showMultiShopToggle.isOn)
                DrawShops();
            if (Instance.showBarrelToggle.isOn)
                DrawBarrelInteractables();
            if (Instance.showScrapperToggle.isOn)
                DrawScrapperInteractables();
            if (Instance.ShowSecretToggle.isOn)
                DrawSecretInteractables();
        }

        private static void DrawShops()
        {
            var str = new StringBuilder();
            foreach (var multiShopController in MultiShops)
            {
                str.Clear();
                var transform1 = multiShopController.transform;
                var position = transform1.position + transform1.up * 1.5f;
                var distanceToObject = Mathf.RoundToInt(Vector3.Distance(Camera.main.transform.position, position));
                var advanced = CheckCursorPosition(position) || Instance.showShopAdvancedToggle && Instance.showShopAdvancedToggle.isOn;
                if (advanced)
                    str.AppendLine("Multi Shop Terminal"); // TODO use lang token
                var costSet = false;
                foreach (var o in multiShopController.terminalGameObjects)
                {
                    var shop = o.GetComponent<ShopTerminalBehavior>();
                    var pickupDef = PickupCatalog.GetPickupDef(shop.pickupIndex);
                    if (pickupDef == null) continue;
                    if (!costSet)
                    {
                        costSet = true;
                        if (advanced)
                        {
                            str.Append(distanceToObject + "m ");
                            str.AppendLine("$" + shop.GetComponent<PurchaseInteraction>().cost);
                        }
                        else
                        {
                            str.AppendLine("+");
                            break;
                        }
                    }
                    if (pickupDef.itemIndex != ItemIndex.None)
                        str.AppendLine("- " + Language.GetString(ItemCatalog.GetItemDef(pickupDef.itemIndex).nameToken));
                    if (pickupDef.equipmentIndex != EquipmentIndex.None)
                        str.AppendLine("- " + Language.GetString(EquipmentCatalog.GetEquipmentDef(pickupDef.equipmentIndex).nameToken));
                }
                if (!costSet) continue;
                Helper.DrawESPLabel(position, Colors.GetColor("Shop"), Color.clear, str.ToString());
            }
        }

        public void Start()
        {
            var configFile = Load.Instance.configFile;
            teleporterEntry = configFile.Bind("ESP", "showTeleporter", false, "");
            showTeleporterToggle.Set(teleporterEntry.Value);
            chestEntry = configFile.Bind("ESP", "showChest", false, "");
            showChestToggle.Set(chestEntry.Value);
            shopEntry = configFile.Bind("ESP", "showShop", false, "");
            showMultiShopToggle.Set(shopEntry.Value);
            chestAdvancedEntry = configFile.Bind("ESP", "showChestAdvanced", false, "");
            showChestAdvancedToggle.Set(chestAdvancedEntry.Value);
            shopAdvancedEntry = configFile.Bind("ESP", "showShopAdvanced", false, "");
            if (showShopAdvancedToggle) // fucking lodington not updating the asset bundle
                showShopAdvancedToggle.Set(shopAdvancedEntry.Value);
            barrelEntry = configFile.Bind("ESP", "showBarrel", false, "");
            showBarrelToggle.Set(barrelEntry.Value);
            scrapperEntry = configFile.Bind("ESP", "showScrapper", false, "");
            showScrapperToggle.Set(scrapperEntry.Value);
            secretEntry = configFile.Bind("ESP", "showSecret", false, "");
            ShowSecretToggle.Set(secretEntry.Value);
            if (Instance) return;
            Instance = this;
            showTeleporterToggle.onValueChanged.AddListener(val => teleporterEntry.Value = val);
            showChestToggle.onValueChanged.AddListener(val => chestEntry.Value = val);
            showMultiShopToggle.onValueChanged.AddListener(val => shopEntry.Value = val);
            showChestAdvancedToggle.onValueChanged.AddListener(val => chestAdvancedEntry.Value = val);
            if (showShopAdvancedToggle) // fucking lodington not updating the asset bundle
                showShopAdvancedToggle.onValueChanged.AddListener(val => shopAdvancedEntry.Value = val);
            showBarrelToggle.onValueChanged.AddListener(val => barrelEntry.Value = val);
            showScrapperToggle.onValueChanged.AddListener(val => scrapperEntry.Value = val);
            ShowSecretToggle.onValueChanged.AddListener(val => secretEntry.Value = val);
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
            foreach (var purchaseInteraction in PurchaseInteractions)
            {
                if (!purchaseInteraction || !purchaseInteraction.available) continue;
                
                var chest = purchaseInteraction.GetComponent<ChestBehavior>();
                //var multiShop = purchaseInteraction.GetComponent<ShopTerminalBehavior>();
                if (chest && Instance.showChestToggle.isOn) 
                    ShowChest(chest, purchaseInteraction);

                /*
                if (multiShop && Instance.showMultiShopToggle.isOn) 
                    ShowShop(multiShop, purchaseInteraction);
                    */
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