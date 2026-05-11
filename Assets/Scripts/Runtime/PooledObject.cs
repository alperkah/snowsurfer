using UnityEngine;

namespace SnowSurfer
{
    public sealed class PooledObject : MonoBehaviour
    {
        public ObjectPool Owner { get; private set; }
        public GameObject Prefab { get; private set; }

        public void Configure(ObjectPool owner, GameObject prefab)
        {
            Owner = owner;
            Prefab = prefab;
        }

        public void Release()
        {
            if (Owner != null)
            {
                Owner.Return(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
