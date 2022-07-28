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
        private string newBody;

        public SetBodyMessage(){}
        public SetBodyMessage(NetworkUser user, CharacterBody newBody)
        {
            this.user = user;
            this.newBody = newBody.name;

        }

        public override void Handle()
        {
            base.Handle();
            user.master.CmdRespawn(newBody);
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