using RoR2;
using RoR2.ContentManagement;
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

        private void OnEnable()
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
        public void dropEquipment(int amount = 1)
        {
            for (int i = 0; i < amount; i++)
            {
                var localUser = LocalUserManager.GetFirstLocalUser();
                if (localUser.cachedMasterController && localUser.cachedMasterController.master)
                {
                    var body = localUser.cachedMasterController.master.GetBody();
                    //PickupDropletController.CreatePickupDroplet(EquipmentCatalog.FindEquipmentIndex(def.nameToken.ToString()), body.transform.position + (Vector3.up * 1.5f), Vector3.up * 20f + body.transform.forward * 2f);
                }
            }
        }
    }
}
