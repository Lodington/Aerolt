using System;
using System.Linq;
using Aerolt.Buttons;
using Aerolt.Classes;
using Aerolt.Enums;
using Aerolt.Messages;
using RoR2;
using RoR2.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Aerolt.Managers
{
	public partial class LobbyPlayerPageManager : MonoBehaviour, IModuleStartup
	{
		[Header("Character Info")]
		private NetworkUser currentUser;
		private CharacterBody body;
		private CharacterMaster master;
		private MenuInfo info;
		public PlayerValuesGenerator bodyStats;

		
		[Header("Inventory Display")]
		public ItemInventoryDisplay inventoryDisplay;
		public EquipmentIcon equipmentIcon;
		public BuffDisplay buffDisplay;

		[Header("Toggles")]
		public Toggle aimbotToggle;
		public Toggle noclipToggle;
		public Toggle godToggle;
		public Toggle infiniteSkillsToggle;
		public Toggle alwaysSprintToggle;
		public Toggle disableMobSpawnToggle;

		[Header("Sliders")]
		public Slider aimbotWeightSlider;
		public Slider xpSlider;

		[Header("DropDowns")]
		public TMP_Dropdown teamDropdown;

		[Header("InputFields")]
		public TMP_InputField moneyInputField;
		public TMP_InputField lunarCoinsInputField;
		public TMP_InputField voidMarkersInputField;
		public TMP_InputField xpToGiveInputField;

		[Header("Content Display")]
		public GameObject mainContent;
		public EditPlayerItemButton itemContent;
		public EquipmentButtonGenerator equipmentContent;
		public BodyManager bodyContent;
		public EditPlayerBuffButton buffContent;
		public TMP_Text LevelLabel;
		
		private ViewState _state = ViewState.Main;
		private LobbyPlayerManager playerManager;
		private bool ownerIsSelected;
		private PlayerConfigBinding PlayerConfig => playerManager.users[currentUser];

		public void SetUser(NetworkUser user)
		{
			SwapViewState();
			ownerIsSelected = user == info.Owner;
			bodyStats.SetTogglesActive(ownerIsSelected);
			if (currentUser != null) currentUser.master.onBodyStart -= SetBody;
			currentUser = user;
			master = currentUser.master;

			var inv = master.inventory;
			inventoryDisplay.SetSubscribedInventory(inv);
			equipmentIcon.targetInventory = inv;

			master.onBodyStart += SetBody;
			var bodyIn = master.GetBody();
			if (bodyIn) SetBody(bodyIn);
			
			godToggle.SetIsOnWithoutNotify(PlayerConfig.GodModeOn);
			aimbotToggle.SetIsOnWithoutNotify(PlayerConfig.AimbotOn);
			aimbotWeightSlider.SetValueWithoutNotify(PlayerConfig.AimbotWeight);
			noclipToggle.SetIsOnWithoutNotify(PlayerConfig.NoclipOn);
			infiniteSkillsToggle.SetIsOnWithoutNotify(PlayerConfig.InfiniteSkillsOn);
			alwaysSprintToggle.SetIsOnWithoutNotify(PlayerConfig.AlwaysSprintOn);
			
			UpdateLevelValues();
		}


		public void Update()
		{
			if (!currentUser) return;
			((TextMeshProUGUI) moneyInputField.placeholder).text = master.money.ToString();
			((TextMeshProUGUI) voidMarkersInputField.placeholder).text = master.voidCoins.ToString();
			((TextMeshProUGUI) lunarCoinsInputField.placeholder).text = currentUser.lunarCoins.ToString();

			//xpSlider.value = TeamManager.instance.GetTeamExperience((TeamIndex) teamDropdown.value);
		}

		private void SetBody(CharacterBody bodyIn)
		{
			body = bodyIn;
			if (!ownerIsSelected) bodyStats.ProfileSelected(0, false);
			buffDisplay.source = body;
			bodyStats.TargetBody = body;
			teamDropdown.SetValueWithoutNotify((int) body.teamComponent.teamIndex);
			if (PlayerConfig.InfiniteSkillsOn) body.onSkillActivatedAuthority += InfiniteSkillsActivated;
		}

		private void OnEnable()
		{
			UpdateLevelValues();
		}

		public void ModuleStart()
		{
			info = GetComponentInParent<MenuInfo>();
			playerManager = GetComponent<LobbyPlayerManager>();
			bodyStats.Setup();

			GlobalEventManager.onTeamLevelUp += OnTeamLevelUp;
			
			teamDropdown.options.Clear(); // ensure it was empty to begin with.
			teamDropdown.AddOptions(Enum.GetNames(typeof(TeamIndex)).Where(x => x != "None").ToList());

			godToggle.onValueChanged.AddListener(val => PlayerConfig.GodModeOn = val);
			aimbotToggle.onValueChanged.AddListener(val => PlayerConfig.AimbotOn = val);
			aimbotWeightSlider.onValueChanged.AddListener(val => PlayerConfig.AimbotWeight = val);
			noclipToggle.onValueChanged.AddListener(val => PlayerConfig.NoclipOn = val);
			infiniteSkillsToggle.onValueChanged.AddListener(val => PlayerConfig.InfiniteSkillsOn = val);
			alwaysSprintToggle.onValueChanged.AddListener(val => PlayerConfig.AlwaysSprintOn = val);
			
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
			xpToGiveInputField.onEndEdit.AddListener(amt =>
			{
				SetCurrency(CurrencyType.Level, amt);
				UpdateLevelValues();
				xpToGiveInputField.SetTextWithoutNotify("");
			});
			
			teamDropdown.onValueChanged.AddListener(TeamChanged);
			TeamComponent.onJoinTeamGlobal += TeamJoined;

		}

		private void TeamJoined(TeamComponent who, TeamIndex team)
		{
			if (who.body == body) teamDropdown.SetValueWithoutNotify((int) team);
		}

		public void SetXp()
		{
			SetCurrency(CurrencyType.Experience,  Mathf.RoundToInt(xpSlider.value).ToString()); 
			
			UpdateLevelValues();

		}
		
		private void OnTeamLevelUp(TeamIndex obj)
		{
			xpSlider.value = TeamManager.instance.GetTeamCurrentLevelExperience((TeamIndex) teamDropdown.value);
			LevelLabel.text = $"Lv : {TeamManager.instance.GetTeamLevel((TeamIndex) teamDropdown.value)}";
		}
		
		private void UpdateLevelValues()
		{
			LevelLabel.text = $"Lv : {TeamManager.instance.GetTeamLevel((TeamIndex) teamDropdown.value)}";
			xpSlider.minValue = TeamManager.instance.GetTeamCurrentLevelExperience(((TeamIndex) teamDropdown.value));
			xpSlider.maxValue = TeamManager.instance.GetTeamNextLevelExperience((TeamIndex)teamDropdown.value);
			
		}

		public void TeamChanged(int team)
		{
			var index = (TeamIndex) team;
			new TeamSwitchMessage(master, index).SendToServer();
			UpdateLevelValues();
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

		
		
		public void ApplyMobSpawns()
		{
			new ToggleMobSpawnsMessage(disableMobSpawnToggle.isOn).SendToEveryone();
		}
		public void KillAllMobs()
		{
			new KillAllTeamMessage(TeamIndex.Player).SendToServer();
		}
		
		public void SwapViewState()
		{
			SwapViewState(ViewState.Main);
		}
	}
}