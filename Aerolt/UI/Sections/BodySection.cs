using System.Linq;
using Aerolt.Classes;
using Aerolt.Messages;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using UniverseLib.UI;

namespace Aerolt.UI.Sections
{
    public class BodySection : IAeroltSection
    {
        private IconGrid<CharacterBody> _grid = null!;

        public string Title => "Spawn As";
        public GameObject Root { get; private set; } = null!;
        public bool ScrollableContent => false;

        public void Build(GameObject contentParent)
        {
            Root = UIFactory.CreateVerticalGroup(contentParent, "BodySection", false, false, true, true, 4,
                default, new Color(0.14f, 0.14f, 0.14f));
            UIFactory.SetLayoutElement(Root, flexibleWidth: 9999, flexibleHeight: 9999);

            _grid = new IconGrid<CharacterBody>(
                body => Language.GetString(body.baseNameToken),
                (body, icon, stillValid) =>
                {
                    var sprite = IconCache.GetBodyIcon(body);
                    if (sprite && icon && stillValid()) icon.sprite = sprite;
                },
                onClick: (body, _) => SpawnAs(body));

            _grid.Build(Root, BodyCatalog.allBodyPrefabBodyBodyComponents
                .Where(x => x)
                .OrderBy(x => Language.GetString(x.baseNameToken)));
        }

        public void OnShown()
        {
        }

        private static void SpawnAs(CharacterBody body)
        {
            var user = PlayerSelection.Selected;
            if (!user) return;
            var prefabBody = BodyCatalog.FindBodyPrefab(body);
            if (!prefabBody) return;

            var message = new SetBodyMessage(user!, prefabBody.GetComponent<CharacterBody>());
            if (!NetworkServer.active) message.Handle();
            message.SendToServer();
        }
    }
}
