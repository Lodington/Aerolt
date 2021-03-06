using System.Linq;
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
    public class ItemButtonGenerator : MonoBehaviour
    {
        public GameObject buttonPrefab;
        public GameObject buttonParent;

        public TMP_Text itemText;
        private ItemDef _itemDef;

        private void Awake()
        {
            foreach(var def in ContentManager._itemDefs.OrderBy(x => x.tier))
            {
                GameObject newButton = Instantiate(buttonPrefab, buttonParent.transform);
                newButton.GetComponent<CustomButton>().buttonText.text = Language.GetString(def.nameToken);
                newButton.GetComponent<Image>().sprite = def.pickupIconSprite;
                newButton.GetComponent<Button>().onClick.AddListener(() => SetItemDef(def));
            }
        }

        public void SetItemDef(ItemDef def)
        {
            itemText.text = Language.GetString(def.nameToken);
            _itemDef = def;
        }


        public void GiveItem(int amount)
        {
            if (!_itemDef)
                Load.CallPopup("Error", "Please Selection a item to spawn it",
                    GetComponentInParent<PanelManager>().transform);
            var localUser = GetUser.FetchUser(GetComponentInParent<PanelManager>().hud);
            if (!localUser.cachedMasterController || !localUser.cachedMasterController.master) return;
            var inventory = localUser.cachedMasterController.master.inventory;

            inventory.GiveItem(_itemDef, amount);
        }
        
        public void DropItem(int amount)
        {
            var localUser = GetUser.FetchUser(GetComponentInParent<PanelManager>().hud);
            if (!localUser.cachedMasterController || !localUser.cachedMasterController.master) return;
            var body = localUser.cachedMasterController.master.GetBody();
            
            for (var i = 0; i < amount; i++)
                PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(_itemDef.itemIndex),
                    body.transform.position + (Vector3.up * 1.5f), Vector3.up * 20f + body.transform.forward * 2f);
        }
    }
}
