using System;
using Aerolt.Social;
using UnityEngine;
using UnityEngine.UIElements;

namespace Aerolt.Helpers
{
    public class GetDownloadProgress : MonoBehaviour
    {
        private readonly Slider _progressbar;

        public GetDownloadProgress() => _progressbar = GetComponent<Slider>();
        public void Update() => _progressbar.value = WebSocketClient.DownloadProgress;
    }
}