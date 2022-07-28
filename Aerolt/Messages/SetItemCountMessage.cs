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
			writer.WritePackedUInt32((uint) itemCounts.Count);
			foreach (var (key, value) in itemCounts)
			{
				writer.WritePackedUInt32((uint) (int) key.itemIndex);
				writer.WritePackedUInt32((uint) value);
			}
		}

		public override void Deserialize(NetworkReader reader)
		{
			base.Deserialize(reader);
			inventory = Util.FindNetworkObject(reader.ReadNetworkId())?.GetComponent<Inventory>();
			itemCounts = new Dictionary<ItemDef, int>();
			var length = reader.ReadPackedUInt32();
			for (var i = 0; i < length; i++)
			{
				var def = ItemCatalog.GetItemDef((ItemIndex) (int) reader.ReadPackedUInt32());
				itemCounts.Add(def, (int) reader.ReadPackedUInt32());
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