using System;
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

        public static string ip = "aerolt.lodington.dev";
        public static string port = "5000";
        
        public static readonly WebSocket Connect = new($"ws://{ip}:{port}/Connect");
        public static readonly WebSocket Message = new($"ws://{ip}:{port}/Message");
        public static readonly WebSocket Usernames = new($"ws://{ip}:{port}/Usernames");

        private static bool connecting;

        static WebSocketClient()
        {
            Connect.OnMessage += (_, e) => AuthUuid = Guid.TryParse(e.Data, out var _) ? e.Data : "";
            Usernames.OnMessage += (_, e) =>
            {
                UsernameText = e.Data + "\n";
                UserCountText = UsernameText.Split('\n').Length.ToString();
            };
            Message.OnMessage += (_, e) => MessageText += e.Data + "\n"; // Retrieve input from all clients
            Message.Log.Disable();
            Usernames.Log.Disable();
            Connect.Log.Disable();
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
            if (!Connect.IsAlive && !Usernames.IsAlive && !Message.IsAlive)
                Task.Run(ConnectClient);
        }

        public static void ConnectClient()
        {
            if (connecting) return;
            connecting = true;
            // Set name variable
            var usernameToSend = GetUsername();
            var MaxTrys = 10;
            var currentTry = 1;
            while (currentTry < MaxTrys)
            {
                Connect.Connect();
                Usernames.Connect();
                Message.Connect();

                if (Connect.IsAlive && Usernames.IsAlive && Message.IsAlive)
                {
                    Connect.Send(usernameToSend);
                    connecting = false;
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