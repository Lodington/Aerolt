using System;
using System.Collections.Generic;
using System.Linq;
using RoR2;
using RoR2.Networking;
using UnityEngine.Networking;

namespace Aerolt.Managers
{
    public static class NetworkManager
    {
        public static Type[] RegisteredMessages = null!;
        private static readonly HashSet<int> ModdedConnectionIds = new();
        public static bool HostOnly;

        public static void Initialize()
        {
            NetworkManagerSystem.onStartServerGlobal += RegisterMessages;
            NetworkManagerSystem.onStartClientGlobal += RegisterMessages;
            NetworkManagerSystem.onServerDisconnectGlobal += OnServerDisconnect;

            RegisteredMessages = typeof(AeroltMessageBase).Assembly.GetTypes().Where(x =>
                typeof(AeroltMessageBase).IsAssignableFrom(x) && x != typeof(AeroltMessageBase)).ToArray();
        }

        private static void OnServerDisconnect(NetworkConnection conn)
        {
            // Forget this connection's modded status so a later client that reuses the same
            // connection id doesn't inherit it.
            if (conn != null) ModdedConnectionIds.Remove(conn.connectionId);
        }

        private static void RegisterMessages()
        {
            ModdedConnectionIds.Clear();
            NetworkServer.RegisterHandler(2004, HandleMessage);
        }

        public static void RegisterMessages(NetworkClient client)
        {
            client.RegisterHandler(2004, HandleMessage);
            if (!NetworkServer.active)
                ClientScene.readyConnection?.SendAerolt(new HandshakeMessage());
        }

        private static void HandleMessage(NetworkMessage netmsg)
        {
            if (NetworkServer.active && netmsg.conn != null)
                ModdedConnectionIds.Add(netmsg.conn.connectionId);

            var message = netmsg.ReadMessage<AeroltMessage>();

            if (HostOnly && NetworkServer.active && message.message is not HandshakeMessage &&
                !IsLocalConnection(netmsg.conn))
                return;

            if (message.message is BroadcastMessage mess && netmsg.conn != null)
                mess.fromConnection = netmsg.conn;
            message.message.Handle();
        }

        private static bool IsLocalConnection(NetworkConnection? connection)
        {
            return connection != null && NetworkServer.localConnections.Contains(connection);
        }

        public static bool IsModded(NetworkConnection connection)
        {
            if (connection == null) return false;
            if (IsLocalConnection(connection)) return true;
            return ModdedConnectionIds.Contains(connection.connectionId);
        }

        //   aerolt_host_only        -> flips the current state
        //   aerolt_host_only 1      -> force on   (also true/on)
        //   aerolt_host_only 0      -> force off  (also false/off)
        [ConCommand(commandName = "aerolt_host_only", flags = ConVarFlags.SenderMustBeServer,
            helpText = "Toggle whether only the host can use Aerolt. When on, Aerolt actions from " +
                       "connected clients are ignored. Optional argument: 1/0 (on/off); omit to toggle.")]
        private static void CCHostOnly(ConCommandArgs args)
        {
            if (args.Count > 0)
            {
                var requested = args.TryGetArgBool(0);
                HostOnly = requested ?? !HostOnly;
            }
            else
            {
                HostOnly = !HostOnly;
            }

            var state = HostOnly ? "ON (only the host can use Aerolt)" : "OFF (connected clients can use Aerolt)";
            Chat.AddMessage($"<color=yellow>[Aerolt]</color> Host-only mode: {state}");
        }

        public static void SendAerolt<T>(this NetworkConnection connection, T message) where T : AeroltMessageBase
        {
            if (connection == null) return;
            if (message is not HandshakeMessage && NetworkServer.active && !IsModded(connection))
                return;

            var mes = new AeroltMessage(message);
            connection.Send(2004, mes);
        }
    }

    public class HandshakeMessage : AeroltMessageBase
    {
        public override void Handle()
        {
        }
    }

    public class AeroltMessageBase : MessageBase
    {
        // Needed to create empty instance of class so it can be read
        public virtual void Handle()
        {
        }

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
        public NetworkConnection fromConnection = null!;
        private AeroltMessageBase message = null!;

        public BroadcastMessage()
        {
        }

        public BroadcastMessage(AeroltMessageBase aeroltMessageBase)
        {
            message = aeroltMessageBase;
        }

        public override void Handle()
        {
            base.Handle();
            foreach (var connection in NetworkServer.connections)
            {
                if (connection == null) continue;
                if (connection == fromConnection) continue;
                if (!connection.isConnected) continue;
                if (!NetworkManager.IsModded(connection)) continue;
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
        private AeroltMessageBase message = null!;
        private NetworkIdentity target = null!;

        public NewAuthMessage()
        {
        }

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

    internal class AeroltMessage : MessageBase
    {
        public AeroltMessageBase message = null!;
        public uint Type;

        public AeroltMessage()
        {
        }

        public AeroltMessage(AeroltMessageBase aeroltMessageBase)
        {
            message = aeroltMessageBase;
            Type = (uint)Array.IndexOf(NetworkManager.RegisteredMessages, message.GetType());
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
            var tmsg = (AeroltMessageBase)Activator.CreateInstance(NetworkManager.RegisteredMessages[Type]);
            tmsg.Deserialize(reader);
            message = tmsg;
        }
    }
}