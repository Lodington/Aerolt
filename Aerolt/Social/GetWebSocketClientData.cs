using System;
using Aerolt.Social;
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

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return)) SendMessage();
        }

        private void SendMessage()
        {
            if(String.IsNullOrEmpty(inputField.text))
                return;
            WebSocketClient.Message.Send($"{WebSocketClient._username} -> {inputField.text}");
            inputField.text = String.Empty;
        }
        
        public void Awake()
        {
            WebSocketClient.Message.OnMessage += UpdateChatWindow;
            WebSocketClient.UserCount.OnMessage += UpdateChatWindow;
            WebSocketClient.Usernames.OnMessage += UpdateChatWindow;
            sendButton.onClick.AddListener(SendMessage);
        }

        private void OnDestroy()
        {
            WebSocketClient.Message.OnMessage -= UpdateChatWindow;
            WebSocketClient.UserCount.OnMessage -= UpdateChatWindow;
            WebSocketClient.Usernames.OnMessage -= UpdateChatWindow;
        }

        private void UpdateChatWindow(object sender, MessageEventArgs e)
        {
            chatWindowText.text = WebSocketClient.MessageText;
            userWindowText.text = WebSocketClient.UsernameText;
            userCount.text = WebSocketClient.UserCountText;
        }
    }
}