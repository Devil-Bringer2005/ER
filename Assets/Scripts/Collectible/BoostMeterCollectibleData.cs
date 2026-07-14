namespace EndlessRunner.Collectibles
{
    using EndlessRunner.Player.Movement;
    using UnityEngine;

    /// <summary>
    /// Adds charge to whatever SpeedBoostMeter is on the collector. If the collector doesn't
    /// have one, this is a silent no-op - same GetComponent-and-move-on pattern PlayerMotorDriver
    /// uses for its contributors, so no cross-reference to the player exists anywhere.
    /// </summary>
    [CreateAssetMenu(menuName = "Collectibles/Boost Meter Collectible", fileName = "New Boost Meter Collectible")]
    public class BoostMeterCollectibleData : CollectibleData
    {
        [SerializeField] private float _meterAmount = 10f;

        public override void Collect(GameObject collector) =>
            collector.GetComponent<SpeedBoostMeter>()?.AddCharge(_meterAmount);
    }
}