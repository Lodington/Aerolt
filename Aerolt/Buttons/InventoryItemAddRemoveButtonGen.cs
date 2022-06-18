using System.Collections.Generic;
using RoR2;
using UnityEngine;
using UnityEngine.UI;

namespace Aerolt.Buttons
{
	public class InventoryItemAddRemoveButtonGen
	{
		public readonly ItemDef def;
		private GameObject prefab;
		private Dictionary<ItemDef, int> itemCounts;
		
		public GameObject button;
		public InventoryItemAddRemoveButtonGen countRemoveButton;
		public CustomButton customButton;
		
		private GameObject parent;
		private GameObject removeParent;
		private bool isDecrease;

		public InventoryItemAddRemoveButtonGen(ItemDef defIn, GameObject prefabIn, Dictionary<ItemDef, int> itemDefDictionary, GameObject parentIn, GameObject removeParentIn = null, bool doDestroy = true)
		{
			def = defIn;
			prefab = prefabIn;
			isDecrease = doDestroy;
			parent = parentIn;
			removeParent = removeParentIn;
			itemCounts = itemDefDictionary;
			
			button = GameObject.Instantiate(prefabIn, parentIn.transform);
			customButton = button.GetComponent<CustomButton>();
			customButton.ButtonText.text = Language.GetString(def.nameToken);
			button.GetComponent<Image>().sprite = def.pickupIconSprite;
			button.GetComponent<Button>().onClick.AddListener(!isDecrease ? Increase : Decrease);
		}

		private void Increase()
		{
			Increase(1);
		}

		private void Decrease()
		{
			Decrease(1);
		}

		public void Decrease(int amount)
		{
			itemCounts[def]-=amount;
			UpdateText();

			if (itemCounts[def] != 0) return;
			itemCounts.Remove(def);
			GameObject.Destroy(button);
		}
		public void Increase(int amount)
		{
			if (!itemCounts.ContainsKey(def))
			{
				countRemoveButton = new InventoryItemAddRemoveButtonGen(def, prefab, itemCounts, removeParent);
				itemCounts.Add(def, 0);
			}
			itemCounts[def] += amount;
			UpdateText();
		}
		public void UpdateText()
		{
			var targetButton = isDecrease ? customButton : countRemoveButton.customButton;
			targetButton.ButtonText.text = $"{Language.GetString(def.nameToken)} x{itemCounts[def]}";
		}

		public void SetAmount(int i)
		{
			if (!itemCounts.ContainsKey(def))
			{
				countRemoveButton = new InventoryItemAddRemoveButtonGen(def, prefab, itemCounts, removeParent);
				itemCounts.Add(def, 0);
				Increase(i);
				return;
			}
			
			if (i == 0)
			{
				itemCounts.Remove(def);
				Object.Destroy(countRemoveButton.button.gameObject);
				return;
			}
			Increase(itemCounts[def] - i);
		}
	}
}