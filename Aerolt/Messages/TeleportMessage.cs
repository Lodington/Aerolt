using Aerolt.Managers;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace Aerolt.Messages
{
	public class TeleportMessage : AeroltMessageBase
	{
		private CharacterBody teleportedBody;
		private Vector3 targetPosition;
		public TeleportMessage(){}
		public TeleportMessage(CharacterBody teleportedBody, Vector3 targetPosition)
		{
			this.teleportedBody = teleportedBody;
			this.targetPosition = targetPosition;
		}

		public override void Handle()
		{
			base.Handle();
			if (teleportedBody && teleportedBody.characterMotor) teleportedBody.characterMotor.Motor.MoveCharacter(targetPosition);
		}

		public override void Deserialize(NetworkReader reader)
		{
			base.Deserialize(reader);
			var obj = Util.FindNetworkObject(reader.ReadNetworkId());
			if (obj) teleportedBody = obj.GetComponent<CharacterBody>();
			targetPosition = reader.ReadVector3();
		}

		public override void Serialize(NetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(teleportedBody.netId);
			writer.Write(targetPosition);
		}
	}
}