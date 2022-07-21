using System;
using Aerolt.Classes;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ZioConfigFile;

namespace Aerolt.Helpers
{
	
	[RequireComponent(typeof(RectTransform))]
	public class DragResizeMove : MonoBehaviour, IBeginDragHandler, IEventSystemHandler, IDragHandler, IEndDragHandler
	{
		public RectTransform targetTransform;
		private Vector2 grabPoint;
		
		private ZioConfigEntry<Vector2> configEntry;
		public string windowName;
		
		private bool resize;
		public Vector2 minSize;
		private Vector2 oldPivot;
		private Vector2 offset;

		private void Awake()
		{
			targetTransform ??= (RectTransform) transform;
			var menuInfo = targetTransform.parent ? GetComponentInParent<MenuInfo>() : GetComponent<MenuInfo>();
			var configFile = menuInfo ? menuInfo.ConfigFile : Load.Instance.configFile;
			configEntry = configFile.Bind("Window Positions", windowName, (Vector2) targetTransform.localPosition, "Stored position of this window.");
			targetTransform.localPosition = new Vector3(configEntry.Value.x, configEntry.Value.y, 0);
		}
		public void OnDrag(PointerEventData eventData)
		{
			UpdateDrag(eventData);
		}
		public void OnBeginDrag(PointerEventData eventData)
		{
			if (!targetTransform) return;
			targetTransform.SetAsLastSibling();
			RectTransformUtility.ScreenPointToLocalPointInRectangle(targetTransform, eventData.position, eventData.pressEventCamera, out grabPoint);
			var targetTransformPivot = targetTransform.pivot;
			oldPivot = targetTransformPivot;
			resize = false;
			var halfSize = targetTransform.sizeDelta * 0.5f; 
			if (halfSize.x - Mathf.Abs(grabPoint.x) < 10)
			{
				targetTransformPivot.x = grabPoint.x < 0 ? 1 : 0;
				resize = true;
			}
			if (halfSize.y - Mathf.Abs(grabPoint.y) < 10)
			{
				targetTransformPivot.y = grabPoint.y < 0 ? 1 : 0;
				resize = true;
			}
			
			offset = new Vector2(Mathf.Approximately(targetTransformPivot.x, 0.5f) ? 0 : targetTransformPivot.x - 0.5f, Mathf.Approximately(targetTransformPivot.y, 0.5f) ? 0 : targetTransformPivot.y - 0.5f);
			var vec = halfSize * offset;
			targetTransform.pivot = targetTransformPivot;
			targetTransform.localPosition += new Vector3(vec.x, vec.y, 0f);
		}
		private void UpdateDrag(PointerEventData eventData)
		{
			if (eventData.button != PointerEventData.InputButton.Left) return;
			if (!targetTransform) return;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(targetTransform, eventData.position, eventData.pressEventCamera, out var point);
			var vector = point - grabPoint;
			if (resize)
			{
				grabPoint = point;
				vector.y = -vector.y;
				var rhs = new Vector2(LayoutUtility.GetMinSize(targetTransform, 0), LayoutUtility.GetMinSize(targetTransform, 1));
				minSize = Vector2.Max(minSize, rhs);
				targetTransform.sizeDelta = Vector2.Max(targetTransform.sizeDelta + vector, minSize);
			}
			else
				targetTransform.localPosition += new Vector3(vector.x, vector.y, 0f);
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			if (!targetTransform) return;
			if (eventData.button != PointerEventData.InputButton.Left) return;
			if (resize)
			{
				var vec = targetTransform.sizeDelta * 0.5f * offset;
				targetTransform.pivot = oldPivot;
				targetTransform.localPosition -= new Vector3(vec.x, vec.y, 0f);
			}

			var localPosition = targetTransform.localPosition;
			var parent = (RectTransform) targetTransform.parent;
			var width = parent.sizeDelta.x * 0.5f - 10f + targetTransform.sizeDelta.x * 0.5f;
			var height = parent.sizeDelta.y * 0.5f - 10f + targetTransform.sizeDelta.y * 0.5f;
			//configEntry.Value = new Vector2(Mathf.Clamp(localPosition.x, -width, width), Mathf.Clamp(localPosition.y, -height, height));
			// TODO reset pivot back to previous if we're resizing
		}
	}
}