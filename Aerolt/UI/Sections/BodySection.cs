using System.Collections.Generic;
using System.Linq;
using Aerolt.Classes;
using Aerolt.Messages;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UniverseLib;
using UniverseLib.UI;

namespace Aerolt.UI.Sections
{
    public class BodySection : IAeroltSection
    {
        private readonly Dictionary<CharacterBody, GameObject> _rows = new();
        private GameObject _listContent = null!;
        private bool _built;

        public string Title => "Spawn As";
        public GameObject Root { get; private set; } = null!;

        public void Build(GameObject contentParent)
        {
            Root = UIFactory.CreateVerticalGroup(contentParent, "BodySection", false, false, true, true, 4,
                default, new Color(0.14f, 0.14f, 0.14f));
            UIFactory.SetLayoutElement(Root, flexibleWidth: 9999, flexibleHeight: 9999);

            var search = UIFactory.CreateInputField(Root, "Search", "Search bodies...");
            UIFactory.SetLayoutElement(search.Component.gameObject, minHeight: 28, flexibleWidth: 9999);
            search.OnValueChanged += FilterUpdated;

            UIFactory.CreateScrollView(Root, "List", out _listContent, out _, new Color(0.1f, 0.1f, 0.1f));
            UIFactory.SetLayoutGroup<VerticalLayoutGroup>(_listContent, false, false, true, true, 2, 2, 2, 2, 2);

            RuntimeHelper.StartCoroutine(BuildRows());
        }

        public void OnShown()
        {
        }

        private System.Collections.IEnumerator BuildRows()
        {
            const int batchSize = 15;
            var count = 0;

            foreach (var body in BodyCatalog.allBodyPrefabBodyBodyComponents.OrderBy(x =>
                         Language.GetString(x.baseNameToken)))
            {
                if (!body) continue;

                var captured = body;
                var row = UIFactory.CreateHorizontalGroup(_listContent, "Row", false, false, true, true, 4,
                    new Vector4(2, 2, 2, 2));
                UIFactory.SetLayoutElement(row, minHeight: 30, flexibleWidth: 9999);

                var iconObj = UIFactory.CreateUIObject("Icon", row);
                UIFactory.SetLayoutElement(iconObj, minWidth: 24, minHeight: 24, preferredWidth: 24,
                    preferredHeight: 24, flexibleWidth: 0, flexibleHeight: 0);
                var icon = iconObj.AddComponent<Image>();
                icon.preserveAspect = true;
                var sprite = IconCache.GetBodyIcon(body);
                if (sprite) icon.sprite = sprite;

                var btn = UIFactory.CreateButton(row, "SpawnAs", Language.GetString(body.baseNameToken));
                UIFactory.SetLayoutElement(btn.Component.gameObject, minHeight: 26, flexibleWidth: 9999);
                btn.OnClick += () => SpawnAs(captured);

                _rows[body] = row;

                if (++count < batchSize) continue;
                count = 0;
                yield return null;
            }

            _built = true;
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

        private void FilterUpdated(string text)
        {
            if (!_built) return;
            var lower = text?.ToLowerInvariant() ?? "";
            foreach (var kvp in _rows)
            {
                var name = Language.GetString(kvp.Key.baseNameToken).ToLowerInvariant();
                kvp.Value.SetActive(string.IsNullOrEmpty(lower) || name.Contains(lower));
            }
        }
    }
}
