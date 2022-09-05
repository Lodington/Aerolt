using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
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
        public static string UsernameText;
        public static string MessageText;

        public static string ip = "aerolt.lodington.dev";
        public static string port = "5001";
        
        public static readonly WebSocket Connect = new($"ws://{ip}:{port}/Connect");
        public static WebSocket Message = new($"ws://{ip}:{port}/Message");
        public static readonly WebSocket AssetBundle = new ($"ws://{ip}:{port}/AssetBundle");
        
        #region Endpoint Specific
        private static ZioConfigEntry<string> _authUuid;
        public static string AuthUuid
        {
            get => _authUuid.Value;
            set => _authUuid.Value = value;
        }
        public static string GetUsername()
        {
            _authUuid = Load.configFile.Bind("UserAuth", "UUID", "", ""); // This is fine to bind multiple times, it just needs to be done late enough that configFile is set.
            return AuthUuid.IsNullOrWhiteSpace() ? RoR2Application.GetBestUserName() : AuthUuid;
        }
        #endregion

        public static float DownloadProgress;
        private static bool _isRetying;
        static WebSocketClient()
        {
            Connect.OnMessage += (_, e) =>
            {
                AuthUuid = Guid.TryParse(e.Data, out var id) ? e.Data : "";
                
                if (AuthUuid == default) return;
                Connect.guid = id;
                Message.guid = id;
                Connect.Close();
            };
            Message.OnMessage += (_, e) => MessageText += e.Data + "\n"; // Retrieve input from all clients
            Message.OnError += (sender, args) =>
            {
                var isInternalError = args.Message == "An exception has occurred while receiving a message.";
                Tools.Log(LogLevel.Error, args.Message);
                
                var socket = (WebSocket) sender;
                if (socket.ReadyState == WebSocketState.Open && !isInternalError) Task.Run(ConnectClient);
            };

            Message.OnClose += HandleClose("Message");

            AssetBundle.OnMessage += (sender, e) =>
            {
                GetAssetBundle(e.Data);
                AssetBundle.Close();
            };
        }

        private static EventHandler<CloseEventArgs> HandleClose(string url) =>
            delegate
            {
                Tools.Log(LogLevel.Warning, "Lost Connection to server.");
                ReconnectSocket($"ws://{ip}:{port}/{url}");
            };

        private static void GetAssetBundle(string obj)
        {
            using var wc = new WebClient();
            wc.DownloadProgressChanged += wc_DownloadProgressChanged;
            wc.DownloadFileAsync (
                new Uri(obj),
                Load.path
            );
        }
        private static void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e) => DownloadProgress = e.ProgressPercentage;

        private static void ConnectToAssetBundle()
        {
            AssetBundle.Connect();
            AssetBundle.Send(GetUsername());
        }
        public static void Bind(EventHandler<MessageEventArgs> action)
        {
            Message.OnMessage += action;
            Connect.OnMessage += action;
        }
        public static void UnBind(EventHandler<MessageEventArgs> action)
        {
            Message.OnMessage -= action;
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
            var usernameToSend = GetUsername();
            var MaxTrys = 10;
            var currentTry = 1;
            while (currentTry < MaxTrys)
            {
                Connect.Connect();
                Message.Connect();

                if (Connect.IsAlive && Message.IsAlive)
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


        private static WebSocket ReconnectSocket(string ipaddress) => new(ipaddress);

        public static void DisconnectClient()
        {
            Connect.Close();
            Message.Close();
        }
        
    }
}