using System;
using TMPro;
using UnityEngine;

namespace Aerolt.Managers
{
    public class PopupManager : MonoBehaviour
    {
        public TextMeshProUGUI title;
        public TextMeshProUGUI body;

        public void SetupPopup(string setTitle, string message)
        {
            title.text = setTitle;
            body.text = message;
        }

        public void DestroyThis()
        {
            Destroy(gameObject);
        }
    }
}