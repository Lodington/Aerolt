
using UnityEngine;

namespace Aerolt.Helpers
{
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour 
    {
        public static T Instance { get; private set; }

        public void Awake() => Instance = (T) (object) this;
    }
}
