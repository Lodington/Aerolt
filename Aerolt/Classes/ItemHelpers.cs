using System.Collections.Generic;
using JetBrains.Annotations;
using RoR2;

namespace Aerolt.Classes
{
    /// <summary>
    ///     Shared item-related data used by the UI. Extracted from the old LobbyPlayerPageManager so
    ///     the (deleted) old UI isn't a dependency.
    /// </summary>
    public static class ItemHelpers
    {
        private static List<ItemDef>? _giveAllFilteredItems;

        /// <summary>Items excluded from "give all" because they misbehave when force-granted.</summary>
        public static List<ItemDef> GiveAllFilteredItems => _giveAllFilteredItems ??= new List<ItemDef>
        {
            DLC1Content.Items.DroneWeaponsBoost, // NRE in bazaar when force-granted
            RoR2Content.Items.HealthDecay,
            RoR2Content.Items.LunarPrimaryReplacement,
            RoR2Content.Items.LunarSecondaryReplacement,
            RoR2Content.Items.LunarUtilityReplacement,
            RoR2Content.Items.LunarSpecialReplacement
        };

        [CanBeNull] private static PickupDropTable? _chest1DropTable;

        /// <summary>Small-chest drop table, used by "give random items".</summary>
        public static PickupDropTable Chest1DropTable =>
            _chest1DropTable ??= LegacyResourcesAPI.Load<PickupDropTable>("DropTables/dtSmallChest");
    }
}
