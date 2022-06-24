using System;
using Aerolt.Managers;
using RoR2.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using ZioConfigFile;

namespace Aerolt.Helpers
{
	[RequireComponent(typeof(DragMove))]
	public class DragMoveSave : MonoBehaviour, IEndDragHandler
	{
		private RectTransform targetTransform;
		private ZioConfigEntry<Vector2> configEntry;
		public string windowName;

		public void Awake()
		{
			targetTransform = (RectTransform) transform;
			var panelManager = targetTransform.parent ? GetComponentInParent<PanelManager>() : GetComponent<PanelManager>();
			if (!panelManager) throw new Exception($"Panel manager not found for object {this}");
			var configFile = panelManager ? panelManager.configFile : Load.Instance.configFile;
			configEntry = configFile.Bind("Window Positions", windowName, (Vector2) targetTransform.localPosition, "Stored position of this window.");
			targetTransform.localPosition = configEntry.Value;
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			if (!targetTransform) return;
			if (eventData.button != PointerEventData.InputButton.Left) return;
			configEntry.Value = targetTransform.localPosition;
		}
	}
}