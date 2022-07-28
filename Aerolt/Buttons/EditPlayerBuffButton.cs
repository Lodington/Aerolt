using System.Collections.Generic;
using System.Linq;
using Aerolt.Managers;
using Aerolt.Messages;
using RoR2;
using RoR2.ContentManagement;
using UnityEngine;

namespace Aerolt.Buttons
{
	public class EditPlayerBuffButton : MonoBehaviour
	{
		public GameObject buttonPrefab;
		public GameObject buttonParent;

		public GameObject itemListParent;
        
		private Dictionary<BuffDef, int> buffDef = new();
		private Dictionary<BuffDef, AddRemoveButtonGen<BuffDef>> buffDefRef = new();
		private NetworkUser user;

		public void Awake()
		{
			foreach (var def in ContentManager.buffDefs)
				buffDefRef[def] = new AddRemoveButtonGen<BuffDef>(def, buttonPrefab, buffDef, buttonParent, itemListParent, false);
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
	}
}