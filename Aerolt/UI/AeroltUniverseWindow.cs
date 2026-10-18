using System.Collections.Generic;
using System.Linq;
using Aerolt.API;
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
        private const int PlayerButtonWidth = 100;
        private const int PlayerButtonHeight = 26;

        private readonly List<IAeroltSection> _sections = new();
        private readonly Dictionary<IAeroltSection, ButtonRef> _navButtons = new();
        private readonly Dictionary<RoR2.NetworkUser, ButtonRef> _playerButtons = new();
        private GameObject _playerBar = null!;
        private GameObject _nav = null!;
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
            var selector = UIFactory.CreateVerticalGroup(ContentRoot, "PlayerSelector", false, false, true, true, 2,
                new Vector4(4, 2, 4, 2), new Color(0.1f, 0.1f, 0.1f));
            UIFactory.SetLayoutElement(selector, minHeight: 32, flexibleHeight: 0, flexibleWidth: 9999);

            var playerLabel = UIFactory.CreateLabel(selector, "PlayerLabel", "Target:", TextAnchor.MiddleLeft);
            UIFactory.SetLayoutElement(playerLabel.gameObject, minHeight: 18, flexibleWidth: 9999);


            _playerBar = UIFactory.CreateUIObject("PlayerGrid", selector);
            var grid = _playerBar.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(PlayerButtonWidth, PlayerButtonHeight);
            grid.spacing = new Vector2(4, 4);
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.childAlignment = TextAnchor.UpperLeft;
            grid.constraint = GridLayoutGroup.Constraint.Flexible;
            var gridFitter = _playerBar.AddComponent<ContentSizeFitter>();
            gridFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            UIFactory.SetLayoutElement(_playerBar, minHeight: PlayerButtonHeight, flexibleWidth: 9999);
            RefreshPlayerButtons();

            var body = UIFactory.CreateHorizontalGroup(ContentRoot, "Body", false, true, true, true, 4,
                new Vector4(4, 4, 4, 4), new Color(0.12f, 0.12f, 0.12f));
            UIFactory.SetLayoutElement(body, flexibleWidth: 9999, flexibleHeight: 9999);

  
            var navScroll = UIFactory.CreateScrollView(body, "NavScroll", out _nav, out _,
                new Color(0.08f, 0.08f, 0.08f));
            UIFactory.SetLayoutElement(navScroll, minWidth: 150, flexibleWidth: 0, flexibleHeight: 9999);
            UIFactory.SetLayoutGroup<VerticalLayoutGroup>(_nav, false, false, true, true, 4, 4, 4, 4, 4);

            _contentArea = UIFactory.CreateVerticalGroup(body, "Content", true, true, true, true, 4,
                new Vector4(6, 6, 6, 6), new Color(0.14f, 0.14f, 0.14f));
            UIFactory.SetLayoutElement(_contentArea, flexibleWidth: 9999, flexibleHeight: 9999);

            RegisterSection(new PlayerSection());
            RegisterSection(new BodyStatsSection());
            RegisterSection(new ItemsSection());
            RegisterSection(new EquipmentSection());
            RegisterSection(new BuffsSection());
            RegisterSection(new BodySection());
            RegisterSection(new MonsterSection());
            RegisterSection(new InteractableSection());
            RegisterSection(new TeleporterSection());
            RegisterSection(new WorldSection());
            RegisterSection(new EspSection());
            _settings = new SettingsSection();
            RegisterSection(_settings);

            AeroltAPI.FlushPendingSections(this);

            if (_sections.Count > 0)
                SetActiveSection(_sections[0]);
        }

        public void RegisterSection(IAeroltSection section)
        {
            if (section == null || _sections.Contains(section)) return;

            _sections.Add(section);

            var btn = UIFactory.CreateButton(_nav, $"Nav_{section.Title}", section.Title);
            UIFactory.SetLayoutElement(btn.Component.gameObject, minHeight: 30, flexibleWidth: 9999);
            btn.OnClick += () => SetActiveSection(section);
            _navButtons[section] = btn;


            if (section.ScrollableContent)
            {
                var scroll = UIFactory.CreateScrollView(_contentArea, $"Scroll_{section.Title}",
                    out var scrollContent, out _, new Color(0.14f, 0.14f, 0.14f));
                UIFactory.SetLayoutElement(scroll, flexibleWidth: 9999, flexibleHeight: 9999);
                section.Build(scrollContent);
            }
            else
            {
                section.Build(_contentArea);
            }

            section.Root.SetActive(false);

            // If this is the only section (e.g. everything else failed), show it.
            if (_active == null)
                SetActiveSection(section);
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
                    isActive ? new Color(0.3f, 0.45f, 0.32f) : new Color(0.2f, 0.2f, 0.2f)); //This is fucked
            }
        }
    }
}
