using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Aerolt.Helpers
{
    public class ObjectPool : Singleton<ObjectPool>
    {
        /// <summary>
        /// List of the objects to be pooled
        /// </summary>
        public List<GameObject> PrefabsForPool;

        /// <summary>
        /// List of the pooled objects
        /// </summary>
        private List<GameObject> _pooledObjects = new List<GameObject>();

        public GameObject GetObjectFromPool(string objectName)
        {
            // Try to get a pooled instance
            var instance = _pooledObjects.FirstOrDefault(obj => obj.name == objectName);

            // If we have a pooled instance already
            if (instance != null)
            {
                // Remove it from the list of pooled objects
                _pooledObjects.Remove(instance);

                // Enable it
                instance.SetActive(true);

                return instance;
            }

            // If we don't have a pooled instance
            var prefab = PrefabsForPool.FirstOrDefault(obj => obj.name == objectName);
            if (prefab != null)
            {
                // Create a new instance
                var newInstace = Instantiate(prefab, prefab.transform.position, Quaternion.identity, transform);

                // Make sure you set it's name (so you remove the Clone that Unity ads)
                newInstace.name = objectName;

                // Set it's position to zero
                newInstace.transform.localPosition = Vector3.zero;

                return newInstace;
            }

            Debug.LogWarning("Object pool doesn't have a prefab for the object with name " + objectName);
            return null;
        }

        public void PoolObject(GameObject obj)
        {
            // Disable the object
            obj.SetActive(false);

            // Add it to the list of pooled objects
            _pooledObjects.Add(obj);
        }
    }
}
