using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aerolt.Helpers;
using Aerolt.Managers;
using RoR2;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using ZioConfigFile;

namespace Aerolt.Classes
{
    public class Esp : MonoBehaviour, IModuleStartup
    {
        public static List<PurchaseInteraction> PurchaseInteractions;
        public static List<BarrelInteraction> BarrelInteractions;
        public static List<PressurePlateController> SecretButtons;
        public static List<ScrapperController> Scrappers;
        public static List<MultiShopController> MultiShops;


        public static Esp Instance;

        public Toggle showAdvancedToggle;
        public Toggle showTeleporterToggle;
        public Toggle showChestToggle;
        public Toggle showMultiShopToggle;
        public Toggle showBarrelToggle;
        public Toggle showScrapperToggle;
        public Toggle ShowSecretToggle;
        public Toggle showDuplicatorToggle;
        public Toggle showDroneToggle;
        public Toggle showShrineToggle;

        [FormerlySerializedAs("showClensingPoolToggle")]
        public Toggle showNewtAlterToggle;

        private ZioConfigEntry<bool> advancedEntry;
        private ZioConfigEntry<bool> barrelEntry;
        private ZioConfigEntry<bool> chestEntry;
        private ZioConfigEntry<bool> droneEntry;
        private ZioConfigEntry<bool> duplicatorEntry;
        private ZioConfigEntry<bool> scrapperEntry;
        private ZioConfigEntry<bool> secretEntry;
        private ZioConfigEntry<bool> shopEntry;
        private ZioConfigEntry<bool> showNewtAlterEntry;
        private ZioConfigEntry<bool> shrineEntry;
        private ZioConfigEntry<bool> teleporterEntry;

        public void ModuleStart()
        {
            var configFile = Load.ConfigFile;

            teleporterEntry = configFile.Bind("ESP", "showTeleporter", false, "");
            showTeleporterToggle.Set(teleporterEntry.Value);
            chestEntry = configFile.Bind("ESP", "showChest", false, "");
            showChestToggle.Set(chestEntry.Value);
            shopEntry = configFile.Bind("ESP", "showShop", false, "");
            showMultiShopToggle.Set(shopEntry.Value);
            advancedEntry = configFile.Bind("ESP", "showAdvanced", false, "");
            showAdvancedToggle.Set(advancedEntry.Value);

            barrelEntry = configFile.Bind("ESP", "showBarrel", false, "");
            showBarrelToggle.Set(barrelEntry.Value);
            scrapperEntry = configFile.Bind("ESP", "showScrapper", false, "");
            showScrapperToggle.Set(scrapperEntry.Value);
            secretEntry = configFile.Bind("ESP", "showSecret", false, "");
            ShowSecretToggle.Set(secretEntry.Value);

            duplicatorEntry = configFile.Bind("ESP", "showDuplicator", false, "");
            showDuplicatorToggle.Set(duplicatorEntry.Value);
            droneEntry = configFile.Bind("ESP", "showDrone", false, "");
            showDroneToggle.Set(droneEntry.Value);
            shrineEntry = configFile.Bind("ESP", "showShrine", false, "");
            showShrineToggle.Set(shrineEntry.Value);

            showNewtAlterEntry = configFile.Bind("ESP", "showNewtAlter", false, "");
            showNewtAlterToggle.Set(showNewtAlterEntry.Value);


            if (Instance) return;
            Instance = this;

            showTeleporterToggle.onValueChanged.AddListener(val => teleporterEntry.Value = val);
            showChestToggle.onValueChanged.AddListener(val => chestEntry.Value = val);
            showMultiShopToggle.onValueChanged.AddListener(val => shopEntry.Value = val);
            showAdvancedToggle.onValueChanged.AddListener(val => advancedEntry.Value = val);


            showBarrelToggle.onValueChanged.AddListener(val => barrelEntry.Value = val);
            showScrapperToggle.onValueChanged.AddListener(val => scrapperEntry.Value = val);
            ShowSecretToggle.onValueChanged.AddListener(val => secretEntry.Value = val);
            showDuplicatorToggle.onValueChanged.AddListener(val => duplicatorEntry.Value = val);
            showDroneToggle.onValueChanged.AddListener(val => droneEntry.Value = val);
            showShrineToggle.onValueChanged.AddListener(val => shrineEntry.Value = val);

            showNewtAlterToggle.onValueChanged.AddListener(val => showNewtAlterEntry.Value = val);

            GatherObjects(); // these objects should exist by hud awake
        }

