namespace EndlessRunner.Collectibles
{
    using MoreMountains.Feedbacks;
    using PlatformRunner.Spawning;
    using UnityEngine;

    /// <summary>
    /// World-space pickup. Delegates its gameplay effect entirely to the assigned CollectibleData,
    /// so adding a new collectible category never touches this script - just author a new SO
    /// subclass and drop an asset of it here.
    ///
    /// Requires a SpawnablePlacement so it can be placed by ObjectSpawner like any other
    /// spawnable, matched to a SpawnPoint by category. On pickup it calls RequestRelease()
    /// so the spawner immediately frees that SpawnPoint and returns this instance to its
    /// pool, rather than waiting for the whole block to recycle.
    /// </summary>
    [AddComponentMenu("Collectibles/Collectible")]
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(SpawnablePlacement))]
    public class Collectible : MonoBehaviour
    {
        [SerializeField] private CollectibleData _data;
        [SerializeField] private MMF_Player _pickupFeedback;

        private SpawnablePlacement _placement;
        private bool _collected;

        private void Reset() => GetComponent<Collider>().isTrigger = true;

        private void Awake() => _placement = GetComponent<SpawnablePlacement>();

        // Pooled instances are reactivated with SetActive(true) rather than
        // re-instantiated, and OnEnable fires every time that happens - this
        // is what makes a recycled instance collectible again.
        private void OnEnable() => _collected = false;

        private void OnTriggerEnter(Collider other)
        {
            if (_collected || _data == null) return;

            _collected = true;
            _data.Collect(other.gameObject);
            CollectibleEvents.RaiseCollected(_data, other.gameObject);

            if (_pickupFeedback != null)
                _pickupFeedback.PlayFeedbacks();

            // Tell whatever ObjectSpawner placed us to free our SpawnPoint
            // and return this instance to its pool.
            _placement.RequestRelease();

            // Deactivate directly too - keeps this working even if the
            // instance was hand-placed with no spawner listening, and is a
            // harmless no-op if the pool already did this via the line above.
            gameObject.SetActive(false);
        }
    }
}