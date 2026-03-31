using Aerolt.Enums;
using Aerolt.Helpers;
using RoR2;
using UnityEngine;
using UnityEngine.UI;

namespace Aerolt.Overrides
{
    public class IconSwap : MonoBehaviour
    {
        public Image discord = null!;
        public Sprite imageToSwapTo = null!;

        public void Awake() => SwapThisImage();
        public void SwapThisImage()
        {
            discord.sprite = imageToSwapTo;
        }
    }
}