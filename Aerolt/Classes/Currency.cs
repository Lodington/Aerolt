using RoR2;
using UnityEngine;

namespace Aerolt.Classes
{
    public class Currency : MonoBehaviour
    {
        public void GiveLunarCoinsToAll(uint coinsToGive)
        {
            foreach (var networkUser in NetworkUser.readOnlyInstancesList)
                networkUser.AwardLunarCoins(coinsToGive);
        }
        public void GiveLunarCoins(uint coinsToGive)
        {
            foreach (NetworkUser networkUser in NetworkUser.readOnlyInstancesList)
                if (networkUser.isLocalPlayer)
                    networkUser.AwardLunarCoins(coinsToGive);
        }

        public void GiveMoneyToALl(uint moneyToGive)
        {
            foreach (var networkUser in NetworkUser.readOnlyInstancesList)
                networkUser.master.GiveMoney(moneyToGive);
        }

        public void GiveXpToAll(uint xpToGive)
        {
            foreach (var networkUser in NetworkUser.readOnlyInstancesList)
                networkUser.master.GiveMoney(xpToGive);
        }
        public void GiveMoney(uint moneyToGive)
        {
            var localUser = LocalUserManager.GetFirstLocalUser();
            if (localUser.cachedMasterController && localUser.cachedMasterController.master)
            {
                localUser.cachedMaster.GiveMoney(moneyToGive);
            }
        }
        public void GiveXp(uint xpToGive)
        {
            var localUser = LocalUserManager.GetFirstLocalUser();
            if (localUser.cachedMasterController && localUser.cachedMasterController.master)
            {
                localUser.cachedMaster.GiveExperience(xpToGive);
            }
        }

        public void GiveVoidMarkers(uint voidCoinsToGive)
        {
            var localUser = LocalUserManager.GetFirstLocalUser();
            if (localUser.cachedMasterController && localUser.cachedMasterController.master)
            {
                localUser.cachedMaster.GiveVoidCoins(voidCoinsToGive);
            }
        }
        public void GiveVoidMarkersToAll(uint voidCoinsToGive)
        {
            foreach (var networkUser in NetworkUser.readOnlyInstancesList)
                networkUser.master.GiveVoidCoins(voidCoinsToGive);
        }
        
        
    }
}