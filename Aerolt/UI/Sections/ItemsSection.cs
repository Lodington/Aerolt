using System.Collections.Generic;
using System.Linq;
using Aerolt.Classes;
using Aerolt.Managers;
using Aerolt.Messages;
using RoR2;
using RoR2.ContentManagement;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib.UI;

namespace Aerolt.UI.Sections
{
    public class ItemsSection : IAeroltSection
    {
        private CountEditorList<ItemDef> _list = null!;

        public string Title => "Items";
        public GameObject Root { get; private set; } = null!;

        public void Build(GameObject contentParent)
        {
            Root = UIFactory.CreateVerticalGroup(contentParent, "ItemsSection", false, false, true, true, 4,
                default, new Color(0.14f, 0.14f, 0.14f));
            UIFactory.SetLayoutElement(Root, flexibleWidth: 9999, flexibleHeight: 9999);

            var actions = UIFactory.CreateHorizontalGroup(Root, "Actions", false, false, true, true, 4);
            UIFactory.SetLayoutElement(actions, minHeight: 30, flexibleHeight: 0, flexibleWidth: 9999);
            AddAction(actions, "Give", GiveItems);
            AddAction(actions, "Give All", GiveAllItems);
            AddAction(actions, "Give Random", GiveRandomItems);
            AddAction(actions, "Clear", ClearInventory);

            _list = new CountEditorList<ItemDef>(
                def => Language.GetString(def.nameToken),
                (entry, icon, stillValid) =>
                {
                    IconCache.LoadItemIcon(entry.Def, sprite =>
                    {
                        if (sprite && icon && stillValid()) icon.sprite = sprite;
                    });
                });
            _list.Build(Root, ContentManager._itemDefs);
        }

        public void OnShown()
        {
            _list.RefreshIcons();
            SeedFromInventory();
        }

        private static void AddAction(GameObject parent, string label, System.Action onClick)
        {
            var btn = UIFactory.CreateButton(parent, $"Action_{label}", label);
            UIFactory.SetLayoutElement(btn.Component.gameObject, minHeight: 28, flexibleWidth: 9999);
            btn.OnClick += onClick;
        }

        private void SeedFromInventory()
        {
            var master = PlayerSelection.Selected ? PlayerSelection.Selected!.master : null;
            var inv = master ? master!.inventory : null;
            if (!inv) return;
            foreach (var def in ContentManager._itemDefs)
                _list.SetCount(def, inv!.GetItemCountEffective(def));
        }

        private void GiveItems()
        {
            var master = GetMaster();
            if (!master) return;
            var counts = _list.Entries
                .Where(kvp => kvp.Value.Count > 0)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Count);
            if (counts.Count == 0) return;
            new SetItemCountMessage(master!.inventory, counts).SendToServer();
        }

        private void GiveAllItems()
        {
            var master = GetMaster();
            if (!master) return;
            var items = ContentManager.itemDefs.Except(ItemHelpers.GiveAllFilteredItems)
                .ToDictionary(x => x, def => master!.inventory.GetItemCountEffective(def) + 1);
            new SetItemCountMessage(master!.inventory, items).SendToServer();
        }

        private void GiveRandomItems()
        {
            var master = GetMaster();
            if (!master) return;
            var items = new Dictionary<ItemDef, int>();
            for (var i = 0; i < Random.Range(0, 100); i++)
            {
                var pickup = ItemHelpers.Chest1DropTable.GeneratePickup(RoR2Application.rng).pickupIndex;
                var item = ItemCatalog.GetItemDef(PickupCatalog.GetPickupDef(pickup)?.itemIndex ?? ItemIndex.None);
                if (item == null) continue;
                if (!items.ContainsKey(item)) items[item] = master!.inventory.GetItemCountEffective(item);
                items[item] += Random.Range(0, 100);
            }

            new SetItemCountMessage(master!.inventory, items).SendToServer();
        }

        private void ClearInventory()
        {
            var master = GetMaster();
            if (!master) return;
            var items = ContentManager.itemDefs.ToDictionary(x => x, _ => 0);
            new SetItemCountMessage(master!.inventory, items).SendToServer();
            foreach (var def in ContentManager._itemDefs) _list.SetCount(def, 0);
        }

        private static CharacterMaster? GetMaster()
        {
            var user = PlayerSelection.Selected;
            return user && user!.master ? user.master : null;
        }
    }
}
