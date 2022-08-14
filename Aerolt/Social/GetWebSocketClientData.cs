using System;
using System.Threading.Tasks;
using RoR2;
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
        private ScrollRect scrollRect;
        private bool setToBottom;

        public void Awake()
        {
            Bind(MarkTextDirty);
            sendButton.onClick.AddListener(SendMessage);
            scrollRect = chatWindowText.gameObject.GetComponentInParent<ScrollRect>();
        }

        public void InsertLobbyID() => inputField.text += $"#{PlatformSystems.lobbyManager.GetLobbyID()}";

        private void Update()
        {
            if (setToBottom && scrollRect.verticalScrollbar.value > 0f)
            {
                scrollRect.verticalScrollbar.value = 0f;
                setToBottom = false;
            }

            if (isTextDirty)
            {
                UpdateChatWindow();
                isTextDirty = false;
            }

            if (Input.GetKeyDown(KeyCode.Return)) SendMessage();
        }

        private void OnEnable()
        {
            TryConnect();
            UpdateChatWindow();
        }

        private void OnDestroy()
        {
            UnBind(MarkTextDirty);
        }

        private void SendMessage()
        {
            if (string.IsNullOrEmpty(inputField.text))
                return;
            Message.Send(inputField.text);
            inputField.text = string.Empty;
        }

        private void MarkTextDirty(object sender, MessageEventArgs e)
        {
            isTextDirty = true;
        }

        private void UpdateChatWindow()
        {
            var scrollToBottom = scrollRect.verticalScrollbar.value < 0.05f;
            chatWindowText.text = MessageText;
            userWindowText.text = UsernameText;
            userCount.text = $"Users Online : {UserCountText}";
            if (scrollToBottom)
                setToBottom = true;
            //WelcomeText.text = $"Welcome {WebSocketClient._username} to the server!";
        }
    }
}