namespace EndlessRunner.Player.Powerups.Effects
{
    using EndlessRunner.Player.Movement;
    using UnityEngine;

    /// <summary>Instant effect - assign to a PowerupData with Duration &lt;= 0.</summary>
    [CreateAssetMenu(menuName = "Powerups/Effects/Fill Boost Meter", fileName = "New Fill Boost Meter Effect")]
    public class FillBoostMeterEffect : PowerupEffect
    {
        public override void Activate(GameObject target)
        {
            var meter = target.GetComponentInParent<SpeedBoostMeter>();
            if (meter != null) meter.AddCharge(meter.MaxCharge);
        }

        public override void Deactivate(GameObject target) { }
    }
}