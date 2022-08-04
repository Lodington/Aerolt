using System;
using System.Collections;
using System.Linq;
using System.Text;
using Aerolt.Helpers;
using RoR2;
using UnityEngine;
using WebSocketSharp;
using Console = System.Console;
using LogLevel = Aerolt.Enums.LogLevel;


namespace Aerolt.Social
{
    public static class WebSocketClient {
        
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

        public static string _username;
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

        public static IEnumerator ConnectClient()
        { // Set name variable
            _username = $"{RoR2Application.GetBestUserName()}";
            int MaxTrys = 10;
            int currentTry = 1; 
            while (currentTry < MaxTrys)
            {
                Connect.Connect();
                Usernames.Connect();
                UserCount.Connect();
                Message.Connect();

                if (Connect.IsAlive && Usernames.IsAlive && UserCount.IsAlive && Message.IsAlive)
                {
                    Connect.Send(_username);
                    Usernames.Send(_username);
                    UserCount.Send(_username);
                    yield break;
                }
                   

                Tools.Log(LogLevel.Error,$"Couldnt Connect to Server. Retrying {MaxTrys - currentTry} times.");
                currentTry++;
                yield return new WaitForSeconds(5f);
            }
        }
        

        public static void DisconnectClient()
        {
            Disconnect.Connect();
            Disconnect.Send(_username);

            Message.Close();
            Usernames.Close();
            Disconnect.Close();
            Connect.Close();
        }
    }
}