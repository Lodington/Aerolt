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

		
		//fuck you bubbet i didnt do anything
		
		
		
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
			Change(1);
		}

		private void Decrease()
		{
			Change(-1);
		}

		public void Change(int relativeAmount)
		{
			if (countRemoveButton == null)
			{
				if (relativeAmount < 0) return;

				countRemoveButton = new InventoryItemAddRemoveButtonGen(def, prefab, itemCounts, removeParent);
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
			targetButton.ButtonText.text = $"{Language.GetString(def.nameToken)} x{itemCounts[def]}";
		}

		public void SetAmount(int i)
		{
			var prev = itemCounts.ContainsKey(def) ? itemCounts[def] : 0;
			if (i == 0 && prev == 0) return;
			Change(i - prev);
		}
	}
}