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
            Connect.OnMessage += (sender, e) => authUUID.Value = e.Data;
            Usernames.OnMessage += (sender, e) =>
            {
                UsernameText = e.Data + "\n";
                UserCountText = UsernameText.Split('\n').Length.ToString();
            };
            Message.OnMessage += (sender, e) => MessageText += e.Data + "\n"; // Retrieve input from all clients
            Message.Log.Disable();
            Usernames.Log.Disable();
            Connect.Log.Disable();
        }

        public static string keypath = System.IO.Path.Combine(Load.path, "elevatedkey.txt");
        private static ZioConfigEntry<string> authUUID;

        public static void ConnectClient()
        {
            if (connecting) return;
            connecting = true;
            // Set name variable
            authUUID = Load.configFile.Bind("UserAuth", "UUID", "", ""); // This is fine to bind multiple times, it just needs to be done late enough that configFile is set.
            var usernameToSend = authUUID.Value.IsNullOrWhiteSpace() ? RoR2Application.GetBestUserName() : authUUID.Value;
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