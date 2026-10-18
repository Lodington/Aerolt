using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aerolt.Helpers;
using Aerolt.Managers;
using RoR2;
using UnityEngine;
using ZioConfigFile;

namespace Aerolt.Classes
{
    public static class Esp
    {
        public static List<PurchaseInteraction> PurchaseInteractions = new();
        public static List<BarrelInteraction> BarrelInteractions = new();
        public static List<PressurePlateController> SecretButtons = new();
        public static List<ScrapperController> Scrappers = new();
        public static List<MultiShopController> MultiShops = new();

        public static Camera MainCamera = null!;

        public static ZioConfigEntry<bool> ShowAdvanced = null!;
        public static ZioConfigEntry<bool> ShowTeleporter = null!;
        public static ZioConfigEntry<bool> ShowChest = null!;
        public static ZioConfigEntry<bool> ShowMultiShop = null!;
        public static ZioConfigEntry<bool> ShowBarrel = null!;
        public static ZioConfigEntry<bool> ShowScrapper = null!;
        public static ZioConfigEntry<bool> ShowSecret = null!;
        public static ZioConfigEntry<bool> ShowDuplicator = null!;
        public static ZioConfigEntry<bool> ShowDrone = null!;
        public static ZioConfigEntry<bool> ShowShrine = null!;
        public static ZioConfigEntry<bool> ShowNewtAlter = null!;

        private static bool _initialized;

        public static bool Enabled => _initialized;

        public static void Init()
        {
            if (_initialized) return;
            var cfg = Load.ConfigFile;

            ShowTeleporter = cfg.Bind("ESP", "showTeleporter", false, "");
            ShowChest = cfg.Bind("ESP", "showChest", false, "");
            ShowMultiShop = cfg.Bind("ESP", "showShop", false, "");
            ShowAdvanced = cfg.Bind("ESP", "showAdvanced", false, "");
            ShowBarrel = cfg.Bind("ESP", "showBarrel", false, "");
            ShowScrapper = cfg.Bind("ESP", "showScrapper", false, "");
            ShowSecret = cfg.Bind("ESP", "showSecret", false, "");
            ShowDuplicator = cfg.Bind("ESP", "showDuplicator", false, "");
            ShowDrone = cfg.Bind("ESP", "showDrone", false, "");
            ShowShrine = cfg.Bind("ESP", "showShrine", false, "");
            ShowNewtAlter = cfg.Bind("ESP", "showNewtAlter", false, "");

            _initialized = true;

            GatherObjects();
            Stage.onStageStartGlobal += _ => GatherObjects();
        }

        public static bool AnyEnabled =>
            _initialized && (ShowTeleporter.Value || ShowChest.Value || ShowMultiShop.Value || ShowBarrel.Value ||
                             ShowScrapper.Value || ShowSecret.Value || ShowDuplicator.Value || ShowDrone.Value ||
                             ShowShrine.Value || ShowNewtAlter.Value);

        public static void Draw()
        {
            if (!_initialized) return;
            MainCamera = Camera.main;
            if (!MainCamera) return; // no camera to project against this frame

            if (ShowTeleporter.Value)
                ShowTeleporterEsp();
            if (ShowChest.Value || ShowDuplicator.Value || ShowDrone.Value || ShowShrine.Value || ShowNewtAlter.Value)
                DrawPurchaseInteractables();
            if (ShowMultiShop.Value)
                DrawShops();
            if (ShowBarrel.Value)
                DrawBarrelInteractables();
            if (ShowScrapper.Value)
                DrawScrapperInteractables();
            if (ShowSecret.Value)
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
                var distanceToObject = Mathf.RoundToInt(Vector3.Distance(MainCamera.transform.position, position));
                var advanced = CheckCursorPosition(position) || ShowAdvanced.Value;
                if (advanced)
                    str.AppendLine("Multi Shop Terminal"); // TODO use lang token
                var costSet = false;
                if (multiShopController.terminalGameObjects == null) continue;

                var itemColors = new List<Color>();
                var itemNames = new List<string>();

                foreach (var o in multiShopController.terminalGameObjects)
                {
                    var shop = o.GetComponent<ShopTerminalBehavior>();
                    var pickupDef = PickupCatalog.GetPickupDef(shop.pickup.pickupIndex);
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
                    {
                        itemNames.Add(Language.GetString(ItemCatalog.GetItemDef(pickupDef.itemIndex).nameToken));
                        itemColors.Add(GetDropColor(pickupDef.itemIndex));
                        // str.AppendLine("- " +
                        //                Language.GetString(ItemCatalog.GetItemDef(pickupDef.itemIndex).nameToken));
                        // EspHelper.DrawRarityESPLabel(purchaseInteraction.transform.position, Colors.GetColor("Chest"), Color.clear,
                        //     GetLabel(purchaseInteraction), GetDropColor(def.itemIndex), Language.GetString(ItemCatalog.GetItemDef(def.itemIndex).nameToken));
                    }
                    else
                    {
                        itemNames.Add(Language.GetString(EquipmentCatalog.GetEquipmentDef(pickupDef.equipmentIndex)
                            .nameToken));
                        itemColors.Add(GetDropColor(pickupDef.itemIndex));
                        // EspHelper.DrawRarityESPLabel(purchaseInteraction.transform.position, Colors.GetColor("Equipment"), Color.clear,
                        //     GetLabel(purchaseInteraction), GetDropColor(def.itemIndex), Language.GetString(EquipmentCatalog.GetEquipmentDef(def.equipmentIndex).nameToken));
                    }
                }

                if (!costSet) continue;
                EspHelper.DrawMultiShopRarityESPLabel(position, Colors.GetColor("Shop"), Color.clear, str.ToString(),
                    itemColors, itemNames);
            }
        }

