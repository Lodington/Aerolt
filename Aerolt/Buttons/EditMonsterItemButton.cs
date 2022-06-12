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

    public class MonsterItemButtonGen
    {
        public ItemDef _def;
        public GameObject _button;
        public MonsterItemButtonGen CountButton;
        public MonsterItemButtonGen(ItemDef def, GameObject ButtonParent, bool doDestroy = true)
        {
            _def = def;
            _button = CreateButton(ButtonParent);
            _button.GetComponent<Button>().onClick.AddListener(!doDestroy ? AddItemToList : DestroyButton);
        }
        public GameObject CreateButton(GameObject buttonParent)
        {
            var newButton = GameObject.Instantiate(EditMonsterItemButton._globalButtonPrefab, buttonParent.transform);
            
            newButton.GetComponent<CustomButton>().ButtonText.text = Language.GetString(_def.nameToken);
            newButton.GetComponent<Image>().sprite = _def.pickupIconSprite;
            return newButton;
        }
        public void DestroyButton()
        {
            MonsterButtonGenerator.ItemDef[_def]--;
            _button.GetComponent<CustomButton>().ButtonText.text = $"{Language.GetString(_def.nameToken)} x{MonsterButtonGenerator.ItemDef[_def]}";

            if (MonsterButtonGenerator.ItemDef[_def] == 0)
            {
                MonsterButtonGenerator.ItemDef.Remove(_def);
                GameObject.Destroy(_button);
            }
            
        }
        public void AddItemToList()
        {
            if (!MonsterButtonGenerator.ItemDef.ContainsKey(_def))
            {
                CountButton = new MonsterItemButtonGen(_def, EditMonsterItemButton._globalitemListParent);
                MonsterButtonGenerator.ItemDef.Add(_def, 0);
            }
            MonsterButtonGenerator.ItemDef[_def]++;
            CountButton._button.GetComponent<CustomButton>().ButtonText.text = $"{Language.GetString(_def.nameToken)} x{MonsterButtonGenerator.ItemDef[_def]}";

        }
    }
    
}