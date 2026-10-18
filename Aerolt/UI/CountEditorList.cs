using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib.UI;
using UniverseLib.UI.Widgets.ScrollView;

namespace Aerolt.UI
{
    public class CountEditorList<T> : ICellPoolDataSource<CountEditorList<T>.Cell> where T : class
    {
        public class Entry
        {
            public T Def = null!;
            public int Count;
        }
        public class Cell : ICell
        {
            public GameObject UIRoot { get; set; } = null!;
            public float DefaultHeight => 30f;
            public bool Enabled => UIRoot && UIRoot.activeSelf;
            public RectTransform Rect { get; set; } = null!;

            public Image Icon = null!;
            public Text NameText = null!;
            public Text CountText = null!;
            public Button Minus = null!;
            public Button Plus = null!;

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

                var nameLabel = UIFactory.CreateLabel(UIRoot, "Name", "", TextAnchor.MiddleLeft);
                UIFactory.SetLayoutElement(nameLabel.gameObject, minWidth: 100, flexibleWidth: 9999);
                NameText = nameLabel;

                Minus = UIFactory.CreateButton(UIRoot, "Minus", "-").Component;
                UIFactory.SetLayoutElement(Minus.gameObject, minWidth: 28, minHeight: 26, flexibleWidth: 0);

                var countLabel = UIFactory.CreateLabel(UIRoot, "Count", "0", TextAnchor.MiddleCenter);
                UIFactory.SetLayoutElement(countLabel.gameObject, minWidth: 40, flexibleWidth: 0);
                CountText = countLabel;

                Plus = UIFactory.CreateButton(UIRoot, "Plus", "+").Component;
                UIFactory.SetLayoutElement(Plus.gameObject, minWidth: 28, minHeight: 26, flexibleWidth: 0);

                return UIRoot;
            }
        }

        private readonly Dictionary<T, Entry> _entries = new();
        private readonly List<T> _all = new();      // every def, in display order
        private readonly List<T> _filtered = new();  // currently-visible subset (search result)
        private readonly Func<T, string> _nameOf;
        private readonly Action<Entry, Image, Func<bool>> _applyIcon;

        private ScrollPool<Cell> _scrollPool = null!;
        private string _filter = "";

        public CountEditorList(Func<T, string> nameOf, Action<Entry, Image, Func<bool>> applyIcon)
        {
            _nameOf = nameOf;
            _applyIcon = applyIcon;
        }

        public IReadOnlyDictionary<T, Entry> Entries => _entries;


        public int ItemCount => _filtered.Count;

        public void OnCellBorrowed(Cell cell)
        {
            cell.Minus.onClick.AddListener(() => OnDelta(cell, Shift() ? -5 : -1));
            cell.Plus.onClick.AddListener(() => OnDelta(cell, Shift() ? 5 : 1));
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

            var def = _filtered[index];
            var entry = _entries[def];

            cell.NameText.text = _nameOf(def);
            cell.CountText.text = entry.Count.ToString();

            cell.Icon.sprite = null;
            cell.Icon.color = Color.white;
            _applyIcon(entry, cell.Icon, () => cell.Generation == generation);
        }

        private void OnDelta(Cell cell, int delta)
        {
            if (cell.BoundIndex < 0 || cell.BoundIndex >= _filtered.Count) return;
            var def = _filtered[cell.BoundIndex];
            var entry = _entries[def];
            entry.Count = Mathf.Max(0, entry.Count + delta);
            cell.CountText.text = entry.Count.ToString();
        }

        private static bool Shift()
        {
            return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        }

        public GameObject Build(GameObject parent, IEnumerable<T> defs)
        {
            var root = UIFactory.CreateVerticalGroup(parent, "CountEditor", false, false, true, true, 4,
                default, new Color(0.14f, 0.14f, 0.14f));
            UIFactory.SetLayoutElement(root, flexibleWidth: 9999, flexibleHeight: 9999);

            var search = UIFactory.CreateInputField(root, "Search", "Search...");
            UIFactory.SetLayoutElement(search.Component.gameObject, minHeight: 28, flexibleWidth: 9999);
            search.OnValueChanged += FilterUpdated;

            foreach (var def in defs)
            {
                if (def == null || _entries.ContainsKey(def)) continue;
                _entries[def] = new Entry { Def = def };
                _all.Add(def);
            }

            _filtered.AddRange(_all);

            _scrollPool = UIFactory.CreateScrollPool<Cell>(root, "List", out var uiRoot, out _,
                new Color(0.1f, 0.1f, 0.1f));
            UIFactory.SetLayoutElement(uiRoot, flexibleWidth: 9999, flexibleHeight: 9999);
            _scrollPool.Initialize(this);

            return root;
        }

        public void SetCount(T def, int value)
        {
            if (!_entries.TryGetValue(def, out var entry)) return;
            entry.Count = Mathf.Max(0, value);
        }

        public void RefreshIcons()
        {
            _scrollPool?.Refresh(true, false);
        }

        private void FilterUpdated(string text)
        {
            _filter = text?.ToLowerInvariant() ?? "";

            _filtered.Clear();
            if (string.IsNullOrEmpty(_filter))
            {
                _filtered.AddRange(_all);
            }
            else
            {
                foreach (var def in _all)
                    if (_nameOf(def).ToLowerInvariant().Contains(_filter))
                        _filtered.Add(def);
            }

            _scrollPool?.Refresh(true, true);
        }
    }
}
