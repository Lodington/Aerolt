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
    public class EquipmentSection : IAeroltSection
    {
        private IconButtonList<EquipmentDef> _list = null!;

        public string Title => "Equipment";
        public GameObject Root { get; private set; } = null!;

        public void Build(GameObject contentParent)
        {
            Root = UIFactory.CreateVerticalGroup(contentParent, "EquipmentSection", false, false, true, true, 4,
                default, new Color(0.14f, 0.14f, 0.14f));
            UIFactory.SetLayoutElement(Root, flexibleWidth: 9999, flexibleHeight: 9999);

            _list = new IconButtonList<EquipmentDef>(
                def => Language.GetString(def.nameToken),
                (def, icon, stillValid) =>
                {
                    IconCache.LoadEquipmentIcon(def, sprite =>
                    {
                        if (sprite && icon && stillValid()) icon.sprite = sprite;
                    });
                },
                GiveEquipment);

            _list.Build(Root, ContentManager._equipmentDefs
                .Where(x => x)
                .OrderBy(x => Language.GetString(x.nameToken)));
        }

        public void OnShown()
        {
        }

        private static void GiveEquipment(EquipmentDef def)
        {
            var user = PlayerSelection.Selected;
            if (!user || !user!.master) return;
            var map = new Dictionary<EquipmentDef, int> { { def, 1 } };
            new SetEquipmentMessage(user.master.inventory, map).SendToServer();
        }
    }
}
