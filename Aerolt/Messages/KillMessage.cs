using Aerolt.Managers;
using RoR2;
using UnityEngine.Networking;

namespace Aerolt.Messages
{
    public class KillMessage : AeroltMessageBase
    {
        private CharacterMaster master;

        public KillMessage()
        {
        }

        public KillMessage(CharacterMaster master)
        {
            this.master = master;
        }

        public override void Handle()
        {
            base.Handle();

            master.TrueKill();
        }

        public override void Deserialize(NetworkReader reader)
        {
            base.Deserialize(reader);
            var obj = Util.FindNetworkObject(reader.ReadNetworkId());
            if (obj) master = obj.GetComponent<CharacterMaster>();
        }

        public override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);
            writer.Write(master.netId);
        }
    }
}