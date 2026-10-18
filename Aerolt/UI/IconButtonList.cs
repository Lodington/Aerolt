using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib.UI;
using UniverseLib.UI.Widgets.ScrollView;

namespace Aerolt.UI
{
    public class IconButtonList<T> : ICellPoolDataSource<IconButtonList<T>.Cell> where T : class
    {
        public class Cell : ICell
        {
            public GameObject UIRoot { get; set; } = null!;
            public float DefaultHeight => 30f;
            public bool Enabled => UIRoot && UIRoot.activeSelf;
            public RectTransform Rect { get; set; } = null!;

            public Image Icon = null!;
            public Button Button = null!;
            public Text ButtonText = null!;

            public int BoundIndex = -1;
            public int Generation;

            public void Enable() => UIRoot.SetActive(true);
            public void Disable() => UIRoot.SetActive(false);

            public GameObject CreateContent(GameObject parent)
            {
                UIRoot = UIFactory.CreateHorizontalGroup(parent, "Row", false, false, true, true, 4,
                    new Vector4(2, 2, 2, 2), new Color(0.13f, 0.13f, 0.13f));
                Rect = UIRoot.GetComponent<RectTransform>();
                UIFactory.SetLayoutElement(UIRoot, minHeight: 30, flexibleWidth: 9999);

                var iconObj = UIFactory.CreateUIObject("Icon", UIRoot);
                UIFactory.SetLayoutElement(iconObj, minWidth: 24, minHeight: 24, preferredWidth: 24,
                    preferredHeight: 24, flexibleWidth: 0, flexibleHeight: 0);
                Icon = iconObj.AddComponent<Image>();
                Icon.preserveAspect = true;

                var btn = UIFactory.CreateButton(UIRoot, "Action", "");
                UIFactory.SetLayoutElement(btn.Component.gameObject, minHeight: 26, flexibleWidth: 9999);
                Button = btn.Component;
                ButtonText = btn.ButtonText;

                return UIRoot;
            }
        }

        private readonly List<T> _all = new();
        private readonly List<T> _filtered = new();
        private readonly Func<T, string> _nameOf;
        private readonly Action<T, Image, Func<bool>> _applyIcon;
        private readonly Action<T> _onClick;

        private ScrollPool<Cell> _scrollPool = null!;

        public IconButtonList(Func<T, string> nameOf, Action<T, Image, Func<bool>> applyIcon, Action<T> onClick)
        {
            _nameOf = nameOf;
            _applyIcon = applyIcon;
            _onClick = onClick;
        }


        public int ItemCount => _filtered.Count;

        public void OnCellBorrowed(Cell cell)
        {
            cell.Button.onClick.AddListener(() =>
            {
                if (cell.BoundIndex < 0 || cell.BoundIndex >= _filtered.Count) return;
                _onClick(_filtered[cell.BoundIndex]);
            });
        }

        public void SetCell(Cell cell, int index)
        {
            if (index < 0 || index >= _filtered.Count)
            {
                cell.Disable();
                return;
            }

            cell.Enable();
            cell.BoundIndex = index;
            var generation = ++cell.Generation;

            var item = _filtered[index];
            cell.ButtonText.text = _nameOf(item);

            cell.Icon.sprite = null;
            cell.Icon.color = Color.white;
            _applyIcon(item, cell.Icon, () => cell.Generation == generation);
        }


        public GameObject Build(GameObject parent, IEnumerable<T> items)
        {
            var root = UIFactory.CreateVerticalGroup(parent, "IconButtonList", false, false, true, true, 4,
                default, new Color(0.14f, 0.14f, 0.14f));
            UIFactory.SetLayoutElement(root, flexibleWidth: 9999, flexibleHeight: 9999);

            var search = UIFactory.CreateInputField(root, "Search", "Search...");
            UIFactory.SetLayoutElement(search.Component.gameObject, minHeight: 28, flexibleWidth: 9999);
            search.OnValueChanged += FilterUpdated;

            foreach (var item in items)
                if (item != null)
                    _all.Add(item);
            _filtered.AddRange(_all);

            _scrollPool = UIFactory.CreateScrollPool<Cell>(root, "List", out var uiRoot, out _,
                new Color(0.1f, 0.1f, 0.1f));
            UIFactory.SetLayoutElement(uiRoot, flexibleWidth: 9999, flexibleHeight: 9999);
            _scrollPool.Initialize(this);

            return root;
        }

        private void FilterUpdated(string text)
        {
            var filter = text?.ToLowerInvariant() ?? "";

            _filtered.Clear();
            if (string.IsNullOrEmpty(filter))
            {
                _filtered.AddRange(_all);
            }
            else
            {
                foreach (var item in _all)
                    if (_nameOf(item).ToLowerInvariant().Contains(filter))
                        _filtered.Add(item);
            }

            _scrollPool?.Refresh(true, true);
        }
    }
}
