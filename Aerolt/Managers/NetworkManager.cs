using System;
using System.Collections.Generic;
using System.Linq;
using RoR2;
using RoR2.Networking;
using UnityEngine;
using UnityEngine.Networking;

namespace Aerolt.Managers
{
	public static class NetworkManager
	{
		public static void Initialize()
		{
			NetworkManagerSystem.onStartServerGlobal += RegisterMessages;
			NetworkManagerSystem.onStartClientGlobal += RegisterMessages;
		}

		private static void RegisterMessages()
		{
			NetworkServer.RegisterHandler(2004, HandleMessage);
		}

		public static void RegisterMessages(NetworkClient client)
		{
			client.RegisterHandler(2004, HandleMessage);
		}
		private static void HandleMessage(NetworkMessage netmsg)
		{
			// we can do some auth checks in here later
			var message = netmsg.ReadMessage<AeroltMessage>();
			message.message.Handle();
		}

		public static void SendAerolt<T>(this NetworkConnection connection, T message) where T : AeroltMessageBase
		{
			var typ = registeredMessages.First(registeredMessage => registeredMessage.Value == typeof(T)).Key;
			var mes = new AeroltMessage {Type = typ, message = message};
			connection.Send(2004, mes);
		}

		public static Dictionary<MessageType, Type> registeredMessages = new()
		{
			{MessageType.InteractableSpawn, typeof(InteractableSpawnMessage)}
		};
		public enum MessageType
		{
			InteractableSpawn
		}
	}

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

	public class AeroltMessageBase : MessageBase
	{
		public AeroltMessageBase() {} // Needed to create empty instance of class so it can be read
		public virtual void Handle() {}
	}

	class AeroltMessage : MessageBase
	{
		public NetworkManager.MessageType Type;
		public AeroltMessageBase message;

		public override void Serialize(NetworkWriter writer)
		{
			base.Serialize(writer);
			writer.WritePackedUInt32((uint) Type);
			writer.Write(message);
		}

		public override void Deserialize(NetworkReader reader)
		{
			base.Deserialize(reader);
			Type = (NetworkManager.MessageType) reader.ReadPackedUInt32();
			var tmsg = (AeroltMessageBase) Activator.CreateInstance(NetworkManager.registeredMessages[Type]);
			tmsg.Deserialize(reader);
			message = tmsg;
		}
	}
}