using Aerolt.Helpers;
using RoR2;
using RoR2.ContentManagement;
using RoR2.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Aerolt.Buttons
{
    public class ItemButtonGenerator : MonoBehaviour
    {
        public GameObject buttonPrefab;
        public GameObject buttonParent;

        public TMP_Text itemText;
        private ItemDef _itemDef;

        private void Awake()
        {
            foreach(var def in ContentManager._itemDefs)
            {
                GameObject newButton = Instantiate(buttonPrefab, buttonParent.transform);
                newButton.GetComponent<CustomButton>().ButtonText.text = Language.GetString(def.nameToken);
                newButton.GetComponent<Image>().sprite = def.pickupIconSprite;
                newButton.GetComponent<Button>().onClick.AddListener(() => SetItemDef(def));
            }
            
        }

        public void SetItemDef(ItemDef def)
        {
            itemText.text = Language.GetString(def.nameToken);
            _itemDef = def;
        }

        public void dropItem(int amount = 1)
        {
            for (int i = 0; i < amount; i++)
            {
                var localUser = GetUser.FetchUser(GetComponentInParent<HUD>());
                if (localUser.cachedMasterController && localUser.cachedMasterController.master)
                {
                    var body = localUser.cachedMasterController.master.GetBody();
                    PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(_itemDef.itemIndex), body.transform.position + (Vector3.up * 1.5f), Vector3.up * 20f + body.transform.forward * 2f);
                }
            }
        }
    }
}
