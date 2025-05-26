

using Aerolt_External.Messages;
using JetBrains.Annotations;
using Newtonsoft.Json;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using WebSocketSharp.Net.WebSockets;

namespace Aerolt_External.Commands;

public class SpawnInteractable : IWebsocketCommand
{
    // user, intactable, position
    
    public static SpawnCard[]? _spawnCards; // mmm yummy linq
    private static bool isScalingInteractablePricesConstantly = false;
    public static SpawnCard[] cards => _spawnCards ??= 
        ClassicStageInfo.instance.interactableDccsPool
        .GenerateWeightedSelection().choices
        .Where(x => !x.Equals(null) && x.value)
        .Select(x => x.value)
        .Where(x => !x.Equals(null) && x.categories != null)
        .Select(x => x.categories)
        .SelectMany(x => x)
        .Where(x => !x.Equals(null) && !x.cards.Equals(null))
        .Select(x => x.cards)
        .SelectMany(x => x)
        .Select(x => x.spawnCard)
        .Union(UnityEngine.Object.FindObjectOfType<SceneDirector>().GenerateInteractableCardSelection()
            .choices.Where(x => x.value != null && x.value.spawnCard != null).Select(x => x.value.spawnCard))
        .ToArray();
    public static Dictionary<SpawnCard, int> startOfRoundScaledInteractableCosts = new();
    
    /*
     *
     *
     * sendCommand("spawnInteractable", {
     *      card: "cardname",
     *      player: "playerid",
     */
    public void Execute(WebSocketContext context)
    {
        startOfRoundScaledInteractableCosts.Clear();
     
        var user = NetworkUser.instancesList.FirstOrDefault(user =>
            user.userName != null &&
            user.userName.Equals(playerID, StringComparison.OrdinalIgnoreCase));

        var position = user.master.GetBody().corePosition;
        var aimRay = user.master.GetBody().inputBank.GetAimRay().direction * 1.6f;
        
        if (NetworkServer.active)
            Spawn((uint)Array.IndexOf(cards, card), position + aimRay);
        else
            ClientScene.readyConnection.SendAerolt(new InteractableSpawnMessage((uint)Array.IndexOf(cards, card),
                position + aimRay));
    }


    public string card;
    public string playerID;
    
    
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
}