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

		public InventoryItemAddRemoveButtonGen(ItemDef def, GameObject prefabIn, Dictionary<ItemDef, int> itemDefDictionary, GameObject parentIn = null, GameObject removeParentIn = null, bool doDestroy = true)
		{
			this.def = def;
			prefab = prefabIn;
			isDecrease = doDestroy;
			button = CreateButton();
			customButton = button.GetComponent<CustomButton>();
			itemCounts = itemDefDictionary;
			parent = parentIn;
			removeParent = removeParentIn;
		}
		public GameObject CreateButton()
		{
			var newButton = GameObject.Instantiate(prefab, parent.transform);
            
			newButton.GetComponent<CustomButton>().ButtonText.text = Language.GetString(def.nameToken);
			newButton.GetComponent<Image>().sprite = def.pickupIconSprite;
			newButton.GetComponent<Button>().onClick.AddListener(!isDecrease ? Increase : Decrease);
			return newButton;
		}
		public void Decrease()
		{
			itemCounts[def]--;
			UpdateText();

			if (itemCounts[def] != 0) return;
			itemCounts.Remove(def);
			GameObject.Destroy(button);
		}
		public void Increase()
		{
			if (!itemCounts.ContainsKey(def))
			{
				countRemoveButton = new InventoryItemAddRemoveButtonGen(def, removeParent, itemCounts);
				itemCounts.Add(def, 0);
			}
			itemCounts[def]++;
			UpdateText();
		}
		public void UpdateText()
		{
			var targetButton = isDecrease ? customButton : countRemoveButton.customButton;
			targetButton.ButtonText.text = $"{Language.GetString(def.nameToken)} x{itemCounts[def]}";
		}
	}
}