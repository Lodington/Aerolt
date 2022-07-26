#nullable enable
using System;
using Aerolt.Classes;
using Aerolt.Messages;
using RoR2;
using UnityEngine;

namespace Aerolt.Managers
{
	public partial class LobbyPlayerPageManager
	{
		public static void ToggleComponent<T>(CharacterBody bod, bool enable, Action<T>? onFirstSetup = null) where T : MonoBehaviour
		{
			if (!bod) return;

			var behavior = bod.GetComponent<T>();
			if (behavior && !enable)
				Destroy(behavior);
			else if (enable && !behavior)
			{
				var comp = bod.gameObject.AddComponent<T>();
				onFirstSetup?.Invoke(comp);
			}
		}
		public void SetCurrency(CurrencyType currencyType, string strAmount)
		{
			if (!uint.TryParse(strAmount, out var amount)) return;
			new CurrencyMessage(master, currencyType, amount).SendToServer(); // Send to server now just calls handle if we're on a server already
		}
		public void Aimbot(bool enable) => Aimbot(body, enable, _playerConfig.AimbotWeight);
		public static void Aimbot(CharacterBody bod, bool enable, float weight) => ToggleComponent<AimbotBehavior>(bod, enable, comp => comp.weight = weight);
		public void AimbotWeight(float weight) => AimbotWeight(body, weight);
		public static void AimbotWeight(CharacterBody bod, float weight)
		{
			var behavior = bod.GetComponent<AimbotBehavior>();
			if (behavior)
				behavior.weight = weight;
		}
		public void GodMode(bool enable) => GodMode(body, enable);
		public static void GodMode(CharacterBody bod, bool enable) => new GodModeMessage(bod.master, enable).SendToServer();
		public void Noclip(bool enable) => Noclip(body, enable);
		public static void Noclip(CharacterBody bod, bool enable) => ToggleComponent<NoclipBehavior>(bod, enable);
		public void InfiniteSkills(bool enable) => InfiniteSkills(body, enable);
		public static void InfiniteSkills(CharacterBody bod, bool enable)
		{
			if (enable && bod) bod.onSkillActivatedAuthority += InfiniteSkillsActivated;
			else if (bod) bod.onSkillActivatedAuthority -= InfiniteSkillsActivated;
		}
		public static void InfiniteSkillsActivated(GenericSkill obj) => obj.AddOneStock();
		public void AlwaysSprint(bool enable) => AlwaysSprint(body, enable);
		public static void AlwaysSprint(CharacterBody bod, bool enable) => ToggleComponent<AlwaysSprintBehavior>(bod, enable);

		public static void ApplyValues(CharacterBody characterBody, PlayerConfigBinding playerConfig)
		{
			Aimbot(characterBody, playerConfig.AimbotOn, playerConfig.AimbotWeight);
			GodMode(characterBody, playerConfig.GodModeOn);
			Noclip(characterBody, playerConfig.NoclipOn);
			InfiniteSkills(characterBody, playerConfig.InfiniteSkillsOn);
			AlwaysSprint(characterBody, playerConfig.AlwaysSprintOn);
		}
	}
}