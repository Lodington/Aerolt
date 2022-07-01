using System.Collections.Generic;
using Aerolt.Managers;
using RoR2;
using UnityEngine.Networking;

namespace Aerolt.Messages
{
	public class SetItemCountMessage : AeroltMessageBase
	{
		private Inventory inventory;
		private Dictionary<ItemDef, int> itemCounts;

		public SetItemCountMessage() {}

		public SetItemCountMessage(Inventory inventory, Dictionary<ItemDef, int> itemCounts)
		{
			this.inventory = inventory;
			this.itemCounts = itemCounts;
		}

		public override void Serialize(NetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(inventory.netId);
			writer.Write(itemCounts.Count);
			foreach (var (key, value) in itemCounts)
			{
				writer.WritePackedUInt32((uint) key.itemIndex);
				writer.Write(value);
			}
		}

		public override void Deserialize(NetworkReader reader)
		{
			base.Deserialize(reader);
			inventory = Util.FindNetworkObject(reader.ReadNetworkId())?.GetComponent<Inventory>();
			itemCounts = new Dictionary<ItemDef, int>();
			for (var i = 0; i < reader.ReadInt32(); i++)
			{
				itemCounts.Add(ItemCatalog.GetItemDef((ItemIndex) reader.ReadPackedUInt32()), reader.ReadInt32());
			}
		}

		public override void Handle()
		{
			base.Handle();
			foreach (var itemCount in itemCounts)
			{
				inventory.GiveItem(itemCount.Key, itemCount.Value - inventory.GetItemCount(itemCount.Key));
			}
		}
	}
}