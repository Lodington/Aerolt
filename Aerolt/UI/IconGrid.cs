using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UniverseLib.UI;

namespace Aerolt.UI
{

    public class IconGrid<T> where T : class
    {
        private class Cell
        {
            public T Value = null!;
            public GameObject Root = null!;
            public Image Icon = null!;
            public Text CountText = null!;
            public int Generation;
            public int Count;
        }

        private const int IconSize = 48;
        private const int NamePlateHeight = 16;
        private const int CellWidth = 64;
        private const int CellHeight = IconSize + NamePlateHeight;

        private readonly List<Cell> _cells = new();
        private readonly Func<T, string> _nameOf;
        private readonly Action<T, Image, Func<bool>> _applyIcon;
        private readonly Action<T, int>? _onClick;
        private readonly Func<T, int>? _getCount;
        private readonly Func<T, Color>? _tint;
        private readonly bool _onlyOwned;

        private GameObject _grid = null!;
        private UniverseLib.UI.Widgets.AutoSliderScrollbar _scrollbar = null!;
        private string _filter = "";

        public IconGrid(
            Func<T, string> nameOf,
            Action<T, Image, Func<bool>> applyIcon,
            Action<T, int>? onClick = null,
            Func<T, int>? getCount = null,
            bool onlyOwned = false,
            Func<T, Color>? tint = null)
        {
            _nameOf = nameOf;
            _applyIcon = applyIcon;
            _onClick = onClick;
            _getCount = getCount;
            _onlyOwned = onlyOwned;
            _tint = tint;
        }

        public GameObject Build(GameObject parent, IEnumerable<T> items)
        {
            var root = UIFactory.CreateVerticalGroup(parent, "IconGrid", false, false, true, true, 4,
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
            gl.cellSize = new Vector2(CellWidth, CellHeight);
            gl.spacing = new Vector2(4, 4);
            gl.padding = new RectOffset(4, 4, 4, 4);
            gl.startCorner = GridLayoutGroup.Corner.UpperLeft;
            gl.startAxis = GridLayoutGroup.Axis.Horizontal;
            gl.childAlignment = TextAnchor.UpperLeft;
            gl.constraint = GridLayoutGroup.Constraint.Flexible;

            foreach (var value in items)
                if (value != null)
                    CreateCell(value);

            Refresh();
            return root;
        }

        private void CreateCell(T value)
        {
            var cellObj = UIFactory.CreateUIObject("Cell", _grid);

            var iconObj = UIFactory.CreateUIObject("Icon", cellObj);
            var iconRect = iconObj.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0f, 0f);
            iconRect.anchorMax = new Vector2(1f, 1f);
            iconRect.offsetMin = new Vector2(0f, NamePlateHeight);
            iconRect.offsetMax = Vector2.zero;
            var icon = iconObj.AddComponent<Image>();
            icon.preserveAspect = true;
            icon.color = _tint != null ? _tint(value) : Color.white;
            icon.raycastTarget = false;

            var cell = new Cell { Value = value, Root = cellObj, Icon = icon };
            var generation = ++cell.Generation;
            _applyIcon(value, icon, () => cell.Generation == generation);

            var badge = UIFactory.CreateLabel(iconObj, "Count", "", TextAnchor.LowerRight);
            var badgeRect = badge.gameObject.GetComponent<RectTransform>();
            badgeRect.anchorMin = Vector2.zero;
            badgeRect.anchorMax = Vector2.one;
            badgeRect.offsetMin = new Vector2(0, 0);
            badgeRect.offsetMax = new Vector2(-2, 0);
            badge.fontStyle = FontStyle.Bold;
            badge.resizeTextForBestFit = false;
            badge.fontSize = 16;
            badge.raycastTarget = false;
            cell.CountText = badge;

            var plateObj = UIFactory.CreateUIObject("NamePlate", cellObj);
            var plateRect = plateObj.GetComponent<RectTransform>();
            plateRect.anchorMin = new Vector2(0f, 0f);
            plateRect.anchorMax = new Vector2(1f, 0f);
            plateRect.pivot = new Vector2(0.5f, 0f);
            plateRect.offsetMin = Vector2.zero;
            plateRect.offsetMax = new Vector2(0f, NamePlateHeight);
            var plateImg = plateObj.AddComponent<Image>();
            plateImg.color = new Color(0f, 0f, 0f, 0.6f);
            plateImg.raycastTarget = false;

            var nameLabel = UIFactory.CreateLabel(plateObj, "Name", _nameOf(value), TextAnchor.MiddleCenter);
            var nameRect = nameLabel.gameObject.GetComponent<RectTransform>();
            nameRect.anchorMin = Vector2.zero;
            nameRect.anchorMax = Vector2.one;
            nameRect.offsetMin = new Vector2(1, 0);
            nameRect.offsetMax = new Vector2(-1, 0);
            nameLabel.fontSize = 10;
            nameLabel.resizeTextForBestFit = true;
            nameLabel.resizeTextMinSize = 6;
            nameLabel.resizeTextMaxSize = 11;
            nameLabel.horizontalOverflow = HorizontalWrapMode.Wrap;
            nameLabel.verticalOverflow = VerticalWrapMode.Truncate;
            nameLabel.raycastTarget = false;

            if (_onClick != null)
            {
                var click = UIFactory.CreateButton(cellObj, "ClickArea", "");
                var btnRect = click.Component.gameObject.GetComponent<RectTransform>();
                btnRect.anchorMin = Vector2.zero;
                btnRect.anchorMax = Vector2.one;
                btnRect.offsetMin = Vector2.zero;
                btnRect.offsetMax = Vector2.zero;
                var btnImg = click.Component.gameObject.GetComponent<Image>();
                if (btnImg) btnImg.color = new Color(1f, 1f, 1f, 0f);

                click.OnClick += () => _onClick(cell.Value, Shift() ? 5 : 1);
                AddRightClick(click.Component.gameObject, () => _onClick(cell.Value, Shift() ? -5 : -1));
            }

            _cells.Add(cell);
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
                if (_getCount != null)
                {
                    cell.Count = _getCount(cell.Value);
                    cell.CountText.text = cell.Count > 0 ? cell.Count.ToString() : "";
                }

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
                                _nameOf(cell.Value).ToLowerInvariant().Contains(_filter);
            var owned = !_onlyOwned || cell.Count > 0;
            cell.Root.SetActive(matchesFilter && owned);
        }
    }
}
