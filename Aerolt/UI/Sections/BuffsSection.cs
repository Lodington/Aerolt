using System.Collections.Generic;
using System.Linq;
using Aerolt.Messages;
using RoR2;
using RoR2.ContentManagement;
using UnityEngine;
using UniverseLib.UI;

namespace Aerolt.UI.Sections
{
    public class BuffsSection : IAeroltSection
    {
        private IconGrid<BuffDef> _grid = null!;

        public string Title => "Buffs";
        public GameObject Root { get; private set; } = null!;
        public bool ScrollableContent => false;

        public void Build(GameObject contentParent)
        {
            Root = UIFactory.CreateVerticalGroup(contentParent, "BuffsSection", false, false, true, true, 4,
                default, new Color(0.14f, 0.14f, 0.14f));
            UIFactory.SetLayoutElement(Root, flexibleWidth: 9999, flexibleHeight: 9999);

            var hint = UIFactory.CreateLabel(Root, "Hint",
                "Click a buff to add one (Shift +5, right-click to remove).", TextAnchor.MiddleLeft);
            UIFactory.SetLayoutElement(hint.gameObject, minHeight: 18, flexibleWidth: 9999);
            hint.fontSize = 12;

            _grid = new IconGrid<BuffDef>(
                def => def.name,
                (def, icon, _) =>
                {
                    if (def.iconSprite) icon.sprite = def.iconSprite;
                },
                onClick: Adjust,
                getCount: GetCount,
                tint: def => def.buffColor);

            _grid.Build(Root, ContentManager.buffDefs.Where(x => x).OrderBy(x => x.name));
        }

        public void OnShown()
        {
            _grid.Refresh();
        }

        private static int GetCount(BuffDef def)
        {
            var body = GetBody();
            return body ? body!.GetBuffCount(def) : 0;
        }

        private void Adjust(BuffDef def, int amount)
        {
            var body = GetBody();
            if (!body || amount == 0) return;

            var current = body!.GetBuffCount(def);
            var target = Mathf.Max(0, current + amount);
            if (target == current) return;

            new SetBuffCountMessage(body, new Dictionary<BuffIndex, uint> { { def.buffIndex, (uint)target } })
                .SendToServer();
            _grid.Refresh();
        }

        private static CharacterBody? GetBody()
        {
            var user = PlayerSelection.Selected;
            if (!user || !user!.master) return null;
            var body = user.master.GetBody();
            return body ? body : null;
        }
    }
}
