using System.Collections;
using Aerolt.UI;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib;
using UniverseLib.UI;
using UniverseLib.UI.Models;

namespace DamageLog
{
    public class DamageLogSection : IAeroltSection
    {
        private GameObject _listContent = null!;
        private Toggle _onlySelected = null!;
        private ButtonRef _pauseButton = null!;
        private bool _visible;

        public string Title => "Damage Log";
        public GameObject Root { get; private set; } = null!;
        public bool ScrollableContent => false;

        public void Build(GameObject contentParent)
        {
            DamageTracker.Init();

            Root = UIFactory.CreateVerticalGroup(contentParent, "DamageLogSection", false, false, true, true, 4,
                default, new Color(0.14f, 0.14f, 0.14f));
            UIFactory.SetLayoutElement(Root, flexibleWidth: 9999, flexibleHeight: 9999);

            var controls = UIFactory.CreateHorizontalGroup(Root, "Controls", false, false, true, true, 4);
            UIFactory.SetLayoutElement(controls, minHeight: 30, flexibleHeight: 0, flexibleWidth: 9999);

            _pauseButton = UIFactory.CreateButton(controls, "Pause", "Pause");
            UIFactory.SetLayoutElement(_pauseButton.Component.gameObject, minHeight: 28, minWidth: 90, flexibleWidth: 0);
            _pauseButton.OnClick += () =>
            {
                DamageTracker.Paused = !DamageTracker.Paused;
                _pauseButton.ButtonText.text = DamageTracker.Paused ? "Resume" : "Pause";
            };

            var clear = UIFactory.CreateButton(controls, "Clear", "Clear");
            UIFactory.SetLayoutElement(clear.Component.gameObject, minHeight: 28, minWidth: 90, flexibleWidth: 0);
            clear.OnClick += () =>
            {
                DamageTracker.Clear();
                RefreshList();
            };

            var toggleObj = UIFactory.CreateToggle(controls, "OnlySelected", out _onlySelected, out var toggleText);
            UIFactory.SetLayoutElement(toggleObj, minHeight: 28, flexibleWidth: 9999);
            toggleText.text = "Selected player only";

            UIFactory.CreateScrollView(Root, "List", out _listContent, out _, new Color(0.1f, 0.1f, 0.1f));
            UIFactory.SetLayoutGroup<VerticalLayoutGroup>(_listContent, false, false, true, true, 1, 2, 2, 2, 2);
        }

        public void OnShown()
        {
            if (_visible) return;
            _visible = true;
            RuntimeHelper.StartCoroutine(RefreshLoop());
        }

        private IEnumerator RefreshLoop()
        {
            while (Root && Root.activeInHierarchy)
            {
                RefreshList();
                yield return new WaitForSecondsRealtime(0.25f);
            }

            _visible = false;
        }

        private void RefreshList()
        {
            if (_listContent == null) return;

            foreach (Transform child in _listContent.transform)
                Object.Destroy(child.gameObject);

            var onlySelected = _onlySelected && _onlySelected.isOn;
            var now = Time.time;

            foreach (var e in DamageTracker.Snapshot())
            {
                if (onlySelected && !e.AttackerIsSelected && !e.VictimIsSelected)
                    continue;

                var age = now - e.Time;
                var crit = e.Crit ? " <color=#ffd24a>CRIT</color>" : "";
                var proc = e.ProcCoefficient > 0 ? $" proc {e.ProcCoefficient:0.##}" : "";
                var line = $"<color=#9ad>{e.Attacker}</color> -> <color=#f99>{e.Victim}</color>  " +
                           $"<b>{e.Amount:0.#}</b>{crit}  <color=#bbb>{e.DamageType}{proc}</color>  " +
                           $"<color=#777>{age:0.0}s</color>";

                var label = UIFactory.CreateLabel(_listContent, "Entry", line, TextAnchor.MiddleLeft);
                UIFactory.SetLayoutElement(label.gameObject, minHeight: 20, flexibleWidth: 9999);
                label.fontSize = 13;
            }
        }
    }
}
