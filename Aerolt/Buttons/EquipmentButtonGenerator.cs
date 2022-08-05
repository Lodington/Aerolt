using System.Collections.Generic;
using System.Linq;
using Aerolt.Helpers;
using Aerolt.Managers;
using Aerolt.Messages;
using BepInEx;
using RoR2;
using RoR2.ContentManagement;
using TMPro;
using UnityEngine;

namespace Aerolt.Buttons
{
    public class EquipmentButtonGenerator : MonoBehaviour
    {
        public GameObject buttonPrefab;
        public GameObject buttonParent;
        public TMP_InputField searchFilter;
        private readonly Dictionary<EquipmentDef, CustomButton> equipmentDefRef = new();

        private NetworkUser target;

        private void Awake()
        {
            foreach (var def in ContentManager._equipmentDefs.OrderBy(x => Language.GetString(x.nameToken)))
            {
                var newButton = Instantiate(buttonPrefab, buttonParent.transform);
                var customButton = newButton.GetComponent<CustomButton>();
                customButton.buttonText.text = Language.GetString(def.nameToken);
                customButton.image.sprite = def.pickupIconSprite;
                customButton.button.onClick.AddListener(() => SetEquipmentDef(def));
                equipmentDefRef[def] = customButton;
            }

            if (searchFilter)
                searchFilter.onValueChanged.AddListener(FilterUpdated);
        }

        public void SetEquipmentDef(EquipmentDef def)
        {
            Dictionary<EquipmentDef, int> equipmentDef = new();
            equipmentDef.Add(def, 1);
            new SetEquipmentMessage(target.master.inventory, equipmentDef).SendToServer();
            GetComponentInParent<LobbyPlayerPageManager>().SwapViewState();
        }

        public void Initialize(NetworkUser currentUser)
        {
            target = currentUser;
        }

        private void FilterUpdated(string text)
        {
            if (text.IsNullOrWhiteSpace())
            {
                foreach (var buttonGen in equipmentDefRef) buttonGen.Value.gameObject.SetActive(true);
                return;
            }

            var arr = equipmentDefRef.Values.ToArray();
            var matches = Tools.FindMatches(arr, x => x.buttonText.text, text);
            foreach (var buttonGen in arr) buttonGen.gameObject.SetActive(matches.Contains(buttonGen));
        }
    }
}