namespace PlatformRunner.Spawning
{
    using UnityEngine;
    using EndlessRunner.Scoring;

    /// <summary>
    /// Put on a collectible prefab (alongside SpawnablePlacement with
    /// category set to Collectible). Awards the multiplierSource's direct
    /// score bonus and multiplier bump when the player touches it, then
    /// removes itself. Tune both amounts on the MultiplierSourceSO asset -
    /// a plain "coin" can set MultiplierIncrease to 0 and just award score.
    /// </summary>
    [RequireComponent(typeof(SpawnablePlacement))]
    [AddComponentMenu("Spawning/Collectible Pickup")]
    public class CollectiblePickup : MonoBehaviour
    {
        [SerializeField] private LayerMask playerLayer;
        [SerializeField] private MultiplierSourceSO multiplierSource;

        private void OnTriggerEnter(Collider other)
        {
            if (((1 << other.gameObject.layer) & playerLayer) == 0) return;

            ScoreManager.Instance?.TriggerMultiplierSource(multiplierSource);
            Destroy(gameObject);
        }
    }
}