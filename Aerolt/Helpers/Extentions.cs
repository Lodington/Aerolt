using System.Collections.Generic;
using RoR2;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Networking;

namespace Aerolt.Helpers
{
    public static class Extentions
    {
        public static void SetZ(this Transform tran, float z)
        {
            var vec = tran.position;
            vec.z = z;
            tran.position = vec;
        }
        public static void AddEventTrigger(this EventTrigger eventTrigger, UnityAction<BaseEventData> action,
            EventTriggerType triggerType)
        {
            EventTrigger.TriggerEvent trigger = new EventTrigger.TriggerEvent();
            trigger.AddListener(action);

            EventTrigger.Entry entry = new EventTrigger.Entry { callback = trigger, eventID = triggerType };
            eventTrigger.triggers.Add(entry);
        }

        public static void Write(this NetworkWriter writer, Dictionary<ItemDef, uint> itemCounts)
        {
            writer.WritePackedUInt32((uint) itemCounts.Count);
            foreach (var (key, value) in itemCounts)
            {
                writer.WritePackedUInt32((uint) (int) key.itemIndex);
                writer.WritePackedUInt32(value);
            }
        }
        public static Dictionary<ItemDef, uint> ReadItemAmounts(this NetworkReader reader)
        {
            var itemCounts = new Dictionary<ItemDef, uint>();
            var length = reader.ReadPackedUInt32();
            for (var i = 0; i < length; i++)
            {
                var def = ItemCatalog.GetItemDef((ItemIndex) (int) reader.ReadPackedUInt32());
                itemCounts.Add(def, reader.ReadPackedUInt32());
            }
            return itemCounts;
        }
    }
}
