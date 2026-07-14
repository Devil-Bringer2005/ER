namespace EndlessRunner.Player.Animation
{
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
        [SerializeField] private PlayerJumpController _jumpController;

        private Animator _animator;

        private void Awake() => _animator = GetComponent<Animator>();

        private void OnEnable()
        {
            if (_attackController != null)
                _attackController.AttackExecuted += HandleAttackExecuted;
            if (_collisionHandler != null)
                _collisionHandler.ImpactOccurred += HandleImpact;
            if (_jumpController != null)
                _jumpController.Jumped += HandleJumped;
        }

        private void OnDisable()
        {
            if (_attackController != null)
                _attackController.AttackExecuted -= HandleAttackExecuted;
            if (_collisionHandler != null)
                _collisionHandler.ImpactOccurred -= HandleImpact;
            if (_jumpController != null)
                _jumpController.Jumped -= HandleJumped;
        }

        private void LateUpdate()
        {
            // LateUpdate runs after PlayerMotorDriver's Update, so these values are fresh
            // for this frame - no explicit ordering/event wiring needed for continuous state.
            if (_lateralMotor != null)
                _animator.SetFloat(PlayerAnimatorParameters.LateralSpeed, _lateralMotor.NormalizedVelocity);

            if (_forwardMotor != null)
                _animator.SetFloat(PlayerAnimatorParameters.ForwardSpeed, _forwardMotor.NormalizedSpeed);

            if (_jumpController != null)
                _animator.SetBool(PlayerAnimatorParameters.IsGrounded, _jumpController.IsGrounded);
        }

        private void HandleAttackExecuted(AttackDefinitionSO attack)
        {
            if (!string.IsNullOrEmpty(attack.AnimatorTrigger))
                _animator.SetTrigger(attack.AnimatorTrigger);
        }

        private void HandleImpact(CollisionImpactInfo info)
        {
            if (!string.IsNullOrEmpty(info.AnimatorTrigger))
                _animator.SetTrigger(info.AnimatorTrigger);
        }

        private void HandleJumped(string animatorTrigger)
        {
            if (!string.IsNullOrEmpty(animatorTrigger))
                _animator.SetTrigger(animatorTrigger);
        }
    }
}