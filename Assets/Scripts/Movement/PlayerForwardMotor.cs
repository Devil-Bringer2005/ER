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

        /// <summary>Current forward speed, units/sec.</summary>
        public float CurrentSpeed { get; private set; }

        /// <summary>Current forward speed divided by max speed, in [0, 1]. Feeds run-speed blend trees.</summary>
        public float NormalizedSpeed => _config != null && _config.MaxSpeed > 0f
            ? Mathf.Clamp01(CurrentSpeed / _config.MaxSpeed)
            : 0f;

        /// <summary>Pause/resume the auto-run, e.g. on death or during a cutscene.</summary>
        public void SetRunning(bool isRunning) => _isRunning = isRunning;

        public Vector3 GetFrameMovement(float deltaTime)
        {
            if (!_isRunning || _config == null)
            {
                CurrentSpeed = 0f;
                return Vector3.zero;
            }

            _elapsed += deltaTime;
            CurrentSpeed = Mathf.Min(_config.BaseSpeed + _config.SpeedRampPerSecond * _elapsed, _config.MaxSpeed);
            return transform.forward * (CurrentSpeed * deltaTime);
        }
    }
}
