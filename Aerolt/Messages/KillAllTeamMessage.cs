using System.Linq;
using Aerolt.Managers;
using RoR2;
using UnityEngine.Networking;

namespace Aerolt.Messages
{
    public class KillAllTeamMessage : AeroltMessageBase
    {
        private TeamIndex team;

        public KillAllTeamMessage()
        {
        }

        public KillAllTeamMessage(TeamIndex allExcept)
        {
            team = allExcept;
        }

        public override void Handle()
        {
            base.Handle();
            var mask = TeamMask.AllExcept(team);
            var mobs = CharacterMaster.instancesList.Where(x => mask.HasTeam(x.teamIndex)).ToArray();
            foreach (var characterMaster in mobs)
            {
                var body = characterMaster.GetBody();
                if (body)
                    Chat.AddMessage($"<color=yellow>Killed {body.GetDisplayName()} </color>");
                characterMaster.TrueKill();
            }
        }

        public override void Deserialize(NetworkReader reader)
        {
            base.Deserialize(reader);
            team = reader.ReadTeamIndex();
        }

        public override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);
            writer.Write(team);
        }
    }
}