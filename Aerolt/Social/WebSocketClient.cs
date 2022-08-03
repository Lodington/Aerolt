using System;
using Aerolt.Classes;
using Aerolt.Managers;
using RoR2;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;

namespace Aerolt.Social
{
    public class WebSocketClient : MonoBehaviour {
        
        public TMP_Text messagePanel;
        public TMP_InputField inputField;
        public Button sendButton;
        public TMP_Text usernamePanel;
        private MenuInfo _info;
        
        public static string ip = "d";
        public static string port = "8000";
        public class ClientName 
        {
            public static string Name = ""; 
        }

        // Initiate socket allowing connection
        public class Connect
        {
            public static readonly WebSocket Socket = new WebSocket($"ws://{ip}:{port}/Connect");
        }
        
        // Initiate socket allowing messages to be received
        public class Message 
        {
            public static readonly WebSocket Socket = new WebSocket($"ws://{ip}:{port}/Message");
        }

        // Initiate socket allowing usernames to be received
        public class Usernames 
        {
            public static readonly WebSocket Socket = new WebSocket($"ws://{ip}:{port}/Usernames");
        }

        // Initiate socket allowing disconnection
        public class Disconnect
        {
            public static readonly WebSocket Socket = new WebSocket($"ws://{ip}:{port}/Disconnect");
        }

        public void Start()
        {
            _info = GetComponentInParent<MenuInfo>();
            var username = _info.Owner.userName;
            ClientName.Name = username; // Set name variable

            // Connect to Connect SocketBehaviour
            Connect.Socket.Connect();
            Connect.Socket.Send(username);
            Connect.Socket.OnMessage += (sender, e) =>
                messagePanel.text += e.Data + "\n";

            // Connect to Usernames SocketBehaviour
            Usernames.Socket.Connect();
            Usernames.Socket.Send(username);
            Usernames.Socket.OnMessage += (sender, e) =>
                usernamePanel.text = e.Data + "\n";

                // Connect to Message SocketBehaviour
            Message.Socket.Connect();
            Message.Socket.OnMessage += (sender, e) =>
                messagePanel.text += e.Data + "\n"; // Retrieve input from all clients
            
            sendButton.onClick.AddListener(SendMessageOnClick);
        }
        
        

        private void SendMessageOnClick()
        {
            // Send message via /Message on button click
            Message.Socket.Send(ClientName.Name + "  ->  " + inputField.text);
            inputField.text = string.Empty; // Clear input
        }

        private void OnDestroy()
        {
            DisconnectClient();
        }

        public void DisconnectClient()
        {
            Disconnect.Socket.Connect();
            
            // Send username of client disconnecting
            Disconnect.Socket.Send(ClientName.Name);

            // Close all sockets and exit application
            Connect.Socket.Close();
            Message.Socket.Close();
            Usernames.Socket.Close();
            Disconnect.Socket.Close();
        }

        private void DisconnectOnCLick(object sender, EventArgs e)
        {
            // Connect to Disconnect SocketBehaviour
            DisconnectClient();
        }
    }
}