namespace EndlessRunner.Player.Movement
{
    using System;
    using UnityEngine;
    using EndlessRunner.Player.Controls;

    /// <summary>
    /// Owns left/right velocity. Speed builds up while input is held toward a direction
    /// (acceleration-based, Chase the Sun style) rather than snapping to a target speed,
    /// and decays back toward zero when input is released or reversed.
    /// </summary>
    [AddComponentMenu("Player/Movement/Player Lateral Motor")]
    public class PlayerLateralMotor : MonoBehaviour, IMovementContributor
    {
        [SerializeField] private LateralMovementConfig _config;

        private IPlayerInputSource _inputSource;
        private float _currentVelocity;
        private float _currentPosition; // distance from lane center, used only for the optional clamp

        /// <summary>Current lateral speed divided by max speed, in [-1, 1]. Drives strafe blend trees.</summary>
        public float NormalizedVelocity => _config != null && _config.MaxSpeed > 0f
            ? Mathf.Clamp(_currentVelocity / _config.MaxSpeed, -1f, 1f)
            : 0f;

        /// <summary>
        /// Signed instantaneous lateral acceleration, clamped to [-1, 1]. Spikes on input
        /// press/release/reversal and settles to 0 once velocity plateaus (at max speed or
        /// at rest) - useful for transient effects like a camera kick or dust puff on a hard
        /// direction change, but NOT what tilt is driven by (see PlayerTiltController, which
        /// uses NormalizedVelocity so the lean persists for as long as you hold input).
        /// </summary>
        public float ClampedAcceleration { get; private set; }

        /// <summary>Fires whenever NormalizedVelocity changes, for listeners that prefer events over polling.</summary>
        public event Action<float> LateralVelocityChanged;

        private void Awake()
        {
            _inputSource = GetComponentInParent<IPlayerInputSource>();
            if (_inputSource == null)
                Debug.LogError($"{nameof(PlayerLateralMotor)} needs an {nameof(IPlayerInputSource)} on this object or a parent.", this);

            if (_config == null)
                Debug.LogError($"{nameof(PlayerLateralMotor)} is missing its {nameof(LateralMovementConfig)}.", this);
        }

        public Vector3 GetFrameMovement(float deltaTime)
        {
            Tick(deltaTime);
            return transform.right * (_currentVelocity * deltaTime);
        }

        private void Tick(float deltaTime)
        {
            if (_config == null) return;

            float input = _inputSource != null ? _inputSource.LateralInput : 0f;
            float previousVelocity = _currentVelocity;

            bool hasInput = Mathf.Abs(input) > 0.01f;
            float targetVelocity = input * _config.MaxSpeed;
            float rate = hasInput ? _config.Acceleration : _config.Deceleration;

            _currentVelocity = Mathf.MoveTowards(_currentVelocity, targetVelocity, rate * deltaTime);

            if (_config.LaneHalfWidth > 0f)
            {
                float projected = _currentPosition + _currentVelocity * deltaTime;
                if (Mathf.Abs(projected) > _config.LaneHalfWidth)
                {
                    projected = Mathf.Clamp(projected, -_config.LaneHalfWidth, _config.LaneHalfWidth);
                    _currentVelocity = 0f;
                }
                _currentPosition = projected;
            }

            float instantaneousAccel = deltaTime > 0f ? (_currentVelocity - previousVelocity) / deltaTime : 0f;
            float maxAccel = Mathf.Max(_config.Acceleration, _config.Deceleration, 0.0001f);
            ClampedAcceleration = Mathf.Clamp(instantaneousAccel / maxAccel, -1f, 1f);

            LateralVelocityChanged?.Invoke(NormalizedVelocity);
        }
    }
}