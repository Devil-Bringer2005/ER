namespace EndlessRunner.Player.Combat
{
    using UnityEngine;

    public interface IDamageable
    {
        void TakeDamage(float amount, GameObject source);
    }
}
