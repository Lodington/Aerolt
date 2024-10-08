using Aerolt.Managers;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace Aerolt.Messages
{
    public class SetBodyMessage : AeroltMessageBase
    {
        private string newBody;
        private NetworkUser user;

        public SetBodyMessage()
        {
        }

        public SetBodyMessage(NetworkUser user, CharacterBody newBody) : this(user)
        {
            this.newBody = newBody.name;
        }

        public SetBodyMessage(NetworkUser networkUser)
        {
            user = networkUser;
            newBody = "";
        }

        public override void Handle()
        {
            base.Handle();
            var gameover = Object.FindObjectOfType<GameOverController>();
            if (gameover)
            {
                foreach (var gameEndReportPanelController in gameover.reportPanels)
                    Object.Destroy(gameEndReportPanelController.Value
                        .gameObject); // TODO this is bad and will fuck gameover forever, not a one time revert
                Object.Destroy(gameover.gameObject);
            }

            if (!NetworkServer.active) return;
            var oldPod = false;
            if (Stage.instance)
            {
                oldPod = Stage.instance.usePod;
                Stage.instance.usePod = false;
            }

            user.master.preventRespawnUntilNextStageServer = false;
            user.master.CmdRespawn(newBody);
            if (Stage.instance) Stage.instance.usePod = oldPod;
        }

        public override void Deserialize(NetworkReader reader)
        {
            base.Deserialize(reader);
            var obj = Util.FindNetworkObject(reader.ReadNetworkId());
            if (obj) user = obj.GetComponent<NetworkUser>();
            newBody = reader.ReadString();
        }

        public override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);
            writer.Write(user.netId);
            writer.Write(newBody);
        }
    }
}