using System;
using System.Collections.Generic;
using System.Linq;
using Aerolt.Classes;
using RoR2;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UniverseLib.UI;

namespace Aerolt.UI
{


    public class ItemGrid
    {
        private class Cell
        {
            public ItemDef Def = null!;
            public GameObject Root = null!;
            public Image Icon = null!;
            public Text CountText = null!;
            public int Count;
        }

        private const int CellSize = 48;

        private readonly List<Cell> _cells = new();
        private readonly Dictionary<ItemDef, Cell> _byDef = new();
        private readonly Func<ItemDef, int> _getCount;
        private readonly Action<ItemDef, int> _onClick; // (item, signedAmount)
        private readonly bool _onlyOwned;                // inventory view: hide 0-count items

        private GameObject _grid = null!;
        private UniverseLib.UI.Widgets.AutoSliderScrollbar _scrollbar = null!;
        private string _filter = "";

        public ItemGrid(Func<ItemDef, int> getCount, Action<ItemDef, int> onClick, bool onlyOwned = false)
        {
            _getCount = getCount;
            _onClick = onClick;
            _onlyOwned = onlyOwned;
        }

        public GameObject Build(GameObject parent, IEnumerable<ItemDef> items)
        {
            var root = UIFactory.CreateVerticalGroup(parent, "ItemGrid", false, false, true, true, 4,
                default, new Color(0.14f, 0.14f, 0.14f));
            UIFactory.SetLayoutElement(root, flexibleWidth: 9999, flexibleHeight: 9999);

            var search = UIFactory.CreateInputField(root, "Search", "Search...");
            UIFactory.SetLayoutElement(search.Component.gameObject, minHeight: 28, flexibleWidth: 9999);
            search.OnValueChanged += OnFilterChanged;

            UIFactory.CreateScrollView(root, "GridScroll", out var scrollContent, out _scrollbar,
                new Color(0.1f, 0.1f, 0.1f));

            _grid = scrollContent;
            var layout = _grid.GetComponent<VerticalLayoutGroup>();
            if (layout) UnityEngine.Object.DestroyImmediate(layout);
            var gl = _grid.AddComponent<GridLayoutGroup>();
            gl.cellSize = new Vector2(CellSize, CellSize);
            gl.spacing = new Vector2(4, 4);
            gl.padding = new RectOffset(4, 4, 4, 4);
            gl.startCorner = GridLayoutGroup.Corner.UpperLeft;
            gl.startAxis = GridLayoutGroup.Axis.Horizontal;
            gl.childAlignment = TextAnchor.UpperLeft;
            gl.constraint = GridLayoutGroup.Constraint.Flexible;

            foreach (var def in items)
                if (def)
                    CreateCell(def);

            Refresh();
            return root;
        }

        private void CreateCell(ItemDef def)
        {
            if (_byDef.ContainsKey(def)) return;

            var cellObj = UIFactory.CreateUIObject($"Cell_{def.name}", _grid);

            var icon = cellObj.AddComponent<Image>();
            icon.preserveAspect = true;
            icon.color = Color.white;

            IconCache.LoadItemIcon(def, sprite =>
            {
                if (sprite && icon) icon.sprite = sprite;
            });

            var badge = UIFactory.CreateLabel(cellObj, "Count", "", TextAnchor.LowerRight);
            var badgeRect = badge.gameObject.GetComponent<RectTransform>();
            badgeRect.anchorMin = Vector2.zero;
            badgeRect.anchorMax = Vector2.one;
            badgeRect.offsetMin = new Vector2(0, 0);
            badgeRect.offsetMax = new Vector2(-2, 0);
            badge.fontStyle = FontStyle.Bold;
            badge.resizeTextForBestFit = false;
            badge.fontSize = 16;

            var tooltip = UIFactory.CreateButton(cellObj, "ClickArea", "");
            var btnRect = tooltip.Component.gameObject.GetComponent<RectTransform>();
            btnRect.anchorMin = Vector2.zero;
            btnRect.anchorMax = Vector2.one;
            btnRect.offsetMin = Vector2.zero;
            btnRect.offsetMax = Vector2.zero;
            var btnImg = tooltip.Component.gameObject.GetComponent<Image>();
            if (btnImg) btnImg.color = new Color(1f, 1f, 1f, 0f);

            var cell = new Cell { Def = def, Root = cellObj, Icon = icon, CountText = badge };

            tooltip.OnClick += () => _onClick(cell.Def, Shift() ? 5 : 1);
            AddRightClick(tooltip.Component.gameObject, () => _onClick(cell.Def, Shift() ? -5 : -1));

            _cells.Add(cell);
            _byDef[def] = cell;
        }

        private static void AddRightClick(GameObject target, Action onRightClick)
        {
            var trigger = target.AddComponent<EventTrigger>();
            var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
            entry.callback.AddListener(data =>
            {
                if (data is PointerEventData ped && ped.button == PointerEventData.InputButton.Right)
                    onRightClick();
            });
            trigger.triggers.Add(entry);
        }

        private static bool Shift()
        {
            return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        }

        public void Refresh()
        {
            foreach (var cell in _cells)
            {
                cell.Count = _getCount(cell.Def);
                cell.CountText.text = cell.Count > 0 ? cell.Count.ToString() : "";
                ApplyVisibility(cell);
            }

            UpdateScrollbar();
        }


        private void UpdateScrollbar()
        {
            if (_grid == null || _scrollbar == null) return;
            var rect = _grid.GetComponent<RectTransform>();
            if (rect) LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
            _scrollbar.UpdateSliderHandle();
        }

        private void OnFilterChanged(string text)
        {
            _filter = text?.ToLowerInvariant() ?? "";
            foreach (var cell in _cells)
                ApplyVisibility(cell);

            UpdateScrollbar();
        }

        private void ApplyVisibility(Cell cell)
        {
            var matchesFilter = string.IsNullOrEmpty(_filter) ||
                                Language.GetString(cell.Def.nameToken).ToLowerInvariant().Contains(_filter);
            var owned = !_onlyOwned || cell.Count > 0;
            cell.Root.SetActive(matchesFilter && owned);
        }
    }
}
