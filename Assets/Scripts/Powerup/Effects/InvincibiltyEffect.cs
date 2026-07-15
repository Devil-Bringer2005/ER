namespace EndlessRunner.Player.Powerups.Effects
{
    using EndlessRunner.Player.Combat;
    using UnityEngine;

    [CreateAssetMenu(menuName = "Powerups/Effects/Invincibility", fileName = "New Invincibility Effect")]
    public class InvincibilityEffect : PowerupEffect
    {
        public override void Activate(GameObject target) =>
            target.GetComponent<PlayerHealth>()?.BeginInvincibility();

        public override void Deactivate(GameObject target) =>
            target.GetComponent<PlayerHealth>()?.EndInvincibility();
    }
}