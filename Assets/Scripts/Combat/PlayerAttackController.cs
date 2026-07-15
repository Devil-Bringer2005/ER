namespace EndlessRunner.Player.Combat
{
    using System;
    using System.Collections.Generic;
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

        // LIFO per slot rather than a single "remembered previous" value, so overlapping
        // temporary weapon powerups nest correctly instead of one clobbering the other's
        // restore point. State lives here on the instance, not on the shared effect SO asset.
        private readonly Stack<AttackDefinitionSO> _meleeOverrides = new Stack<AttackDefinitionSO>();
        private readonly Stack<AttackDefinitionSO> _rangedOverrides = new Stack<AttackDefinitionSO>();

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

        /// <summary>Temporarily replaces the ranged slot; pair with PopRangedAttack to restore it.</summary>
        public void PushRangedAttack(AttackDefinitionSO attack)
        {
            _rangedOverrides.Push(_rangedAttack);
            _rangedAttack = attack;
        }

        public void PopRangedAttack()
        {
            if (_rangedOverrides.Count > 0) _rangedAttack = _rangedOverrides.Pop();
        }

        /// <summary>Temporarily replaces the melee slot; pair with PopMeleeAttack to restore it.</summary>
        public void PushMeleeAttack(AttackDefinitionSO attack)
        {
            _meleeOverrides.Push(_meleeAttack);
            _meleeAttack = attack;
        }

        public void PopMeleeAttack()
        {
            if (_meleeOverrides.Count > 0) _meleeAttack = _meleeOverrides.Pop();
        }

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