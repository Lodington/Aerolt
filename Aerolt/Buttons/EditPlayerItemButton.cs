using System.Collections.Generic;
using Aerolt.Managers;
using Aerolt.Messages;
using RoR2;
using RoR2.ContentManagement;

namespace Aerolt.Buttons
{
    public class EditPlayerItemButton : EditMonsterItemButton
    {
        private NetworkUser user;
        private Dictionary<ItemDef, int> _items = new();
        public override Dictionary<ItemDef, int> itemDef => _items;

        public void GiveItems()
        {
            var inv = user.master.inventory;
            new SetItemCountMessage(inv, itemDef).SendToServer();
            GetComponentInParent<LobbyPlayerPageManager>().SwapViewState();
        }

        public void Initialize(NetworkUser currentUser)
        {
            user = currentUser;
            foreach (var def in ContentManager._itemDefs)
            {
                itemDefRef[def].SetAmount(user.master.inventory.GetItemCount(def));
            }
            
            Sort();
        }
    }
}