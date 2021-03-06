using RoR2;
using RoR2.ContentManagement;
using UnityEngine;

namespace Aerolt.Buttons
{
    public class EditMonsterItemButton : MonoBehaviour
    {
        public GameObject buttonPrefab;
        public GameObject buttonParent;

        public GameObject itemListParent;

        private void Awake()
        {
            foreach (var def in ContentManager._itemDefs)
                new AddRemoveButtonGen<ItemDef>(def, buttonPrefab, MonsterButtonGenerator.ItemDef, buttonParent, itemListParent, false);
        }
    }
}