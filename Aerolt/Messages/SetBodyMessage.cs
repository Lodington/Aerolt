using System;
using Aerolt.Managers;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

namespace Aerolt.Messages
{
    public class SetBodyMessage : AeroltMessageBase
    {
        private NetworkUser user;
        private string newBody;

        public SetBodyMessage(){}
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
                {
                    Object.Destroy(gameEndReportPanelController.Value.gameObject);
                }
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
            if (obj)
            {
                user = obj.GetComponent<NetworkUser>();
            }
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