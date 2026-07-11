using System.Collections.Generic;
using UnityEngine;

namespace PlatformRunner.Pooling
{
    /// <summary>
    /// Central lookup for ObjectPool&lt;T&gt; instances, keyed by prefab.
    /// Entirely generic - not specific to platform blocks. Any system can call
    /// PoolManager.Instance.GetPool(myPrefab) to get (or lazily create) a pool for it.
    /// This is optional: PlatformGenerator manages its own pools directly, but other
    /// systems (bullets, enemies, coins...) can share this one manager instead of each
    /// rolling their own.
    /// </summary>
    public class PoolManager : MonoBehaviour
    {
        public static PoolManager Instance { get; private set; }

        // Keyed by prefab instance ID since prefab assets are stable references.
        private readonly Dictionary<int, object> _pools = new Dictionary<int, object>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public ObjectPool<T> GetPool<T>(T prefab, int prewarm = 0, Transform parent = null) where T : Component
        {
            int key = prefab.GetInstanceID();

            if (_pools.TryGetValue(key, out object existing))
                return (ObjectPool<T>)existing;

            var pool = new ObjectPool<T>(prefab, prewarm, parent);
            _pools[key] = pool;
            return pool;
        }
    }
}
