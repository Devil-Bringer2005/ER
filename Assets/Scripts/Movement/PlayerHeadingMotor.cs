namespace EndlessRunner.Player.Movement
{
    using System;
    using UnityEngine;
    using EndlessRunner.Player.Controls;

    /// <summary>
    /// Owns yaw rotation. Turn speed builds up while input is held toward a direction
    /// (acceleration-based, same feel as the old lateral motor) and decays back toward
    /// zero when input is released or reversed. Unlike a strafe motor, this doesn't move
    /// the player sideways - it rotates the transform, so PlayerForwardMotor (which drives
    /// along transform.forward) automatically carries the player in the new facing
    /// direction. Releasing input holds the current heading instead of snapping back to
    /// world +Z.
    /// </summary>
    [AddComponentMenu("Player/Movement/Player Heading Motor")]
    public class PlayerHeadingMotor : MonoBehaviour, IMovementContributor
    {
        [SerializeField] private HeadingMovementConfig _config; // MaxTurnSpeed, Acceleration, Deceleration, MaxHeadingAngle (0 = no clamp)

        [Tooltip("false = a reversal decelerates through zero and back up as one continuous curve. true = snap angular velocity to zero the instant input reverses, so the new direction starts a fresh turn from rest.")]
        [SerializeField] private bool snapOnDirectionReversal = false;

        private IPlayerInputSource _inputSource;
        private float _currentAngularVelocity; // deg/sec
        private float _currentHeadingOffset;   // deg, relative to the heading at Awake - only used for the optional clamp

        public float NormalizedTurnRate => _config != null && _config.MaxTurnSpeed > 0f
            ? Mathf.Clamp(_currentAngularVelocity / _config.MaxTurnSpeed, -1f, 1f)
            : 0f;

        public float ClampedAcceleration { get; private set; }

        public event Action<float> HeadingTurnRateChanged;

        private void Awake()
        {
            _inputSource = GetComponentInParent<IPlayerInputSource>();
            if (_inputSource == null)
                Debug.LogError($"{nameof(PlayerHeadingMotor)} needs an {nameof(IPlayerInputSource)} on this object or a parent.", this);

            if (_config == null)
                Debug.LogError($"{nameof(PlayerHeadingMotor)} is missing its {nameof(HeadingMovementConfig)}.", this);
        }

        public Vector3 GetFrameMovement(float deltaTime)
        {
            if (_config == null) return Vector3.zero;

            Tick(deltaTime);

            // No displacement of its own - PlayerForwardMotor supplies movement
            // along the heading this rotation just produced.
            return Vector3.zero;
        }

        private void Tick(float deltaTime)
        {
            float input = _inputSource != null ? _inputSource.LateralInput : 0f;
            float previousAngularVelocity = _currentAngularVelocity;

            bool hasInput = Mathf.Abs(input) > 0.01f;
            float targetAngularVelocity = input * _config.MaxTurnSpeed;

            if (snapOnDirectionReversal && _currentAngularVelocity * input < 0f)
                _currentAngularVelocity = 0f;

            float rate = hasInput ? _config.Acceleration : _config.Deceleration;
            _currentAngularVelocity = Mathf.MoveTowards(_currentAngularVelocity, targetAngularVelocity, rate * deltaTime);

            float turnDelta = _currentAngularVelocity * deltaTime;

            if (_config.MaxHeadingAngle > 0f)
            {
                float projected = Mathf.Clamp(_currentHeadingOffset + turnDelta, -_config.MaxHeadingAngle, _config.MaxHeadingAngle);
                turnDelta = projected - _currentHeadingOffset;
                _currentHeadingOffset = projected;
            }

            transform.Rotate(Vector3.up, turnDelta, Space.World);

            float instantaneousAccel = deltaTime > 0f ? (_currentAngularVelocity - previousAngularVelocity) / deltaTime : 0f;
            float maxAccel = Mathf.Max(_config.Acceleration, _config.Deceleration, 0.0001f);
            ClampedAcceleration = Mathf.Clamp(instantaneousAccel / maxAccel, -1f, 1f);

            HeadingTurnRateChanged?.Invoke(NormalizedTurnRate);
        }
    }
}