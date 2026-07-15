namespace EndlessRunner.Player.Movement
{
    using EndlessRunner.Player.Controls;
    using MoreMountains.Feedbacks;
    using System;
    using UnityEngine;

    /// <summary>
    /// Adds extra forward speed while the boost key is held and charge remains, draining
    /// SpeedBoostMeter as it goes. Implements IMovementContributor so PlayerMotorDriver picks
    /// it up on its own - PlayerForwardMotor and PlayerMotorDriver are untouched.
    /// </summary>
    [AddComponentMenu("Player/Movement/Speed Boost Motor")]
    public class SpeedBoostMotor : MonoBehaviour, IMovementContributor
    {
        [SerializeField] private SpeedBoostMeter _meter;
        [SerializeField] private SpeedBoostConfig _config;
        [SerializeField] private MMF_Player boostStartFeedback;
        [SerializeField] private MMF_Player boostEndFeedback;

        private IPlayerInputSource _inputSource;
        private bool boost = false;

        /// <summary>Whether the boost was actively applied on the last frame - hook UI/VFX off this.</summary>
        public bool IsBoosting => _config != null && boost;// && _meter.HasCharge;

        private void Awake()
        {
            _inputSource = GetComponentInParent<IPlayerInputSource>();
            if (_inputSource == null)
                Debug.LogError($"{nameof(PlayerLateralMotor)} needs an {nameof(IPlayerInputSource)} on this object or a parent.", this);

            if (_config == null)
                Debug.LogError($"{nameof(PlayerLateralMotor)} is missing its {nameof(LateralMovementConfig)}.", this);

            _inputSource.SpeedBoostPressed += HandleSpeedBoost;
        }

        private void HandleSpeedBoost(bool isBoosting)
        {
            bool wasBoosting = boost;
            boost = isBoosting;

            if (!wasBoosting && IsBoosting)
            {
                boostStartFeedback?.PlayFeedbacks();
            }
            else if (wasBoosting && !IsBoosting)
            {
                boostEndFeedback?.PlayFeedbacks();
            }
        }

        public Vector3 GetFrameMovement(float deltaTime)
        {
            if (!IsBoosting) return Vector3.zero;

            _meter.Drain(_config.DrainPerSecond * deltaTime);
            return transform.forward * (_config.BoostSpeed * deltaTime);
        }
    }
}