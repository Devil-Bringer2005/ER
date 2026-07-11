using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using PlatformRunner.Pooling;

namespace PlatformRunner.Spawning
{
    /// <summary>
    /// Spawns pooled objects onto SpawnPoints, matched by SpawnCategory, and
    /// aligns each spawned instance's ground-contact point to the spawn
    /// point (see SpawnablePlacement) so nothing floats or clips.
    ///
    /// Two ways to use this:
    /// 1) Per-block (recommended for the runner): put this component
    ///    directly on each PlatformBlock prefab. It finds its own
    ///    SpawnPoints automatically, and each pooled block instance keeps
    ///    its own separate pools/roster - so different block prefabs can
    ///    offer completely different spawnables. Call SpawnAll() /
    ///    ReleaseAll() with no arguments.
    /// 2) Shared/external: keep one ObjectSpawner elsewhere and hand it
    ///    explicit point lists via SpawnInCategory(points, category) /
    ///    ReleaseAll(points) - useful for spawn points that aren't tied to
    ///    a pooled block at all (e.g. a static hand-built section of level).
    /// </summary>
    public class ObjectSpawner : MonoBehaviour
    {
        [Serializable]
        private class SpawnableEntry
        {
            [Tooltip("Prefab root must have a SpawnablePlacement component.")]
            public SpawnablePlacement prefab;

            [Tooltip("Relative likelihood of being picked among other entries that share a category. Doesn't need to sum to anything in particular.")]
            [Min(0.01f)] public float weight = 1f;
        }

        [Tooltip("Every prefab the spawner is allowed to place. Category is read from each prefab's own SpawnablePlacement component.")]
        [SerializeField] private List<SpawnableEntry> spawnables = new List<SpawnableEntry>();

        [Tooltip("Chance (rolled once per point) that a valid, unoccupied point actually gets filled. 1 = always fill. Lower values keep populated blocks from feeling identical every time.")]
        [SerializeField, Range(0f, 1f)] private float fillChance = 1f;

        [Header("Pooling")]
        [SerializeField] private int prewarmPerPrefab = 2;
        [SerializeField] private Transform poolParent;

        private readonly Dictionary<SpawnablePlacement, ObjectPool<SpawnablePlacement>> _pools =
            new Dictionary<SpawnablePlacement, ObjectPool<SpawnablePlacement>>();

        private readonly Dictionary<SpawnablePlacement, ObjectPool<SpawnablePlacement>> _instanceToPool =
            new Dictionary<SpawnablePlacement, ObjectPool<SpawnablePlacement>>();

        private readonly Dictionary<SpawnCategory, List<SpawnableEntry>> _byCategory =
            new Dictionary<SpawnCategory, List<SpawnableEntry>>();

        // Tracks which spawn point produced which live instance, so callers
        // can release everything tied to a set of points (e.g. a whole
        // recycled block) without hunting for instances themselves.
        private readonly Dictionary<SpawnPoint, SpawnablePlacement> _liveInstances =
            new Dictionary<SpawnPoint, SpawnablePlacement>();

        private static readonly SpawnCategory[] AllCategories =
            (SpawnCategory[])Enum.GetValues(typeof(SpawnCategory));

        // Cached once: spawn points are prefab-authored (static children), so
        // there's no need to re-search the hierarchy on every spawn/release.
        // If this spawner lives on a pooled block, Awake only runs once per
        // instance (pooling reuses the GameObject via SetActive, not
        // re-instantiation), so the cache stays valid across recycle cycles.
        private SpawnPoint[] _ownSpawnPoints;

        private void Awake()
        {
            foreach (SpawnableEntry entry in spawnables.Where(e => e.prefab != null))
            {
                _pools[entry.prefab] = new ObjectPool<SpawnablePlacement>(entry.prefab, prewarmPerPrefab, poolParent);

                if (!_byCategory.TryGetValue(entry.prefab.Category, out List<SpawnableEntry> list))
                    _byCategory[entry.prefab.Category] = list = new List<SpawnableEntry>();
                list.Add(entry);
            }

            _ownSpawnPoints = GetComponentsInChildren<SpawnPoint>(true);
        }

        /// <summary>Fills every SpawnPoint found under this GameObject at Awake, one pass per category. Use this when the spawner lives on the block itself.</summary>
        public void SpawnAll()
        {
            foreach (SpawnCategory category in AllCategories)
                SpawnInCategory(_ownSpawnPoints, category);
        }

        /// <summary>Releases everything currently spawned under this GameObject's own SpawnPoints. Use this when the spawner lives on the block itself.</summary>
        public void ReleaseAll()
        {
            ReleaseAll(_ownSpawnPoints);
        }

        /// <summary>
        /// Tries to fill every point in `points` whose Category matches
        /// `category`, skipping inactive/already-occupied points and rolling
        /// fillChance per point. Returns whatever actually got spawned.
        /// </summary>
        public List<SpawnablePlacement> SpawnInCategory(IEnumerable<SpawnPoint> points, SpawnCategory category)
        {
            var spawned = new List<SpawnablePlacement>();

            foreach (SpawnPoint point in points)
            {
                if (point.Category != category || !point.IsActive || point.Occupied) continue;
                if (UnityEngine.Random.value > fillChance) continue;

                SpawnablePlacement instance = SpawnAt(point);
                if (instance != null) spawned.Add(instance);
            }

            return spawned;
        }

        /// <summary>Spawns one object at a single point, picking a random weighted prefab registered for that point's category.</summary>
        public SpawnablePlacement SpawnAt(SpawnPoint point)
        {
            if (point.Occupied)
            {
                Debug.LogWarning($"[ObjectSpawner] SpawnPoint '{point.name}' is already occupied - release it before spawning again.", point);
                return null;
            }

            SpawnableEntry entry = PickWeighted(point.Category);
            if (entry == null)
            {
                Debug.LogWarning($"[ObjectSpawner] No spawnable prefab registered for category '{point.Category}'.", point);
                return null;
            }

            ObjectPool<SpawnablePlacement> pool = _pools[entry.prefab];
            SpawnablePlacement instance = pool.Get(point.Position, point.Rotation);
            _instanceToPool[instance] = pool;
            instance.AlignGroundTo(point.Position, point.Rotation);

            point.Occupied = true;
            _liveInstances[point] = instance;
            return instance;
        }

        /// <summary>Releases whatever is spawned at this point back to its pool and frees the point up again. Safe to call on an empty point.</summary>
        public void Release(SpawnPoint point)
        {
            if (!_liveInstances.TryGetValue(point, out SpawnablePlacement instance)) return;

            if (_instanceToPool.TryGetValue(instance, out ObjectPool<SpawnablePlacement> pool))
            {
                pool.Release(instance);
                _instanceToPool.Remove(instance);
            }

            _liveInstances.Remove(point);
            point.Occupied = false;
        }

        /// <summary>Releases every instance tied to any of the given points - call this right before their owning block is recycled.</summary>
        public void ReleaseAll(IEnumerable<SpawnPoint> points)
        {
            foreach (SpawnPoint point in points)
                Release(point);
        }

        private SpawnableEntry PickWeighted(SpawnCategory category)
        {
            if (!_byCategory.TryGetValue(category, out List<SpawnableEntry> candidates) || candidates.Count == 0)
                return null;

            float totalWeight = candidates.Sum(c => c.weight);
            float roll = UnityEngine.Random.value * totalWeight;

            foreach (SpawnableEntry candidate in candidates)
            {
                if (roll <= candidate.weight) return candidate;
                roll -= candidate.weight;
            }

            return candidates[candidates.Count - 1]; // float rounding fallback
        }
    }
}