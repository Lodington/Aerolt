using System;
using System.Collections.Generic;
using System.Linq;
using Aerolt.Buttons;
using Aerolt.Helpers;
using RoR2;
using RoR2.UI;
using TMPro;
using UnityEngine;
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

        public static List<GameObject> interactableButtons = new List<GameObject>();
        private void OnEnable()
        {
            
            if (!Cards.Any())
                return;
            if(interactableButtons.Count > buttonParent.transform.childCount)
                
                foreach (var card in Cards)
                {
                    GameObject newButton = Instantiate(buttonPrefab, buttonParent.transform);
                    newButton.GetComponent<CustomButton>().ButtonText.text = Language.GetString(card.name);
                    newButton.GetComponent<Image>().sprite = PingIndicator.GetInteractableIcon(card.prefab);
                    newButton.GetComponent<Button>().onClick.AddListener(() => SetInteractable(card));
                    interactableButtons.Add(newButton);
                }
        }
        
        public void SpawnInteractable()
        {
            var user = GetUser.FetchUser(GetComponentInParent<HUD>());
            
            var master = user.cachedMaster;
            if(!master)
                return;
            var body = master.GetBody();
            if(!body) //wats a erorr catch?
                return;

            var position = body.transform.position;
            var aimRay = body.inputBank.GetAimRay().direction * 1.6f;
            
            _spawnCard.DoSpawn(position + aimRay, new Quaternion(), new DirectorSpawnRequest(
                _spawnCard,
                new DirectorPlacementRule
                {
                    placementMode = DirectorPlacementRule.PlacementMode.NearestNode,
                    maxDistance = 100f,
                    minDistance = 20f,
                    position = position + aimRay,
                    preventOverhead = true
                },
                RoR2Application.rng)
            );
        }

        public void SetInteractable(SpawnCard card)
        {
            interactableText.text = card.name;
            _spawnCard = card;
        }
        
    }

}