using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using RoR2;
using RoR2.UI;
using UnityEngine;

namespace Aerolt.Classes
{
	public class MenuInfo : MonoBehaviour
	{
		[NonSerialized] public NetworkUser Owner;
		[NonSerialized] public ZioConfigFile.ZioConfigFile ConfigFile;
		[NonSerialized] public HUD Hud;
		private static readonly Dictionary<LocalUser, ZioConfigFile.ZioConfigFile> Files = new();
		[CanBeNull] public LocalUser LocalUser => Owner.localUser;
		[CanBeNull] public CharacterBody Body => LocalUser?.cachedBody;
		[CanBeNull] public CharacterMaster Master => Owner.master;

		private void Awake()
		{
			Hud = Load.tempHud;
			Owner = Load.tempViewer;
			transform.parent.GetComponentInChildren<ToggleWindow>().Init(Owner);
			
			if (Owner.localUser == null) return;
			if (Files.TryGetValue(Owner.localUser, out ConfigFile)) return;
			ConfigFile = new ZioConfigFile.ZioConfigFile(RoR2Application.cloudStorage, $"/Aerolt/Profiles/{Owner.localUser.userProfile.fileName}.cfg", true);
			Files.Add(Owner.localUser, ConfigFile);
		}
	}
}