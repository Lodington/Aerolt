using Aerolt.Helpers;
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

        public TMP_Text EquipmentText;
        private EquipmentDef _equipmentDef;

        private void Awake()
        {
            foreach (var def in ContentManager._equipmentDefs)
            {
                GameObject newButton = Instantiate(buttonPrefab, buttonParent.transform);
                newButton.GetComponent<CustomButton>().ButtonText.text = Language.GetString(def.nameToken);
                newButton.GetComponent<Image>().sprite = def.pickupIconSprite;
                newButton.GetComponent<Button>().onClick.AddListener(() => SetEquipmentDef(def));
            }
        }

        public void SetEquipmentDef(EquipmentDef def)
        {
            EquipmentText.text = Language.GetString(def.nameToken);
            _equipmentDef = def;
        }

        public void GiveEquipment()
        {
            var localUser = GetUser.FetchUser(GetComponentInParent<HUD>());
            if (!localUser.cachedMasterController || !localUser.cachedMasterController.master) return;
            var inventory = localUser.cachedMasterController.master.GetBody().GetComponent<Inventory>();

            inventory.GiveEquipmentString(_equipmentDef.name);
        }
        
        public void DropEquipment(int amount = 1)
        {
            var localUser = GetUser.FetchUser(GetComponentInParent<HUD>());
            if (!localUser.cachedMasterController || !localUser.cachedMasterController.master) return;
            var body = localUser.cachedMasterController.master.GetBody();
            for (int i = 0; i < amount; i++)
                PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(_equipmentDef.equipmentIndex),
                    body.transform.position + (Vector3.up * 1.5f), Vector3.up * 20f + body.transform.forward * 2f);
        }
    }
}
