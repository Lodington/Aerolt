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
			if (message.message is BroadcastMessage mess)
			{
				mess.fromConnection = netmsg.conn;
			}
			message.message.Handle();
		}

		public static void SendAerolt<T>(this NetworkConnection connection, T message) where T : AeroltMessageBase
		{
			var mes = new AeroltMessage(message);
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
		public void SendToEveryone()
		{
			Handle();
			new BroadcastMessage(this).SendToServer();
		}
		public void SendToAuthority(NetworkIdentity identity)
		{
			if (!Util.HasEffectiveAuthority(identity) && NetworkServer.active)
				identity.clientAuthorityOwner.SendAerolt(this);
			else if (!NetworkServer.active)
				new NewAuthMessage(identity, this).SendToServer();
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

	public class BroadcastMessage : AeroltMessageBase
	{
		private AeroltMessageBase message;
		public NetworkConnection fromConnection;
		public BroadcastMessage(){}
		public BroadcastMessage(AeroltMessageBase aeroltMessageBase)
		{
			message = aeroltMessageBase;
		}
		public override void Handle()
		{
			base.Handle();
			foreach (var connection in NetworkServer.connections)
			{
				if (connection == fromConnection) continue;
				if (!connection.isConnected) continue;
				connection.SendAerolt(message);
			}
			message.Handle();
		}
		public override void Deserialize(NetworkReader reader)
		{
			base.Deserialize(reader);
			message = reader.ReadMessage<AeroltMessage>().message;
		}

		public override void Serialize(NetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(new AeroltMessage(message));
		}
	}

	public class NewAuthMessage : AeroltMessageBase
	{
		private NetworkIdentity target;
		private AeroltMessageBase message;
		public NewAuthMessage(){}
		public NewAuthMessage(NetworkIdentity identity, AeroltMessageBase aeroltMessageBase)
		{
			target = identity;
			message = aeroltMessageBase;
		}

		public override void Handle()
		{
			base.Handle();
			message.SendToAuthority(target);
		}

		public override void Deserialize(NetworkReader reader)
		{
			base.Deserialize(reader);
			var obj = Util.FindNetworkObject(reader.ReadNetworkId());
			if (obj)
				target = obj.GetComponent<NetworkIdentity>();
			message = reader.ReadMessage<AeroltMessage>().message;
		}

		public override void Serialize(NetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(target.netId);
			writer.Write(new AeroltMessage(message));
		}
	}

	class AeroltMessage : MessageBase
	{
		public uint Type;
		public AeroltMessageBase message;

		public AeroltMessage(){}
		public AeroltMessage(AeroltMessageBase aeroltMessageBase)
		{
			message = aeroltMessageBase;
			Type = (uint) Array.IndexOf(NetworkManager.RegisteredMessages, message.GetType());
		}

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