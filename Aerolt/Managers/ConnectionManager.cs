using System;
using Aerolt.Helpers;
using Aerolt.Social;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;
using LogLevel = Aerolt.Enums.LogLevel;

namespace Aerolt.Managers
{
    public class ConnectionManager : MonoBehaviour
    {
        public Button yesButton;
        public Button noButton;
        public GameObject loadingScreen;

        
        public static AssetBundle Assets;
        public static string Path;
        public static GameObject ChatWindow;

        public void DontConnectToServer() => Tools.Log(LogLevel.Warning, "Aerolt will not connect to the server.");

        public void StartConnectToServer()
        {
            WebSocketClient.TryConnect();

            WebSocketClient.DownloadComplete += SetUpAssetBundle;
        }

        private void SetUpAssetBundle()
        {
            Assets = AssetBundle.LoadFromFile(System.IO.Path.Combine(Load.Path!, WebSocketClient.AuthUuid));
            Tools.Log(LogLevel.Information, "Loaded AssetBundle");

            ChatWindow = Assets.LoadAsset<GameObject>("ChatWindowV2");
            var chatwindowRect = ChatWindow.GetComponent(typeof(RectTransform)) as RectTransform;
            chatwindowRect.SetSize(746, 480);
            Instantiate(loadingScreen, transform);
        }
        
    }
}