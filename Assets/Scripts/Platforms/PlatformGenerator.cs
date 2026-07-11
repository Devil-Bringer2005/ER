using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using PlatformRunner.Pooling;

namespace PlatformRunner.Platforms
{
    /// <summary>
    /// Drives the infinite runner: keeps exactly `activeBlockCount` blocks spawned
    /// and connected end-to-end, recycles blocks the player has passed back into
    /// their pool, and periodically recenters the whole world back near the origin
    /// so world-space coordinates never grow without bound.
    /// </summary>
    public class PlatformGenerator : MonoBehaviour
    {
        [Header("Blocks")]
        [Tooltip("Every prefab that can be spawned. Each needs a PlatformBlock component.")]
        [SerializeField] private List<PlatformBlock> blockPrefabs = new List<PlatformBlock>();

        [Tooltip("How many blocks are alive at any given time. Changing this in the Inspector during Play updates the chain immediately.")]
        [SerializeField, Min(1)] private int activeBlockCount = 3;

        [Tooltip("Fallback category for the very first generated block's entry, only used if Initial Platform below is left empty.")]
        [SerializeField] private ConnectorCategory startingCategory = ConnectorCategory.Ground;

        [Header("Initial Platform")]
        [Tooltip("A platform already placed in the scene (not one of the pooled prefabs). If set, the first generated block connects to THIS platform's exit instead of the generator spawning a starting block itself. This platform is never pooled or destroyed by the generator - it's just the anchor generation starts from.")]
        [SerializeField] private PlatformBlock initialPlatform;

        [Header("World Recenter")]
        [Tooltip("When the spawn cursor gets this far from the origin, shift everything back so the oldest active block starts at zero again.")]
        [SerializeField] private float recenterDistance = 1000f;

        [Header("Pooling")]
        [SerializeField] private int prewarmPerPrefab = 2;
        [SerializeField] private Transform poolParent;

        /// <summary>
        /// Raised whenever the world is recentered. Subscribe from your player
        /// controller / camera rig and add the same offset to your own position
        /// so nothing visibly jumps.
        /// </summary>
        public static event Action<Vector3> OnWorldRecentered;

        /// <summary>
        /// Raised right after a block is spawned and fully positioned (after any
        /// world recenter that happened as part of that same spawn, so its
        /// transform is already final). Subscribe to this to populate per-block
        /// content - e.g. fill that block's SpawnPoints with collectibles or
        /// obstacles. Instance event (not static) since content spawning is
        /// naturally scoped to whichever generator raised it.
        /// </summary>
        public event Action<PlatformBlock> OnBlockSpawned;

        /// <summary>
        /// Raised right before a block is returned to its pool, whether because
        /// the player passed it or because activeBlockCount was reduced. The
        /// block and its children are still fully valid at this point, so
        /// subscribers can safely query it - e.g. to release anything spawned at
        /// its SpawnPoints - before it gets pooled and possibly reused elsewhere.
        /// </summary>
        public event Action<PlatformBlock> OnBlockReleasing;

        private readonly Dictionary<PlatformBlock, ObjectPool<PlatformBlock>> _pools = new Dictionary<PlatformBlock, ObjectPool<PlatformBlock>>();
        private readonly Dictionary<PlatformBlock, ObjectPool<PlatformBlock>> _instanceToPool = new Dictionary<PlatformBlock, ObjectPool<PlatformBlock>>();
        private readonly Dictionary<PlatformBlock, BlockExitTrigger[]> _triggerLookup = new Dictionary<PlatformBlock, BlockExitTrigger[]>();
        private readonly Dictionary<PlatformBlock, BlockConnector> _chosenExit = new Dictionary<PlatformBlock, BlockConnector>();
        private readonly LinkedList<PlatformBlock> _active = new LinkedList<PlatformBlock>();

        private Vector3 _nextSpawnPosition;
        private Quaternion _nextSpawnRotation = Quaternion.identity;
        private ConnectorCategory _nextRequiredCategory;
        private Vector3 _originAnchor;

        private void Awake()
        {
            foreach (PlatformBlock prefab in blockPrefabs.Where(p => p != null).Distinct())
                _pools[prefab] = new ObjectPool<PlatformBlock>(prefab, prewarmPerPrefab, poolParent);
        }

        private void Start()
        {
            InitializeSpawnCursor();

            for (int i = 0; i < activeBlockCount; i++)
                SpawnNextBlock();
        }

        /// <summary>
        /// Sets where the next generated block will attach. If an Initial Platform
        /// is assigned, generation starts from its exit (it is never itself pooled
        /// or recycled - it's a fixed, hand-placed piece). Otherwise falls back to
        /// this transform + Starting Category, same as before.
        /// </summary>
        private void InitializeSpawnCursor()
        {
            if (initialPlatform != null)
            {
                BlockConnector startExit = ChooseExit(initialPlatform);
                if (startExit != null)
                {
                    _nextSpawnPosition = startExit.Position;
                    _nextSpawnRotation = startExit.Rotation;
                    _nextRequiredCategory = startExit.Category;
                    _originAnchor = _nextSpawnPosition;
                    return;
                }

                Debug.LogWarning("[PlatformGenerator] Initial Platform is assigned but has no exit connector - falling back to Transform-based start.", this);
            }

            _nextSpawnPosition = transform.position;
            _nextSpawnRotation = transform.rotation;
            _nextRequiredCategory = startingCategory;
            _originAnchor = _nextSpawnPosition;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying && _active.Count > 0)
                AdjustActiveCount();
        }
#endif

