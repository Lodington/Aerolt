using System.Collections.Generic;
using RoR2;
using RoR2.ContentManagement;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Aerolt.Buttons
{
    public class EditItemButton : MonoBehaviour
    {
        public GameObject buttonPrefab;
        public GameObject buttonParent;

        public GameObject itemListParent;
        
        private void OnEnable()
        {
            foreach(var def in ContentManager._itemDefs)
                CreateButton(def,buttonParent).GetComponent<Button>().onClick.AddListener(() => AddItemToList(def));

        }

        public GameObject CreateButton(ItemDef def, GameObject buttonParent)
        {
            
            var newButton = Instantiate(buttonPrefab, buttonParent.transform);
            newButton.GetComponent<CustomButton>().ButtonText.text = Language.GetString(def.nameToken);
            newButton.GetComponent<Image>().sprite = def.pickupIconSprite;
            return newButton;
        }
        
        public void AddItemToList(ItemDef def)
        {
            var newButton = Instantiate(buttonPrefab, itemListParent.transform);
            newButton.GetComponent<CustomButton>().ButtonText.text = Language.GetString(def.nameToken);
            newButton.GetComponent<Image>().sprite = def.pickupIconSprite;
            newButton.GetComponent<Button>().onClick.AddListener(() => DestroyButton(newButton,def));
            MonsterButtonGenerator.ItemDef.Add(def);
        }

        public void DestroyButton(GameObject button, ItemDef def)
        {
            MonsterButtonGenerator.ItemDef.Remove(def);
            Destroy(button);
        }
    }
}