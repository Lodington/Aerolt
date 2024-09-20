using Aerolt.Managers;
using RoR2;
using UnityEngine.Networking;

namespace Aerolt.Messages
{
    public class TeamSwitchMessage : AeroltMessageBase
    {
        private TeamIndex newTeam;
        private CharacterMaster target;

        public TeamSwitchMessage()
        {
        }

        public TeamSwitchMessage(CharacterMaster who, TeamIndex team)
        {
            target = who;
            newTeam = team;
        }

        public override void Handle()
        {
            base.Handle();
            target.teamIndex = newTeam;
            var body = target.GetBody();
            if (body)
                body.teamComponent.teamIndex = newTeam;
        }

        public override void Deserialize(NetworkReader reader)
        {
            base.Deserialize(reader);
            var obj = Util.FindNetworkObject(reader.ReadNetworkId());
            if (obj)
                target = obj.GetComponent<CharacterMaster>();
            newTeam = (TeamIndex)reader.ReadInt32();
        }

        public override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);
            writer.Write(target.netId);
            writer.Write((int)newTeam);
        }
    }
}