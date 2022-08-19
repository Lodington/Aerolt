using System.Collections.Generic;
using System.Linq;
using Aerolt.Enums;
using Aerolt.Messages;
using JetBrains.Annotations;
using RoR2;
using RoR2.ContentManagement;
using UnityEngine;
using UnityEngine.Networking;

namespace Aerolt.Managers
{
    public partial class LobbyPlayerPageManager
    {
        [CanBeNull] private static PickupDropTable _chest1DropTable;

        public static PickupDropTable Chest1DropTable =>
            _chest1DropTable ??= LegacyResourcesAPI.Load<PickupDropTable>("DropTables/dtSmallChest");

        public void Kick()
        {
            new KickBanMessage(currentUser, true).SendToServer();
        }

        public void Ban()
        {
            new KickBanMessage(currentUser, ban: true).SendToServer();
        }

        public void Goto()
        {
            if (info.Body && body) new TeleportMessage(info.Body, body.transform.position).SendToAuthority(info.Body);
        }

        public void Bring()
        {
            if (info.Body && body) new TeleportMessage(body, info.Body.transform.position).SendToAuthority(body);
        }

        public void Kill()
        {
            if (!master) return;
            new KillMessage(master)
                .SendToServer(); // Todo, test to see if this needs to be sent on auth(the function calls to inventory add item inside truekill make me feel like no, but its also not working?)
        }

        public void Revive()
        {
            var message = new SetBodyMessage(currentUser);
            if (!NetworkServer.active) message.Handle();
            message.SendToServer();
        }

        public void GiveAllItems()
        {
            if (!master) return;
            var items = ContentManager.itemDefs.ToDictionary(x => x, def => master.inventory.GetItemCount(def) + 1);
            new SetItemCountMessage(master.inventory, items).SendToServer();
        }

        public void GiveRandomItems()
        {
            if (!master) return;
            var items = new Dictionary<ItemDef, int>();
            for (var i = 0; i < Random.Range(0, 100); i++)
            {
                var pickup = Chest1DropTable.GenerateDrop(RoR2Application.rng);
                var item = ItemCatalog.GetItemDef(PickupCatalog.GetPickupDef(pickup)?.itemIndex ?? ItemIndex.None);
                if (item == null) continue;
                if (!items.ContainsKey(item)) items[item] = master.inventory.GetItemCount(item);
                items[item] += Random.Range(0, 100);
            }

            new SetItemCountMessage(master.inventory, items).SendToServer();
        }

        public void ClearInventory()
        {
            var items = ContentManager.itemDefs.ToDictionary(x => x, _ => 0);
            new SetItemCountMessage(master.inventory, items).SendToServer();
        }

        public void OpenInventory()
        {
            SwapViewState(ViewState.Inventory);
        }

        public void OpenBuffs()
        {
            SwapViewState(ViewState.Buff);
        }

        public void OpenEquipment()
        {
            SwapViewState(ViewState.Equipment);
        }

        public void OpenSpawnAs()
        {
            SwapViewState(ViewState.Body);
        }
    }
}