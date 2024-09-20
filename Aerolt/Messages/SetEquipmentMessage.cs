using System.Collections.Generic;
using Aerolt.Managers;
using RoR2;
using UnityEngine.Networking;

namespace Aerolt.Messages
{
    public class SetEquipmentMessage : AeroltMessageBase
    {
        private Dictionary<EquipmentDef, int> equipmentCounts;
        private Inventory inventory;

        public SetEquipmentMessage()
        {
        }

        public SetEquipmentMessage(Inventory inventory, Dictionary<EquipmentDef, int> equipmentCounts)
        {
            this.inventory = inventory;
            this.equipmentCounts = equipmentCounts;
        }

        public override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);
            writer.Write(inventory.netId);
            writer.WritePackedUInt32((uint)equipmentCounts.Count);
            foreach (var (key, value) in equipmentCounts)
            {
                writer.WritePackedUInt32((uint)(int)key.equipmentIndex);
                writer.WritePackedUInt32((uint)value);
            }
        }

        public override void Deserialize(NetworkReader reader)
        {
            base.Deserialize(reader);
            inventory = Util.FindNetworkObject(reader.ReadNetworkId())?.GetComponent<Inventory>();
            equipmentCounts = new Dictionary<EquipmentDef, int>();
            var length = reader.ReadPackedUInt32();
            for (var i = 0; i < length; i++)
            {
                var def = EquipmentCatalog.GetEquipmentDef((EquipmentIndex)(int)reader.ReadPackedUInt32());
                equipmentCounts.Add(def, (int)reader.ReadPackedUInt32());
            }
        }

        public override void Handle()
        {
            base.Handle();
            foreach (var equipment in equipmentCounts)
                inventory.SetEquipmentIndex(equipment.Key ? equipment.Key.equipmentIndex : EquipmentIndex.None);
        }
    }
}