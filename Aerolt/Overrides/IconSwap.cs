using System;
using RoR2;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Aerolt.Overrides
{
    public class IconSwap : MonoBehaviour
    {
        public Image discord;
        public Sprite imageToSwapTo;

        public void Awake()
        {
            int ran = Random.Range(0, 99);

            if (ran < 1)
            {
                SwapThisImage();
            }
        }
        
        public void SwapThisImage()
        {
            discord.sprite = imageToSwapTo;
        }
        
    }
}