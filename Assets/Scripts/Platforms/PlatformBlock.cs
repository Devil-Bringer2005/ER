using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using PlatformRunner.Pooling;

namespace PlatformRunner.Platforms
{
    /// <summary>
    /// Put this on the root of every block prefab. It caches its own entry/exit
    /// connectors and can snap itself so a chosen entry lines up exactly with a
    /// target position/rotation - this is how consecutive blocks get glued together.
    /// </summary>
    public class PlatformBlock : MonoBehaviour, IPoolable
    {
        [Tooltip("Leave empty to auto-collect every BlockConnector in children at Awake.")]
        [SerializeField] private List<BlockConnector> connectors = new List<BlockConnector>();

        public IReadOnlyList<BlockConnector> Entries { get { EnsureCached(); return _entries; } }
        public IReadOnlyList<BlockConnector> Exits { get { EnsureCached(); return _exits; } }

        private List<BlockConnector> _entries = new List<BlockConnector>();
        private List<BlockConnector> _exits = new List<BlockConnector>();
        private bool _cached;

        private void Awake()
        {
            EnsureCached();
        }

        /// <summary>
        /// Builds the entry/exit lists on first use. This is called lazily (not just
        /// from Awake) because PlatformGenerator inspects prefab *assets* directly
        /// (e.g. to check "does this prefab have a Ground entry?") before they are
        /// ever instantiated - and Awake() never runs on an asset sitting in the
        /// Project, only on an actual scene instance. Without this, every lookup
        /// against an un-instantiated prefab would incorrectly come back empty.
        /// </summary>
        private void EnsureCached()
        {
            if (_cached) return;
            CacheConnectors();
        }

        private void CacheConnectors()
        {
            if (connectors == null || connectors.Count == 0)
                connectors = GetComponentsInChildren<BlockConnector>(true).ToList();

            _entries = connectors.Where(c => c.Type == ConnectorType.Entry).ToList();
            _exits = connectors.Where(c => c.Type == ConnectorType.Exit).ToList();
            _cached = true;
        }

        /// <summary>First entry matching the category, or null if this block can't accept it.</summary>
        public BlockConnector GetEntry(ConnectorCategory category)
        {
            EnsureCached();
            return _entries.FirstOrDefault(e => e.Category == category);
        }

        public IEnumerable<BlockConnector> GetExits()
        {
            EnsureCached();
            return _exits;
        }

        /// <summary>Default branch-selection strategy: pick uniformly among available exits.</summary>
        public BlockConnector GetRandomExit()
        {
            EnsureCached();
            if (_exits.Count == 0) return null;
            return _exits[Random.Range(0, _exits.Count)];
        }

        /// <summary>
        /// Moves and rotates this block so that `entry` ends up exactly at
        /// targetPosition/targetRotation. Rotating first (around the block's own
        /// pivot) then re-measuring the connector's offset keeps this correct
        /// regardless of how far the connector sits from the block's origin.
        /// </summary>
        public void AlignEntryTo(BlockConnector entry, Vector3 targetPosition, Quaternion targetRotation)
        {
            if (entry == null)
            {
                transform.SetPositionAndRotation(targetPosition, targetRotation);
                return;
            }

            Quaternion rotationDelta = targetRotation * Quaternion.Inverse(entry.Rotation);
            transform.rotation = rotationDelta * transform.rotation;

            // Re-read entry.Position now that rotation has been applied - Transform
            // is always live, so the child connector's world position already reflects it.
            Vector3 positionDelta = targetPosition - entry.Position;
            transform.position += positionDelta;
        }

        public void OnSpawn()
        {
            // Hook for per-spawn setup (reset coins/obstacles state, restart animations, etc).
        }

        public void OnDespawn()
        {
            // Hook for cleanup when this block goes back into the pool.
        }
    }
}
