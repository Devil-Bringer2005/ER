namespace EndlessRunner.Player.Animation
{
    using System.Collections.Generic;
    using UnityEngine;
    using EndlessRunner.Player.Movement;
    using EndlessRunner.Player.Combat;
    using EndlessRunner.Player.Collision;

    /// <summary>
    /// The only script that talks to the Animator. Movement, combat, and collision systems
    /// know nothing about animation - they just expose state (properties) and events - so the
    /// rig, blend trees, or even the whole Animator can be swapped without touching gameplay code.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    [AddComponentMenu("Player/Animation/Player Animation Controller")]
    public class PlayerAnimationController : MonoBehaviour
    {
        [SerializeField] private PlayerLateralMotor _lateralMotor;
        [SerializeField] private PlayerForwardMotor _forwardMotor;
        [SerializeField] private PlayerAttackController _attackController;
        [SerializeField] private PlayerCollisionHandler _collisionHandler;

        private readonly Dictionary<AttackDefinitionSO, bool> _usePrimaryTrigger = new();

        private Animator _animator;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        private void OnEnable()
        {
            if (_attackController != null)
                _attackController.AttackExecuted += HandleAttackExecuted;

            if (_collisionHandler != null)
                _collisionHandler.ImpactOccurred += HandleImpact;
        }

        private void OnDisable()
        {
            if (_attackController != null)
                _attackController.AttackExecuted -= HandleAttackExecuted;

            if (_collisionHandler != null)
                _collisionHandler.ImpactOccurred -= HandleImpact;
        }

        private void LateUpdate()
        {
            // LateUpdate runs after PlayerMotorDriver's Update, so these values are fresh
            // for this frame - no explicit ordering/event wiring needed for continuous state.
            if (_lateralMotor != null)
                _animator.SetFloat(
                    PlayerAnimatorParameters.LateralSpeed,
                    _lateralMotor.NormalizedVelocity);

            if (_forwardMotor != null)
                _animator.SetFloat(
                    PlayerAnimatorParameters.ForwardSpeed,
                    _forwardMotor.NormalizedSpeed);
        }

        private void HandleAttackExecuted(AttackDefinitionSO attack)
        {
            if (attack == null)
                return;

            bool usePrimary = true;

            if (_usePrimaryTrigger.TryGetValue(attack, out bool previous))
                usePrimary = !previous;

            _usePrimaryTrigger[attack] = usePrimary;

            string trigger = usePrimary
                ? attack.AnimatorTrigger
                : attack.AnimatorTrigger2;

            if (!string.IsNullOrEmpty(trigger))
                _animator.SetTrigger(trigger);
        }

        private void HandleImpact(CollisionImpactInfo info)
        {
            if (!string.IsNullOrEmpty(info.AnimatorTrigger))
                _animator.SetTrigger(info.AnimatorTrigger);
        }
    }
}