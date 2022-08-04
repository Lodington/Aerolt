using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;

namespace Aerolt.Social
{
    public class GetWebSocketClientData : MonoBehaviour
    {
        public TMP_InputField inputField;
        public TMP_Text chatWindowText;
        public TMP_Text userWindowText;
        public Button sendButton;
        public TMP_Text userCount;
        public TMP_Text WelcomeText;

        private bool isTextDirty;

        private void Update()
        {
            if (isTextDirty)
            {
                UpdateChatWindow();
                isTextDirty = false;
            }
            if (Input.GetKeyDown(KeyCode.Return)) SendMessage();
        }

        private void OnEnable()
        {
            UpdateChatWindow();
        }

        private void SendMessage()
        {
            if(String.IsNullOrEmpty(inputField.text))
                return;
            WebSocketClient.Message.Send($" [{WebSocketClient._username}] -> {inputField.text}");
            inputField.text = String.Empty;
        }
        
        public void Awake()
        {
            WebSocketClient.Message.OnMessage += MarkTextDirty;
            WebSocketClient.UserCount.OnMessage += MarkTextDirty;
            WebSocketClient.Usernames.OnMessage += MarkTextDirty;
            WebSocketClient.Connect.OnMessage += MarkTextDirty;
            sendButton.onClick.AddListener(SendMessage);
        }

        private void OnDestroy()
        {
            WebSocketClient.Message.OnMessage -= MarkTextDirty;
            WebSocketClient.UserCount.OnMessage -= MarkTextDirty;
            WebSocketClient.Usernames.OnMessage -= MarkTextDirty;
            WebSocketClient.Connect.OnMessage -= MarkTextDirty;
        }

        private void MarkTextDirty(object sender, MessageEventArgs e)
        {
            isTextDirty = true;
        }
        private void UpdateChatWindow()
        {
            chatWindowText.text = WebSocketClient.MessageText;
            userWindowText.text = WebSocketClient.UsernameText;
            userCount.text = $"Users Online : {WebSocketClient.UserCountText}";
            //WelcomeText.text = $"Welcome {WebSocketClient._username} to the server!";

        }
    }
}