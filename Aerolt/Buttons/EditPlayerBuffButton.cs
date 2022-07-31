using System.Collections.Generic;
using System.Linq;
using Aerolt.Helpers;
using Aerolt.Managers;
using Aerolt.Messages;
using BepInEx;
using RoR2;
using RoR2.ContentManagement;
using TMPro;
using UnityEngine;

namespace Aerolt.Buttons
{
	public class EditPlayerBuffButton : MonoBehaviour
	{
		public GameObject buttonPrefab;
		public GameObject buttonParent;

		public GameObject itemListParent;
		public TMP_InputField searchFilter;
        
		private Dictionary<BuffDef, int> buffDef = new();
		private Dictionary<BuffDef, AddRemoveButtonGen<BuffDef>> buffDefRef = new();
		private NetworkUser user;

		public void Awake()
		{
			foreach (var def in ContentManager.buffDefs.OrderBy(x => x.name))
				buffDefRef[def] = new AddRemoveButtonGen<BuffDef>(def, buttonPrefab, buffDef, buttonParent, itemListParent, false);
			if(searchFilter)
				searchFilter.onValueChanged.AddListener(FilterUpdated);
		}

		public void Initialize(NetworkUser userIn)
		{
			user = userIn;
			var body = user.master.GetBody();
			foreach (var def in ContentManager.buffDefs) buffDefRef[def].SetAmount(body ? body.GetBuffCount(def) : 0);
		}

		public void GiveBuffs()
		{
			var body = user.master.GetBody();
			if (!body) return;
			new SetBuffCountMessage(body, buffDef.ToDictionary(x => x.Key.buffIndex, x => (uint) x.Value)).SendToServer();
			GetComponentInParent<LobbyPlayerPageManager>().SwapViewState();
		}
		
		private void FilterUpdated(string text)
		{
			if (text.IsNullOrWhiteSpace())
			{
				foreach (var buttonGen in buffDefRef)
				{
					buttonGen.Value.button.SetActive(true);
				}
				return;
			}

			var arr = buffDefRef.Values.ToArray();
			var matches = Tools.FindMatches(arr, x => x.def.name, text);
			foreach (var buttonGen in arr)
			{
				buttonGen.button.SetActive(matches.Contains(buttonGen));
			}
		}
	}
}