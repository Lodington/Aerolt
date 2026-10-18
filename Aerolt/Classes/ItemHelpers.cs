using System.Collections.Generic;
using JetBrains.Annotations;
using RoR2;

namespace Aerolt.Classes
{

    public static class ItemHelpers
    {
        private static List<ItemDef>? _giveAllFilteredItems;

        public static List<ItemDef> GiveAllFilteredItems => _giveAllFilteredItems ??= new List<ItemDef>
        {
            DLC1Content.Items.DroneWeaponsBoost, 
            RoR2Content.Items.HealthDecay,
            RoR2Content.Items.LunarPrimaryReplacement,
            RoR2Content.Items.LunarSecondaryReplacement,
            RoR2Content.Items.LunarUtilityReplacement,
            RoR2Content.Items.LunarSpecialReplacement
        };

        [CanBeNull] private static PickupDropTable? _chest1DropTable;
        public static PickupDropTable Chest1DropTable =>
            _chest1DropTable ??= LegacyResourcesAPI.Load<PickupDropTable>("DropTables/dtSmallChest");
    }
}
