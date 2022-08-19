using System.Collections.Generic;
using RoR2;
using UnityEngine;
using UnityEngine.UI;

namespace Aerolt.Buttons
{
    public class AddRemoveButtonGen<T>
    {
        public readonly T def;

        public GameObject button;
        public AddRemoveButtonGen<T> countRemoveButton;
        public CustomButton customButton;
        private readonly bool isDecrease;
        private readonly Dictionary<T, int> itemCounts;

        private GameObject parent;
        private readonly GameObject prefab;
        private readonly GameObject removeParent;

        public AddRemoveButtonGen(T defIn, GameObject prefabIn, Dictionary<T, int> itemDefDictionary,
            GameObject parentIn, GameObject removeParentIn = null, bool doDestroy = true)
        {
            def = defIn;
            prefab = prefabIn;
            isDecrease = doDestroy;
            parent = parentIn;
            removeParent = removeParentIn;
            itemCounts = itemDefDictionary;

            button = Object.Instantiate(prefabIn, parentIn.transform);
            customButton = button.GetComponent<CustomButton>();
            //customButton.onRightClick.AddListener(isDecrease ? Increase : Decrease); There is a bug with this, when decreasing on the left side it can destroy the left side button
            button.GetComponent<Button>().onClick.AddListener(!isDecrease ? Increase : Decrease);

            switch (defIn) // Generics are fucked
            {
                case ItemDef itemDef:
                    customButton.buttonText.text = Language.GetString(itemDef.nameToken);
                    customButton.image.sprite = itemDef.pickupIconSprite;
                    break;
                case EquipmentDef eqDef:
                    customButton.buttonText.text = Language.GetString(eqDef.nameToken);
                    customButton.image.sprite = eqDef.pickupIconSprite;
                    break;
                case BuffDef buffDef:
                    customButton.buttonText.text = buffDef.name;
                    var image = customButton.image;
                    image.sprite = buffDef.iconSprite;
                    image.color = buffDef.buffColor;
                    break;
            }
        }


        private void Increase()
        {
            if (customButton.EventSystem.player.GetButton(9)
            ) // action 9 is shift by default //(Input.GetKey(KeyCode.LeftShift))
            {
                Change(5);
                return;
            }

            Change(1);
        }

        private void Decrease()
        {
            if (customButton.EventSystem.player.GetButton(9)) //Input.GetKey(KeyCode.LeftShift))
            {
                Change(-itemCounts[def]);
                return;
            }

            Change(-1);
        }

        public void Change(int relativeAmount)
        {
            if (countRemoveButton == null)
            {
                if (relativeAmount < 0) return;

                countRemoveButton = new AddRemoveButtonGen<T>(def, prefab, itemCounts, removeParent);
                countRemoveButton.countRemoveButton = this;
                if (!itemCounts.ContainsKey(def))
                    itemCounts.Add(def, 0);
            }

            itemCounts[def] += relativeAmount;
            UpdateText();

            if (itemCounts[def] > 0) return;
            Object.Destroy(isDecrease ? button : countRemoveButton.button);
            if (!isDecrease) countRemoveButton = null; else countRemoveButton.countRemoveButton = null;
        }

        public void UpdateText()
        {
            var targetButton = isDecrease ? customButton : countRemoveButton.customButton;
            targetButton.buttonText.text = def switch // Generics are fucked
            {
                ItemDef itemDef => $"{Language.GetString(itemDef.nameToken)} x{itemCounts[def]}",
                EquipmentDef eqDef => $"{Language.GetString(eqDef.nameToken)} x{itemCounts[def]}",
                BuffDef buffDef => $"{buffDef.name} x{itemCounts[def]}",
                _ => targetButton.buttonText.text
            };
        }

        public void SetAmount(int i)
        {
            var prev = itemCounts.ContainsKey(def) ? itemCounts[def] : 0;
            if (prev == i) return;
            Change(i - prev);
        }
    }
}