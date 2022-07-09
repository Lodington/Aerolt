using System;
using ZioConfigFile;

namespace Aerolt.Managers
{
	public class PlayerConfigBinding
	{
		private ZioConfigEntry<bool> noclipEntry;
		private ZioConfigEntry<bool> aimbotEntry;
		private ZioConfigEntry<bool> infiniteSkillsEntry;
		private ZioConfigEntry<bool> godModeEntry;
		private ZioConfigEntry<bool> alwaysSprintEntry;
		
		private ZioConfigEntry<bool> noclipInteractDown;
		
		private ZioConfigEntry<float> aimbotWeightEntry;

		public bool NoclipOn
		{
			get => noclipEntry.Value;
			set => noclipEntry.Value = value;
		}
		public bool AimbotOn
		{
			get => aimbotEntry.Value;
			set => aimbotEntry.Value = value;
		}
		public bool InfiniteSkillsOn
		{
			get => infiniteSkillsEntry.Value;
			set => infiniteSkillsEntry.Value = value;
		}
		public bool GodModeOn
		{
			get => godModeEntry.Value;
			set => godModeEntry.Value = value;
		}
		public bool AlwaysSprintOn
		{
			get => alwaysSprintEntry.Value;
			set => alwaysSprintEntry.Value = value;
		}
		public bool NoclipInteractDownOn
		{
			get => noclipInteractDown.Value;
			set => noclipInteractDown.Value = value;
		}
		public float AimbotWeight
		{
			get => aimbotWeightEntry.Value;
			set => aimbotWeightEntry.Value = value;
		}

		public Action<float> aimbotWeightChanged;
		public Action<bool> noclipInteractDownChanged;

		public PlayerConfigBinding(ZioConfigFile.ZioConfigFile configFile)
		{
			noclipEntry = configFile.Bind("PlayerMenu", "Noclip", false, "");
			aimbotEntry = configFile.Bind("PlayerMenu", "Aimbot", false, "");
			infiniteSkillsEntry = configFile.Bind("PlayerMenu", "InfiniteSkills", false, "");
			alwaysSprintEntry = configFile.Bind("PlayerMenu", "AlwaysSprint", false, "");
			aimbotWeightEntry = configFile.Bind("PlayerMenu", "AimbotWeight", 0.5f, "0 is weighted entirely to distance, while 1 is entirely to angle.");
			godModeEntry = configFile.Bind("PlayerMenu", "GodMode", false, "");
			noclipInteractDown = configFile.Bind("PlayerMenu", "NoclipInteractDown", true, "Should holding interact move you down when noclipping.");

			noclipInteractDown.SettingChanged += NoclipDownChanged;
			aimbotWeightEntry.SettingChanged += AimbotWeightChanged;
		}
		public void OnDestroy()
		{
			noclipInteractDown.SettingChanged -= NoclipDownChanged;
			aimbotWeightEntry.SettingChanged -= AimbotWeightChanged;
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