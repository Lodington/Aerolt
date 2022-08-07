using Aerolt.Classes;
using UnityEngine;
using UnityEngine.EventSystems;
using ZioConfigFile;

namespace Aerolt.Helpers
{
    [RequireComponent(typeof(EventTrigger))]
    public class MovePanel : MonoBehaviour
    {
        private EventTrigger _eventTrigger;
        private ZioConfigEntry<Vector2> configEntry;
        private Transform Target;

        private void Awake()
        {
            Target = transform;

            var menuInfo = Target.transform.parent ? GetComponentInParent<MenuInfo>() : GetComponent<MenuInfo>();
            var configFile = menuInfo ? menuInfo.ConfigFile : Load.configFile;
            configEntry = configFile.Bind("Window Positions", Target.name, (Vector2) Target.transform.localPosition,
                "Stored position of this window.");
            if (configEntry.Value.x < 0 - Screen.width / 2  || configEntry.Value.x > Screen.width - Screen.width / 2  || configEntry.Value.y > Screen.height - Screen.height / 2 || configEntry.Value.y < 0 - Screen.height / 2)
                Target.transform.localPosition = new Vector3(0,0,0);
            else
            {
                Target.transform.localPosition = new Vector3(configEntry.Value.x, configEntry.Value.y, 0);
            }
            
        }

        private void Start()
        {
            _eventTrigger = GetComponent<EventTrigger>();
            _eventTrigger.AddEventTrigger(OnDrag, EventTriggerType.Drag);
            _eventTrigger.AddEventTrigger(OnDragEnd, EventTriggerType.EndDrag);
        }


        private void OnDragEnd(BaseEventData data)
        {
            if (!Target.transform) return;
            var targetTransform = (RectTransform) Target.transform;
            var localPosition = targetTransform.localPosition;
            configEntry.Value = new Vector2(localPosition.x, localPosition.y);
        }

        private void OnDrag(BaseEventData data)
        {
            var ped = (PointerEventData) data;
            Target.transform.Translate(ped.delta);
        }
    }
}