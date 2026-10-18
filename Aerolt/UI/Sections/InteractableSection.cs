using System;
using System.Collections.Generic;
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
using UnityEngine.UI;
using UniverseLib;
using UniverseLib.UI;

namespace Aerolt.UI.Sections
{
    public class InteractableSection : IAeroltSection
    {
        private readonly Dictionary<SpawnCard, GameObject> _rows = new();
        private GameObject _listContent = null!;
        private bool _built;

        public string Title => "Interactables";
        public GameObject Root { get; private set; } = null!;

        public void Build(GameObject contentParent)
        {
            Root = UIFactory.CreateVerticalGroup(contentParent, "InteractableSection", false, false, true, true, 4,
                default, new Color(0.14f, 0.14f, 0.14f));
            UIFactory.SetLayoutElement(Root, flexibleWidth: 9999, flexibleHeight: 9999);

            var search = UIFactory.CreateInputField(Root, "Search", "Search interactables...");
            UIFactory.SetLayoutElement(search.Component.gameObject, minHeight: 28, flexibleWidth: 9999);
            search.OnValueChanged += FilterUpdated;

            var refresh = UIFactory.CreateButton(Root, "Refresh", "Refresh list");
            UIFactory.SetLayoutElement(refresh.Component.gameObject, minHeight: 26, flexibleWidth: 9999);
            refresh.OnClick += Rebuild;

            UIFactory.CreateScrollView(Root, "List", out _listContent, out _, new Color(0.1f, 0.1f, 0.1f));
            UIFactory.SetLayoutGroup<VerticalLayoutGroup>(_listContent, false, false, true, true, 2, 2, 2, 2, 2);
        }

        public void OnShown()
        {
            if (_rows.Count == 0)
                Rebuild();
        }

        private void Rebuild()
        {
            foreach (var go in _rows.Values)
                if (go) UnityEngine.Object.Destroy(go);
            _rows.Clear();
            _built = false;

            RuntimeHelper.StartCoroutine(BuildRows());
        }

        private System.Collections.IEnumerator BuildRows()
        {
            SpawnCard[] cards;
            try
            {
                cards = AeroltInteractableSpawner.Cards;
            }
            catch (Exception e)
            {
                Tools.Log(LogLevel.Warning, $"Interactables unavailable (not in a stage?): {e.Message}");
                yield break;
            }

            AeroltInteractableSpawner.StartOfRoundScaledInteractableCosts.Clear();

            var ordered = cards
                .Where(x => !x.Equals(null) && !x.Equals(default) && x.prefab)
                .Select(card =>
                {
                    var provider = card.prefab.GetComponentInChildren<IDisplayNameProvider>();
                    var name = provider != null ? provider.GetDisplayName() : card.name;
                    return (card, name);
                })
                .OrderBy(x => x.name)
                .ToArray();

            const int batchSize = 15;
            var count = 0;

            foreach (var (card, displayName) in ordered)
            {
                var captured = card;

                var row = UIFactory.CreateHorizontalGroup(_listContent, "Row", false, false, true, true, 4,
                    new Vector4(2, 2, 2, 2));
                UIFactory.SetLayoutElement(row, minHeight: 30, flexibleWidth: 9999);

                var iconObj = UIFactory.CreateUIObject("Icon", row);
                UIFactory.SetLayoutElement(iconObj, minWidth: 24, minHeight: 24, preferredWidth: 24,
                    preferredHeight: 24, flexibleWidth: 0, flexibleHeight: 0);
                var icon = iconObj.AddComponent<Image>();
                icon.preserveAspect = true;
                var sprite = PingIndicator.GetInteractableIcon(card.prefab);
                if (sprite) icon.sprite = sprite;

                var btn = UIFactory.CreateButton(row, "Spawn", displayName);
                UIFactory.SetLayoutElement(btn.Component.gameObject, minHeight: 26, flexibleWidth: 9999);
                btn.OnClick += () => SpawnInteractable(captured);

                _rows[card] = row;

                var purchaseInteraction = card.prefab.GetComponent<PurchaseInteraction>();
                if (purchaseInteraction && purchaseInteraction.costType == CostTypeIndex.Money &&
                    Run.instance && !AeroltInteractableSpawner.StartOfRoundScaledInteractableCosts.ContainsKey(card))
                    AeroltInteractableSpawner.StartOfRoundScaledInteractableCosts.Add(card,
                        Run.instance.GetDifficultyScaledCost(purchaseInteraction.cost));

                if (++count < batchSize) continue;
                count = 0;
                yield return null;
            }

            _built = true;
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

        private void FilterUpdated(string text)
        {
            if (!_built) return;
            var lower = text?.ToLowerInvariant() ?? "";
            foreach (var kvp in _rows)
            {
                var provider = kvp.Key.prefab.GetComponentInChildren<IDisplayNameProvider>();
                var name = (provider != null ? provider.GetDisplayName() : kvp.Key.name).ToLowerInvariant();
                kvp.Value.SetActive(string.IsNullOrEmpty(lower) || name.Contains(lower));
            }
        }
    }
}
