using System.Collections.Generic;
using UnityEngine;

namespace SnowSurfer
{
    public sealed class ObjectPool : MonoBehaviour
    {
        [SerializeField] private Transform poolRoot;

        private readonly Dictionary<GameObject, Queue<GameObject>> pools = new Dictionary<GameObject, Queue<GameObject>>();

        public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (!pools.TryGetValue(prefab, out Queue<GameObject> queue))
            {
                queue = new Queue<GameObject>();
                pools.Add(prefab, queue);
            }

            GameObject instance = queue.Count > 0 ? queue.Dequeue() : CreateInstance(prefab);
            instance.transform.SetPositionAndRotation(position, rotation);
            instance.SetActive(true);
            return instance;
        }

        public void Warm(GameObject prefab, int count)
        {
            for (int i = 0; i < count; i++)
            {
                GameObject instance = CreateInstance(prefab);
                Return(instance);
            }
        }

        public void Return(GameObject instance)
        {
            if (instance == null)
            {
                return;
            }

            PooledObject pooled = instance.GetComponent<PooledObject>();
            if (pooled == null || pooled.Prefab == null)
            {
                instance.SetActive(false);
                return;
            }

            instance.SetActive(false);
            instance.transform.SetParent(poolRoot != null ? poolRoot : transform);

            if (!pools.TryGetValue(pooled.Prefab, out Queue<GameObject> queue))
            {
                queue = new Queue<GameObject>();
                pools.Add(pooled.Prefab, queue);
            }

            queue.Enqueue(instance);
        }

        public void ReturnAllActive()
        {
            PooledObject[] activeObjects = FindObjectsByType<PooledObject>(FindObjectsInactive.Exclude);
            foreach (PooledObject pooled in activeObjects)
            {
                if (pooled.gameObject.activeInHierarchy && pooled.Owner == this)
                {
                    Return(pooled.gameObject);
                }
            }
        }

        private GameObject CreateInstance(GameObject prefab)
        {
            GameObject instance = Instantiate(prefab, poolRoot != null ? poolRoot : transform);
            instance.name = prefab.name;
            PooledObject pooled = instance.GetComponent<PooledObject>();
            if (pooled == null)
            {
                pooled = instance.AddComponent<PooledObject>();
            }

            pooled.Configure(this, prefab);
            return instance;
        }
    }
}
