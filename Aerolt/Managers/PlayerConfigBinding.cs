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
			AimbotWeight.settingChanged += () => SetAimbotWeight(user.master.GetBody(), AimbotWeight.Value);
			Aimbot = ValueWrapper.Get("PlayerMenu", "Aimbot", false, "", user);
			Aimbot.settingChanged += () => SetAimbot(user.master.GetBody(), Aimbot.Value, AimbotWeight.Value);
			InfiniteSkills = ValueWrapper.Get("PlayerMenu", "InfiniteSkills", false, "", user);
			InfiniteSkills.settingChanged += () => SetInfiniteSkills(user.master.GetBody(), InfiniteSkills.Value);
			AlwaysSprint = ValueWrapper.Get("PlayerMenu", "AlwaysSprint", false, "", user);
			AlwaysSprint.settingChanged += () => SetAlwaysSprint(user.master.GetBody(), AlwaysSprint.Value);
			GodMode = ValueWrapper.Get("PlayerMenu", "GodMode", false, "", user);
			GodMode.settingChanged += () => SetGodMode(user.master.GetBody(), GodMode.Value);
			Noclip = ValueWrapper.Get("PlayerMenu", "Noclip", false, "", user);
			Noclip.settingChanged += () => SetNoclip(user.master.GetBody(), Noclip.Value);
		}

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
		
		public static void SetAimbot(CharacterBody bod, bool enable, float weight) => bod.ToggleComponent<AimbotBehavior>(enable, comp => comp.weight = weight);
		public static void SetAimbotWeight(CharacterBody bod, float weight)
		{
			var behavior = bod.GetComponent<AimbotBehavior>();
			if (behavior)
				behavior.weight = weight;
		}
		public static void SetGodMode(CharacterBody bod, bool enable) => new GodModeMessage(bod.master, enable).SendToServer();
		public static void SetNoclip(CharacterBody bod, bool enable) => bod.ToggleComponent<NoclipBehavior>(enable);
		public static void SetInfiniteSkills(CharacterBody bod, bool enable)
		{
			if (enable && bod) bod.onSkillActivatedAuthority += InfiniteSkillsActivated;
			else if (bod) bod.onSkillActivatedAuthority -= InfiniteSkillsActivated;
		}
		public static void SetAlwaysSprint(CharacterBody bod, bool enable) => bod.ToggleComponent<AlwaysSprintBehavior>(enable);
		
		public static void InfiniteSkillsActivated(GenericSkill obj) => obj.AddOneStock();
	}
}