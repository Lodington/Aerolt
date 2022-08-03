using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Aerolt.Social
{
    public class SocketGetComponets : MonoBehaviour
    {
        public static SocketGetComponets Instance;
        public TMP_Text messagePanel;
        public TMP_InputField inputField;
        public Button sendButton;
        public TMP_Text usernamePanel;
        
        private void Awake()
        {
            Instance = this;
        }
    }
}