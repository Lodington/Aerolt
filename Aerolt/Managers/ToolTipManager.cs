using System;
using TMPro;
using UnityEngine;

namespace Aerolt.Managers
{
    public class ToolTipManager : MonoBehaviour
    {
        public static ToolTipManager Instance;

        public TextMeshProUGUI messageText;
        
        public void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
            }
        }

        public void Start()
        {
            gameObject.SetActive(false);
        }

        public void Update()
        {
            transform.position = Input.mousePosition;
        }

        public void SetAndShowToolTip(string message)
        {
            gameObject.SetActive(true);
            messageText.text = message;
        }

        public void HideToolTip()
        {
            gameObject.SetActive(false);
            messageText.text = string.Empty;
        }
        
    }
}