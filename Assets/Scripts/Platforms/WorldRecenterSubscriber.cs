using UnityEngine;

namespace PlatformRunner.Platforms
{
    /// <summary>
    /// Example only - drop this on your player and/or camera rig (or copy the two
    /// lines into your existing controller). It keeps the object's position
    /// consistent whenever PlatformGenerator recenters the world.
    /// </summary>
    public class WorldRecenterSubscriber : MonoBehaviour
    {
        private void OnEnable() => PlatformGenerator.OnWorldRecentered += HandleRecenter;
        private void OnDisable() => PlatformGenerator.OnWorldRecentered -= HandleRecenter;

        private void HandleRecenter(Vector3 offset)
        {
            transform.position += offset;
        }
    }
}
