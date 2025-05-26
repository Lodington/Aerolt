
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aerolt_External;
using Aerolt_External.Helpers;
using Newtonsoft.Json;
using RoR2;
using RoR2.UI;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;



    public class Esp : MonoBehaviour
    {
        public static List<PurchaseInteraction> PurchaseInteractions;
        public static List<BarrelInteraction> BarrelInteractions;
        public static List<PressurePlateController> SecretButtons;
        public static List<ScrapperController> Scrappers;
        public static List<MultiShopController> MultiShops;
        
        public static Esp Instance;

        public bool showNewtAlterToggle;
        public bool showAdvancedToggle;
        public bool showTeleporterToggle;
        public bool showChestToggle;
        public bool showMultiShopToggle;
        public bool showBarrelToggle;
        public bool showScrapperToggle;
        public bool ShowSecretToggle;
        public bool showDuplicatorToggle;
        public bool showDroneToggle;
        public bool showShrineToggle;

        public class EspToggleMessage
        {
            public bool showNewtAlterToggle {get; set;}
            public bool showAdvancedToggle {get; set;}
            public bool showTeleporterToggle{get; set;}
            public bool showChestToggle{get; set;}
            public bool showMultiShopToggle{get; set;}
            public bool showBarrelToggle{get; set;}
            public bool showScrapperToggle{get; set;}
            public bool ShowSecretToggle{get; set;}
            public bool showDuplicatorToggle{get; set;}
            public bool showDroneToggle {get; set;}
            public bool showShrineToggle {get; set;}
        }

        public static void ToggleEspOptions(string payload)
        {
            var toggleMessage = JsonConvert.DeserializeObject<EspToggleMessage>(payload);
            var messageProps = typeof(EspToggleMessage).GetProperties();
            var espInstance = Instance;

            foreach (var prop in messageProps)
            {
                var value = (bool)prop.GetValue(toggleMessage);
                
                var field = typeof(Esp).GetField(prop.Name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                if (field != null && field.FieldType == typeof(bool))
                {
                    field.SetValue(espInstance, value);
                }
            }
        }

        private void Awake()
        {
            if (Instance) return;
            Instance = this;
            
            AeroltPlugin.Log.LogInfo("instance" + Instance);

            GatherObjects();
        }

        public static void Draw()
        {
                ShowTeleporter();
                AeroltPlugin.Log.LogInfo("Show");
            if (Instance.showChestToggle || Instance.showDuplicatorToggle || Instance.showDroneToggle ||
                Instance.showShrineToggle || Instance.showNewtAlterToggle)
                DrawPurchaseInteractables();
            if (Instance.showMultiShopToggle)
                DrawShops();
            if (Instance.showBarrelToggle)
                DrawBarrelInteractables();
            if (Instance.showScrapperToggle)
                DrawScrapperInteractables();
            if (Instance.ShowSecretToggle)
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
                var advanced = CheckCursorPosition(position) || Instance.showAdvancedToggle; //todo add check for advanced esp
                if (advanced)
                    str.AppendLine("Multi Shop Terminal"); // TODO use lang token
                var costSet = false;
                if (multiShopController.terminalGameObjects.src == null) continue; // TODO properly fix this

                var itemColors = new List<Color>();
                var itemNames = new List<string>();

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

        public static void GatherObjects()
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
                    float distance = (int)Vector3.Distance(Camera.main.transform.position, barrel.transform.position);
                    var boxText = $"{friendlyName}\n{distance}m";

                    //todo check if draw barrel is on
                    if (true  || CheckCursorPosition(barrel.transform.position))
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
                        (int)Vector3.Distance(Camera.main.transform.position, secretButton.transform.position);
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
                    float distance = (int)Vector3.Distance(Camera.main.transform.position, position);
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
                if (chest && Instance.showChestToggle)
                    ShowChest(chest, purchaseInteraction);

                var optionChest = purchaseInteraction.GetComponent<OptionChestBehavior>();
                if (optionChest && Instance.showChestToggle)
                    ShowChest(optionChest, purchaseInteraction);

                var casinoChest = purchaseInteraction.GetComponent<RouletteChestController>();
                if (casinoChest && Instance.showChestToggle)
                    ShowChest(casinoChest, purchaseInteraction);

                var timedChest =
                    purchaseInteraction.GetComponent<TimedChestController>(); // TODO this is not a purchase interaction
                if (timedChest && Instance.showChestToggle)
                    ShowChest(timedChest, purchaseInteraction);

                var shopTerminal = purchaseInteraction.GetComponent<ShopTerminalBehavior>();
                if (shopTerminal)
                    if (Instance.showDuplicatorToggle)
                    {
                        var possibleNames = new[]
                            { "DUPLICATOR_NAME", "DUPLICATOR_MILITARY_NAME", "BAZAAR_CAULDRON_NAME" };
                        foreach (var name in possibleNames)
                            if (purchaseInteraction.displayNameToken == name)
                            {
                                ShowDuplicator(name, shopTerminal);
                                break;
                            }
                    }

                var masterSummon = purchaseInteraction.GetComponent<SummonMasterBehavior>();
                if (masterSummon && Instance.showDroneToggle) ShowDrone(purchaseInteraction);

                var nameProvider = purchaseInteraction.GetComponent<GenericDisplayNameProvider>();
                if (nameProvider)
                    if (Instance.showShrineToggle && nameProvider.displayToken.ToUpper().Contains("SHRINE"))
                        ShowShrine(purchaseInteraction);

                var portalBehaviour = purchaseInteraction.GetComponent<PortalStatueBehavior>();
                if (portalBehaviour && Instance.showNewtAlterToggle) ShowNewtAlter(purchaseInteraction);
            }
        }

        private static void ShowNewtAlter(PurchaseInteraction purchaseInteraction)
        {
            EspHelper.DrawEspLabel(purchaseInteraction.transform.position, Colors.GetColor("NewtAlter"), Color.clear,
                GetLabel(purchaseInteraction));
        }

        private static void ShowShrine(PurchaseInteraction purchaseInteraction)
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

        private static void ShowDrone(PurchaseInteraction purchaseInteraction)
        {
            EspHelper.DrawEspLabel(purchaseInteraction.transform.position, Colors.GetColor("Drone"), Color.clear,
                GetLabel(purchaseInteraction));
        }

        private static void ShowDuplicator(string token, ShopTerminalBehavior shopTerminal)
        {
            var def = PickupCatalog.GetPickupDef(shopTerminal.pickupIndex);
            if (def.itemIndex != ItemIndex.None)
                EspHelper.DrawRarityEspLabel(shopTerminal.transform.position, Colors.GetColor("Printer"), Color.clear,
                    GetLabelDuplicator(Language.GetString(token), shopTerminal), GetDropColor(def.itemIndex),
                    Language.GetString(ItemCatalog
                        .GetItemDef(PickupCatalog.GetPickupDef(shopTerminal.pickupIndex).itemIndex).nameToken));
        }

        private static void ShowChest(TimedChestController optionChestBehavior, PurchaseInteraction purchaseInteraction)
        {
            if (Instance.showAdvancedToggle || CheckCursorPosition(purchaseInteraction.transform.position))
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

        private static void ShowChest(RouletteChestController optionChestBehavior,
            PurchaseInteraction purchaseInteraction)
        {
            if (Instance.showAdvancedToggle || CheckCursorPosition(purchaseInteraction.transform.position))
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
                EspHelper.DrawEspLabel(purchaseInteraction.transform.position, Colors.GetColor("Chest"), Color.clear,
                    GetLabel(purchaseInteraction) + "\n-" + string.Join("\n-", items));
            }
            else
            {
                EspHelper.DrawEspLabel(purchaseInteraction.transform.position, Colors.GetColor("Chest"), Color.clear,
                    "+");
            }
        }

        private static void ShowChest(ChestBehavior chest, PurchaseInteraction purchaseInteraction)
        {
            if (Instance.showAdvancedToggle || CheckCursorPosition(purchaseInteraction.transform.position))
            {
                var def = PickupCatalog.GetPickupDef(chest.dropPickup);
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

        public static void ShowChest(OptionChestBehavior optionChestBehavior, PurchaseInteraction purchaseInteraction)
        {
            if (Instance.showAdvancedToggle || CheckCursorPosition(purchaseInteraction.transform.position))
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
            var distanceToObject = Vector3.Distance(Camera.main.transform.position, position);
            var distance = (int)distanceToObject;
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
