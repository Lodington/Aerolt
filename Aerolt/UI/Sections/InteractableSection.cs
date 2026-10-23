using System;
using System.Linq;
using Aerolt.Classes;
using Aerolt.Enums;
using Aerolt.Helpers;
using Aerolt.Managers;
using Aerolt.Messages;
using RoR2;
using RoR2.UI;
using UnityEngine;
using UnityEngine.Networking;
using UniverseLib.UI;

namespace Aerolt.UI.Sections
{
    public class InteractableSection : IAeroltSection
    {
        private IconGrid<SpawnCard> _grid = null!;
        private GameObject _gridHost = null!;
        private bool _built;

        public string Title => "Interactables";
        public GameObject Root { get; private set; } = null!;
        public bool ScrollableContent => false;

        public void Build(GameObject contentParent)
        {
            Root = UIFactory.CreateVerticalGroup(contentParent, "InteractableSection", false, false, true, true, 4,
                default, new Color(0.14f, 0.14f, 0.14f));
            UIFactory.SetLayoutElement(Root, flexibleWidth: 9999, flexibleHeight: 9999);

            var refresh = UIFactory.CreateButton(Root, "Refresh", "Refresh list");
            UIFactory.SetLayoutElement(refresh.Component.gameObject, minHeight: 26, flexibleWidth: 9999);
            refresh.OnClick += Rebuild;

            _gridHost = UIFactory.CreateVerticalGroup(Root, "GridHost", false, false, true, true, 0,
                default, new Color(0.14f, 0.14f, 0.14f));
            UIFactory.SetLayoutElement(_gridHost, flexibleWidth: 9999, flexibleHeight: 9999);
        }

        public void OnShown()
        {
            if (!_built)
                Rebuild();
        }

        private void Rebuild()
        {
            foreach (Transform child in _gridHost.transform)
                UnityEngine.Object.Destroy(child.gameObject);

            SpawnCard[] cards;
            try
            {
                cards = AeroltInteractableSpawner.Cards;
            }
            catch (Exception e)
            {
                Tools.Log(LogLevel.Warning, $"Interactables unavailable (not in a stage?): {e.Message}");
                _built = false;
                return;
            }

            AeroltInteractableSpawner.StartOfRoundScaledInteractableCosts.Clear();

            var ordered = cards
                .Where(x => x && x.prefab)
                .OrderBy(DisplayName)
                .ToArray();

            foreach (var card in ordered)
            {
                var purchaseInteraction = card.prefab.GetComponent<PurchaseInteraction>();
                if (purchaseInteraction && purchaseInteraction.costType == CostTypeIndex.Money &&
                    Run.instance &&
                    !AeroltInteractableSpawner.StartOfRoundScaledInteractableCosts.ContainsKey(card))
                    AeroltInteractableSpawner.StartOfRoundScaledInteractableCosts.Add(card,
                        Run.instance.GetDifficultyScaledCost(purchaseInteraction.cost));
            }

            _grid = new IconGrid<SpawnCard>(
                DisplayName,
                (card, icon, stillValid) =>
                {
                    var sprite = PingIndicator.GetInteractableIcon(card.prefab);
                    if (sprite && icon && stillValid()) icon.sprite = sprite;
                },
                onClick: (card, _) => SpawnInteractable(card));

            _grid.Build(_gridHost, ordered);
            _built = true;
        }

        private static string DisplayName(SpawnCard card)
        {
            var provider = card.prefab.GetComponentInChildren<IDisplayNameProvider>();
            return provider != null ? provider.GetDisplayName() : card.name;
        }

        private static void SpawnInteractable(SpawnCard card)
        {
            var localUser = LocalUserManager.GetFirstLocalUser();
            var body = localUser?.cachedBody;
            if (!body)
            {
                Tools.Log(LogLevel.Error, "Can't spawn interactable: local body is null");
                return;
            }

            var position = body!.transform.position;
            var aimRay = body.inputBank.GetAimRay().direction * 1.6f;
            var index = (uint)Array.IndexOf(AeroltInteractableSpawner.Cards, card);

            if (NetworkServer.active)
                AeroltInteractableSpawner.Spawn(index, position + aimRay);
            else
                ClientScene.readyConnection.SendAerolt(new InteractableSpawnMessage(index, position + aimRay));
        }
    }
}
