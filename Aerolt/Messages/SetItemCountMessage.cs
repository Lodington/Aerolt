using System.Collections.Generic;
using System.Linq;
using Aerolt.Helpers;
using Aerolt.Managers;
using RoR2;
using UnityEngine.Networking;

namespace Aerolt.Messages
{
    public class SetItemCountMessage : AeroltMessageBase
    {
        private Inventory inventory;
        private Dictionary<ItemDef, uint> itemCounts;

        public SetItemCountMessage()
        {
        }

        public SetItemCountMessage(Inventory inventory, Dictionary<ItemDef, int> itemCounts)
        {
            this.inventory = inventory;
            this.itemCounts = itemCounts.ToDictionary(x => x.Key, x => (uint)x.Value);
        }

        public override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);
            writer.Write(inventory.netId);
            writer.Write(itemCounts);
        }

        public override void Deserialize(NetworkReader reader)
        {
            base.Deserialize(reader);
            inventory = Util.FindNetworkObject(reader.ReadNetworkId())?.GetComponent<Inventory>();
            itemCounts = reader.ReadItemAmounts();
        }

        public override void Handle()
        {
            base.Handle();
            foreach (var itemCount in itemCounts)
                inventory.GiveItem(itemCount.Key, (int)itemCount.Value - inventory.GetItemCount(itemCount.Key));
        }
    }
}