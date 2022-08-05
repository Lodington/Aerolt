using Aerolt.Managers;
using RoR2;
using UnityEngine.Networking;

namespace Aerolt.Messages
{
    public class GodModeMessage : AeroltMessageBase
    {
        private bool enabled;
        private CharacterMaster master;

        public GodModeMessage()
        {
        }

        public GodModeMessage(CharacterMaster master, bool enable)
        {
            this.master = master;
            enabled = enable;
        }

        public override void Handle()
        {
            base.Handle();
            master.godMode = enabled;
            master.UpdateBodyGodMode();
        }

        public override void Deserialize(NetworkReader reader)
        {
            base.Deserialize(reader);
            var obj = Util.FindNetworkObject(reader.ReadNetworkId());
            if (obj)
                master = obj.GetComponent<CharacterMaster>();
            enabled = reader.ReadBoolean();
        }

        public override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);
            writer.Write(master.netId);
            writer.Write(enabled);
        }
    }
}