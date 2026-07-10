namespace EndlessRunner.Player.Combat
{
    using UnityEngine;

    [CreateAssetMenu(menuName = "Player/Combat/Melee Attack", fileName = "NewMeleeAttack")]
    public class MeleeAttackDefinitionSO : AttackDefinitionSO
    {
        [Header("Melee Timing")]
        [Tooltip("Delay after the trigger fires before the hitbox becomes active - line this up with the swing's wind-up.")]
        [SerializeField] private float _startupDelay = 0.1f;

        [Tooltip("How long the hitbox stays active once it opens.")]
        [SerializeField] private float _activeDuration = 0.15f;

        [Header("Melee Shape")]
        [SerializeField] private float _range = 1.5f;
        [SerializeField] private float _radius = 0.75f;
        [SerializeField] private LayerMask _hittableLayers = ~0;

        public float StartupDelay => _startupDelay;
        public float ActiveDuration => _activeDuration;
        public float Range => _range;
        public float Radius => _radius;
        public LayerMask HittableLayers => _hittableLayers;

        public override void Execute(in PlayerAttackContext context)
        {
            if (context.MeleeHitbox == null)
            {
                Debug.LogWarning($"{name}: no {nameof(PlayerMeleeHitbox)} in context, melee attack did nothing.");
                return;
            }

            context.MeleeHitbox.Swing(this);
        }
    }
}
