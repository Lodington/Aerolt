using System;
using Aerolt.Buttons;
using Aerolt.Classes;
using RoR2;
using ZioConfigFile;

namespace Aerolt.Managers
{
	public class PlayerConfigBinding // TODO network the values;
	{
		private ZioConfigEntry<bool> noclipEntry;
		private ZioConfigEntry<bool> aimbotEntry;
		private ZioConfigEntry<bool> infiniteSkillsEntry;
		private ZioConfigEntry<bool> godModeEntry;
		private ZioConfigEntry<bool> alwaysSprintEntry;
		private ZioConfigEntry<bool> noclipInteractDown;
		private ZioConfigEntry<float> aimbotWeightEntry;
		
		public Action<float> aimbotWeightChanged; // these are probably going to go unused, and get removed.
		public Action<bool> noclipInteractDownChanged;
		
		public CustomButton customButton; // this probably shouldn't be in here, but it was convenient
		
		public bool isLocalUser;
		private ZioConfigFile.ZioConfigFile configFile;
		
		// Used for remote users, and always default if its local
		private bool _noclipValue;
		private bool _aimbotValue;
		private bool _infiniteSkillsValue;
		private bool _godModeValue;
		private bool _alwaysSprintValue;
		private bool _noclipInteractDownValue;
		private float _aimbotWeightValue;


		public bool NoclipOn
		{
			get => isLocalUser ? noclipEntry.Value : _noclipValue;
			set
			{
				if (isLocalUser) noclipEntry.Value = value;
				else _noclipValue = value;
			}
		}
		public bool AimbotOn
		{
			get => isLocalUser ? aimbotEntry.Value : _aimbotValue;
			set
			{
				if (isLocalUser)
					aimbotEntry.Value = value;
				else
					_aimbotValue = value;
			}
		}

		public bool InfiniteSkillsOn
		{
			get => isLocalUser ? infiniteSkillsEntry.Value : _infiniteSkillsValue;
			set
			{
				if (isLocalUser)
					infiniteSkillsEntry.Value = value;
				else
					_infiniteSkillsValue = value;
			}
		}

		public bool GodModeOn
		{
			get => isLocalUser ? godModeEntry.Value : _godModeValue;
			set
			{
				if (isLocalUser)
					godModeEntry.Value = value;
				else
					_godModeValue = value;
			}
		}

		public bool AlwaysSprintOn
		{
			get => isLocalUser ? alwaysSprintEntry.Value : _alwaysSprintValue;
			set
			{
				if (isLocalUser)
					alwaysSprintEntry.Value = value;
				else
					_alwaysSprintValue = value;
			}
		}

		public bool NoclipInteractDownOn
		{
			get => isLocalUser ? noclipInteractDown.Value : _noclipInteractDownValue;
			set
			{
				if(isLocalUser)
					noclipInteractDown.Value = value;
				else
					_noclipInteractDownValue = value;
			}
		}

		public float AimbotWeight
		{
			get => isLocalUser ? aimbotWeightEntry.Value : _aimbotWeightValue;
			set
			{
				if(isLocalUser)
					aimbotWeightEntry.Value = value;
				else
					_aimbotWeightValue = value;
			}
		}
		public PlayerConfigBinding(NetworkUser currentUser, CustomButton button)
		{
			customButton = button;
			if (currentUser.localUser != null && MenuInfo.Files.TryGetValue(currentUser.localUser, out var configFile))
			{
				isLocalUser = true;

				this.configFile = configFile;
				noclipEntry = configFile.Bind("PlayerMenu", "Noclip", false, "");
				aimbotEntry = configFile.Bind("PlayerMenu", "Aimbot", false, "");
				infiniteSkillsEntry = configFile.Bind("PlayerMenu", "InfiniteSkills", false, "");
				alwaysSprintEntry = configFile.Bind("PlayerMenu", "AlwaysSprint", false, "");
				aimbotWeightEntry = configFile.Bind("PlayerMenu", "AimbotWeight", 0.5f, "0 is weighted entirely to distance, while 1 is entirely to angle.");
				godModeEntry = configFile.Bind("PlayerMenu", "GodMode", false, "");
				noclipInteractDown = configFile.Bind("PlayerMenu", "NoclipInteractDown", true, "Should holding interact move you down when noclipping.");
			}
		}

		public void OnDestroy()
		{
			//noclipInteractDown.SettingChanged -= NoclipDownChanged;
			//aimbotWeightEntry.SettingChanged -= AimbotWeightChanged;
		}

		private void AimbotWeightChanged(ZioConfigEntryBase arg1, object arg2, bool arg3)
		{
			aimbotWeightChanged?.Invoke(aimbotWeightEntry.Value);
		}

		private void NoclipDownChanged(ZioConfigEntryBase arg1, object arg2, bool arg3)
		{
			noclipInteractDownChanged?.Invoke(noclipEntry.Value);
		}
	}
}