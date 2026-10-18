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
        private CountEditorList<BuffDef> _list = null!;

        public string Title => "Buffs";
        public GameObject Root { get; private set; } = null!;

        public void Build(GameObject contentParent)
        {
            Root = UIFactory.CreateVerticalGroup(contentParent, "BuffsSection", false, false, true, true, 4,
                default, new Color(0.14f, 0.14f, 0.14f));
            UIFactory.SetLayoutElement(Root, flexibleWidth: 9999, flexibleHeight: 9999);

            var actions = UIFactory.CreateHorizontalGroup(Root, "Actions", false, false, true, true, 4);
            UIFactory.SetLayoutElement(actions, minHeight: 30, flexibleHeight: 0, flexibleWidth: 9999);
            var give = UIFactory.CreateButton(actions, "GiveBuffs", "Give");
            UIFactory.SetLayoutElement(give.Component.gameObject, minHeight: 28, flexibleWidth: 9999);
            give.OnClick += GiveBuffs;

            _list = new CountEditorList<BuffDef>(
                def => def.name,
                (entry, icon, _) =>
                {
                    if (entry.Def.iconSprite) icon.sprite = entry.Def.iconSprite;
                    icon.color = entry.Def.buffColor;
                });
            _list.Build(Root, ContentManager.buffDefs.OrderBy(x => x.name));
        }

        public void OnShown()
        {
            _list.RefreshIcons();
            SeedFromBody();
        }

        private void SeedFromBody()
        {
            var body = GetBody();
            if (!body) return;
            foreach (var def in ContentManager.buffDefs)
                _list.SetCount(def, body!.GetBuffCount(def));
        }

        private void GiveBuffs()
        {
            var body = GetBody();
            if (!body) return;
            var counts = _list.Entries
                .ToDictionary(kvp => kvp.Key.buffIndex, kvp => (uint)kvp.Value.Count);
            new SetBuffCountMessage(body!, counts).SendToServer();
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
