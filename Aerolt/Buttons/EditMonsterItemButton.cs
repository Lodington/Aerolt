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
                new InventoryItemAddRemoveButtonGen(def, buttonPrefab, MonsterButtonGenerator.ItemDef, itemListParent, buttonParent, false);
        }
    }
}