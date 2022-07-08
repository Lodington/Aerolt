using System;
using System.Collections.Generic;
using System.Linq;
using Aerolt.Buttons;
using Aerolt.Classes;
using Aerolt.Helpers;
using Aerolt.Messages;
using JetBrains.Annotations;
using RoR2;
using RoR2.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Aerolt.Managers
{
    public class InteractableManager : MonoBehaviour, IModuleStartup
    {
        public GameObject buttonPrefab;
        public GameObject buttonParent;

        
        [CanBeNull] public static SpawnCard[] _spawnCards; // mmm yummy linq
        private MenuInfo _info;
        public static SpawnCard[] cards => _spawnCards ??= ClassicStageInfo.instance.interactableDccsPool.GenerateWeightedSelection().choices.Where(x => !x.Equals(null) && x.value).Select(x => x.value).Where(x => !x.Equals(null) && x.categories != null).Select(x => x.categories).SelectMany(x => x).Where(x => !x.Equals(null) && !x.cards.Equals(null)).Select(x => x.cards).SelectMany(x => x).Select(x => x.spawnCard).ToArray();

        static InteractableManager()
        {
            Run.onRunStartGlobal += _ => _spawnCards = null; // clear the spawncards so they can be filled if you disable/enable expansions
        }
        public void ModuleStart()
        {
            _info = GetComponentInParent<MenuInfo>();
            foreach (var card in cards)
            {
                if (card.Equals(null) || card.Equals(default)) continue;
                GameObject newButton = Instantiate(buttonPrefab, buttonParent.transform);
                var provider = card.prefab.GetComponentInChildren<IDisplayNameProvider>();

                var buttonComponet = newButton.GetComponent<CustomButton>();
                buttonComponet.buttonText.text = provider != null ? provider.GetDisplayName() : card.name;
                buttonComponet.image.sprite = PingIndicator.GetInteractableIcon(card.prefab);
                buttonComponet.button.onClick.AddListener(() => SpawnInteractable(card));
            }
        }

        public void SpawnInteractable(SpawnCard card)
        {
            if (!_info.Master)
            {
                Tools.Log(Aerolt.Enums.LogLevel.Error, $"Cant Spawn Interactable Localuser Master is null");
                return;
            }

            var body = _info.Body;
            if (!body) //wats a erorr catch?
            {
                Tools.Log(Aerolt.Enums.LogLevel.Error, $"Cant Spawn Interactable Localuser Body is null");
                return;
            }

            var position = body.transform.position;
            var aimRay = body.inputBank.GetAimRay().direction * 1.6f;

            
            if (NetworkServer.active)
            {
                Spawn((uint) Array.IndexOf(cards, card), position + aimRay);
            }
            else
            {
                ClientScene.readyConnection.SendAerolt(new InteractableSpawnMessage((uint) Array.IndexOf(cards, card), position + aimRay));
            }
        }

        public static void Spawn(uint index, Vector3 position)
        {
            var spawnCard = cards[index];
            spawnCard.DoSpawn(position, new Quaternion(), new DirectorSpawnRequest(
                spawnCard,
                new DirectorPlacementRule
                {
                    placementMode = DirectorPlacementRule.PlacementMode.NearestNode,
                    maxDistance = 100f,
                    minDistance = 20f,
                    position = position,
                    preventOverhead = true
                },
                RoR2Application.rng)
            );
        }
    }

}