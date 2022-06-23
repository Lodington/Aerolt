using System;
using Aerolt.Managers;
using UnityEngine;

namespace Aerolt.Helpers
{
    public class ToolTip : MonoBehaviour
    {
        public string message;

        public void OnMouseEnter()
        {
            ToolTipManager.Instance.SetAndShowToolTip(message);
        }

        public void OnMouseExit()
        {
            ToolTipManager.Instance.HideToolTip();
        }
    }
}