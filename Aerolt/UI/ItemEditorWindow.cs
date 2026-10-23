using System;
using System.Collections.Generic;
using Aerolt.Classes;
using Aerolt.Messages;
using RoR2;
using RoR2.ContentManagement;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib.UI;
using UniverseLib.UI.Panels;

namespace Aerolt.UI
{

    public interface IItemEditTarget
    {
 
        string RightLabel { get; }
        int GetCount(ItemDef def);
        void Adjust(ItemDef def, int amount);
    }

    public class PlayerInventoryTarget : IItemEditTarget
    {
        public string RightLabel => "Player Inventory";

        public int GetCount(ItemDef def)
        {
            var inv = Inv();
            return inv ? inv!.GetItemCountEffective(def) : 0;
        }

        public void Adjust(ItemDef def, int amount)
        {
            var inv = Inv();
            if (!inv || amount == 0) return;
            var current = inv!.GetItemCountEffective(def);
            var target = Mathf.Max(0, current + amount);
            if (target == current) return;
            new SetItemCountMessage(inv, new Dictionary<ItemDef, int> { { def, target } }).SendToServer();
        }

        private static Inventory? Inv()
        {
            var user = PlayerSelection.Selected;
            if (!user || !user!.master) return null;
            return user.master.inventory ? user.master.inventory : null;
        }
    }

    public class DictionaryItemTarget : IItemEditTarget
    {
        private readonly Dictionary<ItemDef, int> _items;

        public DictionaryItemTarget(Dictionary<ItemDef, int> items, string rightLabel)
        {
            _items = items;
            RightLabel = rightLabel;
        }

        public string RightLabel { get; }

        public int GetCount(ItemDef def)
        {
            return _items.TryGetValue(def, out var c) ? c : 0;
        }

        public void Adjust(ItemDef def, int amount)
        {
            if (amount == 0) return;
            var target = Mathf.Max(0, GetCount(def) + amount);
            if (target == 0) _items.Remove(def);
            else _items[def] = target;
        }
    }

    public class ItemEditorWindow : PanelBase
    {
        private IconGrid<ItemDef> _catalogGrid = null!;
        private IconGrid<ItemDef> _inventoryGrid = null!;
        private Text _rightLabel = null!;
        private IItemEditTarget _target = new PlayerInventoryTarget();

        public ItemEditorWindow(UIBase owner) : base(owner)
        {
        }

        public override string Name => "Aerolt - Edit Items";

        public override int MinWidth => 760;
        public override int MinHeight => 500;
        public override Vector2 DefaultAnchorMin => new(0.2f, 0.2f);
        public override Vector2 DefaultAnchorMax => new(0.8f, 0.8f);
        public override Vector2 DefaultPosition => new(-380f, 250f);

        protected override void OnClosePanelClicked()
        {
            SetActive(false);
        }

        public void SetTarget(IItemEditTarget target)
        {
            _target = target;
            if (_rightLabel) _rightLabel.text = target.RightLabel;
            RefreshGrids();
        }

        protected override void ConstructPanelContent()
        {
            var hint = UIFactory.CreateLabel(ContentRoot, "Hint",
                "Left grid: click to add (Shift +5, right-click to remove).  " +
                "Right grid: current items, click to remove (Shift -5, right-click to add).",
                TextAnchor.MiddleLeft);
            UIFactory.SetLayoutElement(hint.gameObject, minHeight: 20, flexibleWidth: 9999);
            hint.fontSize = 12;

            var body = UIFactory.CreateHorizontalGroup(ContentRoot, "Body", false, true, true, true, 6,
                new Vector4(4, 4, 4, 4), new Color(0.12f, 0.12f, 0.12f));
            UIFactory.SetLayoutElement(body, flexibleWidth: 9999, flexibleHeight: 9999);

            var left = UIFactory.CreateVerticalGroup(body, "CatalogSide", false, false, true, true, 2,
                new Vector4(2, 2, 2, 2), new Color(0.14f, 0.14f, 0.14f));
            UIFactory.SetLayoutElement(left, flexibleWidth: 9999, flexibleHeight: 9999);
            var leftLabel = UIFactory.CreateLabel(left, "CatalogLabel", "All Items", TextAnchor.MiddleCenter);
            UIFactory.SetLayoutElement(leftLabel.gameObject, minHeight: 22, flexibleWidth: 9999);

            _catalogGrid = new IconGrid<ItemDef>(
                def => Language.GetString(def.nameToken),
                (def, icon, stillValid) => IconCache.LoadItemIcon(def, sprite =>
                {
                    if (sprite && icon && stillValid()) icon.sprite = sprite;
                }),
                onClick: (def, amount) => Adjust(def, amount),
                getCount: def => _target.GetCount(def));
            _catalogGrid.Build(left, ContentManager._itemDefs);

            var right = UIFactory.CreateVerticalGroup(body, "InventorySide", false, false, true, true, 2,
                new Vector4(2, 2, 2, 2), new Color(0.14f, 0.14f, 0.14f));
            UIFactory.SetLayoutElement(right, flexibleWidth: 9999, flexibleHeight: 9999);
            _rightLabel = UIFactory.CreateLabel(right, "InventoryLabel", _target.RightLabel, TextAnchor.MiddleCenter);
            UIFactory.SetLayoutElement(_rightLabel.gameObject, minHeight: 22, flexibleWidth: 9999);

            _inventoryGrid = new IconGrid<ItemDef>(
                def => Language.GetString(def.nameToken),
                (def, icon, stillValid) => IconCache.LoadItemIcon(def, sprite =>
                {
                    if (sprite && icon && stillValid()) icon.sprite = sprite;
                }),
                onClick: (def, amount) => Adjust(def, -amount),
                getCount: def => _target.GetCount(def),
                onlyOwned: true);
            _inventoryGrid.Build(right, ContentManager._itemDefs);
        }

        public void RefreshGrids()
        {
            _catalogGrid?.Refresh();
            _inventoryGrid?.Refresh();
        }

        private void Adjust(ItemDef def, int amount)
        {
            _target.Adjust(def, amount);
            RefreshGrids();
        }
    }
}
