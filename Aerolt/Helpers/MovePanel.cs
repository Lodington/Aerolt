using Aerolt.Classes;
using UnityEngine;
using UnityEngine.EventSystems;
using ZioConfigFile;

namespace Aerolt.Helpers
{
    [RequireComponent(typeof(EventTrigger))]
    public class MovePanel : MonoBehaviour
    {
        public GameObject Target;
        private EventTrigger _eventTrigger;
        private ZioConfigEntry<Vector2> configEntry;
        
        void Start()
        {
            _eventTrigger = GetComponent<EventTrigger>();
            _eventTrigger.AddEventTrigger(OnDrag, EventTriggerType.Drag);
        }


        void OnDrag(BaseEventData data)
        {
            PointerEventData ped = (PointerEventData) data;
            
            Target.transform.Translate(ped.delta);
        }
    }
}