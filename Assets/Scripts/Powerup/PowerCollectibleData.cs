namespace EndlessRunner.Collectibles
{
    using EndlessRunner.Player.Powerups;
    using UnityEngine;

    /// <summary>
    /// Reuses the existing Collectible world-pickup pipeline (trigger detection, feedback,
    /// CollectibleEvents) rather than a parallel powerup-pickup script - a powerup is just
    /// another CollectibleData category that happens to forward to PowerupController.
    /// </summary>
    [CreateAssetMenu(menuName = "Collectibles/Powerup Collectible", fileName = "New Powerup Collectible")]
    public class PowerupCollectibleData : CollectibleData
    {
        [SerializeField] private PowerupData _powerup;

        public override void Collect(GameObject collector) =>
            collector.GetComponent<PowerupController>()?.Activate(_powerup);
    }
}