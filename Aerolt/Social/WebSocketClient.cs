using System.Collections;
using System.IO;
using System.Threading.Tasks;
using Aerolt.Helpers;
using RoR2;
using UnityEngine;
using WebSocketSharp;
using LogLevel = Aerolt.Enums.LogLevel;
using Path = RoR2.Path;

namespace Aerolt.Social
{
    public static class WebSocketClient
    {
        public static string UsernameText;
        public static string UserCountText;
        public static string MessageText;
        public static string JoinText;

        public static string ip = "aerolt.lodington.dev";
        public static string port = "5000";

        public static readonly WebSocket Disconnect = new($"ws://{ip}:{port}/Disconnect");
        public static readonly WebSocket Connect = new($"ws://{ip}:{port}/Connect");
        public static readonly WebSocket Message = new($"ws://{ip}:{port}/Message");
        public static readonly WebSocket UserCount = new($"ws://{ip}:{port}/UserCount");
        public static readonly WebSocket Usernames = new($"ws://{ip}:{port}/Usernames");
        public static readonly WebSocket Admin = new($"ws://{ip}:{port}/Admin");

        public static string _username;
        private static bool connecting;

        static WebSocketClient()
        {
            Connect.OnMessage += (sender, e) =>
                JoinText = e.Data + "\n";
            Usernames.OnMessage += (sender, e) =>
                UsernameText = e.Data + "\n";
            UserCount.OnMessage += (sender, e) =>
                UserCountText = e.Data;
            Message.OnMessage += (sender, e) =>
                MessageText += e.Data + "\n"; // Retrieve input from all clients
            Message.Log.Disable();
            Usernames.Log.Disable();
            UserCount.Log.Disable();
            Connect.Log.Disable();
        }

        public static string keypath = System.IO.Path.Combine(Load.path, "elevatedkey.txt");

        public static void ConnectClient()
        {
            if (connecting) return;
            connecting = true;
            // Set name variable
            _username = $"{RoR2Application.GetBestUserName()}";
            var MaxTrys = 10;
            var currentTry = 1;
            while (currentTry < MaxTrys)
            {
                Connect.Connect();
                Usernames.Connect();
                UserCount.Connect();
                Message.Connect();
                
                if (File.Exists(keypath))
                {
                    var token = File.ReadAllText(keypath);
                    Admin.Connect();
                    if (Admin.IsAlive)
                        Admin.Send(token);
                }

                if (Connect.IsAlive && Usernames.IsAlive && UserCount.IsAlive && Message.IsAlive)
                {
                    Connect.Send(_username);
                    Usernames.Send(_username);
                    UserCount.Send(_username);
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
            Disconnect.Connect();
            Disconnect.Send(_username);

            Connect.Close();
            Usernames.Close();
            Message.Close();
            Disconnect.Close();
            if (Admin.IsAlive)
                Admin.Close();
        }
    }
}