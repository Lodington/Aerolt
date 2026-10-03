using System;
using Aerolt.Enums;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Aerolt.Helpers
{
    [RequireComponent(typeof(EventTrigger))]
    public class ResizePanel : MonoBehaviour
    {
        public HandlerType Type;
        public RectTransform Target = null!;
        public Vector2 MinimumDimmensions = new(50, 50);
        public Vector2 MaximumDimmensions = new(800, 800);

        private EventTrigger _eventTrigger = null!;

        private void Awake()
        {
            Target = (RectTransform)transform.parent;
        }


        private void Start()
        {
            _eventTrigger = GetComponent<EventTrigger>();
            _eventTrigger.AddEventTrigger(OnDrag, EventTriggerType.Drag);
        }


        private void OnDrag(BaseEventData data)
        {
            if (Target == null) return;
            var ped = (PointerEventData)data;

            // Which edges this handle moves. The opposite edge stays anchored.
            // +X = drag grows width on the right, -X = on the left; same idea vertically.
            var horizontalDir = 0; // -1 left handle, +1 right handle, 0 none
            var verticalDir = 0; // -1 bottom handle, +1 top handle, 0 none

            switch (Type)
            {
                case HandlerType.TopRight:
                    horizontalDir = 1;
                    verticalDir = 1;
                    break;
                case HandlerType.Right:
                    horizontalDir = 1;
                    break;
                case HandlerType.BottomRight:
                    horizontalDir = 1;
                    verticalDir = -1;
                    break;
                case HandlerType.Bottom:
                    verticalDir = -1;
                    break;
                case HandlerType.BottomLeft:
                    horizontalDir = -1;
                    verticalDir = -1;
                    break;
                case HandlerType.Left:
                    horizontalDir = -1;
                    break;
                case HandlerType.TopLeft:
                    horizontalDir = -1;
                    verticalDir = 1;
                    break;
                case HandlerType.Top:
                    verticalDir = 1;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var size = Target.sizeDelta;
            var pos = Target.anchoredPosition;
            var pivot = Target.pivot;

            if (horizontalDir != 0)
            {
                // Requested size change along X for this edge.
                var desiredWidth = size.x + horizontalDir * ped.delta.x;
                var newWidth = Mathf.Clamp(desiredWidth, MinimumDimmensions.x, MaximumDimmensions.x);
                var appliedDelta = newWidth - size.x; // what actually changed after clamping

                pos.x += horizontalDir > 0
                    ? (1f - pivot.x) * appliedDelta
                    : -pivot.x * appliedDelta;
                size.x = newWidth;
            }

            if (verticalDir != 0)
            {
                var desiredHeight = size.y + verticalDir * ped.delta.y;
                var newHeight = Mathf.Clamp(desiredHeight, MinimumDimmensions.y, MaximumDimmensions.y);
                var appliedDelta = newHeight - size.y;

                pos.y += verticalDir > 0
                    ? (1f - pivot.y) * appliedDelta
                    : -pivot.y * appliedDelta;
                size.y = newHeight;
            }

            Target.sizeDelta = size;
            Target.anchoredPosition = pos;
        }
    }
}