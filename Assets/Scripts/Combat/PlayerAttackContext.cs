namespace EndlessRunner.Player.Combat
{
    using UnityEngine;

    /// <summary>
    /// Everything an AttackDefinitionSO needs to execute, bundled so attack data never has to
    /// hold scene references and stays a reusable, shareable asset across gun/player instances.
    /// </summary>
    public readonly struct PlayerAttackContext
    {
        public readonly GameObject Owner;
        public readonly Transform MuzzlePoint;
        public readonly PlayerMeleeHitbox MeleeHitbox;

        public PlayerAttackContext(GameObject owner, Transform muzzlePoint, PlayerMeleeHitbox meleeHitbox)
        {
            Owner = owner;
            MuzzlePoint = muzzlePoint;
            MeleeHitbox = meleeHitbox;
        }
    }
}
