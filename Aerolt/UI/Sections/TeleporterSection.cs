using System.Collections.Generic;
using System.Linq;
using Aerolt.Classes;
using Aerolt.Helpers;
using Aerolt.Messages;
using BepInEx;
using RoR2;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib.UI;
using UniverseLib.UI.Models;

namespace Aerolt.UI.Sections
{
    public class TeleporterSection : IAeroltSection
    {
        private readonly Dictionary<SceneDef, GameObject> _sceneButtons = new();
        private bool _built;

        public string Title => "Teleporter";
        public GameObject Root { get; private set; } = null!;
        public bool ScrollableContent => false; 

        public void Build(GameObject contentParent)
        {
            Root = UIFactory.CreateVerticalGroup(contentParent, "TeleporterSection", false, false, true, true, 4,
                default, new Color(0.14f, 0.14f, 0.14f));
            UIFactory.SetLayoutElement(Root, flexibleWidth: 9999, flexibleHeight: 9999);

            var actions = UIFactory.CreateHorizontalGroup(Root, "Actions", false, false, true, true, 4);
            UIFactory.SetLayoutElement(actions, minHeight: 30, flexibleHeight: 0, flexibleWidth: 9999);
            AddAction(actions, "Charge TP", InstaTeleporter);
            AddAction(actions, "Skip Stage", SkipStage);
            AddAction(actions, "Add Mountain", AddMountain);

            var portals = UIFactory.CreateHorizontalGroup(Root, "Portals", false, false, true, true, 4);
            UIFactory.SetLayoutElement(portals, minHeight: 30, flexibleHeight: 0, flexibleWidth: 9999);
            AddAction(portals, "Gold", () => SpawnPortals("gold"));
            AddAction(portals, "Newt", () => SpawnPortals("newt"));
            AddAction(portals, "Blue", () => SpawnPortals("blue"));
            AddAction(portals, "Void", () => SpawnPortals("void"));
            AddAction(portals, "All", () => SpawnPortals("all"));

            var search = UIFactory.CreateInputField(Root, "SceneSearch", "Search scenes...");
            UIFactory.SetLayoutElement(search.Component.gameObject, minHeight: 28, flexibleWidth: 9999);
            search.OnValueChanged += FilterUpdated;

            var scroll = UIFactory.CreateScrollView(Root, "SceneList", out var listContent, out _,
                new Color(0.1f, 0.1f, 0.1f));
            UIFactory.SetLayoutElement(scroll, flexibleWidth: 9999, flexibleHeight: 9999);
            UIFactory.SetLayoutGroup<VerticalLayoutGroup>(listContent, false, false, true, true, 2, 2, 2, 2, 2);

            BuildSceneButtons(listContent);
            _built = true;
        }

        private void BuildSceneButtons(GameObject listContent)
        {
            foreach (var scene in SceneCatalog.allSceneDefs.OrderByDescending(x => x.sceneType))
            {
                if (!scene) continue;

                var label = !string.IsNullOrEmpty(scene.nameToken)
                    ? Language.GetString(scene.nameToken)
                    : scene.cachedName;

                var captured = scene;
                var btn = UIFactory.CreateButton(listContent, $"Scene_{scene.cachedName}", label);
                UIFactory.SetLayoutElement(btn.Component.gameObject, minHeight: 26, flexibleWidth: 9999);
                btn.OnClick += () => SetScene(captured);
                _sceneButtons[scene] = btn.Component.gameObject;
            }
        }

        public void OnShown()
        {
        }

        private static void AddAction(GameObject parent, string label, System.Action onClick)
        {
            var btn = UIFactory.CreateButton(parent, $"Action_{label}", label);
            UIFactory.SetLayoutElement(btn.Component.gameObject, minHeight: 28, flexibleWidth: 9999);
            btn.OnClick += onClick;
        }

        private static void SetScene(SceneDef scene)
        {
            new SceneChangeMessage(scene.sceneDefIndex).SendToServer();
        }

        private static void InstaTeleporter()
        {
            new TeleporterChargeMessage().SendToServer();
            Chat.AddMessage("<color=yellow>Charged teleporter</color>");
        }

        private static void SkipStage()
        {
            new SceneChangeMessage().SendToServer();
            Chat.AddMessage("<color=yellow>Skipping Stage</color>");
        }

        private static void AddMountain()
        {
            if (TeleporterInteraction.instance)
            {
                TeleporterInteraction.instance.AddShrineStack();
                Chat.AddMessage("<color=yellow>Added Mountain stack</color>");
            }
        }

        private static void SpawnPortals(string portal)
        {
            new PortalSpawnMessage(portal).SendToServer();
        }

        private void FilterUpdated(string text)
        {
            if (!_built) return;

            if (text.IsNullOrWhiteSpace())
            {
                foreach (var go in _sceneButtons.Values) go.SetActive(true);
                return;
            }

            foreach (var kvp in _sceneButtons)
            {
                var name = !string.IsNullOrEmpty(kvp.Key.nameToken)
                    ? Language.GetString(kvp.Key.nameToken)
                    : kvp.Key.cachedName;
                kvp.Value.SetActive(name.ToLowerInvariant().Contains(text.ToLowerInvariant()));
            }
        }
    }
}
