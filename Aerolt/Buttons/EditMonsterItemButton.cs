using RoR2;
using RoR2.ContentManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Aerolt.Buttons
{
    public class EditMonsterItemButton : MonoBehaviour
    {
        public GameObject buttonPrefab;
        public GameObject buttonParent;

        public GameObject itemListParent;

        public static GameObject _globalButtonPrefab;
        public static GameObject _globalitemListParent;

        private void Awake()
        {
            _globalButtonPrefab = buttonPrefab;
            _globalitemListParent = itemListParent;

            foreach (var def in ContentManager._itemDefs)
                new MonsterItemButtonGen(def, buttonParent,false);
        }
    }

    
    
}