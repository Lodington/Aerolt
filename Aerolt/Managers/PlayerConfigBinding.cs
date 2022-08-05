using System;
using Aerolt.Buttons;
using Aerolt.Classes;
using Aerolt.Helpers;
using Aerolt.Messages;
using RoR2;

namespace Aerolt.Managers
{
	public class PlayerConfigBinding // TODO network the values;
	{
		public CustomButton customButton; // this probably shouldn't be in here, but it was convenient
		private NetworkUser user;
		public ValueWrapper<bool> Noclip;
		public ValueWrapper<float> AimbotWeight;
		public ValueWrapper<bool> Aimbot;
		public ValueWrapper<bool> InfiniteSkills;
		public ValueWrapper<bool> AlwaysSprint;
		public ValueWrapper<bool> GodMode;

		public PlayerConfigBinding(NetworkUser currentUser, CustomButton button)
		{
			customButton = button;
			user = currentUser;
			
			AimbotWeight = ValueWrapper.Get("PlayerMenu", "AimbotWeight", 0.5f, "", user);
			AimbotWeight.settingChanged += OnAimbotWeightChanged;
			Aimbot = ValueWrapper.Get("PlayerMenu", "Aimbot", false, "", user);
			Aimbot.settingChanged += OnAimbotChanged;
			InfiniteSkills = ValueWrapper.Get("PlayerMenu", "InfiniteSkills", false, "", user);
			InfiniteSkills.settingChanged += OnInfiniteSkillsChanged;
			AlwaysSprint = ValueWrapper.Get("PlayerMenu", "AlwaysSprint", false, "", user);
			AlwaysSprint.settingChanged += OnAlwaysSprintChanged;
			GodMode = ValueWrapper.Get("PlayerMenu", "GodMode", false, "", user);
			GodMode.settingChanged += OnGodModeChanged;
			Noclip = ValueWrapper.Get("PlayerMenu", "Noclip", false, "", user);
			Noclip.settingChanged += OnNoclipChanged;
		}

		public void OnDestroy()
		{
			AimbotWeight.settingChanged -= OnAimbotWeightChanged;
			Aimbot.settingChanged -= OnAimbotChanged;
			InfiniteSkills.settingChanged -= OnInfiniteSkillsChanged;
			AlwaysSprint.settingChanged -= OnAlwaysSprintChanged;
			GodMode.settingChanged -= OnGodModeChanged;
			Noclip.settingChanged -= OnNoclipChanged;
		}

		private void OnNoclipChanged() => SetNoclip(user.master.GetBody(), Noclip.Value);
		private void OnGodModeChanged() => SetGodMode(user.master.GetBody(), GodMode.Value);
		private void OnAlwaysSprintChanged() => SetAlwaysSprint(user.master.GetBody(), AlwaysSprint.Value);
		private void OnInfiniteSkillsChanged() => SetInfiniteSkills(user.master.GetBody(), InfiniteSkills.Value);
		private void OnAimbotChanged() => SetAimbot(user.master.GetBody(), Aimbot.Value, AimbotWeight.Value);
		private void OnAimbotWeightChanged() => SetAimbotWeight(user.master.GetBody(), AimbotWeight.Value);

		public void Bind(Action updateCheckboxValues)
		{
			AimbotWeight.settingChanged += updateCheckboxValues;
			Aimbot.settingChanged += updateCheckboxValues;
			InfiniteSkills.settingChanged += updateCheckboxValues;
			AlwaysSprint.settingChanged += updateCheckboxValues;
			GodMode.settingChanged += updateCheckboxValues;
			Noclip.settingChanged += updateCheckboxValues;
		}

		public void UnBind(Action updateCheckboxValues)
		{
			AimbotWeight.settingChanged -= updateCheckboxValues;
			Aimbot.settingChanged -= updateCheckboxValues;
			InfiniteSkills.settingChanged -= updateCheckboxValues;
			AlwaysSprint.settingChanged -= updateCheckboxValues;
			GodMode.settingChanged -= updateCheckboxValues;
			Noclip.settingChanged -= updateCheckboxValues;
		}

		public void Sync()
		{
			AimbotWeight.Sync();
			Aimbot.Sync();
			InfiniteSkills.Sync();
			AlwaysSprint.Sync();
			GodMode.Sync();
			Noclip.Sync();
		}
		
		public static void SetAimbot(CharacterBody bod, bool enable, float weight)
		{
			if (bod) bod.ToggleComponent<AimbotBehavior>(enable, comp => comp.weight = weight);
		}

		public static void SetAimbotWeight(CharacterBody bod, float weight)
		{
			if (!bod) return; 
			var behavior = bod.GetComponent<AimbotBehavior>();
			if (behavior)
				behavior.weight = weight;
		}
		public static void SetGodMode(CharacterBody bod, bool enable)
		{
			if (bod) new GodModeMessage(bod.master, enable).SendToServer();
		}
		public static void SetNoclip(CharacterBody bod, bool enable)
		{
			if (bod) bod.ToggleComponent<NoclipBehavior>(enable);
		}

		public static void SetInfiniteSkills(CharacterBody bod, bool enable)
		{
			if (enable && bod) bod.onSkillActivatedAuthority += InfiniteSkillsActivated;
			else if (bod) bod.onSkillActivatedAuthority -= InfiniteSkillsActivated;
		}
		public static void SetAlwaysSprint(CharacterBody bod, bool enable)
		{
			if (bod) bod.ToggleComponent<AlwaysSprintBehavior>(enable);
		}

		public static void InfiniteSkillsActivated(GenericSkill obj) => obj.AddOneStock();
	}
}