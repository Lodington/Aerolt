using System;
using System.Collections.Generic;
using System.Linq;
using Aerolt.Classes;
using RoR2;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using Console = RoR2.Console;

namespace Aerolt.Managers
{
    public class LobbyManager : MonoBehaviour
    {
        public readonly Dictionary<NetworkUser, LobbyManagerStrip> Players = new();

        private NetworkUser _localPlayer;
        
        public GameObject stripParent;
        public GameObject stripPrefab;
        
        private NetworkUser[] cachedUsers = {};

        public void OnEnable()
        {
            GameObject StripPrefabInstantiated;

            var array = NetworkUser.readOnlyInstancesList.ToArray();
            var newUsers = array.Except(cachedUsers).ToArray();
            foreach (var networkUser in newUsers)
            {
                if (networkUser.isLocalPlayer)
                {
                    _localPlayer = networkUser;
                }
                StripPrefabInstantiated = Instantiate(stripPrefab, stripParent.transform);
                var strip = StripPrefabInstantiated.GetComponent<LobbyManagerStrip>();
                strip.Init(networkUser, this);
                Players.Add(networkUser, strip);
            }
            var removedUsers = cachedUsers.Except(array).ToArray();
            foreach (var user in removedUsers)
            {
                Destroy(Players[user].gameObject);
                Players.Remove(user);
            }

            cachedUsers = array;
        }
        
        public NetworkUser GetNetUserFromString(string playerString)
        {
            if (playerString != "")
                
                if (int.TryParse(playerString, out int result))
                {
                    if (result < NetworkUser.readOnlyInstancesList.Count && result >= 0)
                        return NetworkUser.readOnlyInstancesList[result];
                    return null;
                }
                else
                {
                    foreach (NetworkUser n in NetworkUser.readOnlyInstancesList)
                        if (n.userName.Equals(playerString, StringComparison.CurrentCultureIgnoreCase))
                            return n;
                    return null;
                }

            return null;
        }

        public void KickPlayer(TMP_Text playerName)
        {
            Console.instance.RunClientCmd(_localPlayer, "kick_steam", new string[]
            {
                GetNetUserFromString(playerName.text).Network_id.steamId.ToString()
            });
        }

        public void BanPlayer(TMP_Text playerName)
        {
            Console.instance.RunClientCmd(_localPlayer, "ban_steam", new string[]
            {
                GetNetUserFromString(playerName.text).Network_id.steamId.ToString()
            });
        }

    }
}