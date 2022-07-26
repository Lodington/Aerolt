using System;
using System.Linq;
using Aerolt.Buttons;
using Aerolt.Classes;
using Aerolt.Messages;
using RoR2;
using RoR2.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Aerolt.Managers
{
	public partial class LobbyPlayerPageManager : MonoBehaviour, IModuleStartup
	{
		private NetworkUser currentUser;
		private CharacterBody body;
		private CharacterMaster master;
		private MenuInfo info;

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

		public GameObject mainContent;
		public EditPlayerItemButton itemContent;
		public EquipmentButtonGenerator equipmentContent;
		public BodyManager bodyContent;
		public EditPlayerBuffButton buffContent;
		private ViewState _state = ViewState.Main;

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
				_playerConfig = new PlayerConfigBinding(configFile); // TODO iterate the local users to ensure all the settings are set for each user
			}

			master.onBodyStart += SetBody;
			var bodyIn = master.GetBody();
			if (bodyIn) SetBody(bodyIn);

			if (!isLocalUser) return;
			godToggle.SetIsOnWithoutNotify(_playerConfig.GodModeOn);
			aimbotToggle.SetIsOnWithoutNotify(_playerConfig.AimbotOn);
			aimbotWeightSlider.SetValueWithoutNotify(_playerConfig.AimbotWeight);
			noclipToggle.SetIsOnWithoutNotify(_playerConfig.NoclipOn);
			infiniteSkillsToggle.SetIsOnWithoutNotify(_playerConfig.InfiniteSkillsOn);
			alwaysSprintToggle.SetIsOnWithoutNotify(_playerConfig.AlwaysSprintOn);

			ApplyValues(body, _playerConfig);
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
			if (isLocalUser && _playerConfig.InfiniteSkillsOn) body.onSkillActivatedAuthority += InfiniteSkillsActivated;
		}

		public void ModuleStart()
		{
			info = GetComponentInParent<MenuInfo>();
			
			teamDropdown.options.Clear(); // ensure it was empty to begin with.
			teamDropdown.AddOptions(Enum.GetNames(typeof(TeamIndex)).Where(x => x != "None").ToList());

			godToggle.onValueChanged.AddListener(val => _playerConfig.GodModeOn = val);
			aimbotToggle.onValueChanged.AddListener(val => _playerConfig.AimbotOn = val);
			aimbotWeightSlider.onValueChanged.AddListener(val => _playerConfig.AimbotWeight = val);
			noclipToggle.onValueChanged.AddListener(val => _playerConfig.NoclipOn = val);
			infiniteSkillsToggle.onValueChanged.AddListener(val => _playerConfig.InfiniteSkillsOn = val);
			alwaysSprintToggle.onValueChanged.AddListener(val => _playerConfig.AlwaysSprintOn = val);
			
			godToggle.onValueChanged.AddListener(GodMode);
			aimbotToggle.onValueChanged.AddListener(Aimbot);
			aimbotWeightSlider.onValueChanged.AddListener(AimbotWeight);
			noclipToggle.onValueChanged.AddListener(Noclip);
			infiniteSkillsToggle.onValueChanged.AddListener(InfiniteSkills);
			alwaysSprintToggle.onValueChanged.AddListener(AlwaysSprint);

			moneyInputField.onEndEdit.AddListener(amt =>
			{
				SetCurrency(CurrencyType.Money, amt);
				moneyInputField.SetTextWithoutNotify("");
			});
			lunarCoinsInputField.onEndEdit.AddListener(amt =>
			{
				SetCurrency(CurrencyType.Lunar, amt);
				lunarCoinsInputField.SetTextWithoutNotify("");
			});
			voidMarkersInputField.onEndEdit.AddListener(amt =>
			{
				SetCurrency(CurrencyType.Void, amt);
				lunarCoinsInputField.SetTextWithoutNotify("");
			});
			
			teamDropdown.onValueChanged.AddListener(TeamChanged);
		}

		public void TeamChanged(int team)
		{
			var index = (TeamIndex) team;
			master.teamIndex = index;
			if (body)
				body.teamComponent.teamIndex = index;
		}

		public void ModuleEnd()
		{
			if (isLocalUser)
			{
				_playerConfig.OnDestroy();
			}
		}
		public void SwapViewState(ViewState newState) {
			switch (_state)
			{
				case ViewState.Main:
					mainContent.SetActive(false);
					break;
				case ViewState.Inventory:
					itemContent.gameObject.SetActive(false);
					break;
				case ViewState.Equipment:
					equipmentContent.gameObject.SetActive(false);
					break;
				case ViewState.Body:
					bodyContent.gameObject.SetActive(false);
					break;
				case ViewState.Buff:
					buffContent.gameObject.SetActive(false);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			switch (newState)
			{
				case ViewState.Main:
					mainContent.SetActive(true);
					break;
				case ViewState.Inventory:
					itemContent.gameObject.SetActive(true);
					itemContent.Initialize(currentUser);
					break;
				case ViewState.Equipment:
					equipmentContent.gameObject.SetActive(true);
					equipmentContent.Initialize(currentUser);
					break;
				case ViewState.Body:
					bodyContent.gameObject.SetActive(true);
					bodyContent.Initialize(currentUser);
					break;
				case ViewState.Buff:
					buffContent.gameObject.SetActive(true);
					buffContent.Initialize(currentUser);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
			}
			
			_state = newState;
		}

		[Serializable]
		public enum ViewState
		{
			Main,
			Inventory,
			Equipment,
			Body,
			Buff
		}

		public void SwapViewState()
		{
			SwapViewState(ViewState.Main);
		}
	}
}