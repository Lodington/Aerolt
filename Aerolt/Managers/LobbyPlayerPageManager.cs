using System;
using System.Linq;
using Aerolt.Buttons;
using Aerolt.Classes;
using RoR2;
using RoR2.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Aerolt.Managers
{
	public class LobbyPlayerPageManager : MonoBehaviour, IModuleStartup
	{
		private NetworkUser currentUser;
		private CharacterBody body;
		private CharacterMaster master;

		public PlayerValuesGenerator bodyStats;
		
		public ItemInventoryDisplay inventoryDisplay;
		public EquipmentIcon equipmentIcon;
		public BuffDisplay buffDisplay;

		public Toggle aimbotToggle;
		public Toggle noclipToggle;
		public Toggle godToggle;
		public Toggle infiniteSkillsToggle;
		public Toggle alwaysSprintToggle;

		public Slider aimbotWeightSlider;

		public TMP_Dropdown teamDropdown;

		public TMP_InputField moneyInputField;
		public TMP_InputField lunarCoinsInputField;
		public TMP_InputField voidMarkersInputField;
		
		private bool isLocalUser;
		private PlayerConfigBinding _playerConfig;

		public void SetUser(NetworkUser user)
		{
			if (currentUser != null) currentUser.master.onBodyStart -= SetBody;
			currentUser = user;
			master = currentUser.master;
			
			var inv = master.inventory;
			inventoryDisplay.SetSubscribedInventory(inv);
			equipmentIcon.targetInventory = inv;

			isLocalUser = false;
			if (currentUser.localUser != null && MenuInfo.Files.TryGetValue(currentUser.localUser, out var configFile))
			{
				isLocalUser = true;

				_playerConfig?.OnDestroy();
				_playerConfig = new PlayerConfigBinding(configFile);
			}
			
			master.onBodyStart += SetBody;
			var bodyIn = master.GetBody();
			if (bodyIn) SetBody(bodyIn);
		}

		public void Update()
		{
			if (!currentUser) return;
			((TextMeshProUGUI) moneyInputField.placeholder).text = master.money.ToString();
			((TextMeshProUGUI) voidMarkersInputField.placeholder).text = master.voidCoins.ToString();
			((TextMeshProUGUI) lunarCoinsInputField.placeholder).text = currentUser.lunarCoins.ToString();
		}

		private void SetBody(CharacterBody bodyIn)
		{
			body = bodyIn;
			buffDisplay.source = body;
			bodyStats.TargetBody = body;
			teamDropdown.SetValueWithoutNotify((int) body.teamComponent.teamIndex);
		}

		public void ModuleStart()
		{
			teamDropdown.options.Clear(); // ensure it was empty to begin with.
			teamDropdown.AddOptions(Enum.GetNames(typeof(TeamIndex)).Where(x => x != "None").ToList());
		}

		public void ModuleEnd()
		{
			if (isLocalUser)
			{
				_playerConfig.OnDestroy();
			}
		}
	}
}