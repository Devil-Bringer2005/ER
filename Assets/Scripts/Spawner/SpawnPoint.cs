using UnityEngine;

namespace PlatformRunner.Spawning
{
    /// <summary>
    /// Marks one location an object can be spawned at. Place these as child
    /// transforms wherever you want something to appear - typically as
    /// children of a PlatformBlock, positioned exactly on the surface the
    /// spawned object should rest on (the ObjectSpawner handles the rest of
    /// the ground alignment via SpawnablePlacement).
    /// </summary>
    [DisallowMultipleComponent]
    public class SpawnPoint : MonoBehaviour
    {
        [Tooltip("Only a SpawnablePlacement with this same category can be spawned here.")]
        [SerializeField] private SpawnCategory category;

        [Tooltip("Untick to pull this point out of rotation (e.g. difficulty gating) without deleting it.")]
        [SerializeField] private bool isActive = true;

        public SpawnCategory Category => category;
        public bool IsActive { get => isActive; set => isActive = value; }

        /// <summary>Runtime-only: true once ObjectSpawner has placed something here that hasn't been released yet.</summary>
        public bool Occupied { get; set; }

        public Vector3 Position => transform.position;
        public Quaternion Rotation => transform.rotation;

#if UNITY_EDITOR
        // Color-coded gizmo so a block with mixed spawn points (e.g. coins +
        // obstacles) stays readable in the Scene view.
        private void OnDrawGizmos()
        {
            Gizmos.color = GizmoColor(category);
            Gizmos.DrawWireSphere(transform.position, 0.25f);
            Gizmos.DrawLine(transform.position, transform.position + transform.up * 0.5f);
        }

        private static Color GizmoColor(SpawnCategory c) => c switch
        {
            SpawnCategory.Collectible => Color.yellow,
            SpawnCategory.Obstacle => Color.red,
            SpawnCategory.Decoration => Color.green,
            SpawnCategory.PowerUp => Color.cyan,
            _ => Color.white
        };
#endif
    }
}
