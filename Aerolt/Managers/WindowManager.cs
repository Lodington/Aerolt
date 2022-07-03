using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Aerolt.Managers
{
	public class WindowManager : MonoBehaviour
	{
		public List<Panel> panels;
		
		[Serializable]
		public struct Panel
		{
			public GameObject window;
			public Toggle button;
		}
	}
}