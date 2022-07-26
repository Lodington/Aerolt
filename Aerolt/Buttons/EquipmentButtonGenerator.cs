using Aerolt.Classes;
using Aerolt.Helpers;
using Aerolt.Managers;
using RoR2;
using RoR2.ContentManagement;
using RoR2.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Aerolt.Buttons
{
    public class EquipmentButtonGenerator : MonoBehaviour
    {
        public GameObject buttonPrefab;
        public GameObject buttonParent;
        
        private EquipmentDef _equipmentDef;
        private NetworkUser target;

        private void Awake()
        {
            foreach (var def in ContentManager._equipmentDefs)
            {
                var newButton = Instantiate(buttonPrefab, buttonParent.transform);
                var customButton = newButton.GetComponent<CustomButton>(); 
                customButton.buttonText.text = Language.GetString(def.nameToken);
                customButton.image.sprite = def.pickupIconSprite;
                customButton.button.onClick.AddListener(() => SetEquipmentDef(def));
            }
        }

        public void SetEquipmentDef(EquipmentDef def)
        {
            _equipmentDef = def;
            GiveEquipment();
        }

        public void GiveEquipment() // TODO Network this
        {
            if (!target.master) return;
            var inventory = target.master.inventory;

            inventory.SetEquipmentIndex(_equipmentDef ? _equipmentDef.equipmentIndex : EquipmentIndex.None);
            GetComponentInParent<LobbyPlayerPageManager>().SwapViewState();
        }
        public void Initialize(NetworkUser currentUser)
        {
            target = currentUser;
        }
    }
}
