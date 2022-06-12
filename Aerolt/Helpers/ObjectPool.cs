using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Aerolt.Helpers
{
    public class ObjectPool : Singleton<ObjectPool>
    {
        public List<GameObject> prefabsForPool;
        private List<GameObject> _pooledObjects = new List<GameObject>();

        public GameObject GetObjectFromPool(string objectName, bool isDetachable = false)
        {
            var instance = _pooledObjects.FirstOrDefault(obj => obj.name == objectName);
            
            if (instance != null)
            {
                _pooledObjects.Remove(instance);
                instance.SetActive(true);

                return instance;
            }

            // If we don't have a pooled instance
            var prefab = prefabsForPool.FirstOrDefault(obj => obj.name == objectName);
            if (prefab != null)
            {
                GameObject newInstace;

                switch (isDetachable)
                {
                    case true:
                        newInstace = Instantiate(prefab, prefab.transform.position, Quaternion.identity);
                        break;
                    case false:
                        newInstace = Instantiate(prefab, prefab.transform.position, Quaternion.identity, transform);
                        break;
                }
                
                newInstace.name = objectName;
                
                newInstace.transform.localPosition = Vector3.zero;

                return newInstace;
            }
            Tools.Log(Aerolt.Enums.LogLevel.Error, $"Object pool doesn't have a prefab for the object with name {objectName}");
            return null;
        }

        public void PoolObject(GameObject obj)
        {
            obj.SetActive(false);

            _pooledObjects.Add(obj);
        }
    }
}
