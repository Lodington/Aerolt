using System.Collections.Generic;
using Rewired;
using RoR2;
using UnityEngine;
using UnityEngine.UI;

namespace Aerolt.Buttons
{
	
	public class AddRemoveButtonGen<T>
	{
		public static AddRemoveButtonGen<T> Create(T def1, GameObject gameObject, Dictionary<T,int> dictionary, GameObject o)
		{
			return new(def1, gameObject, dictionary, o);
		}
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
			button.GetComponent<Button>().onClick.AddListener(!isDecrease ? Increase : Decrease);

			switch (defIn) // Generics are fucked
			{
				case ItemDef itemDef:
					customButton.ButtonText.text = Language.GetString(itemDef.nameToken);
					button.GetComponent<Image>().sprite = itemDef.pickupIconSprite;
					break;
				case EquipmentDef eqDef:
					customButton.ButtonText.text = Language.GetString(eqDef.nameToken);
					button.GetComponent<Image>().sprite = eqDef.pickupIconSprite;
					break;
				case BuffDef buffDef:
					customButton.ButtonText.text = buffDef.name;
					button.GetComponent<Image>().sprite = buffDef.iconSprite;
					break;
			}
		}
		
		
		private void Increase()
		{
			if (Input.GetKey(KeyCode.LeftShift))
			{
				Change(5);
				return;
			}
			Change(1);
		}

		private void Decrease()
		{
			if (Input.GetKey(KeyCode.LeftShift))
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

				countRemoveButton = Create(def, prefab, itemCounts, removeParent);
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
			targetButton.ButtonText.text = def switch // Generics are fucked
			{
				ItemDef itemDef => $"{Language.GetString(itemDef.nameToken)} x{itemCounts[def]}",
				EquipmentDef eqDef => $"{Language.GetString(eqDef.nameToken)} x{itemCounts[def]}",
				BuffDef buffDef => $"{buffDef.name} x{itemCounts[def]}",
				_ => targetButton.ButtonText.text
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