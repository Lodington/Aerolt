using System.Collections;
using Aerolt.API;
using Aerolt.UI;
using RoR2;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib;
using UniverseLib.UI;

namespace SkillPanel
{

    public class SkillPanelSection : IAeroltSection
    {
        private Toggle _infinite = null!;
        private GameObject _slotsParent = null!;
        private readonly string[] _slotNames = { "Primary", "Secondary", "Utility", "Special" };
        private readonly Text[] _slotTexts = new Text[4];
        private bool _visible;

        public string Title => "Skills";
        public GameObject Root { get; private set; } = null!;

        public void Build(GameObject contentParent)
        {
            Root = UIFactory.CreateVerticalGroup(contentParent, "SkillPanelSection", false, false, true, true, 4,
                default, new Color(0.14f, 0.14f, 0.14f));
            UIFactory.SetLayoutElement(Root, flexibleWidth: 9999, flexibleHeight: 9999);

            var controls = UIFactory.CreateHorizontalGroup(Root, "Controls", false, false, true, true, 4);
            UIFactory.SetLayoutElement(controls, minHeight: 30, flexibleHeight: 0, flexibleWidth: 9999);

            var reset = UIFactory.CreateButton(controls, "Reset", "Reset Cooldowns");
            UIFactory.SetLayoutElement(reset.Component.gameObject, minHeight: 28, minWidth: 130, flexibleWidth: 0);
            reset.OnClick += ResetCooldowns;

            var toggleObj = UIFactory.CreateToggle(controls, "Infinite", out _infinite, out var toggleText);
            UIFactory.SetLayoutElement(toggleObj, minHeight: 28, flexibleWidth: 9999);
            toggleText.text = "Infinite stock / no cooldown";

            _slotsParent = UIFactory.CreateVerticalGroup(Root, "Slots", false, false, true, true, 4,
                new Vector4(4, 4, 4, 4), new Color(0.12f, 0.12f, 0.12f));
            UIFactory.SetLayoutElement(_slotsParent, flexibleWidth: 9999, flexibleHeight: 9999);

            for (var i = 0; i < 4; i++)
                BuildSlotRow(i);
        }

        private void BuildSlotRow(int index)
        {
            var row = UIFactory.CreateHorizontalGroup(_slotsParent, $"Slot_{index}", false, false, true, true, 4,
                new Vector4(4, 4, 4, 4), new Color(0.13f, 0.13f, 0.13f));
            UIFactory.SetLayoutElement(row, minHeight: 34, flexibleWidth: 9999);

            var slotLabel = UIFactory.CreateLabel(row, "Slot", _slotNames[index], TextAnchor.MiddleLeft);
            UIFactory.SetLayoutElement(slotLabel.gameObject, minWidth: 90, flexibleWidth: 0);
            slotLabel.fontStyle = FontStyle.Bold;

            var value = UIFactory.CreateLabel(row, "Value", "-", TextAnchor.MiddleLeft);
            UIFactory.SetLayoutElement(value.gameObject, minWidth: 100, flexibleWidth: 9999);
            _slotTexts[index] = value;
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
                Tick();
                yield return new WaitForSecondsRealtime(0.1f);
            }

            _visible = false;
        }

        private void Tick()
        {
            var locator = SelectedSkills();
            var skills = Slots(locator);

            for (var i = 0; i < 4; i++)
            {
                var skill = skills[i];
                if (skill == null)
                {
                    _slotTexts[i].text = "<color=#777>(none)</color>";
                    continue;
                }

                if (_infinite && _infinite.isOn)
                {
                    if (skill.stock < skill.maxStock) skill.stock = skill.maxStock;
                    skill.rechargeStopwatch = skill.baseRechargeInterval;
                }

                var name = NameOf(skill);
                var cd = skill.cooldownRemaining;
                var cdText = cd > 0.05f ? $"<color=#f99>{cd:0.0}s</color>" : "<color=#9f9>ready</color>";
                _slotTexts[i].text = $"{name}   stock {skill.stock}/{skill.maxStock}   {cdText}";
            }
        }

        private void ResetCooldowns()
        {
            var skills = Slots(SelectedSkills());
            foreach (var skill in skills)
            {
                if (skill == null) continue;
                skill.stock = skill.maxStock;
                skill.rechargeStopwatch = skill.baseRechargeInterval;
            }
        }

        private static string NameOf(GenericSkill skill)
        {
            var token = skill.skillNameToken;
            if (!string.IsNullOrEmpty(token))
            {
                var s = Language.GetString(token);
                if (!string.IsNullOrEmpty(s)) return s;
            }

            return string.IsNullOrEmpty(skill.skillName) ? "Skill" : skill.skillName;
        }

        private static GenericSkill?[] Slots(SkillLocator? locator)
        {
            if (!locator) return new GenericSkill?[4];
            return new[] { locator!.primary, locator.secondary, locator.utility, locator.special };
        }

        private static SkillLocator? SelectedSkills()
        {
            var user = AeroltAPI.SelectedPlayer;
            if (!user || !user!.master) return null;
            var body = user.master.GetBody();
            return body && body.skillLocator ? body.skillLocator : null;
        }
    }
}
