namespace EndlessRunner.Player.Powerups.Effects
{
    using EndlessRunner.Player.Movement;
    using UnityEngine;

    [CreateAssetMenu(menuName = "Powerups/Effects/Double Jump", fileName = "New Double Jump Effect")]
    public class DoubleJumpEffect : PowerupEffect
    {
        [SerializeField] private int _bonusJumps = 1;

        public override void Activate(GameObject target)
        {
            var jump = target.GetComponent<PlayerJumpController>();
            if (jump != null) jump.BonusAirJumps += _bonusJumps;
        }

        public override void Deactivate(GameObject target)
        {
            var jump = target.GetComponent<PlayerJumpController>();
            if (jump != null) jump.BonusAirJumps -= _bonusJumps;
        }
    }
}