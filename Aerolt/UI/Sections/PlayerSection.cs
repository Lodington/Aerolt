using System;
using Aerolt.Classes;
using Aerolt.Enums;
using Aerolt.Managers;
using Aerolt.Messages;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UniverseLib.UI;
using UniverseLib.UI.Models;

namespace Aerolt.UI.Sections
{
    public class PlayerSection : IAeroltSection
    {
        private Toggle _god = null!;
        private Toggle _noclip = null!;
        private Toggle _aimbot = null!;
        private Toggle _infiniteSkills = null!;
        private Toggle _alwaysSprint = null!;
        private Slider _aimbotWeight = null!;
        private Toggle _disableMobSpawns = null!;
        private Dropdown _teamDropdown = null!;
        private ValueWrapper<bool> _disableMobSpawnsWrapper = null!;

        private bool _suppress; // prevents feedback loops while syncing UI from config

        public string Title => "Player";
        public GameObject Root { get; private set; } = null!;

        public void Build(GameObject contentParent)
        {
            Root = UIFactory.CreateVerticalGroup(contentParent, "PlayerSection", false, false, true, true, 4,
                default, new Color(0.14f, 0.14f, 0.14f));
            UIFactory.SetLayoutElement(Root, flexibleWidth: 9999, flexibleHeight: 9999);

            _god = AddToggle("God Mode", on => Apply(b => b.GodMode.Value = on));
            _noclip = AddToggle("Noclip", on => Apply(b => b.Noclip.Value = on));
            _aimbot = AddToggle("Aimbot", on => Apply(b => b.Aimbot.Value = on));
            _infiniteSkills = AddToggle("Infinite Skills", on => Apply(b => b.InfiniteSkills.Value = on));
            _alwaysSprint = AddToggle("Always Sprint", on => Apply(b => b.AlwaysSprint.Value = on));

            // Aimbot weight slider.
            var sliderRow = UIFactory.CreateHorizontalGroup(Root, "AimbotWeightRow", false, false, true, true, 4);
            UIFactory.SetLayoutElement(sliderRow, minHeight: 28, flexibleWidth: 9999);
            var sliderLabel = UIFactory.CreateLabel(sliderRow, "AimbotWeightLabel", "Aimbot Weight",
                TextAnchor.MiddleLeft);
            UIFactory.SetLayoutElement(sliderLabel.gameObject, minWidth: 110, flexibleWidth: 0);
            UIFactory.CreateSlider(sliderRow, "AimbotWeightSlider", out _aimbotWeight);
            UIFactory.SetLayoutElement(_aimbotWeight.gameObject, minHeight: 25, minWidth: 150, flexibleWidth: 9999);
            _aimbotWeight.minValue = 0f;
            _aimbotWeight.maxValue = 1f;
            _aimbotWeight.onValueChanged.AddListener(val => Apply(b => b.AimbotWeight.Value = val));

            _disableMobSpawnsWrapper = ValueWrapper.Get("PlayerMenu", "DisableMobSpawns", false, "");
            _disableMobSpawnsWrapper.settingChanged += OnMobSpawnsChanged;
            _disableMobSpawns = AddToggle("Disable Mob Spawns", on => _disableMobSpawnsWrapper.Value = on);

            // Action buttons.
            var actions = UIFactory.CreateHorizontalGroup(Root, "Actions", false, false, true, true, 4);
            UIFactory.SetLayoutElement(actions, minHeight: 30, flexibleHeight: 0, flexibleWidth: 9999);
            AddButton(actions, "Kill", Kill);
            AddButton(actions, "Revive", Revive);
            AddButton(actions, "Goto", Goto);
            AddButton(actions, "Bring", Bring);
            AddButton(actions, "Kick", Kick);
            AddButton(actions, "Ban", Ban);

            // Currency setters.
            AddCurrencyRow("Money", CurrencyType.Money);
            AddCurrencyRow("Lunar Coins", CurrencyType.Lunar);
            AddCurrencyRow("Void Coins", CurrencyType.Void);
            AddCurrencyRow("Level", CurrencyType.Level);

            // Team switch.
            var teamRow = UIFactory.CreateHorizontalGroup(Root, "TeamRow", false, false, true, true, 4);
            UIFactory.SetLayoutElement(teamRow, minHeight: 30, flexibleHeight: 0, flexibleWidth: 9999);
            var teamLabel = UIFactory.CreateLabel(teamRow, "TeamLabel", "Team", TextAnchor.MiddleLeft);
            UIFactory.SetLayoutElement(teamLabel.gameObject, minWidth: 110, flexibleWidth: 0);
            var teamNames = Enum.GetNames(typeof(TeamIndex));
            UIFactory.CreateDropdown(teamRow, "TeamDropdown", out _teamDropdown, "Team", 14, OnTeamChanged, teamNames);
            UIFactory.SetLayoutElement(_teamDropdown.gameObject, minHeight: 28, flexibleWidth: 9999);

            PlayerSelection.SelectionChanged += SyncFromConfig;
        }

        public void OnShown()
        {
            SyncFromConfig();
        }

        // --- builders ---

