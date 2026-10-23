using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using RoR2;
using UnityEngine;

namespace Aerolt.Classes
{

    public static class AeroltInteractableSpawner
    {
        [CanBeNull] private static SpawnCard[]? _spawnCards;

        private const bool IsScalingInteractablePricesConstantly = false;

        static AeroltInteractableSpawner()
        {

            Run.onRunStartGlobal += _ => _spawnCards = null;
        }

        public static Dictionary<SpawnCard, int> StartOfRoundScaledInteractableCosts { get; } = new();

        public static SpawnCard[] Cards => _spawnCards ??= BuildCards();

        private static SpawnCard[] BuildCards()
        {

            var all = Resources.FindObjectsOfTypeAll<InteractableSpawnCard>()
                .Where(x => x && x.prefab)
                .Cast<SpawnCard>();

            IEnumerable<SpawnCard> stagePool = System.Array.Empty<SpawnCard>();
            try
            {
                stagePool = ClassicStageInfo.instance.interactableDccsPool
                    .GenerateWeightedSelection().choices.Where(x => !x.Equals(null) && x.value).Select(x => x.value)
                    .Where(x => !x.Equals(null) && x.categories != null).Select(x => x.categories).SelectMany(x => x)
                    .Where(x => !x.Equals(null) && !x.cards.Equals(null)).Select(x => x.cards).SelectMany(x => x)
                    .Select(x => x.spawnCard)
                    .Union(Object.FindObjectOfType<SceneDirector>().GenerateInteractableCardSelection()
                        .choices.Where(x => x.value != null && x.value.spawnCard != null)
                        .Select(x => x.value.spawnCard));
            }
            catch
            {
                
            }

            return all.Union(stagePool)
                .Where(x => x && x.prefab)
                .Distinct()
                .ToArray();
        }

        public static void Spawn(uint index, Vector3 position)
        {
            var spawnCard = Cards[index];
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
                purchaseInteraction.Networkcost = IsScalingInteractablePricesConstantly
                    ? Run.instance.GetDifficultyScaledCost(purchaseInteraction.cost)
                    : StartOfRoundScaledInteractableCosts[spawnCard];
        }
    }
}
