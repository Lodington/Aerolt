using System;
using System.Collections.Generic;
using System.Linq;
using Aerolt.Buttons;
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
    public class InteractableManager : MonoBehaviour
    {
        public GameObject buttonPrefab;
        public GameObject buttonParent;

        public TMP_Text interactableText;
        public static List<SpawnCard> Cards = new List<SpawnCard>();

        private SpawnCard _spawnCard;

        public static Dictionary<SpawnCard, GameObject> interactableButtons = new();
        private List<SpawnCard> cachedCards = new();

        private void Awake()
        {
            //var newCards = Cards.Except(cachedCards).ToArray();
            //if (newCards.Any())
            {
                // add new buttons
                foreach (var card in cards) //newCards)
                {
                    if (card.Equals(null) || card.Equals(default)) continue;
                    GameObject newButton = Instantiate(buttonPrefab, buttonParent.transform);
                    var provider = card.prefab.GetComponentInChildren<IDisplayNameProvider>();
                    newButton.GetComponent<CustomButton>().ButtonText.text =
                        provider != null ? provider.GetDisplayName() : card.name; // Language.GetString(card.name);
                    newButton.GetComponent<Image>().sprite = PingIndicator.GetInteractableIcon(card.prefab);
                    newButton.GetComponent<Button>().onClick.AddListener(() => SetInteractable(card));
                    //interactableButtons.Add(card, newButton);
                }
            }

            /*
            var removedCards = cachedCards.Except(Cards).ToArray();
            if (removedCards.Any())
            {
                // remove cards
                foreach (var card in removedCards)
                {
                    Destroy(interactableButtons[card]);
                    interactableButtons.Remove(card);
                }
            }

            cachedCards = Cards;
            */
        }

        static InteractableManager()
        {
            Run.onRunStartGlobal += _ => _spawnCards = null; // clear the spawncards so they can be filled if you disable/enable expansions
        }

        [CanBeNull] public static SpawnCard[] _spawnCards; // mmm yummy linq
        public static SpawnCard[] cards => _spawnCards ??= ClassicStageInfo.instance.interactableDccsPool.GenerateWeightedSelection().choices.Where(x => !x.Equals(null) && x.value).Select(x => x.value).Where(x => !x.Equals(null) && x.categories != null).Select(x => x.categories).SelectMany(x => x).Where(x => !x.Equals(null) && !x.cards.Equals(null)).Select(x => x.cards).SelectMany(x => x).Select(x => x.spawnCard).ToArray();

        public void SpawnInteractable()
        {
            var user = GetUser.FetchUser(GetComponentInParent<PanelManager>().hud);
            
            var master = user.cachedMaster;
            if (!master)
            {
                Tools.Log(Aerolt.Enums.LogLevel.Error, $"Cant Spawn Interactable Localuser Master is null");
                return;
            }
                
            var body = master.GetBody();
            if (!body) //wats a erorr catch?
            {
                Tools.Log(Aerolt.Enums.LogLevel.Error, $"Cant Spawn Interactable Localuser Body is null");
                return;
            }

            var position = body.transform.position;
            var aimRay = body.inputBank.GetAimRay().direction * 1.6f;

            
            if (NetworkServer.active)
            {
                Spawn((uint) Array.IndexOf(cards, _spawnCard), position + aimRay);
            }
            else
            {
                ClientScene.readyConnection.SendAerolt(new InteractableSpawnMessage((uint) Array.IndexOf(cards, _spawnCard), position + aimRay));
            }
        }

        public void SetInteractable(SpawnCard card)
        {
            var provider = card.prefab.GetComponentInChildren<IDisplayNameProvider>();
            interactableText.text = provider != null ? provider.GetDisplayName() : card.name; 
            //interactableText.text = card.name;
            _spawnCard = card;
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