        private void AdjustActiveCount()
        {
            while (_active.Count < activeBlockCount)
                SpawnNextBlock();

            while (_active.Count > activeBlockCount)
            {
                PlatformBlock last = _active.Last.Value;
                _active.RemoveLast();
                ReleaseBlock(last);

                // Recompute the spawn cursor from the new tail so the chain stays connected.
                if (_active.Count > 0 && _chosenExit.TryGetValue(_active.Last.Value, out BlockConnector exit) && exit != null)
                {
                    _nextSpawnPosition = exit.Position;
                    _nextSpawnRotation = exit.Rotation;
                    _nextRequiredCategory = exit.Category;
                }
                else if (_active.Count == 0)
                {
                    InitializeSpawnCursor();
                }
            }
        }

        private void SpawnNextBlock()
        {
            PlatformBlock prefab = PickPrefab(_nextRequiredCategory);
            if (prefab == null)
            {
                Debug.LogError($"[PlatformGenerator] No block prefab has an entry of category '{_nextRequiredCategory}'.");
                return;
            }

            ObjectPool<PlatformBlock> pool = _pools[prefab];
            PlatformBlock block = pool.Get(_nextSpawnPosition, _nextSpawnRotation);
            _instanceToPool[block] = pool;

            BlockConnector entry = block.GetEntry(_nextRequiredCategory);
            block.AlignEntryTo(entry, _nextSpawnPosition, _nextSpawnRotation);

            HookExitTriggers(block);
            _active.AddLast(block);

            BlockConnector chosenExit = ChooseExit(block);
            _chosenExit[block] = chosenExit;

            if (chosenExit != null)
            {
                _nextSpawnPosition = chosenExit.Position;
                _nextSpawnRotation = chosenExit.Rotation;
                _nextRequiredCategory = chosenExit.Category;
            }

            MaybeRecenterWorld();

            // Fired last, after position/rotation are fully final for this
            // spawn (including any recenter shift above), so subscribers see
            // the block exactly where it will actually sit.
            OnBlockSpawned?.Invoke(block);
        }

        /// <summary>Override in a subclass to control branching (e.g. follow player's lane) instead of random.</summary>
        protected virtual BlockConnector ChooseExit(PlatformBlock block) => block.GetRandomExit();

        private PlatformBlock PickPrefab(ConnectorCategory requiredEntryCategory)
        {
            var candidates = blockPrefabs.Where(p => p != null && p.GetEntry(requiredEntryCategory) != null).ToList();
            return candidates.Count == 0 ? null : candidates[UnityEngine.Random.Range(0, candidates.Count)];
        }

        private void HookExitTriggers(PlatformBlock block)
        {
            BlockExitTrigger[] triggers = block.GetComponentsInChildren<BlockExitTrigger>(true);
            foreach (BlockExitTrigger trigger in triggers)
            {
                trigger.PlayerExited -= RecycleThroughBlock; // guard against double-subscribe on reused instances
                trigger.PlayerExited += RecycleThroughBlock;
            }
            _triggerLookup[block] = triggers;
        }

        private void RecycleThroughBlock(PlatformBlock passedBlock)
        {
            // Guard: a block that isn't part of the tracked chain (e.g. a stray
            // BlockExitTrigger placed on the Initial Platform, which is intentionally
            // never added to _active) has nothing to recycle against. Without this
            // check the loop below would never hit its break condition and would
            // drain the entire active chain instead.
            if (!_active.Contains(passedBlock)) return;

            // Recycle from the oldest active block up to and including passedBlock,
            // spawning a fresh block onto the tail for each one removed so the
            // configured count stays constant.
            while (_active.Count > 0)
            {
                PlatformBlock oldest = _active.First.Value;
                _active.RemoveFirst();
                ReleaseBlock(oldest);

                SpawnNextBlock();

                if (oldest == passedBlock) break;
            }
        }

        private void ReleaseBlock(PlatformBlock block)
        {
            // Fired first, while the block and its children are still fully
            // active/valid, so subscribers (e.g. a spawn-point coordinator) can
            // still query it before pool.Release() below potentially disables
            // or repositions it.
            OnBlockReleasing?.Invoke(block);

            if (_triggerLookup.TryGetValue(block, out BlockExitTrigger[] triggers))
                foreach (BlockExitTrigger trigger in triggers)
                    if (trigger != null) trigger.PlayerExited -= RecycleThroughBlock;
            _triggerLookup.Remove(block);
            _chosenExit.Remove(block);

            if (_instanceToPool.TryGetValue(block, out ObjectPool<PlatformBlock> pool))
            {
                pool.Release(block);
                _instanceToPool.Remove(block);
            }
        }

        private void MaybeRecenterWorld()
        {
            if (_active.Count == 0) return;

            Vector3 oldestBlockOrigin = _active.First.Value.transform.position;
            if (Vector3.Distance(oldestBlockOrigin, _originAnchor) < recenterDistance) return;

            Vector3 offset = _originAnchor - oldestBlockOrigin;

            foreach (PlatformBlock block in _active)
                block.transform.position += offset;

            if (initialPlatform != null)
                initialPlatform.transform.position += offset;

            _nextSpawnPosition += offset;

            OnWorldRecentered?.Invoke(offset);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(_nextSpawnPosition, 0.5f);
        }
#endif
    }
}