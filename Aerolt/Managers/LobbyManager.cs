using System;
using System.Collections.Generic;
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
        public readonly List<string> Players = new List<string>();

        private NetworkUser _localPlayer;
        
        public GameObject stripParent;
        public GameObject stripPrefab;
        
        public static Dictionary<ItemDef, int> ItemDef = new Dictionary<ItemDef, int>();
        
        public void OnEnable()
        {
            GameObject StripPrefabInstantiated;

            while (stripParent.transform.childCount != 0)
            {
                Destroy(stripParent.transform.GetChild(0));
                Players.RemoveAt(0);
            }
            foreach (var networkUser in NetworkUser.readOnlyInstancesList)
            {
                if (networkUser.isLocalPlayer)
                {
                    _localPlayer = networkUser;
                }
                Players.Add(networkUser.userName);
                StripPrefabInstantiated = Instantiate(stripPrefab, stripParent.transform);
                StripPrefabInstantiated.GetComponent<LobbyManagerStrip>().Init(networkUser, this);
            }
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