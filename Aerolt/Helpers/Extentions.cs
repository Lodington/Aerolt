using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

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
    }
}
