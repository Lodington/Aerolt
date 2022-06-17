using System;
using TMPro;
using UnityEngine;

namespace Aerolt.Helpers
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class ChangeTitle : MonoBehaviour
    {
        public void Awake()
        {
            GetComponent<TextMeshProUGUI>().text = $"v{Load.Version}";
        }
    }
}