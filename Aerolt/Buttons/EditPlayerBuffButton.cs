using System.Collections.Generic;
using System.Linq;
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
			foreach (var (key, value) in buffDef)
			{
				body.SetBuffCount(key.buffIndex, value);
			}
		}
	}
}