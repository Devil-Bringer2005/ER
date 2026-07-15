namespace EndlessRunner.Player.Movement
{
    using System;
    using UnityEngine;
    using EndlessRunner.Player.Controls;

    /// <summary>
    /// Owns vertical velocity: gravity integration, ground-stick, and jump (coyote time,
    /// buffering, air jumps) all live here - the same shape as PlayerLateralMotor owning
    /// lateral velocity and PlayerForwardMotor owning forward speed. Implements
    /// IMovementContributor like they do, so PlayerMotorDriver just discovers and sums it;
    /// no direct reference or special-casing on the driver's side, and no cross-component
    /// call-order dependency the way a push-based "tell the driver to jump" API would need.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    [AddComponentMenu("Player/Movement/Player Jump Controller")]
    public class PlayerJumpController : MonoBehaviour, IMovementContributor
    {
        [SerializeField] private JumpConfig _config;

        private CharacterController _controller;
        private IPlayerInputSource _inputSource;
        private float _verticalVelocity;
        private float _coyoteTimer;
        private float _jumpBufferTimer;
        private int _airJumpsUsed;

        /// <summary>True if the CharacterController is touching the ground this frame.</summary>
        public bool IsGrounded => _controller != null && _controller.isGrounded;

        /// <summary>Current vertical velocity, units/sec. Positive = ascending, negative = falling.</summary>
        public float VerticalVelocity => _verticalVelocity;

        /// <summary>
        /// Extra air jumps on top of JumpConfig.MaxAirJumps. Lives here rather than on the SO
        /// so a temporary or permanent "double jump" powerup can add/remove it at runtime
        /// without mutating a config asset shared by every instance using it.
        /// </summary>
        public int BonusAirJumps { get; set; }

        /// <summary>Fires the moment a jump (ground or air) is actually performed, with the animator trigger to play.</summary>
        public event Action<string> Jumped;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            if (_controller == null)
                Debug.LogError($"{nameof(PlayerJumpController)} needs a {nameof(CharacterController)} on this object.", this);

            _inputSource = GetComponentInParent<IPlayerInputSource>();
            if (_inputSource == null)
                Debug.LogError($"{nameof(PlayerJumpController)} needs an {nameof(IPlayerInputSource)} on this object or a parent.", this);

            if (_config == null)
                Debug.LogError($"{nameof(PlayerJumpController)} is missing its {nameof(JumpConfig)}.", this);
        }

        private void OnEnable()
        {
            if (_inputSource != null) _inputSource.JumpPressed += BufferJump;
        }

        private void OnDisable()
        {
            if (_inputSource != null) _inputSource.JumpPressed -= BufferJump;
        }

        private void BufferJump()
        {
            if (_config != null) _jumpBufferTimer = _config.JumpBufferTime;
        }

        public Vector3 GetFrameMovement(float deltaTime)
        {
            if (_config == null || _controller == null) return Vector3.zero;

            Tick(deltaTime);
            return Vector3.up * (_verticalVelocity * deltaTime);
        }

        private void Tick(float deltaTime)
        {
            bool grounded = _controller.isGrounded;

            if (grounded)
            {
                _coyoteTimer = _config.CoyoteTime;
                _airJumpsUsed = 0;
            }
            else
            {
                _coyoteTimer = Mathf.Max(0f, _coyoteTimer - deltaTime);
            }

            _jumpBufferTimer = Mathf.Max(0f, _jumpBufferTimer - deltaTime);

            if (_jumpBufferTimer > 0f)
            {
                if (grounded || _coyoteTimer > 0f)
                {
                    _coyoteTimer = 0f;
                    PerformJump(_config.JumpAnimatorTrigger);
                }
                else if (_airJumpsUsed < _config.MaxAirJumps + BonusAirJumps)
                {
                    _airJumpsUsed++;
                    PerformJump(_config.AirJumpAnimatorTrigger);
                }
            }

            // Ground-stick vs. gravity. Checked after the jump attempt above, so a jump
            // performed this frame (positive velocity) falls into the gravity branch below
            // instead of being immediately reset to the stick value.
            if (grounded && _verticalVelocity <= 0f)
                _verticalVelocity = -1f; 
            else
                _verticalVelocity += _config.Gravity * deltaTime;
        }

        private void PerformJump(string animatorTrigger)
        {
            float jumpVelocity = Mathf.Sqrt(2f * Mathf.Abs(_config.Gravity) * Mathf.Max(_config.JumpHeight, 0f));
            _verticalVelocity = jumpVelocity;
            _jumpBufferTimer = 0f;
            Jumped?.Invoke(animatorTrigger);
        }
    }
}