        public static void Draw()
        {
            if (Instance.showTeleporterToggle.isOn)
                ShowTeleporter();
            if (Instance.showChestToggle.isOn || Instance.showDuplicatorToggle.isOn || Instance.showDroneToggle.isOn ||
                Instance.showShrineToggle.isOn || Instance.showNewtAlterToggle.isOn)
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
                if (!multiShopController) continue;
                var transform1 = multiShopController.transform;
                var position = transform1.position + transform1.up * 1.5f;
                var distanceToObject = Mathf.RoundToInt(Vector3.Distance(Camera.main.transform.position, position));
                var advanced = CheckCursorPosition(position) || Instance.showAdvancedToggle.isOn;
                if (advanced)
                    str.AppendLine("Multi Shop Terminal"); // TODO use lang token
                var costSet = false;
                if (multiShopController.terminalGameObjects.src == null) continue; // TODO properly fix this
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
                        str.AppendLine("- " +
                                       Language.GetString(ItemCatalog.GetItemDef(pickupDef.itemIndex).nameToken));
                    if (pickupDef.equipmentIndex != EquipmentIndex.None)
                        str.AppendLine("- " +
                                       Language.GetString(EquipmentCatalog.GetEquipmentDef(pickupDef.equipmentIndex)
                                           .nameToken));
                }

