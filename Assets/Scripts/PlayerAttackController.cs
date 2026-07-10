namespace EndlessRunner.Player.Combat
{
    using System;
    using UnityEngine;
    using EndlessRunner.Player.Controls;

    /// <summary>
    /// Orchestrates equipped attacks: listens for input, enforces per-attack cooldowns, builds
    /// the context each AttackDefinitionSO needs, and announces what fired so animation/UI can
    /// react without knowing anything about combat rules. Fields are typed as the base
    /// AttackDefinitionSO so either slot accepts any attack type - assign a MeleeAttackDefinitionSO
    /// or RangedAttackDefinitionSO asset in the Inspector.
    /// </summary>
    [AddComponentMenu("Player/Combat/Player Attack Controller")]
    public class PlayerAttackController : MonoBehaviour
    {
        [SerializeField] private AttackDefinitionSO _meleeAttack;
        [SerializeField] private AttackDefinitionSO _rangedAttack;
        [SerializeField] private Transform _muzzlePoint;
        [SerializeField] private PlayerMeleeHitbox _meleeHitbox;

        private IPlayerInputSource _inputSource;
        private float _nextMeleeTime;
        private float _nextRangedTime;

        /// <summary>Fired the moment an attack is executed, with the SO that fired. Animation listens here.</summary>
        public event Action<AttackDefinitionSO> AttackExecuted;

        private void Awake()
        {
            _inputSource = GetComponentInParent<IPlayerInputSource>();
            if (_inputSource == null)
                Debug.LogError($"{nameof(PlayerAttackController)} needs an {nameof(IPlayerInputSource)} on this object or a parent.", this);
        }

        private void OnEnable()
        {
            if (_inputSource == null) return;
            _inputSource.MeleeAttackPressed += TryMeleeAttack;
            _inputSource.RangedAttackPressed += TryRangedAttack;
        }

        private void OnDisable()
        {
            if (_inputSource == null) return;
            _inputSource.MeleeAttackPressed -= TryMeleeAttack;
            _inputSource.RangedAttackPressed -= TryRangedAttack;
        }

        public void TryMeleeAttack() => TryExecute(_meleeAttack, ref _nextMeleeTime);
        public void TryRangedAttack() => TryExecute(_rangedAttack, ref _nextRangedTime);

        private void TryExecute(AttackDefinitionSO attack, ref float nextAllowedTime)
        {
            if (attack == null || Time.time < nextAllowedTime) return;

            nextAllowedTime = Time.time + attack.Cooldown;
            var context = new PlayerAttackContext(gameObject, _muzzlePoint, _meleeHitbox);
            attack.Execute(in context);
            AttackExecuted?.Invoke(attack);
        }
    }
}
