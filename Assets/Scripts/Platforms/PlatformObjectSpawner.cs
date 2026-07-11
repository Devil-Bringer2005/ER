using UnityEngine;
using PlatformRunner.Platforms;

namespace PlatformRunner.Spawning
{
    /// <summary>
    /// Bridges PlatformGenerator's block lifecycle to each block's OWN
    /// ObjectSpawner. Every block prefab that wants spawn content carries
    /// its own ObjectSpawner component (with its own roster of spawnables
    /// and its own pools) - this coordinator just calls SpawnAll() on it
    /// right after the generator spawns that block, and ReleaseAll() right
    /// before the generator recycles it. A block with no ObjectSpawner on
    /// it is simply skipped, so plain decorative/empty blocks don't need one.
    ///
    /// PlatformGenerator has no knowledge of spawning at all, and
    /// ObjectSpawner has no knowledge of PlatformGenerator - this is the
    /// only script that ties the two together.
    /// </summary>
    public class PlatformObjectSpawner : MonoBehaviour
    {
        [Tooltip("The generator whose blocks should have their own ObjectSpawner (if present) triggered.")]
        [SerializeField] private PlatformGenerator generator;

        private void OnEnable()
        {
            if (generator == null)
            {
                Debug.LogWarning("[PlatformObjectSpawnCoordinator] No PlatformGenerator assigned - block spawn points will never be filled.", this);
                return;
            }

            generator.OnBlockSpawned += HandleBlockSpawned;
            generator.OnBlockReleasing += HandleBlockReleasing;
        }

        private void OnDisable()
        {
            if (generator == null) return;

            generator.OnBlockSpawned -= HandleBlockSpawned;
            generator.OnBlockReleasing -= HandleBlockReleasing;
        }

        private void HandleBlockSpawned(PlatformBlock block)
        {
            ObjectSpawner spawner = block.GetComponentInChildren<ObjectSpawner>(true);
            spawner?.SpawnAll();
        }

        private void HandleBlockReleasing(PlatformBlock block)
        {
            ObjectSpawner spawner = block.GetComponentInChildren<ObjectSpawner>(true);
            spawner?.ReleaseAll();
        }
    }
}