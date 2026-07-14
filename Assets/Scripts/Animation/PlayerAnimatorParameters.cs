namespace EndlessRunner.Player.Animation
{
    using UnityEngine;

    /// <summary>
    /// Cached hashes for the continuous (per-frame) Animator parameters, so we don't pay
    /// string-hashing cost every frame. Attack triggers are NOT here on purpose - each
    /// AttackDefinitionSO carries its own trigger name so different attacks can drive
    /// different Animator states without editing this file.
    /// </summary>
    public static class PlayerAnimatorParameters
    {
        public static readonly int LateralSpeed = Animator.StringToHash("LateralSpeed");
        public static readonly int ForwardSpeed = Animator.StringToHash("ForwardSpeed");
        public static readonly int IsGrounded = Animator.StringToHash("IsGrounded");
    }
}