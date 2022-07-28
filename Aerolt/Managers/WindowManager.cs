using System;
using System.Collections.Generic;
using Aerolt.Classes;
using Rewired.UI.ControlMapper;
using UnityEngine;
using UnityEngine.UI;
using ZioConfigFile;

namespace Aerolt.Managers
{
	public class WindowManager : MonoBehaviour
	{
		public List<Panel> panels;
		private Dictionary<Panel, ZioConfigEntry<bool>> windowOpen = new();

		public void Awake()
		{
			var info = GetComponentInParent<MenuInfo>();
			foreach (var panel in panels)
			{
				var entry = info.ConfigFile.Bind("Window Open", panel.window.name, panel.button.isOn, "");
				windowOpen[panel] = entry;
				panel.button.onValueChanged.AddListener(on => entry.Value = on);
			}
		}

		[Serializable]
		public struct Panel
		{
			public GameObject window;
			public Toggle button;
		}
	}
}