        private static void GatherObjects()
        {
            BarrelInteractions = Object.FindObjectsOfType<BarrelInteraction>().ToList();
            PurchaseInteractions = Object.FindObjectsOfType<PurchaseInteraction>().ToList();
            SecretButtons = Object.FindObjectsOfType<PressurePlateController>().ToList();
            Scrappers = Object.FindObjectsOfType<ScrapperController>().ToList();
            MultiShops = Object.FindObjectsOfType<MultiShopController>().ToList();
        }

        private static void ShowTeleporterEsp()
        {
            if (TeleporterInteraction.instance)
            {
                var teleporterInteraction = TeleporterInteraction.instance;
                var distanceToObject = Vector3.Distance(MainCamera.transform.position,
                    teleporterInteraction.transform.position);
                var distance = (int)distanceToObject;
                var friendlyName = "Teleporter";

                Color teleporterColor;

                switch (TeleporterInteraction.instance.currentState)
                {
                    case TeleporterInteraction.IdleState:
                        teleporterColor = Colors.GetColor("Teleporter Idle");
                        break;
                    case TeleporterInteraction.ChargingState:
                    case TeleporterInteraction.IdleToChargingState:
                        teleporterColor = Colors.GetColor("Teleporter Charging");
                        break;
                    case TeleporterInteraction.ChargedState:
                        teleporterColor = Colors.GetColor("Teleporter Charged");
                        break;
                    default:
                        teleporterColor = Colors.GetColor("Teleporter Finished");
                        ;
                        break;
                }

                var status = TeleporterInteraction.instance.activationState.ToString();

                var boxText = $"{friendlyName}\n{status}\n{distance}m";
                EspHelper.DrawEspLabel(teleporterInteraction.transform.position, teleporterColor, Color.clear, boxText);
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
                    float distance = (int)Vector3.Distance(MainCamera.transform.position, barrel.transform.position);
                    var boxText = $"{friendlyName}\n{distance}m";

                    if (ShowBarrel.Value || CheckCursorPosition(barrel.transform.position))
                        EspHelper.DrawEspLabel(barrel.transform.position, Colors.GetColor("Barrels"), Color.clear,
                            boxText);
                    else
                        EspHelper.DrawEspLabel(barrel.transform.position, Colors.GetColor("Barrels"), Color.clear, "+");
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
                        (int)Vector3.Distance(MainCamera.transform.position, secretButton.transform.position);
                    var boxText = $"{friendlyName}\n{distance}m";
                    EspHelper.DrawEspLabel(secretButton.transform.position, Colors.GetColor("Secret_Plates"),
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
                    float distance = (int)Vector3.Distance(MainCamera.transform.position, position);
                    var boxText = $"{friendlyName}\n{distance}m";
                    EspHelper.DrawEspLabel(position, Colors.GetColor("Scrappers"), Color.clear, boxText);
                }
        }

        public static void DrawPurchaseInteractables()
        {
            foreach (var purchaseInteraction in PurchaseInteractions)
            {
                if (!purchaseInteraction || !purchaseInteraction.available) continue;

                var chest = purchaseInteraction.GetComponent<ChestBehavior>();
                if (chest && ShowChest.Value)
                    DrawChest(chest, purchaseInteraction);

                var optionChest = purchaseInteraction.GetComponent<OptionChestBehavior>();
                if (optionChest && ShowChest.Value)
                    DrawChest(optionChest, purchaseInteraction);

                var casinoChest = purchaseInteraction.GetComponent<RouletteChestController>();
                if (casinoChest && ShowChest.Value)
                    DrawChest(casinoChest, purchaseInteraction);

                var timedChest =
                    purchaseInteraction.GetComponent<TimedChestController>(); // TODO this is not a purchase interaction
                if (timedChest && ShowChest.Value)
                    DrawChest(timedChest, purchaseInteraction);

                var shopTerminal = purchaseInteraction.GetComponent<ShopTerminalBehavior>();
                if (shopTerminal)
                    if (ShowDuplicator.Value)
                    {
                        var possibleNames = new[]
                            { "DUPLICATOR_NAME", "DUPLICATOR_MILITARY_NAME", "BAZAAR_CAULDRON_NAME" };
                        foreach (var name in possibleNames)
                            if (purchaseInteraction.displayNameToken == name)
                            {
                                DrawDuplicator(name, shopTerminal);
                                break;
                            }
                    }

                var masterSummon = purchaseInteraction.GetComponent<SummonMasterBehavior>();
                if (masterSummon && ShowDrone.Value) DrawDrone(purchaseInteraction);

                var nameProvider = purchaseInteraction.GetComponent<GenericDisplayNameProvider>();
                if (nameProvider)
                    if (ShowShrine.Value && nameProvider.displayToken.ToUpper().Contains("SHRINE"))
                        DrawShrine(purchaseInteraction);

                var portalBehaviour = purchaseInteraction.GetComponent<PortalStatueBehavior>();
                if (portalBehaviour && ShowNewtAlter.Value) DrawNewtAlter(purchaseInteraction);
            }
        }

        private static void DrawNewtAlter(PurchaseInteraction purchaseInteraction)
        {
            EspHelper.DrawEspLabel(purchaseInteraction.transform.position, Colors.GetColor("NewtAlter"), Color.clear,
                GetLabel(purchaseInteraction));
        }

        private static void DrawShrine(PurchaseInteraction purchaseInteraction)
        {
            var colorName = purchaseInteraction.displayNameToken switch
            {
                "SHRINE_BLOOD_NAME" => "Shrine of Blood",
                "SHRINE_CHANCE_NAME" => "Shrine of Chance",
                "SHRINE_CLEANSE_NAME" => "Cleansing Pool",
                "SHRINE_COMBAT_NAME" => "Shrine of Combat",
                "SHRINE_GOLDSHORES_NAME" => "Altar of Gold",
                "SHRINE_BOSS_NAME" => "Shrine of the Mountain",
                "SHRINE_RESTACK_NAME" => "Shrine of Order",
                "SHRINE_HEALING_NAME" => "Shrine of the Woods",
                _ => "Shrine"
            };
            var position = purchaseInteraction.transform.position;
            EspHelper.DrawEspLabel(position, Colors.GetColor(colorName), Color.clear,
                purchaseInteraction.GetDisplayName() + "\n" + GetDistance(position) + "m");
        }

        private static void DrawDrone(PurchaseInteraction purchaseInteraction)
        {
            EspHelper.DrawEspLabel(purchaseInteraction.transform.position, Colors.GetColor("Drone"), Color.clear,
                GetLabel(purchaseInteraction));
        }

        private static void DrawDuplicator(string token, ShopTerminalBehavior shopTerminal)
        {
            var def = PickupCatalog.GetPickupDef(shopTerminal.pickup.pickupIndex);
            if (def.itemIndex != ItemIndex.None)
                EspHelper.DrawRarityEspLabel(shopTerminal.transform.position, Colors.GetColor("Printer"), Color.clear,
                    GetLabelDuplicator(Language.GetString(token), shopTerminal), GetDropColor(def.itemIndex),
                    Language.GetString(ItemCatalog
                        .GetItemDef(PickupCatalog.GetPickupDef(shopTerminal.pickup.pickupIndex).itemIndex).nameToken));
        }

        private static void DrawChest(TimedChestController optionChestBehavior, PurchaseInteraction purchaseInteraction)
        {
            if (ShowAdvanced.Value || CheckCursorPosition(purchaseInteraction.transform.position))
            {
                var items = optionChestBehavior.remainingTime;
                EspHelper.DrawEspLabel(purchaseInteraction.transform.position, Colors.GetColor("Chest"), Color.clear,
                    GetLabel(purchaseInteraction) + "\n-" + items + "s");
            }
            else
            {
                EspHelper.DrawEspLabel(purchaseInteraction.transform.position, Colors.GetColor("Chest"), Color.clear,
                    "+");
            }
        }

        private static void DrawChest(RouletteChestController optionChestBehavior,
            PurchaseInteraction purchaseInteraction)
        {
            if (ShowAdvanced.Value || CheckCursorPosition(purchaseInteraction.transform.position))
            {
                var items = optionChestBehavior.entries != null
                    ? optionChestBehavior.entries.OrderByDescending(x =>
                    {
                        var def = PickupCatalog.GetPickupDef(x.pickup.pickupIndex);
                        return def != null && def.itemIndex != ItemIndex.None
                            ? ItemCatalog.GetItemDef(def.itemIndex).tier
                            : ItemTier.Tier1;
                    }).Take(3).OrderBy(x => x.endTime.timeUntil).Select(x =>
                    {
                        var def = PickupCatalog.GetPickupDef(x.pickup.pickupIndex);
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
                EspHelper.DrawEspLabel(purchaseInteraction.transform.position, Colors.GetColor("Chest"), Color.clear,
                    GetLabel(purchaseInteraction) + "\n-" + string.Join("\n-", items));
            }
            else
            {
                EspHelper.DrawEspLabel(purchaseInteraction.transform.position, Colors.GetColor("Chest"), Color.clear,
                    "+");
            }
        }

        private static void DrawChest(ChestBehavior chest, PurchaseInteraction purchaseInteraction)
        {
            if (ShowAdvanced.Value || CheckCursorPosition(purchaseInteraction.transform.position))
            {
                var def = PickupCatalog.GetPickupDef(chest.currentPickup.pickupIndex);
                if (def.itemIndex != ItemIndex.None)
                    EspHelper.DrawRarityEspLabel(purchaseInteraction.transform.position, Colors.GetColor("Chest"),
                        Color.clear,
                        GetLabel(purchaseInteraction), GetDropColor(def.itemIndex),
                        Language.GetString(ItemCatalog.GetItemDef(def.itemIndex).nameToken));
                else
                    EspHelper.DrawRarityEspLabel(purchaseInteraction.transform.position, Colors.GetColor("Equipment"),
                        Color.clear,
                        GetLabel(purchaseInteraction), GetDropColor(def.itemIndex),
                        Language.GetString(EquipmentCatalog.GetEquipmentDef(def.equipmentIndex).nameToken));
            }
            else
            {
                EspHelper.DrawEspLabel(purchaseInteraction.transform.position, Colors.GetColor("Chest"), Color.clear,
                    "+");
            }
        }

        private static void DrawChest(OptionChestBehavior optionChestBehavior, PurchaseInteraction purchaseInteraction)
        {
            if (ShowAdvanced.Value || CheckCursorPosition(purchaseInteraction.transform.position))
            {
                var items = optionChestBehavior.generatedPickups != null
                    ? optionChestBehavior.generatedPickups.Select(x =>
                    {
                        var def = PickupCatalog.GetPickupDef(x.pickupIndex);
                        return def.itemIndex != ItemIndex.None
                            ? Language.GetString(ItemCatalog.GetItemDef(def.itemIndex).nameToken)
                            : Language.GetString(EquipmentCatalog.GetEquipmentDef(def.equipmentIndex).nameToken);
                    }).ToArray()
                    : new string[0];
                EspHelper.DrawEspLabel(purchaseInteraction.transform.position, Colors.GetColor("Chest"), Color.clear,
                    GetLabel(purchaseInteraction) + "\n-" + string.Join("\n-", items));
            }
            else
            {
                EspHelper.DrawEspLabel(purchaseInteraction.transform.position, Colors.GetColor("Chest"), Color.clear,
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
            var itemTierDef = ItemTierCatalog.GetItemTierDef(itemTier);

            if (itemTierDef == null) return color;

            color = ColorCatalog.GetColor(itemTierDef.colorIndex);

            return color;
        }

        public static string? GetLabel(PurchaseInteraction purchaseInteraction)
        {
            var distance = GetDistance(purchaseInteraction.transform.position);
            var friendlyName = purchaseInteraction.GetDisplayName();
            var cost = purchaseInteraction.cost;

            return $"{friendlyName}\n${cost}\n{distance}m";
        }

        public static string? GetLabelDuplicator(string token, ShopTerminalBehavior shopTerminal)
        {
            var distance = GetDistance(shopTerminal.transform.position);
            return $"{token}\n{distance}m";
        }

        public static float GetDistance(Vector3 position)
        {
            var distanceToObject = Vector3.Distance(MainCamera.transform.position, position);
            var distance = (int)distanceToObject;
            return distance;
        }

        public static bool CheckCursorPosition(Vector3 worldPos)
        {
            var mpEventSystem = LocalUserManager.GetFirstLocalUser().eventSystem;
            mpEventSystem.GetCursorPosition(out var cursorPos);
            cursorPos.y = MainCamera.pixelRect.height - cursorPos.y;
            if (!mpEventSystem.isCursorVisible)
                cursorPos = MainCamera.pixelRect.center;
            return Vector3.Distance(EspHelper.WorldToScreen(worldPos), cursorPos) < 50;
        }
    }
}
