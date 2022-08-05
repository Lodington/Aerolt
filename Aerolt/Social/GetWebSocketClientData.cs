using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;
using static Aerolt.Social.WebSocketClient;

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

        public void Awake()
        {
            Message.OnMessage += MarkTextDirty;
            UserCount.OnMessage += MarkTextDirty;
            Usernames.OnMessage += MarkTextDirty;
            Connect.OnMessage += MarkTextDirty;
            sendButton.onClick.AddListener(SendMessage);
        }

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

        private void OnDestroy()
        {
            Message.OnMessage -= MarkTextDirty;
            UserCount.OnMessage -= MarkTextDirty;
            Usernames.OnMessage -= MarkTextDirty;
            Connect.OnMessage -= MarkTextDirty;
        }

        private void SendMessage()
        {
            if (string.IsNullOrEmpty(inputField.text))
                return;
            Message.Send($" [{_username}] -> {inputField.text}");
            inputField.text = string.Empty;
        }

        private void MarkTextDirty(object sender, MessageEventArgs e)
        {
            isTextDirty = true;
        }

        private void UpdateChatWindow()
        {
            chatWindowText.text = MessageText;
            userWindowText.text = UsernameText;
            userCount.text = $"Users Online : {UserCountText}";
            //WelcomeText.text = $"Welcome {WebSocketClient._username} to the server!";
        }
    }
}