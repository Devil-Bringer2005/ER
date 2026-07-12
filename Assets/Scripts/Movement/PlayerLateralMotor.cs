namespace EndlessRunner.Player.Movement
{
    using System;
    using UnityEngine;
    using EndlessRunner.Player.Controls;

    /// <summary>
    /// Owns left/right velocity. Speed builds up while input is held toward a direction
    /// (acceleration-based, Chase the Sun style) rather than snapping to a target speed,
    /// and decays back toward zero when input is released or reversed.
    ///
    /// Velocity (what drives tilt/animation) and position (what the lane bound clamps) are
    /// deliberately separate: hitting a lane wall clips how far you actually move, but never
    /// zeroes the underlying velocity, so holding input into a wall still reads as "pressing"
    /// for animation purposes instead of going dead the instant you touch the boundary.
    ///
    /// snapOnDirectionReversal picks which of two feels a reversal (holding right, then
    /// switching to left mid-motion) gets: false (default) keeps the original momentum-
    /// preserving curve - velocity decelerates through zero and back up as one continuous
    /// MoveTowards ramp. true snaps velocity straight to zero the instant the reversal is
    /// detected, so the new direction starts its own fresh acceleration ramp from rest
    /// instead of continuing the old ramp in reverse.
    /// </summary>
    [AddComponentMenu("Player/Movement/Player Lateral Motor")]
    public class PlayerLateralMotor : MonoBehaviour, IMovementContributor
    {
        [SerializeField] private LateralMovementConfig _config;

        [Tooltip("false = original behaviour: a reversal decelerates through zero and back up as one continuous curve, preserving momentum. true = snap velocity to zero the instant input reverses direction, so the new direction accelerates from rest instead of from the old velocity.")]
        [SerializeField] private bool snapOnDirectionReversal = false;

        private IPlayerInputSource _inputSource;
        private float _currentVelocity;
        private float _currentPosition; // distance from lane center, used only for the optional clamp

        /// <summary>Current lateral speed divided by max speed, in [-1, 1]. Drives strafe blend trees and tilt.</summary>
        public float NormalizedVelocity => _config != null && _config.MaxSpeed > 0f
            ? Mathf.Clamp(_currentVelocity / _config.MaxSpeed, -1f, 1f)
            : 0f;

        /// <summary>
        /// Signed instantaneous lateral acceleration, clamped to [-1, 1]. Spikes on input
        /// press/release/reversal and settles to 0 once velocity plateaus (at max speed or
        /// at rest) - useful for transient effects like a camera kick or dust puff on a hard
        /// direction change, but NOT what tilt is driven by (see PlayerTiltController, which
        /// uses NormalizedVelocity so the lean persists for as long as you hold input).
        /// With snapOnDirectionReversal enabled, a reversal produces an even sharper spike
        /// here (the whole old velocity is undone in one frame), which suits camera-kick-style
        /// effects well.
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

        /// <summary>
        /// Instantly adds to the current lateral velocity, clamped to the configured max speed.
        /// Used for collision knockback - tilt and animation react automatically since they
        /// read this motor's velocity, no separate wiring needed on the caller's end.
        /// </summary>
        public void ApplyLateralImpulse(float velocityDelta)
        {
            if (_config == null) return;
            //_currentVelocity = Mathf.Clamp(_currentVelocity + velocityDelta, -_config.MaxSpeed, _config.MaxSpeed);
            _currentVelocity = _currentVelocity + velocityDelta;
        }

        public Vector3 GetFrameMovement(float deltaTime)
        {
            if (_config == null) return Vector3.zero;

            Tick(deltaTime);

            float delta = _currentVelocity * deltaTime;

            if (_config.LaneHalfWidth > 0f)
            {
                float projected = Mathf.Clamp(_currentPosition + delta, -_config.LaneHalfWidth, _config.LaneHalfWidth);
                delta = projected - _currentPosition;
                _currentPosition = projected;
            }
            else
            {
                _currentPosition += delta;
            }

            return transform.right * delta;
        }

        private void Tick(float deltaTime)
        {
            float input = _inputSource != null ? _inputSource.LateralInput : 0f;
            float previousVelocity = _currentVelocity;

            bool hasInput = Mathf.Abs(input) > 0.01f;
            float targetVelocity = input * _config.MaxSpeed;

            // _currentVelocity * input < 0f is true exactly when the two have
            // opposite signs and are both non-zero - i.e. genuinely moving one
            // way while input now wants the other way. (Using this product
            // instead of comparing Mathf.Sign() avoids Mathf.Sign(0) == 1
            // falsely counting rest-to-moving as a "reversal".)
            if (snapOnDirectionReversal && _currentVelocity * input < 0f)
            {
                // Snap straight to zero instead of letting MoveTowards below
                // decelerate through it - so THIS frame's MoveTowards starts a
                // fresh acceleration ramp toward the new target instead of
                // continuing the old ramp in reverse.
                _currentVelocity = 0f;
            }

            float rate = hasInput ? _config.Acceleration : _config.Deceleration;

            _currentVelocity = Mathf.MoveTowards(_currentVelocity, targetVelocity, rate * deltaTime);

            float instantaneousAccel = deltaTime > 0f ? (_currentVelocity - previousVelocity) / deltaTime : 0f;
            float maxAccel = Mathf.Max(_config.Acceleration, _config.Deceleration, 0.0001f);
            ClampedAcceleration = Mathf.Clamp(instantaneousAccel / maxAccel, -1f, 1f);

            LateralVelocityChanged?.Invoke(NormalizedVelocity);
        }
    }
}