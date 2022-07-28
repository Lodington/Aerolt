using System;
using System.Collections.Generic;
using System.Linq;
using Aerolt.Messages;
using RoR2;
using RoR2.Networking;
using UnityEngine.Networking;

namespace Aerolt.Managers
{
	public static class NetworkManager
	{
		public static void Initialize()
		{
			NetworkManagerSystem.onStartServerGlobal += RegisterMessages;
			NetworkManagerSystem.onStartClientGlobal += RegisterMessages;

			RegisteredMessages = typeof(AeroltMessageBase).Assembly.GetTypes().Where(x => typeof(AeroltMessageBase).IsAssignableFrom(x) && x != typeof(AeroltMessageBase)).ToArray();
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
			var typ = (uint) Array.IndexOf(RegisteredMessages, message.GetType());// typeof(T)); this returns AeroltMessageBase, and not the actual type. fucking generics
			var mes = new AeroltMessage {Type = typ, message = message};
			connection.Send(2004, mes);
		}

		public static Type[] RegisteredMessages;
	}

	public class AeroltMessageBase : MessageBase
	{
		public AeroltMessageBase() {} // Needed to create empty instance of class so it can be read
		public virtual void Handle() {}

		public void SendToServer()
		{
			if (!NetworkServer.active)
				ClientScene.readyConnection.SendAerolt(this);
			else
				Handle();
		}

		public void SendToAuthority(NetworkIdentity identity)
		{
			if (!identity.localPlayerAuthority)
				identity.clientAuthorityOwner.SendAerolt(this);
			else
				Handle();
		}
		public void SendToAuthority(NetworkUser user)
		{
			SendToAuthority(user.netIdentity);
		}
		public void SendToAuthority(CharacterMaster master)
		{
			SendToAuthority(master.networkIdentity);
		}
		public void SendToAuthority(CharacterBody body)
		{
			SendToAuthority(body.networkIdentity);
		}
	}

	class AeroltMessage : MessageBase
	{
		public uint Type;
		public AeroltMessageBase message;

		public override void Serialize(NetworkWriter writer)
		{
			base.Serialize(writer);
			writer.WritePackedUInt32(Type);
			writer.Write(message);
		}

		public override void Deserialize(NetworkReader reader)
		{
			base.Deserialize(reader);
			Type = reader.ReadPackedUInt32();
			var tmsg = (AeroltMessageBase) Activator.CreateInstance(NetworkManager.RegisteredMessages[Type]);
			tmsg.Deserialize(reader);
			message = tmsg;
		}
	}
}