using System;
using System.Collections.Generic;
using System.Linq;
using Aerolt.Buttons;
using Aerolt.Classes;
using Aerolt.Enums;
using Aerolt.Helpers;
using Aerolt.Messages;
using BepInEx;
using JetBrains.Annotations;
using RoR2;
using RoR2.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

namespace Aerolt.Managers
{
    public class InteractableManager : MonoBehaviour, IModuleStartup
    {
        [CanBeNull] public static SpawnCard[] _spawnCards; // mmm yummy linq
        public GameObject buttonPrefab;
        public GameObject buttonParent;
        public TMP_InputField searchFilter;
        private MenuInfo _info;
        private readonly Dictionary<SpawnCard, CustomButton> cardDefRef = new();
        private static bool isScalingInteractablePricesConstantly = false;

        static InteractableManager()
        {
            Run.onRunStartGlobal +=
                _ => _spawnCards = null; // clear the spawncards so they can be filled if you disable/enable expansions
        }

        public static SpawnCard[] cards => _spawnCards ??= ClassicStageInfo.instance.interactableDccsPool
            .GenerateWeightedSelection().choices.Where(x => !x.Equals(null) && x.value).Select(x => x.value)
            .Where(x => !x.Equals(null) && x.categories != null).Select(x => x.categories).SelectMany(x => x)
            .Where(x => !x.Equals(null) && !x.cards.Equals(null)).Select(x => x.cards).SelectMany(x => x)
            .Select(x => x.spawnCard).Union(FindObjectOfType<SceneDirector>().GenerateInteractableCardSelection()
                .choices.Where(x => x.value != null && x.value.spawnCard != null).Select(x => x.value.spawnCard))
            .ToArray();

        public static Dictionary<SpawnCard, int> startOfRoundScaledInteractableCosts = new();

        public void ModuleStart()
        {
            _info = GetComponentInParent<MenuInfo>();
            startOfRoundScaledInteractableCosts.Clear();
            foreach (var card in cards.OrderBy(x =>
                         x.prefab.GetComponentInChildren<IDisplayNameProvider>() != null
                             ? x.prefab.GetComponentInChildren<IDisplayNameProvider>().GetDisplayName()
                             : x.name))
            {
                if (card.Equals(null) || card.Equals(default)) continue;
                var newButton = Instantiate(buttonPrefab, buttonParent.transform);
                var provider = card.prefab.GetComponentInChildren<IDisplayNameProvider>();

                var buttonComponet = newButton.GetComponent<CustomButton>();
                buttonComponet.buttonText.text = provider != null ? provider.GetDisplayName() : card.name;
                buttonComponet.image.sprite = PingIndicator.GetInteractableIcon(card.prefab);
                buttonComponet.button.onClick.AddListener(() => SpawnInteractable(card));
                cardDefRef[card] = buttonComponet;

                var prefab = card.prefab;
                var purchaseInteraction = prefab.GetComponent<PurchaseInteraction>();
                if (purchaseInteraction && purchaseInteraction.costType == CostTypeIndex.Money)
                {
                    var scaledCost = Run.instance.GetDifficultyScaledCost(purchaseInteraction.cost);
                    startOfRoundScaledInteractableCosts.Add(card, scaledCost);
                }
            }

            if (searchFilter)
                searchFilter.onValueChanged.AddListener(FilterUpdated);
        }

        public void SpawnInteractable(SpawnCard card)
        {
            if (!_info.Master)
            {
                Tools.Log(LogLevel.Error, "Cant Spawn Interactable Localuser Master is null");
                return;
            }

            var body = _info.Body;
            if (!body) //wats a erorr catch?
            {
                Tools.Log(LogLevel.Error, "Cant Spawn Interactable Localuser Body is null");
                return;
            }

            var position = body.transform.position;
            var aimRay = body.inputBank.GetAimRay().direction * 1.6f;


            if (NetworkServer.active)
                Spawn((uint)Array.IndexOf(cards, card), position + aimRay);
            else
                ClientScene.readyConnection.SendAerolt(new InteractableSpawnMessage((uint)Array.IndexOf(cards, card),
                    position + aimRay));
        }

        public static void Spawn(uint index, Vector3 position)
        {
            var spawnCard = cards[index];
            var placementRule = new DirectorPlacementRule
            {
                placementMode = DirectorPlacementRule.PlacementMode.NearestNode,
                maxDistance = 30f,
                minDistance = 10f,
                position = position,
                preventOverhead = true
            };
            var directorSpawnRequest = new DirectorSpawnRequest(spawnCard, placementRule, RoR2Application.rng);
            var interactableObject = DirectorCore.instance.TrySpawnObject(directorSpawnRequest);

            if (!interactableObject) return;
            var purchaseInteraction = interactableObject.GetComponent<PurchaseInteraction>();
            if (purchaseInteraction && purchaseInteraction.costType == CostTypeIndex.Money)
                purchaseInteraction.Networkcost = isScalingInteractablePricesConstantly
                    ? Run.instance.GetDifficultyScaledCost(purchaseInteraction.cost)
                    : startOfRoundScaledInteractableCosts[spawnCard];
        }

        private void FilterUpdated(string text)
        {
            if (text.IsNullOrWhiteSpace())
            {
                foreach (var buttonGen in cardDefRef) buttonGen.Value.gameObject.SetActive(true);
                return;
            }

            var arr = cardDefRef.Values.ToArray();
            var matches = Tools.FindMatches(arr, x => x.buttonText.text, text);
            foreach (var buttonGen in arr) buttonGen.gameObject.SetActive(matches.Contains(buttonGen));
        }
    }
}