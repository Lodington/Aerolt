using System.Collections.Generic;
using RoR2;
using UnityEngine;
using UnityEngine.UI;

namespace Aerolt.Buttons
{
	public class AddRemoveButtonGen<T>
	{
		public readonly T def;
		private GameObject prefab;
		private Dictionary<T, int> itemCounts;
		
		public GameObject button;
		public AddRemoveButtonGen<T> countRemoveButton;
		public CustomButton customButton;
		
		private GameObject parent;
		private GameObject removeParent;
		private bool isDecrease;

		public AddRemoveButtonGen(T defIn, GameObject prefabIn, Dictionary<T, int> itemDefDictionary, GameObject parentIn, GameObject removeParentIn = null, bool doDestroy = true)
		{
			def = defIn;
			prefab = prefabIn;
			isDecrease = doDestroy;
			parent = parentIn;
			removeParent = removeParentIn;
			itemCounts = itemDefDictionary;
			
			button = GameObject.Instantiate(prefabIn, parentIn.transform);
			customButton = button.GetComponent<CustomButton>();
			//customButton.onRightClick.AddListener(isDecrease ? Increase : Decrease); There is a bug with this, when decreasing on the left side it can destroy the left side button
			button.GetComponent<Button>().onClick.AddListener(!isDecrease ? Increase : Decrease);

			switch (defIn) // Generics are fucked
			{
				case ItemDef itemDef:
					customButton.buttonText.text = Language.GetString(itemDef.nameToken);
					button.GetComponent<Image>().sprite = itemDef.pickupIconSprite;
					break;
				case EquipmentDef eqDef:
					customButton.buttonText.text = Language.GetString(eqDef.nameToken);
					button.GetComponent<Image>().sprite = eqDef.pickupIconSprite;
					break;
				case BuffDef buffDef:
					customButton.buttonText.text = buffDef.name;
					var image = button.GetComponent<Image>();
					image.sprite = buffDef.iconSprite;
					image.color = buffDef.buffColor;
					break;
			}
		}
		
		
		private void Increase()
		{
			if (customButton.EventSystem.player.GetButton(9)) // action 9 is shift by default //(Input.GetKey(KeyCode.LeftShift))
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
			GameObject.Destroy(button);
			countRemoveButton.countRemoveButton = null;
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
			if (i == 0 && prev == 0) return;
			Change(i - prev);
		}
	}
}