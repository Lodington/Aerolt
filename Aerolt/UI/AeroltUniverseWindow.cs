using System.Collections.Generic;
using System.Linq;
using Aerolt.UI.Sections;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib;
using UniverseLib.UI;
using UniverseLib.UI.Models;
using UniverseLib.UI.Panels;

namespace Aerolt.UI
{
    public class AeroltUniverseWindow : PanelBase
    {
        private readonly List<IAeroltSection> _sections = new();
        private readonly Dictionary<IAeroltSection, ButtonRef> _navButtons = new();
        private readonly Dictionary<RoR2.NetworkUser, ButtonRef> _playerButtons = new();
        private GameObject _playerBar = null!;
        private GameObject _contentArea = null!;
        private IAeroltSection? _active;
        private float _nextPlayerRefresh;
        private Sections.SettingsSection? _settings;

        public AeroltUniverseWindow(UIBase owner) : base(owner)
        {
        }

        public override string Name => "Aerolt";

        public override int MinWidth => 650;
        public override int MinHeight => 450;
        public override Vector2 DefaultAnchorMin => new(0.25f, 0.25f);
        public override Vector2 DefaultAnchorMax => new(0.75f, 0.75f);
        public override Vector2 DefaultPosition => new(-325f, 225f);

        protected override void ConstructPanelContent()
        {
            _playerBar = UIFactory.CreateHorizontalGroup(ContentRoot, "PlayerBar", false, false, true, true, 4,
                new Vector4(4, 4, 4, 4), new Color(0.1f, 0.1f, 0.1f));
            UIFactory.SetLayoutElement(_playerBar, minHeight: 30, flexibleHeight: 0, flexibleWidth: 9999);
            var playerLabel = UIFactory.CreateLabel(_playerBar, "PlayerLabel", "Target:", TextAnchor.MiddleLeft);
            UIFactory.SetLayoutElement(playerLabel.gameObject, minWidth: 55, flexibleWidth: 0);
            RefreshPlayerButtons();

            var body = UIFactory.CreateHorizontalGroup(ContentRoot, "Body", false, true, true, true, 4,
                new Vector4(4, 4, 4, 4), new Color(0.12f, 0.12f, 0.12f));
            UIFactory.SetLayoutElement(body, flexibleWidth: 9999, flexibleHeight: 9999);

            var nav = UIFactory.CreateVerticalGroup(body, "Nav", false, false, true, true, 4,
                new Vector4(4, 4, 4, 4), new Color(0.08f, 0.08f, 0.08f));
            UIFactory.SetLayoutElement(nav, minWidth: 140, flexibleWidth: 0, flexibleHeight: 9999);

            _contentArea = UIFactory.CreateVerticalGroup(body, "Content", true, true, true, true, 4,
                new Vector4(6, 6, 6, 6), new Color(0.14f, 0.14f, 0.14f));
            UIFactory.SetLayoutElement(_contentArea, flexibleWidth: 9999, flexibleHeight: 9999);

            RegisterSection(new PlayerSection(), nav);
            RegisterSection(new BodyStatsSection(), nav);
            RegisterSection(new ItemsSection(), nav);
            RegisterSection(new EquipmentSection(), nav);
            RegisterSection(new BuffsSection(), nav);
            RegisterSection(new BodySection(), nav);
            RegisterSection(new MonsterSection(), nav);
            RegisterSection(new InteractableSection(), nav);
            RegisterSection(new TeleporterSection(), nav);
            RegisterSection(new WorldSection(), nav);
            RegisterSection(new EspSection(), nav);
            _settings = new SettingsSection();
            RegisterSection(_settings, nav);

            if (_sections.Count > 0)
                SetActiveSection(_sections[0]);
        }

        private void RegisterSection(IAeroltSection section, GameObject nav)
        {
            _sections.Add(section);

            var btn = UIFactory.CreateButton(nav, $"Nav_{section.Title}", section.Title);
            UIFactory.SetLayoutElement(btn.Component.gameObject, minHeight: 30, flexibleWidth: 9999);
            btn.OnClick += () => SetActiveSection(section);
            _navButtons[section] = btn;

            section.Build(_contentArea);
            section.Root.SetActive(false);
        }

        public override void Update()
        {
            base.Update();

            _settings?.TickRebind();

            if (Time.unscaledTime < _nextPlayerRefresh) return;
            _nextPlayerRefresh = Time.unscaledTime + 1f;
            RefreshPlayerButtons();
        }

        private void RefreshPlayerButtons()
        {
            var players = PlayerSelection.GetPlayers();

            var stale = new List<RoR2.NetworkUser>();
            foreach (var kvp in _playerButtons)
                if (!kvp.Key || !players.Contains(kvp.Key))
                    stale.Add(kvp.Key);
            foreach (var user in stale)
            {
                if (_playerButtons[user]?.Component)
                    Object.Destroy(_playerButtons[user].Component.gameObject);
                _playerButtons.Remove(user);
            }

            foreach (var user in players)
            {
                if (!user || _playerButtons.ContainsKey(user)) continue;
                var captured = user;
                var btn = UIFactory.CreateButton(_playerBar, $"Player_{user.id}", GetPlayerName(user));
                UIFactory.SetLayoutElement(btn.Component.gameObject, minHeight: 26, minWidth: 90, flexibleWidth: 9999);
                btn.OnClick += () => PlayerSelection.Select(captured);
                _playerButtons[user] = btn;
            }

            foreach (var kvp in _playerButtons)
            {
                if (!kvp.Key || kvp.Value?.Component == null) continue;
                kvp.Value.ButtonText.text = GetPlayerName(kvp.Key);
                var isSelected = kvp.Key == PlayerSelection.Selected;
                RuntimeHelper.SetColorBlock(kvp.Value.Component,
                    isSelected ? new Color(0.3f, 0.45f, 0.32f) : new Color(0.2f, 0.2f, 0.2f));
            }
        }

        private static string GetPlayerName(RoR2.NetworkUser user)
        {
            return user && !string.IsNullOrEmpty(user.userName) ? user.userName : "Player";
        }

        private void SetActiveSection(IAeroltSection section)
        {
            if (_active == section) return;

            if (_active != null)
                _active.Root.SetActive(false);

            _active = section;
            section.Root.SetActive(true);
            section.OnShown();

            foreach (var kvp in _navButtons)
            {
                var isActive = kvp.Key == section;
                RuntimeHelper.SetColorBlock(kvp.Value.Component,
                    isActive ? new Color(0.3f, 0.45f, 0.32f) : new Color(0.2f, 0.2f, 0.2f));
            }
        }
    }
}
