using System;
using Aerolt.Managers;
using RoR2;
using UnityEngine.Networking;

namespace Aerolt.Messages
{
    public class CurrencyMessage : AeroltMessageBase
    {
        private CurrencyType _type;
        private uint amount;
        private CharacterMaster master;

        public CurrencyMessage()
        {
        }

        public CurrencyMessage(CharacterMaster master, CurrencyType type, uint amount)
        {
            _type = type;
            this.amount = amount;
            this.master = master;
        }

        public override void Handle()
        {
            base.Handle();
            switch (_type)
            {
                case CurrencyType.Money:
                    master.GiveMoney(amount - master.money);
                    break;
                case CurrencyType.Lunar:
                    var user = master.playerCharacterMasterController.networkUser;
                    var val = (int) amount - user.NetworknetLunarCoins;
                    if (val < 0)
                        user.DeductLunarCoins((uint) Math.Abs(val));
                    else
                        user.AwardLunarCoins((uint) val);
                    break;
                case CurrencyType.Void:
                    master.GiveVoidCoins(amount - master.voidCoins);
                    break;
                case CurrencyType.Experience:
                    TeamManager.instance.SetTeamExperience(master.teamIndex, amount);
                    break;
                case CurrencyType.Level:
                    TeamManager.instance.SetTeamLevel(master.teamIndex, amount);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override void Deserialize(NetworkReader reader)
        {
            base.Deserialize(reader);
            var obj = Util.FindNetworkObject(reader.ReadNetworkId());
            if (obj)
                master = obj.GetComponent<CharacterMaster>();
            _type = (CurrencyType) reader.ReadPackedUInt32();
            amount = reader.ReadPackedUInt32();
        }

        public override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);
            writer.Write(master.netId);
            writer.WritePackedUInt32((uint) _type);
            writer.WritePackedUInt32(amount);
        }
    }

    public enum CurrencyType
    {
        Money,
        Lunar,
        Void,
        Experience,
        Level
    }
}