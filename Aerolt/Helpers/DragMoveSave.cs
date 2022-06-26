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

		public void Start()
		{
			targetTransform = (RectTransform) transform;
			var panelManager = targetTransform.parent ? GetComponentInParent<PanelManager>() : GetComponent<PanelManager>();
			var configFile = panelManager ? panelManager.configFile : Load.Instance.configFile;
			configEntry = configFile.Bind("Window Positions", windowName, (Vector2) targetTransform.localPosition, "Stored position of this window.");
			targetTransform.localPosition = new Vector3(configEntry.Value.x, configEntry.Value.y, 0);
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			if (!targetTransform) return;
			if (eventData.button != PointerEventData.InputButton.Left) return;
			var localPosition = targetTransform.localPosition;
			configEntry.Value = new Vector2(localPosition.x, localPosition.y);
		}
	}
}