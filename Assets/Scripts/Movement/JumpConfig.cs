namespace EndlessRunner.Player.Movement
{
    using UnityEngine;

    [CreateAssetMenu(menuName = "Player/Movement/Jump Config", fileName = "JumpConfig")]
    public class JumpConfig : ScriptableObject
    {
        [Header("Gravity")]
        [Tooltip("Downward acceleration applied while airborne, units/sec^2. Should be negative.")]
        [SerializeField] private float _gravity = -20f;

        [Header("Jump Arc")]
        [Tooltip("Peak height of a ground jump, in units. Jump velocity is derived from this and Gravity, so retuning gravity later keeps the arc physically correct automatically.")]
        [SerializeField] private float _jumpHeight = 2f;

        [Header("Air Jumps")]
        [Tooltip("Extra jumps allowed while airborne, after coyote time has expired. 0 = no double jump.")]
        [SerializeField] private int _maxAirJumps = 0;

        [Header("Feel")]
        [Tooltip("Grace period after walking off a ledge where a jump still counts as a ground jump.")]
        [SerializeField] private float _coyoteTime = 0.12f;

        [Tooltip("How early a jump press is remembered before landing - pressing jump just before touchdown still triggers a jump.")]
        [SerializeField] private float _jumpBufferTime = 0.12f;

        [Header("Animation")]
        [SerializeField] private string _jumpAnimatorTrigger = "Jump";
        [SerializeField] private string _airJumpAnimatorTrigger = "AirJump";

        public float Gravity => _gravity;
        public float JumpHeight => _jumpHeight;
        public int MaxAirJumps => _maxAirJumps;
        public float CoyoteTime => _coyoteTime;
        public float JumpBufferTime => _jumpBufferTime;
        public string JumpAnimatorTrigger => _jumpAnimatorTrigger;
        public string AirJumpAnimatorTrigger => _airJumpAnimatorTrigger;
    }
}