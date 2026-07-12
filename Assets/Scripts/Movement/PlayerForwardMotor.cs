namespace EndlessRunner.Player.Movement
{
    using UnityEngine;

    /// <summary>
    /// Constant, non-input-driven forward motion with an optional endless-runner speed ramp.
    /// Fully independent from lateral movement so difficulty tuning never touches strafe feel.
    /// </summary>
    [AddComponentMenu("Player/Movement/Player Forward Motor")]
    public class PlayerForwardMotor : MonoBehaviour, IMovementContributor
    {
        [SerializeField] private ForwardMovementConfig _config;
        [SerializeField] private bool _isRunning = true;

        private float _elapsed;
        private float _speedPenalty;
        private float _penaltyRecoveryRate;

        /// <summary>Current forward speed, units/sec.</summary>
        [field: SerializeField] public float CurrentSpeed { get; private set; }

        /// <summary>Current forward speed divided by max speed, in [0, 1]. Feeds run-speed blend trees.</summary>
        public float NormalizedSpeed => _config != null && _config.MaxSpeed > 0f
            ? Mathf.Clamp01(CurrentSpeed / _config.MaxSpeed)
            : 0f;

        /// <summary>Pause/resume the auto-run, e.g. on death or during a cutscene.</summary>
        public void SetRunning(bool isRunning) => _isRunning = isRunning;

        /// <summary>
        /// Knocks the effective forward speed down by <paramref name="amount"/> (units/sec),
        /// recovering linearly back to full speed over <paramref name="recoveryTime"/> seconds.
        /// The underlying ramp-up (elapsed time) is untouched, so recovery always lands back
        /// on the correct value for how long the run has been going. Used for collision response.
        /// </summary>
        public void ApplySpeedPenalty(float amount, float recoveryTime)
        {
            if (amount <= 0f) return;
            _speedPenalty = Mathf.Max(_speedPenalty, amount);
            _penaltyRecoveryRate = amount / Mathf.Max(recoveryTime, 0.0001f);
        }

        public Vector3 GetFrameMovement(float deltaTime)
        {
            if (!_isRunning || _config == null)
            {
                CurrentSpeed = 0f;
                return Vector3.zero;
            }

            _elapsed += deltaTime;
            _speedPenalty = Mathf.MoveTowards(_speedPenalty, 0f, _penaltyRecoveryRate * deltaTime);

            float rampedSpeed = Mathf.Min(_config.BaseSpeed + _config.SpeedRampPerSecond * _elapsed, _config.MaxSpeed);
            CurrentSpeed = Mathf.Max(0f, rampedSpeed - _speedPenalty);
            return transform.forward * (CurrentSpeed * deltaTime);
        }
    }
}