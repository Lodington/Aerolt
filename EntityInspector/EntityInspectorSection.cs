using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aerolt.API;
using Aerolt.UI;
using RoR2;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib;
using UniverseLib.UI;
using UniverseLib.UI.Models;

namespace EntityInspector
{
    public class EntityInspectorSection : IAeroltSection
    {
        private Dropdown _targetDropdown = null!;
        private Text _readout = null!;
        private readonly List<CharacterBody> _bodies = new();
        private bool _visible;

        public string Title => "Inspector";
        public GameObject Root { get; private set; } = null!;

        public void Build(GameObject contentParent)
        {
            Root = UIFactory.CreateVerticalGroup(contentParent, "InspectorSection", false, false, true, true, 4,
                default, new Color(0.14f, 0.14f, 0.14f));
            UIFactory.SetLayoutElement(Root, flexibleWidth: 9999, flexibleHeight: 9999);

            var controls = UIFactory.CreateHorizontalGroup(Root, "Controls", false, false, true, true, 4);
            UIFactory.SetLayoutElement(controls, minHeight: 32, flexibleHeight: 0, flexibleWidth: 9999);

            var label = UIFactory.CreateLabel(controls, "Label", "Target:", TextAnchor.MiddleLeft);
            UIFactory.SetLayoutElement(label.gameObject, minWidth: 55, flexibleWidth: 0);

            UIFactory.CreateDropdown(controls, "TargetDropdown", out _targetDropdown, "Selected player", 14, null);
            UIFactory.SetLayoutElement(_targetDropdown.gameObject, minHeight: 28, flexibleWidth: 9999);

            var refresh = UIFactory.CreateButton(controls, "RefreshList", "Refresh list");
            UIFactory.SetLayoutElement(refresh.Component.gameObject, minHeight: 28, minWidth: 100, flexibleWidth: 0);
            refresh.OnClick += RebuildTargetList;

            UIFactory.CreateScrollView(Root, "Readout", out var content, out _, new Color(0.1f, 0.1f, 0.1f));
            UIFactory.SetLayoutGroup<VerticalLayoutGroup>(content, false, false, true, true, 2, 4, 4, 4, 4);
            _readout = UIFactory.CreateLabel(content, "ReadoutText", "", TextAnchor.UpperLeft);
            UIFactory.SetLayoutElement(_readout.gameObject, minHeight: 20, flexibleWidth: 9999, flexibleHeight: 9999);
            _readout.fontSize = 13;
            _readout.horizontalOverflow = HorizontalWrapMode.Wrap;

            RebuildTargetList();
        }

        public void OnShown()
        {
            RebuildTargetList();
            if (_visible) return;
            _visible = true;
            RuntimeHelper.StartCoroutine(RefreshLoop());
        }

        private IEnumerator RefreshLoop()
        {
            while (Root && Root.activeInHierarchy)
            {
                UpdateReadout();
                yield return new WaitForSecondsRealtime(0.1f);
            }

            _visible = false;
        }

        private void RebuildTargetList()
        {
            if (_targetDropdown == null) return;

            var previous = _targetDropdown.value;

            _bodies.Clear();
            var options = new List<string> { "Selected player" };

            foreach (var body in CharacterBody.readOnlyInstancesList.OrderBy(SortKey))
            {
                if (!body) continue;
                _bodies.Add(body);
                options.Add(DisplayName(body));
            }

            _targetDropdown.ClearOptions();
            _targetDropdown.AddOptions(options);
            _targetDropdown.value = Mathf.Clamp(previous, 0, options.Count - 1);
        }

        private CharacterBody? CurrentTarget()
        {
            if (_targetDropdown == null) return null;

            if (_targetDropdown.value == 0)
            {
                var user = AeroltAPI.SelectedPlayer;
                if (!user || !user!.master) return null;
                return user.master.GetBody();
            }

            var idx = _targetDropdown.value - 1;
            if (idx < 0 || idx >= _bodies.Count) return null;
            var body = _bodies[idx];
            return body ? body : null;
        }

        private void UpdateReadout()
        {
            var body = CurrentTarget();
            if (!body)
            {
                _readout.text = "<color=#999>No target. Pick one from the dropdown or select a player.</color>";
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine($"<b>{DisplayName(body!)}</b>");

            var hc = body!.healthComponent;
            if (hc)
                sb.AppendLine($"Health: {hc.health:0.#} / {body.maxHealth:0.#}" +
                              (body.maxShield > 0 ? $"   Shield: {hc.shield:0.#} / {body.maxShield:0.#}" : ""));

            sb.AppendLine($"Level: {body.level:0.##}   Regen: {body.regen:0.##}/s");
            sb.AppendLine($"Move Speed: {body.moveSpeed:0.##}");
            sb.AppendLine($"Damage: {body.damage:0.##}   Attack Speed: {body.attackSpeed:0.##}");
            sb.AppendLine($"Crit: {body.crit:0.#}%   Crit Mult: x{body.critMultiplier:0.##}");
            sb.AppendLine($"Armor: {body.armor:0.#}");

            // Inventory summary.
            var inv = body.inventory;
            if (inv)
            {
                var itemCount = 0;
                var stackCount = 0;
                foreach (var def in RoR2.ContentManagement.ContentManager.itemDefs)
                {
                    if (!def) continue;
                    var c = inv.GetItemCountEffective(def);
                    if (c <= 0) continue;
                    itemCount++;
                    stackCount += c;
                }

                sb.AppendLine();
                sb.AppendLine($"<b>Inventory:</b> {itemCount} unique, {stackCount} total");
                if (inv.currentEquipmentIndex != EquipmentIndex.None)
                {
                    var eq = EquipmentCatalog.GetEquipmentDef(inv.currentEquipmentIndex);
                    if (eq != null)
                        sb.AppendLine($"Equipment: {Language.GetString(eq.nameToken)}");
                }
            }

            // Active buffs.
            var buffs = ActiveBuffs(body);
            if (buffs.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("<b>Buffs:</b>");
                foreach (var (name, count) in buffs)
                    sb.AppendLine($"  {name}{(count > 1 ? $" x{count}" : "")}");
            }

            _readout.text = sb.ToString();
        }

        private static List<(string name, int count)> ActiveBuffs(CharacterBody body)
        {
            var result = new List<(string, int)>();

            for (var i = 0; i < BuffCatalog.buffCount; i++)
            {
                var buffDef = BuffCatalog.GetBuffDef((BuffIndex)i);
                if (!buffDef) continue;
                var count = body.GetBuffCount(buffDef);
                if (count <= 0) continue;
                var name = !string.IsNullOrEmpty(buffDef.name) ? buffDef.name : buffDef.buffIndex.ToString();
                result.Add((name, count));
            }

            return result;
        }

        private static string DisplayName(CharacterBody body)
        {
            string name;
            try
            {
                name = body.GetDisplayName();
            }
            catch
            {
                name = body.name;
            }

            if (string.IsNullOrEmpty(name)) name = "?";

            // Tag players so they're easy to find in a crowded list.
            if (body.isPlayerControlled) name = $"[P] {name}";
            return name;
        }

        private static float SortKey(CharacterBody body)
        {
            // Players first, then everything else.
            return body && body.isPlayerControlled ? 0f : 1f;
        }
    }
}