                if (!costSet) continue;
                EspHelper.DrawESPLabel(position, Colors.GetColor("Shop"), Color.clear, str.ToString());
            }
        }

        private void GatherObjects()
        {
            BarrelInteractions = FindObjectsOfType<BarrelInteraction>().ToList();
            PurchaseInteractions = FindObjectsOfType<PurchaseInteraction>().ToList();
            SecretButtons = FindObjectsOfType<PressurePlateController>().ToList();
            Scrappers = FindObjectsOfType<ScrapperController>().ToList();
            MultiShops = FindObjectsOfType<MultiShopController>().ToList();
        }

        private static void ShowTeleporter()
        {
            if (TeleporterInteraction.instance)
            {
                var teleporterInteraction = TeleporterInteraction.instance;
                var distanceToObject = Vector3.Distance(Camera.main.transform.position,
                    teleporterInteraction.transform.position);
                var distance = (int) distanceToObject;
                var friendlyName = "Teleporter";

                Color teleporterColor;

                switch (TeleporterInteraction.instance.currentState)
                {
                    case TeleporterInteraction.IdleState:
                        teleporterColor = Color.magenta;
                        break;
                    case TeleporterInteraction.ChargingState:
                    case TeleporterInteraction.IdleToChargingState:
                        teleporterColor = Color.yellow;
                        break;
                    case TeleporterInteraction.ChargedState:
                        teleporterColor = Color.green;
                        break;
                    default:
                        teleporterColor = Color.red;
                        break;
                }

                var status = TeleporterInteraction.instance.activationState.ToString();

                var boxText = $"{friendlyName}\n{status}\n{distance}m";
                EspHelper.DrawESPLabel(teleporterInteraction.transform.position, teleporterColor, Color.clear, boxText);
            }
        }

        private static void DrawBarrelInteractables()
        {
            foreach (var barrel in BarrelInteractions)
            {
                if (!barrel) continue;
                if (!barrel.Networkopened)
                {
                    var friendlyName = "Barrel";
                    float distance = (int) Vector3.Distance(Camera.main.transform.position, barrel.transform.position);
                    var boxText = $"{friendlyName}\n{distance}m";

                    if (Instance.showBarrelToggle.isOn || CheckCursorPosition(barrel.transform.position))
                        EspHelper.DrawESPLabel(barrel.transform.position, Colors.GetColor("Barrels"), Color.clear,
                            boxText);
                    else
                        EspHelper.DrawESPLabel(barrel.transform.position, Colors.GetColor("Barrels"), Color.clear, "+");
                }
            }
        }

        private static void DrawSecretInteractables()
        {
            foreach (var secretButton in SecretButtons)
                if (secretButton)
                {
                    var friendlyName = "Secret Button";

                    float distance =
                        (int) Vector3.Distance(Camera.main.transform.position, secretButton.transform.position);
                    var boxText = $"{friendlyName}\n{distance}m";
                    EspHelper.DrawESPLabel(secretButton.transform.position, Colors.GetColor("Secret_Plates"),
                        Color.clear, boxText);
                }
        }

        private static void DrawScrapperInteractables()
        {
            foreach (var scrapper in Scrappers)
                if (scrapper)
                {
                    var friendlyName = "Scrapper";

                    var position = scrapper.transform.position;
                    float distance = (int) Vector3.Distance(Camera.main.transform.position, position);
                    var boxText = $"{friendlyName}\n{distance}m";
                    EspHelper.DrawESPLabel(position, Colors.GetColor("Scrappers"), Color.clear, boxText);
                }
        }

        public static void DrawPurchaseInteractables()
        {
            foreach (var purchaseInteraction in PurchaseInteractions)
            {
                if (!purchaseInteraction || !purchaseInteraction.available) continue;

                var chest = purchaseInteraction.GetComponent<ChestBehavior>();
                if (chest && Instance.showChestToggle.isOn)
                    ShowChest(chest, purchaseInteraction);

                var optionChest = purchaseInteraction.GetComponent<OptionChestBehavior>();
                if (optionChest && Instance.showChestToggle.isOn)
                    ShowChest(optionChest, purchaseInteraction);

                var casinoChest = purchaseInteraction.GetComponent<RouletteChestController>();
                if (casinoChest && Instance.showChestToggle.isOn)
                    ShowChest(casinoChest, purchaseInteraction);

                var timedChest =
                    purchaseInteraction.GetComponent<TimedChestController>(); // TODO this is not a purchase interaction
                if (timedChest && Instance.showChestToggle.isOn)
                    ShowChest(timedChest, purchaseInteraction);

                var shopTerminal = purchaseInteraction.GetComponent<ShopTerminalBehavior>();
                if (shopTerminal)
                    if (Instance.showDuplicatorToggle.isOn)
                    {
                        if (purchaseInteraction.displayNameToken == "DUPLICATOR_NAME")
                            ShowDuplicator("DUPLICATOR_NAME", shopTerminal);
                        if (purchaseInteraction.displayNameToken == "BAZAAR_CAULDRON_NAME")
                            ShowDuplicator("BAZAAR_CAULDRON_NAME", shopTerminal);
                    }

                var masterSummon = purchaseInteraction.GetComponent<SummonMasterBehavior>();
                if (masterSummon && Instance.showDroneToggle.isOn) ShowDrone(purchaseInteraction);

                var nameProvider = purchaseInteraction.GetComponent<GenericDisplayNameProvider>();
                if (nameProvider)
                    if (Instance.showShrineToggle.isOn && nameProvider.displayToken.ToUpper().Contains("SHRINE"))
                        ShowShrine(purchaseInteraction);

                var portalBehaviour = purchaseInteraction.GetComponent<PortalStatueBehavior>();
                if (portalBehaviour && Instance.showNewtAlterToggle.isOn) ShowNewtAlter(purchaseInteraction);
            }
        }

        private static void ShowNewtAlter(PurchaseInteraction purchaseInteraction)
        {
            EspHelper.DrawESPLabel(purchaseInteraction.transform.position, Colors.GetColor("NewtAlter"), Color.clear,
                GetLabel(purchaseInteraction));
        }

        private static void ShowShrine(PurchaseInteraction purchaseInteraction)
        {
            string colorName = purchaseInteraction.displayNameToken switch
            {
                "SHRINE_BLOOD_NAME"         => "Shrine of Blood",
                "SHRINE_CHANCE_NAME"        => "Shrine of Chance",
                "SHRINE_CLEANSE_NAME"       => "Cleansing Pool",
                "SHRINE_COMBAT_NAME"        => "Shrine of Combat",
                "SHRINE_GOLDSHORES_NAME"    => "Altar of Gold",
                "SHRINE_BOSS_NAME"          => "Shrine of the Mountain",
                "SHRINE_RESTACK_NAME"       => "Shrine of Order",
                "SHRINE_HEALING_NAME"       => "Shrine of the Woods",
                _ => "Shrine"
            };
            var position = purchaseInteraction.transform.position;
            EspHelper.DrawESPLabel(position, Colors.GetColor(colorName), Color.clear,
                purchaseInteraction.GetDisplayName() + "\n" + GetDistance(position) + "m");
        }

        private static void ShowDrone(PurchaseInteraction purchaseInteraction)
        {
            EspHelper.DrawESPLabel(purchaseInteraction.transform.position, Colors.GetColor("Drone"), Color.clear,
                GetLabel(purchaseInteraction));
        }

        private static void ShowDuplicator(string token, ShopTerminalBehavior shopTerminal)
        {
            var text =
                $"{Language.GetString(token)}\n{Language.GetString(ItemCatalog.GetItemDef(PickupCatalog.GetPickupDef(shopTerminal.pickupIndex)?.itemIndex ?? ItemIndex.None).nameToken)}\n{shopTerminal.itemTier.ToString()}\n{GetDistance(shopTerminal.transform.position)}m";
            EspHelper.DrawESPLabel(shopTerminal.transform.position, Colors.GetColor("Printer"), Color.clear, text);
        }

        private static void ShowChest(TimedChestController optionChestBehavior, PurchaseInteraction purchaseInteraction)
        {
            if (Instance.showAdvancedToggle.isOn || CheckCursorPosition(purchaseInteraction.transform.position))
            {
                var items = optionChestBehavior.remainingTime;
                EspHelper.DrawESPLabel(purchaseInteraction.transform.position, Colors.GetColor("Chest"), Color.clear,
                    GetLabel(purchaseInteraction) + "\n-" + items + "s");
            }
            else
            {
                EspHelper.DrawESPLabel(purchaseInteraction.transform.position, Colors.GetColor("Chest"), Color.clear,
                    "+");
            }
        }

        private static void ShowChest(RouletteChestController optionChestBehavior,
            PurchaseInteraction purchaseInteraction)
        {
            if (Instance.showAdvancedToggle.isOn || CheckCursorPosition(purchaseInteraction.transform.position))
            {
                var items = optionChestBehavior.entries != null
                    ? optionChestBehavior.entries.OrderByDescending(x =>
                    {
                        var def = PickupCatalog.GetPickupDef(x.pickupIndex);
                        return def != null && def.itemIndex != ItemIndex.None
                            ? ItemCatalog.GetItemDef(def.itemIndex).tier
                            : ItemTier.Tier1;
                    }).Take(3).OrderBy(x => x.endTime.timeUntil).Select(x =>
                    {
                        var def = PickupCatalog.GetPickupDef(x.pickupIndex);
                        var tier = def != null && def.itemIndex != ItemIndex.None
                            ? ItemCatalog.GetItemDef(def.itemIndex).tier
                            : ItemTier.Tier1;
                        var itemName = def != null && def.itemIndex != ItemIndex.None
                            ? Language.GetString(ItemCatalog.GetItemDef(def.itemIndex).nameToken)
                            : Language.GetString(EquipmentCatalog
                                .GetEquipmentDef(def?.equipmentIndex ?? EquipmentIndex.None).nameToken);
                        return $"{itemName} : {tier} : {x.endTime.timeUntil:0.##}s";
                    }).ToArray()
                    : new string[0];
                EspHelper.DrawESPLabel(purchaseInteraction.transform.position, Colors.GetColor("Chest"), Color.clear,
                    GetLabel(purchaseInteraction) + "\n-" + string.Join("\n-", items));
            }
            else
            {
                EspHelper.DrawESPLabel(purchaseInteraction.transform.position, Colors.GetColor("Chest"), Color.clear,
                    "+");
            }
        }

        private static void ShowChest(ChestBehavior chest, PurchaseInteraction purchaseInteraction)
        {
            if (Instance.showAdvancedToggle.isOn || CheckCursorPosition(purchaseInteraction.transform.position))
            {
                var def = PickupCatalog.GetPickupDef(chest.dropPickup);
                var item = def != null
                    ? def.itemIndex != ItemIndex.None
                        ?
                        Language.GetString(ItemCatalog.GetItemDef(def.itemIndex).nameToken)
                        : Language.GetString(EquipmentCatalog.GetEquipmentDef(def.equipmentIndex).nameToken)
                    : "";
                if (def.itemIndex != ItemIndex.None) {
                    EspHelper.DrawRarityESPLabel(purchaseInteraction.transform.position, Colors.GetColor("Chest"), Color.clear,
                        GetLabel(purchaseInteraction), GetDropColor(def.itemIndex), Language.GetString(ItemCatalog.GetItemDef(def.itemIndex).nameToken));
                } else {
                    EspHelper.DrawRarityESPLabel(purchaseInteraction.transform.position, Colors.GetColor("Equipment"), Color.clear,
                        GetLabel(purchaseInteraction), GetDropColor(def.itemIndex), Language.GetString(EquipmentCatalog.GetEquipmentDef(def.equipmentIndex).nameToken));
                }
                
            }
            else
            {
                EspHelper.DrawESPLabel(purchaseInteraction.transform.position, Colors.GetColor("Chest"), Color.clear,
                    "+");
            }
        }

        public static void ShowChest(OptionChestBehavior optionChestBehavior, PurchaseInteraction purchaseInteraction)
        {
            if (Instance.showAdvancedToggle.isOn || CheckCursorPosition(purchaseInteraction.transform.position))
            {
                var items = optionChestBehavior.generatedDrops != null
                    ? optionChestBehavior.generatedDrops.Select(x =>
                    {
                        var def = PickupCatalog.GetPickupDef(x);
                        return def.itemIndex != ItemIndex.None
                            ? Language.GetString(ItemCatalog.GetItemDef(def.itemIndex).nameToken)
                            : Language.GetString(EquipmentCatalog.GetEquipmentDef(def.equipmentIndex).nameToken);
                    }).ToArray()
                    : new string[0];
                EspHelper.DrawESPLabel(purchaseInteraction.transform.position, Colors.GetColor("Chest"), Color.clear,
                    GetLabel(purchaseInteraction) + "\n-" + string.Join("\n-", items));
            }
            else
            {
                EspHelper.DrawESPLabel(purchaseInteraction.transform.position, Colors.GetColor("Chest"), Color.clear,
                    "+");
            }
        }

        private static Color GetDropColor(ItemIndex itemIndex)
        {
            if (itemIndex == ItemIndex.None) return ColorCatalog.GetColor(ColorCatalog.ColorIndex.Equipment);

            var color = ColorCatalog.GetColor(ColorCatalog.ColorIndex.Tier1Item);

            var itemDef = ItemCatalog.GetItemDef(itemIndex);

            if (itemDef == null) return color;

            var itemTier = itemDef.tier;
            var itemTierDef = ItemTierCatalog.FindTierDef(itemTier.ToString());

            if (itemTierDef == null) return color;

            color = ColorCatalog.GetColor(itemTierDef.colorIndex);

            return color;
        }

        public static string GetLabel(PurchaseInteraction purchaseInteraction)
        {
            var distance = GetDistance(purchaseInteraction.transform.position);
            var friendlyName = purchaseInteraction.GetDisplayName();
            var cost = purchaseInteraction.cost;

            return $"{friendlyName}\n${cost}\n{distance}m";
        }

        public static float GetDistance(Vector3 position)
        {
            var distanceToObject = Vector3.Distance(Camera.main.transform.position, position);
            var distance = (int) distanceToObject;
            return distance;
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