using System;
using System.Collections.Generic;
using System.Diagnostics;
using Aerolt.Buttons;
using Aerolt.Classes;
using Aerolt.Messages;
using RoR2;
using ZioConfigFile;

namespace Aerolt.Managers
{
	public class PlayerConfigBinding // TODO network the values;
	{
		public CustomButton customButton; // this probably shouldn't be in here, but it was convenient

		public bool isLocalUser;
		private ZioConfigFile.ZioConfigFile configFile;
		private NetworkUser user;
		
		public static readonly string[] entriesNames = {
			"Noclip",
			"Aimbot",
			"InfiniteSkills",
			"AlwaysSprint",
			"GodMode"
		};

		private Dictionary<string, ZioConfigEntry<bool>> entries;
		private Dictionary<string, bool> values = new();
		private ZioConfigEntry<float> aimbotWeightEntry;
		private float _aimbotWeightValue;

		public bool NoclipOn
		{
			get => this["Noclip"];
			set => this["Noclip"] = value;
		}
		public bool AimbotOn
		{
			get => this["Aimbot"];
			set => this["Aimbot"] = value;
		}

		public bool InfiniteSkillsOn
		{
			get => this["InfiniteSkills"];
			set => this["InfiniteSkills"] = value;
		}

		public bool GodModeOn
		{
			get => this["GodMode"];
			set => this["GodMode"] = value;
		}

		public bool AlwaysSprintOn
		{
			get => this["AlwaysSprint"];
			set => this["AlwaysSprint"] = value;
		}

		public bool NoclipInteractDownOn
		{
			get => this["NoclipInteractDown"];
			set => this["NoclipInteractDown"] = value;
		}

		public float AimbotWeight
		{
			get => isLocalUser ? aimbotWeightEntry.Value : _aimbotWeightValue;
			set
			{
				new PlayerConfigBindingSyncMessage(user, "AimbotWeight", value).SendToEveryone();
				SetValue("AimbotWeight", value);
			}
		}

		public PlayerConfigBinding(NetworkUser currentUser, CustomButton button)
		{
			customButton = button;
			user = currentUser;
			if (currentUser.localUser != null && MenuInfo.Files.TryGetValue(currentUser.localUser, out var configFile))
			{
				isLocalUser = true;

				this.configFile = configFile;
				entries = new Dictionary<string, ZioConfigEntry<bool>>
				{
					{"NoclipInteractDown", configFile.Bind("PlayerMenu", "NoclipInteractDown", true, "Should holding interact move you down when noclipping.")}
				};
				foreach (var entriesName in entriesNames)
				{
					entries[entriesName] = configFile.Bind("PlayerMenu", entriesName, false, "");
				}
				
				aimbotWeightEntry = configFile.Bind("PlayerMenu", "AimbotWeight", 0.5f, "0 is weighted entirely to distance, while 1 is entirely to angle.");
			}
		}
		public bool this[string name]
		{
			get => isLocalUser ? entries[name].Value : values.ContainsKey(name) && values[name];
			set
			{
				new PlayerConfigBindingSyncMessage(user, name, value).SendToEveryone();
				SetValue(name, value);
			}
		}

		public void SetValue(string name, bool value)
		{
			if (isLocalUser) entries[name].Value = value;
			else values[name] = value;
			LobbyPlayerPageManager.ApplyValues(user.GetCurrentBody(), this);
		}

		public void SetValue(string name, float value)
		{
			if (isLocalUser) aimbotWeightEntry.Value = value; // TODO make this not retarded
			else _aimbotWeightValue = value;
			LobbyPlayerPageManager.ApplyValues(user.GetCurrentBody(), this);
		}
	}
}