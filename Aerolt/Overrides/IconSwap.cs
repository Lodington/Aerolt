using UnityEngine;
using UnityEngine.UI;

namespace Aerolt.Overrides
{
    public class IconSwap : MonoBehaviour
    {
        public Image discord;
        public Sprite imageToSwapTo;

        public void Awake()
        {
            var ran = Random.Range(0, 99);

            if (ran < 20) SwapThisImage();
        }

        public void SwapThisImage()
        {
            discord.sprite = imageToSwapTo;
        }
    }
}