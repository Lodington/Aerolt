using System.Collections.Generic;
using System.Linq;
using Aerolt.Classes;
using Aerolt.Messages;
using RoR2;
using RoR2.ContentManagement;
using UnityEngine;
using UniverseLib.UI;

namespace Aerolt.UI.Sections
{

    public class ItemsSection : IAeroltSection
    {
        private ItemGrid _grid = null!;

        public string Title => "Items";
        public GameObject Root { get; private set; } = null!;
        public bool ScrollableContent => false; // the grid scrolls internally

        public void Build(GameObject contentParent)
        {
            Root = UIFactory.CreateVerticalGroup(contentParent, "ItemsSection", false, false, true, true, 4,
                default, new Color(0.14f, 0.14f, 0.14f));
            UIFactory.SetLayoutElement(Root, flexibleWidth: 9999, flexibleHeight: 9999);

            var actions = UIFactory.CreateHorizontalGroup(Root, "Actions", false, false, true, true, 4);
            UIFactory.SetLayoutElement(actions, minHeight: 30, flexibleHeight: 0, flexibleWidth: 9999);
            AddAction(actions, "Give All", GiveAllItems);
            AddAction(actions, "Give Random", GiveRandomItems);
            AddAction(actions, "Clear", ClearInventory);
            AddAction(actions, "Edit Items", () => AeroltUniverseUI.OpenItemEditor());

            var hint = UIFactory.CreateLabel(Root, "Hint",
                "Click an item to give one (Shift +5, right-click to remove).", TextAnchor.MiddleLeft);
            UIFactory.SetLayoutElement(hint.gameObject, minHeight: 18, flexibleWidth: 9999);
            hint.fontSize = 12;

            _grid = new ItemGrid(GetCount, Adjust);
            _grid.Build(Root, ContentManager._itemDefs);
        }

        public void OnShown()
        {
            _grid.Refresh();
        }

        private static void AddAction(GameObject parent, string label, System.Action onClick)
        {
            var btn = UIFactory.CreateButton(parent, $"Action_{label}", label);
            UIFactory.SetLayoutElement(btn.Component.gameObject, minHeight: 28, flexibleWidth: 9999);
            btn.OnClick += onClick;
        }

        private static int GetCount(ItemDef def)
        {
            var inv = GetInventory();
            return inv ? inv!.GetItemCountEffective(def) : 0;
        }

        private void Adjust(ItemDef def, int amount)
        {
            var inv = GetInventory();
            if (!inv || amount == 0) return;

            var current = inv!.GetItemCountEffective(def);
            var target = Mathf.Max(0, current + amount);
            if (target == current) return;

            new SetItemCountMessage(inv, new Dictionary<ItemDef, int> { { def, target } }).SendToServer();
            _grid.Refresh();
        }

        private void GiveAllItems()
        {
            var master = GetMaster();
            if (!master) return;
            var items = ContentManager.itemDefs.Except(ItemHelpers.GiveAllFilteredItems)
                .ToDictionary(x => x, def => master!.inventory.GetItemCountEffective(def) + 1);
            new SetItemCountMessage(master!.inventory, items).SendToServer();
            _grid.Refresh();
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
            _grid.Refresh();
        }

        private void ClearInventory()
        {
            var master = GetMaster();
            if (!master) return;
            var items = ContentManager.itemDefs.ToDictionary(x => x, _ => 0);
            new SetItemCountMessage(master!.inventory, items).SendToServer();
            _grid.Refresh();
        }

        private static Inventory? GetInventory()
        {
            var master = GetMaster();
            return master && master!.inventory ? master.inventory : null;
        }

        private static CharacterMaster? GetMaster()
        {
            var user = PlayerSelection.Selected;
            return user && user!.master ? user.master : null;
        }
    }
}
