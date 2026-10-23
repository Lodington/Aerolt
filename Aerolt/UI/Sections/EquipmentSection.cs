using System.Collections.Generic;
using System.Linq;
using Aerolt.Classes;
using Aerolt.Messages;
using RoR2;
using RoR2.ContentManagement;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib.UI;

namespace Aerolt.UI.Sections
{
    public class EquipmentSection : IAeroltSection
    {
        private IconGrid<EquipmentDef> _grid = null!;
        private Image _currentIcon = null!;
        private Text _currentLabel = null!;
        private int _currentIconGeneration;

        public string Title => "Equipment";
        public GameObject Root { get; private set; } = null!;
        public bool ScrollableContent => false; 

        public void Build(GameObject contentParent)
        {
            Root = UIFactory.CreateVerticalGroup(contentParent, "EquipmentSection", false, false, true, true, 4,
                default, new Color(0.14f, 0.14f, 0.14f));
            UIFactory.SetLayoutElement(Root, flexibleWidth: 9999, flexibleHeight: 9999);

            var currentRow = UIFactory.CreateHorizontalGroup(Root, "Current", false, false, true, true, 6,
                new Vector4(4, 4, 4, 4), new Color(0.12f, 0.12f, 0.12f));
            UIFactory.SetLayoutElement(currentRow, minHeight: 44, flexibleHeight: 0, flexibleWidth: 9999);

            var equippedLabel = UIFactory.CreateLabel(currentRow, "EquippedLabel", "Equipped:", TextAnchor.MiddleLeft);
            UIFactory.SetLayoutElement(equippedLabel.gameObject, minWidth: 75, flexibleWidth: 0);
            equippedLabel.fontStyle = FontStyle.Bold;

            var iconObj = UIFactory.CreateUIObject("CurrentIcon", currentRow);
            UIFactory.SetLayoutElement(iconObj, minWidth: 36, minHeight: 36, preferredWidth: 36, preferredHeight: 36,
                flexibleWidth: 0, flexibleHeight: 0);
            _currentIcon = iconObj.AddComponent<Image>();
            _currentIcon.preserveAspect = true;
            _currentIcon.raycastTarget = false;

            _currentLabel = UIFactory.CreateLabel(currentRow, "CurrentName", "(none)", TextAnchor.MiddleLeft);
            UIFactory.SetLayoutElement(_currentLabel.gameObject, minWidth: 100, flexibleWidth: 9999);

            _grid = new IconGrid<EquipmentDef>(
                def => Language.GetString(def.nameToken),
                (def, icon, stillValid) => IconCache.LoadEquipmentIcon(def, sprite =>
                {
                    if (sprite && icon && stillValid()) icon.sprite = sprite;
                }),
                onClick: (def, _) => GiveEquipment(def));

            _grid.Build(Root, ContentManager._equipmentDefs
                .Where(x => x)
                .OrderBy(x => Language.GetString(x.nameToken)));

            RefreshCurrent();
        }

        public void OnShown()
        {
            RefreshCurrent();
        }

        private void RefreshCurrent()
        {
            if (_currentIcon == null) return;

            var def = CurrentEquipment();
            var generation = ++_currentIconGeneration;

            if (def == null)
            {
                _currentIcon.sprite = null;
                _currentIcon.color = new Color(1f, 1f, 1f, 0f);
                _currentLabel.text = "(none)";
                return;
            }

            _currentIcon.color = Color.white;
            _currentLabel.text = Language.GetString(def.nameToken);

            var icon = _currentIcon;
            IconCache.LoadEquipmentIcon(def, sprite =>
            {
                if (sprite && icon && _currentIconGeneration == generation) icon.sprite = sprite;
            });
        }

        private static EquipmentDef? CurrentEquipment()
        {
            var user = PlayerSelection.Selected;
            if (!user || !user!.master || !user.master.inventory) return null;
            var index = user.master.inventory.currentEquipmentIndex;
            if (index == EquipmentIndex.None) return null;
            var def = EquipmentCatalog.GetEquipmentDef(index);
            return def ? def : null;
        }

        private void GiveEquipment(EquipmentDef def)
        {
            var user = PlayerSelection.Selected;
            if (!user || !user!.master) return;
            var map = new Dictionary<EquipmentDef, int> { { def, 1 } };
            new SetEquipmentMessage(user.master.inventory, map).SendToServer();
            RefreshCurrent();
        }
    }
}
