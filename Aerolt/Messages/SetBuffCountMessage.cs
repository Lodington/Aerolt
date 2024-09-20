using System.Collections.Generic;
using Aerolt.Managers;
using RoR2;
using UnityEngine.Networking;

namespace Aerolt.Messages
{
    public class SetBuffCountMessage : AeroltMessageBase
    {
        public CharacterBody body;
        public Dictionary<BuffIndex, uint> buffCounts;

        public SetBuffCountMessage()
        {
        }

        public SetBuffCountMessage(CharacterBody bodyIn, Dictionary<BuffIndex, uint> buffCountsIn)
        {
            body = bodyIn;
            buffCounts = buffCountsIn;
        }

        public override void Handle()
        {
            base.Handle();
            foreach (var buffCount in buffCounts) body.SetBuffCount(buffCount.Key, (int)buffCount.Value);
        }

        public override void Deserialize(NetworkReader reader)
        {
            base.Deserialize(reader);
            var obj = Util.FindNetworkObject(reader.ReadNetworkId());
            if (obj)
                body = obj.GetComponent<CharacterBody>();
            var length = reader.ReadPackedUInt32();
            buffCounts = new Dictionary<BuffIndex, uint>();
            for (var i = 0; i < length; i++)
                buffCounts.Add((BuffIndex)reader.ReadPackedUInt32(), reader.ReadPackedUInt32());
        }

        public override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);
            writer.Write(body.netId);
            writer.WritePackedUInt32((uint)buffCounts.Count);
            foreach (var (key, value) in buffCounts)
            {
                writer.WritePackedUInt32((uint)key);
                writer.WritePackedUInt32(value);
            }
        }
    }
}