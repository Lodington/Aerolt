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
		public UserChangedEvent userChanged;
		public readonly Dictionary<NetworkUser, PlayerConfigBinding> users = new();
		private ToggleGroup toggleGroup;
		private NetworkUser selectedUser;
		private MenuInfo info;

		public void ModuleStart()
		{
			// var wasActive = gameObject.activeSelf;
			// gameObject.SetActive(true);
			NetworkUser.onPostNetworkUserStart += UserAdded;
			//NetworkUser.onNetworkUserDiscovered += UserAdded;
			NetworkUser.onNetworkUserLost += UserLost;

			info = GetComponent<MenuInfo>();
			toggleGroup = GetComponent<ToggleGroup>();

			if (userChanged == null) // I give up
			{
				userChanged = new UserChangedEvent();
				userChanged.AddListener(GetComponent<LobbyPlayerPageManager>().SetUser);
			}
			
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
			var body = user.master.GetBody();
			if (body)
				LobbyPlayerPageManager.ApplyValues(body, users[user]); // TODO move this somewhere, it doesnt work for multiplayer, fires too early.
			var toggle = button.GetComponent<Toggle>();
			toggle.onValueChanged.AddListener(val => { if (val) SetUser(user); });
			toggle.group = toggleGroup;
			if (user == info.Owner) toggle.isOn = true;
		}
		private void UserLost(NetworkUser user)
		{
			if (!users.ContainsKey(user)) return;
			Destroy(users[user].customButton.gameObject);
			users.Remove(user);
			if (selectedUser == user && users.Any())
				SetUser(users.Keys.Last());
		}
		private void SetUser(NetworkUser user)
		{
			selectedUser = user;
			userChanged?.Invoke(user);
		}

		[Serializable]
		public class UserChangedEvent : UnityEvent<NetworkUser> {}
	}
}