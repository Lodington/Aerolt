using System.Linq;
using Aerolt.Classes;
using Aerolt.Messages;
using RoR2;
using UnityEngine;
using UniverseLib.UI;

namespace Aerolt.UI.Sections
{
    public class TeleporterSection : IAeroltSection
    {
        private IconGrid<SceneDef> _grid = null!;

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

            _grid = new IconGrid<SceneDef>(
                SceneName,
                (scene, icon, stillValid) =>
                {
                    var sprite = IconCache.GetSceneIcon(scene);
                    if (sprite && icon && stillValid()) icon.sprite = sprite;
                },
                onClick: (scene, _) => SetScene(scene));

            _grid.Build(Root, SceneCatalog.allSceneDefs
                .Where(x => x)
                .OrderByDescending(x => x.sceneType));
        }

        public void OnShown()
        {
        }

        private static string SceneName(SceneDef scene)
        {
            return !string.IsNullOrEmpty(scene.nameToken) ? Language.GetString(scene.nameToken) : scene.cachedName;
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
    }
}
