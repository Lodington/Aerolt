using System;
using System.Collections.Generic;
using System.Linq;
using Aerolt.Buttons;
using Aerolt.Classes;
using RoR2;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Aerolt.Managers
{
	[RequireComponent(typeof(ToggleGroup))]
	public class LobbyPlayerManager : MonoBehaviour, IModuleStartup
	{
		public GameObject playerEntryPrefab;
		public Transform playerEntryParent;
		public readonly Dictionary<NetworkUser, PlayerConfigBinding> users = new();
		private ToggleGroup toggleGroup;
		private NetworkUser selectedUser;
		private MenuInfo info;
		private LobbyPlayerPageManager _pageManager;

		public void ModuleStart()
		{
			// var wasActive = gameObject.activeSelf;
			// gameObject.SetActive(true);
			NetworkUser.onPostNetworkUserStart += UserAdded;
			//NetworkUser.onNetworkUserDiscovered += UserAdded;
			NetworkUser.onNetworkUserLost += UserLost;

			info = GetComponentInParent<MenuInfo>();
			toggleGroup = GetComponent<ToggleGroup>();

			// I give up
			_pageManager = GetComponent<LobbyPlayerPageManager>();
			
			foreach (var networkUser in NetworkUser.instancesList) UserAdded(networkUser);
			// gameObject.SetActive(wasActive);
		}

		void IModuleStartup.ModuleEnd()
		{
			NetworkUser.onPostNetworkUserStart -= UserAdded;
			//NetworkUser.onNetworkUserDiscovered -= UserAdded;
			NetworkUser.onNetworkUserLost -= UserLost;
		}

		private void OnEnable()
		{
			foreach (var user in users.Keys) UpdateUserLobbyButton(user);
		}
		private void UpdateUserLobbyButton(NetworkUser user)
		{
			var button = users[user].customButton;
			button.buttonText.text = user.userName;
			if (user.master.bodyPrefab)
				button.rawImage.texture = user.master.bodyPrefab.GetComponent<CharacterBody>().portraitIcon;
		}

		private void UserAdded(NetworkUser user)
		{
			if (users.ContainsKey(user)) return;
			var button = Instantiate(playerEntryPrefab, playerEntryParent, false).GetComponent<CustomButton>();
			users[user] = new PlayerConfigBinding(user, button);
			var toggle = button.GetComponent<Toggle>();
			toggle.group = toggleGroup;
			toggle.onValueChanged.AddListener(val => { if (val) SetUser(user); });
			if (user == info.Owner) toggle.Set(true);
			if (NetworkServer.active) BodyStart(user.master.GetBody());
			user.master.onBodyStart += BodyStart;
		}

		private void BodyStart(CharacterBody body)
		{
			if (!body) return;
			var user = body.master.playerCharacterMasterController.networkUser;
			users[user].Sync();
		}

		private void UserLost(NetworkUser user)
		{
			if (!users.ContainsKey(user)) return;
			Destroy(users[user].customButton.gameObject);
			users.Remove(user);
			if (selectedUser == user && users.Any())
				SetUser(users.Keys.Last());
			user.master.onBodyStart -= BodyStart;
		}
		private void SetUser(NetworkUser user)
		{
			selectedUser = user;
			_pageManager.SetUser(user);
		}
	}
}