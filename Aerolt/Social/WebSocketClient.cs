using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Aerolt.Helpers;
using BepInEx;
using RoR2;
using WebSocketSharp;
using ZioConfigFile;
using LogLevel = Aerolt.Enums.LogLevel;

namespace Aerolt.Social
{
    public static class WebSocketClient
    {
        #region Endpoint Specific
        private static ZioConfigEntry<string> authUUID;
        public static string AuthUuid
        {
            get => authUUID.Value;
            set => authUUID.Value = value;
        }
        public static string GetUsername()
        {
            authUUID = Load.configFile.Bind("UserAuth", "UUID", "", ""); // This is fine to bind multiple times, it just needs to be done late enough that configFile is set.
            return AuthUuid.IsNullOrWhiteSpace() ? RoR2Application.GetBestUserName() : AuthUuid;
        }
        #endregion
        
        public static string UsernameText;
        public static string UserCountText;
        public static string MessageText;

        //public static string ip = "aerolt.lodington.dev";
        public static string ip = IPAddress.Any.ToString();
        public static string port = "5001";
        
        public static readonly WebSocket Connect = new($"ws://{ip}:{port}/Connect");
        public static readonly WebSocket Message = new($"ws://{ip}:{port}/Message");
        public static readonly WebSocket Usernames = new ($"ws://{ip}:{port}/Usernames");
        public static readonly WebSocket AssetBundle = new ($"ws://{ip}:{port}/AssetBundle");

        private static bool _isRetying;

        static WebSocketClient()
        {
            Connect.OnMessage += (_, e) =>
            {
                AuthUuid = Guid.TryParse(e.Data, out var id) ? e.Data : "";
                
                if (AuthUuid == default) return;
                Connect.guid = id;
                Usernames.guid = id;
                Message.guid = id;
                AssetBundle.Send(id.ToString());
            };
            Usernames.OnMessage += (_, e) => UsernameText = e.Data;
            Message.OnMessage += (_, e) => MessageText += e.Data + "\n"; // Retrieve input from all clients
            Message.OnError += (sender, args) =>
            {
                var isInternalError = args.Message == "An exception has occurred while receiving a message.";

                Tools.Log(LogLevel.Error, args.Message);
                
                var socket = (WebSocket) sender;
                if (socket.ReadyState == WebSocketState.Open && !isInternalError) Task.Run(ConnectClient);
            };

            Message.OnClose += (sender, args) =>
            {
                Tools.Log(LogLevel.Warning, "Lost Connection to server.");
                Task.Run(ConnectClient);
            };
            AssetBundle.OnMessage += (sender, e) =>
            {
                File.Create(e.Data);
            };
        }

        public static void Bind(EventHandler<MessageEventArgs> action)
        {
            Message.OnMessage += action;
            Usernames.OnMessage += action;
            Connect.OnMessage += action;
        }
        public static void UnBind(EventHandler<MessageEventArgs> action)
        {
            Message.OnMessage -= action;
            Usernames.OnMessage -= action;
            Connect.OnMessage -= action;
        }

        public static void TryConnect()
        {
            if (!_isRetying) Task.Run(ConnectClient);
        }

        public static void ConnectClient()
        {
            if (_isRetying) return;
            _isRetying = true;
            // Set name variable
            var usernameToSend = GetUsername();
            var MaxTrys = 10;
            var currentTry = 1;
            while (currentTry < MaxTrys)
            {
                Connect.Connect();
                Usernames.Connect();
                Message.Connect();
                AssetBundle.Connect();

                if (Connect.IsAlive && Usernames.IsAlive && Message.IsAlive)
                {
                    Connect.Send(usernameToSend);
                    
                    _isRetying = false;
                    return;
                }
                Tools.Log(LogLevel.Error, $"Couldnt Connect to Server. Retrying {MaxTrys - currentTry} times.");
                currentTry++;
                Task.Delay(5000);
            }
        }


        public static void DisconnectClient()
        {
            Connect.Close();
            Usernames.Close();
            Message.Close();
        }
    }
}