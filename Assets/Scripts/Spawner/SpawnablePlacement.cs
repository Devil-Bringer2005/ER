using System;
using UnityEngine;

namespace PlatformRunner.Spawning
{
    /// <summary>
    /// Put this on the root of any prefab you want ObjectSpawner to place.
    /// Solves the "floating object" problem: most models aren't modeled with
    /// their pivot exactly at their base, so if the spawner just dropped the
    /// root at the spawn point's position, the object would hover or clip
    /// depending on wherever its pivot happens to sit.
    ///
    /// Instead, drag whichever child transform actually touches the ground
    /// (bottom of a collider, a character's feet, etc.) into `groundPoint`.
    /// The spawner then offsets the root so THAT point lands exactly on the
    /// spawn point - not the root's arbitrary pivot.
    ///
    /// Leave groundPoint empty if the prefab's root pivot is already
    /// authored at its base; the object is then placed as-is.
    /// </summary>
    [DisallowMultipleComponent]
    public class SpawnablePlacement : MonoBehaviour
    {
        [Tooltip("Category this object belongs to - must match a SpawnPoint's category to be placed there.")]
        [SerializeField] private SpawnCategory category;

        [Tooltip("Child transform marking where this object touches the ground. Leave empty if the root pivot is already at the base.")]
        [SerializeField] private Transform groundPoint;

        public SpawnCategory Category => category;

        /// <summary>
        /// Raised when this instance wants to hand itself back before its
        /// owning block/section would normally recycle it - e.g. a
        /// Collectible that's just been picked up. Whoever spawned this
        /// instance (typically an ObjectSpawner) subscribes to this, so a
        /// one-shot spawnable can give itself up without knowing anything
        /// about spawners, pools, or SpawnPoints.
        /// </summary>
        public event Action<SpawnablePlacement> ReleaseRequested;

        /// <summary>Call this to hand the instance back to whatever spawned it, instead of waiting for the owning block to recycle.</summary>
        public void RequestRelease() => ReleaseRequested?.Invoke(this);

        /// <summary>
        /// Offset from the root to the ground point, expressed in the root's
        /// own local space. Because it's derived from the current local
        /// hierarchy (not a cached world-space value), it's correct
        /// regardless of how the root is later moved or rotated when placed
        /// in the world.
        /// </summary>
        private Vector3 GroundOffsetLocal => groundPoint != null
            ? transform.InverseTransformPoint(groundPoint.position)
            : Vector3.zero;

        /// <summary>
        /// Moves/rotates this instance so groundPoint lands exactly on
        /// `worldPosition` with the given `worldRotation`, instead of the
        /// root pivot landing there.
        /// </summary>
        public void AlignGroundTo(Vector3 worldPosition, Quaternion worldRotation)
        {
            transform.rotation = worldRotation;
            transform.position = worldPosition - (worldRotation * GroundOffsetLocal);
        }
    }
}