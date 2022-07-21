using System;
using System.Collections.Generic;
using Aerolt.Classes;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ZioConfigFile;
using ZioRiskOfOptions;

namespace Aerolt.Helpers
{
	// Its set up this way, so we can put all the child recolorers on the prefabs we use in the variants and then on the variants just have this component, resulting in considerably less unity setup
	public class ComponentColorer : MonoBehaviour, IModuleStartup
	{
		public string configName;

		private static Dictionary<ColorLayer, Color> defaultColors = new()
		{
			{ColorLayer.Foreground, new Color(0.3490196f, 0.3372549f, 0.9372549f)},
			{ColorLayer.Background, new Color(0.1176471f, 0.1098039f, 0.1490196f, 0.8509804f)}, //new Color(0, 0, 0, 0.1960784f)},
			{ColorLayer.Text, Color.white},
			{ColorLayer.TextGrey, new Color(0.6705883f, 0.6666667f, 0.6941177f)},
			{ColorLayer.Accent, new Color(0.9254902f, 0.3215686f, 0.4705882f)},
		};
		public Dictionary<ColorLayer, ZioConfigEntryBase> colorEntries = new();
		public Image toggleImage; // Might want to put these under their own config entry/color, especially this one considering its paired to the grey text color which is ew.
		public Image toggleOnImage;
		private MenuInfo menuInfo;
		private bool initialized;

		private void Awake()
		{
			ModuleStart();
		}

		public void ModuleStart()
		{
			if (initialized) return;
			menuInfo = GetComponentInParent<MenuInfo>();
			var configFile = menuInfo.ConfigFile;
			if (configFile == null) return;

			if (string.IsNullOrWhiteSpace(configName))
			{
				var save = GetComponent<DragMoveSave>();
				if (save)
					configName = save.windowName;
				var moveresize = GetComponent<DragResizeMove>();
				if (moveresize)
					configName = moveresize.windowName;
			}

			var i = 0;
			foreach (var colorName in Enum.GetNames(typeof(ColorLayer)))
			{
				var definition = new ConfigDefinition(configName + " Colors", colorName);

				if (!configFile.TryGetValue(definition, out var entry))
				{
					entry = configFile.Bind(definition, defaultColors[(ColorLayer) i], new ConfigDescription($"{colorName} color of the window."));
					if (Chainloader.PluginInfos.ContainsKey("bubbet.zioriskofoptions")) MakeRiskOfOptions(entry);
				}
				
				colorEntries[(ColorLayer) i] = entry; 
				i++;
			}

			colorEntries[ColorLayer.TextGrey].SettingChanged += ColorChanged;
			ColorChanged(colorEntries[ColorLayer.TextGrey], null, false);
			colorEntries[ColorLayer.Foreground].SettingChanged += ColorChangedOn;
			ColorChangedOn(colorEntries[ColorLayer.Foreground], null, false);
			initialized = true;
		}

		private void MakeRiskOfOptions(ZioConfigEntryBase value)
		{
			var who = menuInfo ? Load.Name + " " + menuInfo.Owner.GetNetworkPlayerName().GetResolvedName() : Load.Guid;
			RiskOfOptions.ModSettingsManager.AddOption(new ZioColorOption((ZioConfigEntry<Color>) value), who, who);
		}

		private void ColorChangedOn(ZioConfigEntryBase arg1, object arg2, bool arg3)
		{
			if (!toggleOnImage) return;
			toggleOnImage.color = (Color) arg1.BoxedValue;
		}

		private void ColorChanged(ZioConfigEntryBase arg1, object arg2, bool arg3)
		{
			if (!toggleImage) return;
			toggleImage.color = (Color) arg1.BoxedValue;
		}

		public void ModuleEnd()
		{
			colorEntries[ColorLayer.TextGrey].SettingChanged -= ColorChanged;
			colorEntries[ColorLayer.Foreground].SettingChanged -= ColorChangedOn;
		}
	}

	public abstract class ColorableComponent : MonoBehaviour
	{
		private ZioConfigEntryBase parent;

		private void Awake()
		{
			if (GetComponentInParent<ComponentColorer>().colorEntries.TryGetValue(colorLayer, out parent))
				parent.SettingChanged += Colorize;
		}

		private void OnEnable()
		{
			if (parent != null)
				Colorize(parent, null, false);
		}

		private void OnDestroy()
		{
			if (parent != null)
				parent.SettingChanged -= Colorize;
		}

		public ColorLayer colorLayer = ColorLayer.Foreground; // this cant be a property for some fucking reason unity wont serialize it
		public abstract void Colorize(ZioConfigEntryBase configEntry, object oldValue, bool _);

		protected Color Color;
	}
	
	public enum ColorLayer
	{
		Foreground,
		Background,
		Text,
		Accent,
		TextGrey
	}

	public class ColorableText : ColorableComponent
	{
		public override void Colorize(ZioConfigEntryBase configEntry, object oldValue, bool _)
		{
			GetComponent<TextMeshProUGUI>().color = (Color) configEntry.BoxedValue;
		}
	}
	public class ColorableImage : ColorableComponent
	{
		public float alphaMult = 1f;
		public override void Colorize(ZioConfigEntryBase configEntry, object oldValue, bool _)
		{
			var color = (Color) configEntry.BoxedValue;
			color.a *= alphaMult;
			GetComponent<Image>().color = color;
		}
	}
	public class ColorableRawImage : ColorableComponent
	{
		public override void Colorize(ZioConfigEntryBase configEntry, object oldValue, bool _)
		{
			GetComponent<RawImage>().color = (Color) configEntry.BoxedValue;
		}
	}
}