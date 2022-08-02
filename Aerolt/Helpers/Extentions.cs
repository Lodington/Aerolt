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
        public static Vector2 GetSize(this RectTransform source) => source.rect.size;
        public static float GetWidth(this RectTransform source) => source.rect.size.x;
        public static float GetHeight(this RectTransform source) => source.rect.size.y;
        public static void SetSize(this RectTransform source, RectTransform toCopy) {
            source.SetSize(toCopy.GetSize());
        }
 
        /// <summary>
        /// Sets the sources RT size to the same as the newSize.
        /// </summary>
        public static void SetSize(this RectTransform source, Vector2 newSize) {
            source.SetSize(newSize.x, newSize.y);
        }
 
        /// <summary>
        /// Sets the sources RT size to the new width and height.
        /// </summary>
        public static void SetSize(this RectTransform source, float width, float height) {
            source.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            source.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }
 
        public static void SetWidth(this RectTransform source, float width) {
            source.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        }
 
        public static void SetHeight(this RectTransform source, float height) {
            source.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }
    }
}
