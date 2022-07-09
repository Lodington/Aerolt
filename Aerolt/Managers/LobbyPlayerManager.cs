using System;
using System.Collections.Generic;
using System.Linq;
using Aerolt.Buttons;
using Aerolt.Classes;
using RoR2;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Aerolt.Managers
{
	[RequireComponent(typeof(ToggleGroup))]
	public class LobbyPlayerManager : MonoBehaviour, IModuleStartup
	{
		public GameObject playerEntryPrefab;
		public Transform playerEntryParent;
		public UserChangedEvent userChanged;
		private readonly Dictionary<NetworkUser, CustomButton> users = new();
		private ToggleGroup toggleGroup;
		public void ModuleStart()
		{
			// var wasActive = gameObject.activeSelf;
			// gameObject.SetActive(true);
			NetworkUser.onNetworkUserDiscovered += UserAdded;
			NetworkUser.onNetworkUserLost += UserLost;

			toggleGroup = GetComponent<ToggleGroup>();

			if (userChanged == null) // I give up
			{
				userChanged = new UserChangedEvent();
				userChanged.AddListener(GetComponent<LobbyPlayerPageManager>().SetUser);
			}
			
			foreach (var networkUser in NetworkUser.instancesList) UserAdded(networkUser);
			// gameObject.SetActive(wasActive);
		}
		private void OnEnable()
		{
			foreach (var user in users.Keys) UpdateUserLobbyButton(user);
		}
		private void UpdateUserLobbyButton(NetworkUser user)
		{
			var button = users[user];
			button.buttonText.text = user.userName;
			button.rawImage.texture = user.master.bodyPrefab.GetComponent<CharacterBody>().portraitIcon;
		}
		private void OnDestroy()
		{
			NetworkUser.onNetworkUserDiscovered -= UserAdded;
			NetworkUser.onNetworkUserLost -= UserLost;
		}

		private void UserAdded(NetworkUser user)
		{
			if (users.ContainsKey(user)) return;
			var button = Instantiate(playerEntryPrefab, playerEntryParent, false).GetComponent<CustomButton>();
			var toggle = button.GetComponent<Toggle>();
			toggle.onValueChanged.AddListener(val => { if (val) SetUser(user); });
			toggle.group = toggleGroup;
			if (users.Count == 0) toggle.isOn = true;
			users[user] = button;
		}
		private void UserLost(NetworkUser user)
		{
			if (!users.ContainsKey(user)) return;
			Destroy(users[user].gameObject);
			users.Remove(user);
		}
		private void SetUser(NetworkUser user)
		{
			userChanged?.Invoke(user);
		}

		[Serializable]
		public class UserChangedEvent : UnityEvent<NetworkUser> {}
	}
}