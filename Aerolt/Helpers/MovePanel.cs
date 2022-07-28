using System;
using Aerolt.Classes;
using UnityEngine;
using UnityEngine.EventSystems;
using ZioConfigFile;

namespace Aerolt.Helpers
{
    [RequireComponent(typeof(EventTrigger))]
    public class MovePanel : MonoBehaviour
    {
        private GameObject Target;
        private EventTrigger _eventTrigger;
        private ZioConfigEntry<Vector2> configEntry;

        void Awake()
        {
            Target.transform.parent = transform.parent;

            var menuInfo = Target.transform.parent ? GetComponentInParent<MenuInfo>() : GetComponent<MenuInfo>();
            var configFile = menuInfo ? menuInfo.ConfigFile : Load.Instance.configFile;
            configEntry = configFile.Bind("Window Positions", Target.name, (Vector2) Target.transform.localPosition, "Stored position of this window.");
            Target.transform.localPosition = new Vector3(configEntry.Value.x, configEntry.Value.y, 0); 
        }

        void Start()
        {
            _eventTrigger = GetComponent<EventTrigger>();
            _eventTrigger.AddEventTrigger(OnDrag, EventTriggerType.Drag);
            _eventTrigger.AddEventTrigger(OnDragEnd, EventTriggerType.EndDrag);
        }


        void OnDragEnd(BaseEventData data)
        {
            if (!Target.transform) return;
            var targetTransform = (RectTransform) Target.transform;
            var localPosition = targetTransform.localPosition;
            var parent = (RectTransform) targetTransform.parent;
            var width = parent.sizeDelta.x * 0.5f - 10f + targetTransform.sizeDelta.x * 0.5f;
            var height = parent.sizeDelta.y * 0.5f - 10f + targetTransform.sizeDelta.y * 0.5f;
            configEntry.Value = new Vector2(Mathf.Clamp(localPosition.x, -width, width), Mathf.Clamp(localPosition.y, -height, height));
        }
        
        void OnDrag(BaseEventData data)
        {
            PointerEventData ped = (PointerEventData) data;
            Target.transform.Translate(ped.delta);
        }
    }
}