
using UnityEngine;

namespace Aerolt.Helpers
{
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour 
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                return _instance;
            }
        }

        public void Awake()
        {
            _instance = (T) (object) this;
        }

    }
}
