namespace EndlessRunner.Player.Combat
{
    using UnityEngine;

    /// <summary>
    /// Base for a piece of equippable, designer-tunable attack data + behaviour, mirroring the
    /// fire-mode ScriptableObject strategy pattern used by the weapon system. Concrete attacks
    /// (melee, ranged, future throwables/magic) live in their own asset type with their own
    /// Execute() implementation, and are swappable in the Inspector without touching
    /// PlayerAttackController at all.
    /// </summary>
    public abstract class AttackDefinitionSO : ScriptableObject
    {
        [SerializeField] private string _animatorTrigger = "Attack";
        [SerializeField] private float _cooldown = 0.5f;
        [SerializeField] private float _damage = 10f;

        public string AnimatorTrigger => _animatorTrigger;
        public float Cooldown => _cooldown;
        public float Damage => _damage;

        public abstract void Execute(in PlayerAttackContext context);
    }
}