        private Toggle AddToggle(string label, UnityEngine.Events.UnityAction<bool> onChanged)
        {
            var obj = UIFactory.CreateToggle(Root, $"Toggle_{label}", out var toggle, out var text);
            UIFactory.SetLayoutElement(obj, minHeight: 28, flexibleWidth: 9999);
            text.text = label;
            toggle.onValueChanged.AddListener(on =>
            {
                if (_suppress) return;
                onChanged(on);
            });
            return toggle;
        }

        private static void AddButton(GameObject parent, string label, Action onClick)
        {
            var btn = UIFactory.CreateButton(parent, $"Btn_{label}", label);
            UIFactory.SetLayoutElement(btn.Component.gameObject, minHeight: 28, flexibleWidth: 9999);
            btn.OnClick += onClick;
        }

        private void AddCurrencyRow(string label, CurrencyType type)
        {
            var row = UIFactory.CreateHorizontalGroup(Root, $"Currency_{label}", false, false, true, true, 4);
            UIFactory.SetLayoutElement(row, minHeight: 30, flexibleHeight: 0, flexibleWidth: 9999);
            var lbl = UIFactory.CreateLabel(row, "Label", label, TextAnchor.MiddleLeft);
            UIFactory.SetLayoutElement(lbl.gameObject, minWidth: 110, flexibleWidth: 0);

            InputFieldRef input = UIFactory.CreateInputField(row, "Input", "amount...");
            UIFactory.SetLayoutElement(input.Component.gameObject, minHeight: 28, flexibleWidth: 9999);

            var set = UIFactory.CreateButton(row, "Set", "Set");
            UIFactory.SetLayoutElement(set.Component.gameObject, minHeight: 28, minWidth: 50, flexibleWidth: 0);
            set.OnClick += () =>
            {
                SetCurrency(type, input.Text);
                input.Text = "";
            };
        }

        private void Apply(Action<PlayerConfigBinding> action)
        {
            var binding = PlayerSelection.SelectedBinding;
            if (binding != null) action(binding);
        }

        private static CharacterMaster? GetMaster()
        {
            var user = PlayerSelection.Selected;
            return user && user!.master ? user.master : null;
        }

        private static CharacterBody? LocalBody()
        {
            var localUser = LocalUserManager.GetFirstLocalUser();
            return localUser?.cachedBody;
        }

        private void Kill()
        {
            var master = GetMaster();
            if (master) new KillMessage(master!).SendToServer();
        }

        private void Revive()
        {
            var user = PlayerSelection.Selected;
            if (!user) return;
            var message = new SetBodyMessage(user!);
            if (!NetworkServer.active) message.Handle();
            message.SendToServer();
        }

        private void Goto()
        {
            var localBody = LocalBody();
            var master = GetMaster();
            var targetBody = master ? master!.GetBody() : null;
            if (localBody && targetBody)
                new TeleportMessage(localBody!, targetBody!.transform.position).SendToAuthority(localBody!);
        }

        private void Bring()
        {
            var localBody = LocalBody();
            var master = GetMaster();
            var targetBody = master ? master!.GetBody() : null;
            if (localBody && targetBody)
                new TeleportMessage(targetBody!, localBody!.transform.position).SendToAuthority(targetBody!);
        }

        private void Kick()
        {
            var user = PlayerSelection.Selected;
            if (user) new KickBanMessage(user!, true).SendToServer();
        }

        private void Ban()
        {
            var user = PlayerSelection.Selected;
            if (user) new KickBanMessage(user!, ban: true).SendToServer();
        }

        private void SetCurrency(CurrencyType type, string strAmount)
        {
            var master = GetMaster();
            if (!master) return;
            if (!uint.TryParse(strAmount, out var amount)) return;
            new CurrencyMessage(master!, type, amount).SendToServer();
        }

        private void OnTeamChanged(int team)
        {
            if (_suppress) return;
            var master = GetMaster();
            if (!master) return;
            new TeamSwitchMessage(master!, (TeamIndex)team).SendToServer();
        }

        private void OnMobSpawnsChanged()
        {
            _suppress = true;
            if (_disableMobSpawns) _disableMobSpawns.isOn = _disableMobSpawnsWrapper.Value;
            _suppress = false;

            if (!NetworkServer.active) return;
            foreach (var director in CombatDirector.instancesList)
                director.monsterSpawnTimer =
                    _disableMobSpawnsWrapper.Value ? float.PositiveInfinity : 0f;
        }

        private void SyncFromConfig()
        {
            var binding = PlayerSelection.SelectedBinding;

            _suppress = true;
            if (binding != null)
            {
                _god.isOn = binding.GodMode.Value;
                _noclip.isOn = binding.Noclip.Value;
                _aimbot.isOn = binding.Aimbot.Value;
                _infiniteSkills.isOn = binding.InfiniteSkills.Value;
                _alwaysSprint.isOn = binding.AlwaysSprint.Value;
                _aimbotWeight.value = binding.AimbotWeight.Value;
            }

            _disableMobSpawns.isOn = _disableMobSpawnsWrapper.Value;

            var master = GetMaster();
            var body = master ? master!.GetBody() : null;
            if (body && body!.teamComponent)
                _teamDropdown.value = (int)body.teamComponent.teamIndex;
            _suppress = false;
        }
    }
}
