using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlatformRunner.Pooling
{
    /// <summary>
    /// Optional callback interface. Implement this on any pooled component to get
    /// notified when it is taken from or returned to the pool (e.g. to reset state,
    /// restart a particle system, re-enable a Rigidbody, etc).
    /// </summary>
    public interface IPoolable
    {
        void OnSpawn();
        void OnDespawn();
    }

    /// <summary>
    /// A plain C# generic object pool - no MonoBehaviour required to own it.
    /// Works with any Component type, so the same class can back platform blocks,
    /// bullets, pickups, enemies, VFX, etc. Create one instance per prefab.
    /// </summary>
    public class ObjectPool<T> where T : Component
    {
        private readonly T _prefab;
        private readonly Transform _parent;
        private readonly Func<T, T> _customInstantiate;

        private readonly Stack<T> _inactive = new Stack<T>();
        private readonly HashSet<T> _allInstances = new HashSet<T>();

        public int CountActive { get; private set; }
        public int CountInactive => _inactive.Count;
        public int CountAll => CountActive + CountInactive;

        /// <param name="prefab">Prefab (or template instance) to clone.</param>
        /// <param name="initialSize">How many instances to pre-create up front.</param>
        /// <param name="parent">Optional transform new/returned instances get parented under.</param>
        /// <param name="customInstantiate">Optional override for how a new instance is created.</param>
        public ObjectPool(T prefab, int initialSize = 0, Transform parent = null, Func<T, T> customInstantiate = null)
        {
            _prefab = prefab;
            _parent = parent;
            _customInstantiate = customInstantiate;
            Prewarm(initialSize);
        }

        public void Prewarm(int count)
        {
            for (int i = 0; i < count; i++)
            {
                T instance = CreateNew();
                instance.gameObject.SetActive(false);
                _inactive.Push(instance);
            }
        }

        private T CreateNew()
        {
            T instance = _customInstantiate != null
                ? _customInstantiate(_prefab)
                : UnityEngine.Object.Instantiate(_prefab, _parent);

            _allInstances.Add(instance);
            return instance;
        }

        public T Get(Vector3 position, Quaternion rotation, Transform parent = null)
        {
            T instance = _inactive.Count > 0 ? _inactive.Pop() : CreateNew();

            Transform t = instance.transform;
            if (parent != null) t.SetParent(parent, false);
            t.SetPositionAndRotation(position, rotation);

            instance.gameObject.SetActive(true);
            CountActive++;

            if (instance is IPoolable poolable) poolable.OnSpawn();
            return instance;
        }

        public T Get() => Get(Vector3.zero, Quaternion.identity);

        public void Release(T instance)
        {
            if (instance == null) return;

            if (instance is IPoolable poolable) poolable.OnDespawn();

            instance.gameObject.SetActive(false);
            if (_parent != null) instance.transform.SetParent(_parent, false);

            _inactive.Push(instance);
            CountActive = Mathf.Max(0, CountActive - 1);
        }

        /// <summary>Destroys every instance this pool has ever created (active or not).</summary>
        public void Clear(bool destroyInstances = true)
        {
            if (destroyInstances)
            {
                foreach (T instance in _allInstances)
                    if (instance != null) UnityEngine.Object.Destroy(instance.gameObject);
            }

            _allInstances.Clear();
            _inactive.Clear();
            CountActive = 0;
        }
    }
}
