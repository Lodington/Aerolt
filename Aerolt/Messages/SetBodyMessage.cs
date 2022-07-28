using System;
using Aerolt.Managers;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace Aerolt.Messages
{
    public class SetBodyMessage : AeroltMessageBase
    {
        private NetworkUser user;
        private CharacterBody newBody;

        public SetBodyMessage(){}
        public SetBodyMessage(NetworkUser user, CharacterBody newBody)
        {
            this.user = user;
            this.newBody = newBody;

        }

        public override void Handle()
        {
            base.Handle();
            if (!newBody) return;
            if (!user || !user.master) return;

            if (NetworkServer.active)
                user.master.CmdRespawn(newBody.name);
            else
                user.master.CallCmdRespawn(newBody.name);
        }

        public override void Deserialize(NetworkReader reader)
        {
            base.Deserialize(reader);
            var obj = Util.FindNetworkObject(reader.ReadNetworkId());
            if (obj)
            {
                user = obj.GetComponent<NetworkUser>();
                newBody = obj.GetComponent<CharacterBody>();
            }
        }

        public override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);
            writer.Write(user.netId);
            writer.Write(newBody.netId);
        }
    }
}