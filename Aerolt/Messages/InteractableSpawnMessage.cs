using Aerolt.Managers;
using UnityEngine;
using UnityEngine.Networking;

namespace Aerolt.Messages
{
	public class InteractableSpawnMessage : AeroltMessageBase
	{
		private uint index;
		private Vector3 position;

		public InteractableSpawnMessage(){}
		public InteractableSpawnMessage(uint index, Vector3 position)
		{
			this.index = index;
			this.position = position;
		}

		public override void Serialize(NetworkWriter writer)
		{
			base.Serialize(writer);
			writer.WritePackedUInt32(index);
			writer.Write(position);
		}

		public override void Deserialize(NetworkReader reader)
		{
			base.Deserialize(reader);
			index = reader.ReadPackedUInt32();
			position = reader.ReadVector3();
		}

		public override void Handle()
		{
			base.Handle();
			InteractableManager.Spawn(index, position);
		}
	}
}